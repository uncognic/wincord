using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace WinCord
{

    public partial class MainForm : Form
    {
        private DiscordClient _discord;
        private string _currentChannelId;
        private string _guildId;
        private ClientWebSocket _ws;
        public MainForm(string token)
        {
            InitializeComponent();
            _discord = new DiscordClient(token);
            _currentChannelId = null;
            StartWebSocket(token);
            _ = LoadUserInfo();
        }

        private async Task LoadUserInfo()
        {
            try
            {
                var user = await _discord.GetCurrentUser();
                SetTitle(user.username);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load user info: {ex.Message}");
            }
        }

        private void SetTitle(string username)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => SetTitle(username)));
                return;
            }

            this.Text = $"WinCord - {username}";
        }

        private void AddMessage(string author, string content, DateTime? timestamp = null)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => AddMessage(author, content, timestamp)));
                return;
            }
            
            string time = (timestamp ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss");
            chatBox.AppendText($"[{time}] {author}: {content}\n");
            chatBox.SelectionStart = chatBox.Text.Length;
            chatBox.ScrollToCaret();
        }
        private void UpdateConnectionStatus(string status)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateConnectionStatus(status)));
                return;
            }

            connectionStatusLabel.Text = status;
        }
        private async Task SendMessage()
        {
            string message = textBox1.Text.Trim();

            if (string.IsNullOrEmpty(message))
                return;

            textBox1.Clear();
            if (string.IsNullOrEmpty(_currentChannelId))
            {
                MessageBox.Show("Please select a channel first.");
                return;
            }
            try
            {
                await _discord.SendMessage(_currentChannelId, message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Send failed");
            }
        }

        private async void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                try
                {
                    await SendMessage();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void sendButton_Click(object sender, EventArgs e)
        {
            try
            {
                await SendMessage();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private int? _lastSequence = null;
        private async void StartWebSocket(string token)
        {
            try
            {
                UpdateConnectionStatus("Connecting...");
                _ws = new ClientWebSocket();
                await _ws.ConnectAsync(new Uri("wss://gateway.discord.gg/?v=9&encoding=json"), CancellationToken.None);
                UpdateConnectionStatus("Connected");

                var identify = new
                {
                    op = 2,
                    d = new
                    {
                        token = token,
                        properties = new
                        {
                            os = "windows",
                            browser = "wincord",
                            device = "wincord"
                        }
                    }
                };
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(identify);
                var bytes = Encoding.UTF8.GetBytes(json);
                await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

                _ = Task.Run(async () =>
                {
                    var buffer = new ArraySegment<byte>(new byte[8192]);
                    var ms = new MemoryStream();
                    int heartbeatInterval = 0;

                    while (_ws.State == WebSocketState.Open)
                    {
                        try
                        {
                            ms.SetLength(0);
                            WebSocketReceiveResult result;

                            do
                            {
                                result = await _ws.ReceiveAsync(buffer, CancellationToken.None);
                                ms.Write(buffer.Array, buffer.Offset, result.Count);
                            } while (!result.EndOfMessage);

                            if (result.MessageType == WebSocketMessageType.Text)
                            {
                                var msg = Encoding.UTF8.GetString(ms.ToArray());
                                try
                                {
                                    var obj = JObject.Parse(msg);

                                    if (obj["s"] != null)
                                        _lastSequence = (int?)obj["s"];

                                    if (obj["op"] != null && (int)obj["op"] == 10)
                                    {
                                        heartbeatInterval = (int)obj["d"]["heartbeat_interval"];
                                        _ = StartHeartbeat(heartbeatInterval);
                                    }

                                    if (obj["t"] != null && obj["t"].ToString() == "MESSAGE_CREATE")
                                    {
                                        var channelId = obj["d"]["channel_id"].ToString();
                                        if (channelId == _currentChannelId)
                                        {
                                            var author = obj["d"]["author"]["username"].ToString();
                                            var content = obj["d"]["content"].ToString();
                                            AddMessage(author, content);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"JSON parse error: {ex.Message}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"WebSocket receive error: {ex.Message}");
                            UpdateConnectionStatus("Disconnected");
                            break;
                        }
                    }

                    UpdateConnectionStatus("Disconnected");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WebSocket connection error: {ex.Message}");
                UpdateConnectionStatus($"Connection failed: {ex.Message}");
                MessageBox.Show($"Failed to connect to Discord: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task StartHeartbeat(int interval)
        {
            while (_ws.State == WebSocketState.Open)
            {
                var heartbeat = new { op = 1, d = _lastSequence };
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(heartbeat);
                var bytes = Encoding.UTF8.GetBytes(json);

                try
                {
                    await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Heartbeat error: {ex.Message}");
                    UpdateConnectionStatus("Connection lost");
                    break;
                }

                await Task.Delay(interval);
            }
        }
        private async Task PopulateChannels(string guildId)
        {
            var channels = await _discord.GetChannels(guildId);

            listBoxChannels.Items.Clear();

            foreach (var c in channels.Where(c => c.type == 0))
            {
                listBoxChannels.Items.Add(new ChannelItem { Id = c.id, Name = c.name });
            }
        }

        public class ChannelItem
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public override string ToString() => Name;
        }
        public class GuildItem
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public override string ToString() => Name;
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            
            _ = PopulateGuilds();
        }

        private async void listBoxChannels_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (listBoxChannels.SelectedItem is ChannelItem selected)
            {
                _currentChannelId = selected.Id;
                await LoadMessages(_currentChannelId);
            }
        }
        private async Task LoadMessages(string channelId)
        {
            if (string.IsNullOrEmpty(channelId)) return;

            chatBox.Clear();

            try
            {
                var messages = await _discord.GetMessages(channelId, 50); 
                foreach (var msg in messages.AsEnumerable().Reverse())
                {
                    AddMessage(msg.author.username, msg.content, msg.timestamp);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load messages: {ex.Message}");
            }
        }
        private async Task PopulateGuilds()
        {
            var guilds = await _discord.GetGuilds();

            listBoxGuilds.Items.Clear();
            foreach (var g in guilds)
            {
                listBoxGuilds.Items.Add(new GuildItem
                {
                    Id = g.id,
                    Name = g.name
                });
            }
        }
        private void chatBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            using (var about = new AboutForm())
            {
                about.ShowDialog(this);
            }
        }
       

        private async void listBoxGuilds_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxGuilds.SelectedItem is GuildItem guild)
            {
                _guildId = guild.Id;
                await PopulateChannels(_guildId);
                chatBox.Clear();
            }
        }

        private void LogOut_Click(object sender, EventArgs e)
        {
            TokenStorage.DeleteToken();
            Application.Restart();
        }

        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(chatBox.Text))
            {
                MessageBox.Show("No chat messages to export.", "Export Chat", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                saveDialog.DefaultExt = "txt";

                string channelName = "Chat";
                if (listBoxChannels.SelectedItem is ChannelItem selectedChannel)
                {
                    channelName = selectedChannel.Name;
                }

                string safeChannelName = string.Join("_", channelName.Split(Path.GetInvalidFileNameChars()));
                saveDialog.FileName = $"WinCord_{safeChannelName}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                saveDialog.Title = "Export Chat to File";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllText(saveDialog.FileName, chatBox.Text);
                        MessageBox.Show($"Chat exported successfully to:\n{saveDialog.FileName}", "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to export chat:\n{ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
