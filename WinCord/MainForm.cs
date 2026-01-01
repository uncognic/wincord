using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebSocketSharp;

namespace WinCord
{

    public partial class MainForm : Form
    {
        private DiscordClient _discord;
        private string _currentChannelId;
        private string _guildId;
        private WebSocket _ws;
        private string _token;
        private int? _lastSequence = null;
        private CancellationTokenSource _heartbeatCts;
        private UserPreferences _preferences;

        public MainForm(string token)
        {
            try
            {
                SimpleLogger.Log("MainForm constructor started");
                Debug.WriteLine("MainForm constructor started");

                SimpleLogger.Log("Calling InitializeComponent...");
                Debug.WriteLine("Calling InitializeComponent...");
                InitializeComponent();
                SimpleLogger.Log("InitializeComponent completed");
                Debug.WriteLine("InitializeComponent completed");

                SimpleLogger.Log("Loading preferences...");
                Debug.WriteLine("Loading preferences...");
                _preferences = UserPreferences.Load();
                SimpleLogger.Log("Preferences loaded");
                Debug.WriteLine("Preferences loaded");

                SimpleLogger.Log("Creating DiscordClient...");
                Debug.WriteLine("Creating DiscordClient...");
                _discord = new DiscordClient(token);
                SimpleLogger.Log("DiscordClient created");
                Debug.WriteLine("DiscordClient created");

                _token = token;
                _currentChannelId = null;

                SimpleLogger.Log("MainForm constructor completed");
                Debug.WriteLine("MainForm constructor completed");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"ERROR in MainForm constructor: {ex.Message}");
                Debug.WriteLine($"ERROR in MainForm constructor: {ex.Message}");
                SimpleLogger.Log($"Stack trace: {ex.StackTrace}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private async Task LoadUserInfo()
        {
            try
            {
                SimpleLogger.Log("LoadUserInfo started");
                Debug.WriteLine("LoadUserInfo started");
                var user = await _discord.GetCurrentUser();
                SetTitle(user.username);
                await UpdateUserProfile(user.username, user.id, user.avatar);
                SimpleLogger.Log($"User info loaded: {user.username}");
                Debug.WriteLine($"User info loaded: {user.username}");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"Failed to load user info: {ex.Message}");
                Debug.WriteLine($"Failed to load user info: {ex.Message}");
            }
        }

        private async Task UpdateUserProfile(string username, string userId, string avatarHash)
        {
            nameLabel.Text = username;

            try
            {
                SimpleLogger.Log($"Loading avatar for user {userId}, hash: {avatarHash ?? "null"}");
                Debug.WriteLine($"Loading avatar for user {userId}, hash: {avatarHash ?? "null"}");
                
                var avatarImage = await _discord.GetUserAvatar(userId, avatarHash);
                
                if (avatarImage != null)
                {
                    SimpleLogger.Log("Avatar loaded successfully");
                    Debug.WriteLine("Avatar loaded successfully");
                    
                    if (InvokeRequired)
                    {
                        BeginInvoke(new Action(() => 
                        {
                            pictureBox1.Image = avatarImage;
                            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                        }));
                    }
                    else
                    {
                        pictureBox1.Image = avatarImage;
                        pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                }
                else
                {
                    SimpleLogger.Log("Avatar is null, setting gray background");
                    Debug.WriteLine("Avatar is null, setting gray background");
                    
                    if (InvokeRequired)
                    {
                        BeginInvoke(new Action(() => 
                        {
                            pictureBox1.Image = null;
                            pictureBox1.BackColor = Color.Gray;
                        }));
                    }
                    else
                    {
                        pictureBox1.Image = null;
                        pictureBox1.BackColor = Color.Gray;
                    }
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"Failed to load avatar: {ex.Message}");
                Debug.WriteLine($"Failed to load avatar: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => pictureBox1.BackColor = Color.Gray));
                }
                else
                {
                    pictureBox1.BackColor = Color.Gray;
                }
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

            string time = (timestamp ?? DateTime.Now).ToString("HH:mm:ss");

            if (_preferences.ChatStyle == UserPreferences.MessageStyle.IRC)
            {
                chatBox.AppendText($"[{time}] {author}: {content}\n");
            }
            else
            {
                chatBox.AppendText($"[{time}]\n {author}: {content}\n\n");
            }

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

        private void StartWebSocket(string token)
        {
            try
            {
                SimpleLogger.Log("StartWebSocket called");
                Debug.WriteLine("StartWebSocket called");
                UpdateConnectionStatus("Connecting...");
                _ws = new WebSocket("wss://gateway.discord.gg/?v=9&encoding=json");

                SimpleLogger.Log("WebSocket object created");
                Debug.WriteLine("WebSocket object created");
                _ws.WaitTime = TimeSpan.FromSeconds(10);

                _ws.OnOpen += (sender, e) =>
                {
                    SimpleLogger.Log("WebSocket OnOpen event triggered");
                    Debug.WriteLine("WebSocket OnOpen event triggered");
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
                            },
                            presence = new
                            {
                                status = "online",
                                since = (int?)null,
                                activities = new object[0],
                                afk = false
                            }
                        }
                    };
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(identify);
                    _ws.Send(json);
                    
                };

                _ws.OnMessage += (sender, e) =>
                {
                    try
                    {
                        var obj = JObject.Parse(e.Data);

                        if (obj["s"] != null)
                            _lastSequence = (int?)obj["s"];

                        if (obj["op"] != null && (int)obj["op"] == 10)
                        {
                            int heartbeatInterval = (int)obj["d"]["heartbeat_interval"];
                            _ = StartHeartbeat(heartbeatInterval);
                        }

                        if (obj["t"] != null && obj["t"].ToString() == "MESSAGE_CREATE")
                        {
                            var channelId = obj["d"]["channel_id"]?.ToString();
                            if (!string.IsNullOrEmpty(channelId) && channelId == _currentChannelId)
                            {
                                var author = obj["d"]["author"]?["username"]?.ToString();
                                var content = obj["d"]["content"]?.ToString();

                                if (!string.IsNullOrEmpty(author) && content != null)
                                {
                                    AddMessage(author, content);
                                }
                            }
                        }
                        
                      
                    }
                    catch (Exception ex)
                    {
                        SimpleLogger.Log($"JSON parse error: {ex.Message}");
                        Debug.WriteLine($"JSON parse error: {ex.Message}");
                    }
                };

                _ws.OnError += (sender, e) =>
                {
                    SimpleLogger.Log($"WebSocket error: {e.Message}");
                    Debug.WriteLine($"WebSocket error: {e.Message}");
                    UpdateConnectionStatus($"Error: {e.Message}");
                };

                _ws.OnClose += (sender, e) =>
                {
                    SimpleLogger.Log($"WebSocket closed: {e.Reason}");
                    Debug.WriteLine($"WebSocket closed: {e.Reason}");
                    UpdateConnectionStatus("Disconnected");
                    
                    _heartbeatCts?.Cancel();
                };

                SimpleLogger.Log("Calling _ws.ConnectAsync()...");
                Debug.WriteLine("Calling _ws.ConnectAsync()...");
                _ws.ConnectAsync();
                SimpleLogger.Log("ConnectAsync() returned");
                Debug.WriteLine("ConnectAsync() returned");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"WebSocket connection error: {ex.Message}");
                Debug.WriteLine($"WebSocket connection error: {ex.Message}");
                SimpleLogger.Log($"Stack trace: {ex.StackTrace}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                UpdateConnectionStatus($"Connection failed: {ex.Message}");
            }
        }

        private async Task StartHeartbeat(int interval)
        {
            _heartbeatCts?.Cancel();
            _heartbeatCts?.Dispose();
            _heartbeatCts = new CancellationTokenSource();

            try
            {
                while (_ws != null && _ws.IsAlive && !_heartbeatCts.Token.IsCancellationRequested)
                {
                    var heartbeat = new { op = 1, d = _lastSequence };
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(heartbeat);

                    try
                    {
                        if (_ws.IsAlive)
                        {
                            _ws.Send(json);
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        SimpleLogger.Log($"Heartbeat error: {ex.Message}");
                        Debug.WriteLine($"Heartbeat error: {ex.Message}");
                        UpdateConnectionStatus("Connection lost");
                        break;
                    }

                    await Task.Delay(interval, _heartbeatCts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                SimpleLogger.Log("Heartbeat cancelled");
                Debug.WriteLine("Heartbeat cancelled");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"Heartbeat task error: {ex.Message}");
                Debug.WriteLine($"Heartbeat task error: {ex.Message}");
            }
        }

        private async Task PopulateChannels(string guildId)
        {
            try
            {
                var channels = await _discord.GetChannels(guildId);

                listBoxChannels.Items.Clear();

                foreach (var c in channels.Where(c => c.type == 0))
                {
                    listBoxChannels.Items.Add(new ChannelItem { Id = c.id, Name = c.name });
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"PopulateChannels error: {ex.Message}");
                Debug.WriteLine($"PopulateChannels error: {ex.Message}");
                MessageBox.Show($"Failed to load channels: {ex.Message}");
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
            try
            {
                SimpleLogger.Log("MainForm_Load event started");
                Debug.WriteLine("MainForm_Load event started");

                _ = PopulateGuilds();
                StartWebSocket(_token);
                _ = LoadUserInfo();

                SimpleLogger.Log("MainForm_Load event completed");
                Debug.WriteLine("MainForm_Load event completed");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"MainForm_Load error: {ex.Message}");
                Debug.WriteLine($"MainForm_Load error: {ex.Message}");
                MessageBox.Show($"Error during form load: {ex.Message}");
            }
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
            try
            {
                SimpleLogger.Log("PopulateGuilds started");
                Debug.WriteLine("PopulateGuilds started");
                var guilds = await _discord.GetGuilds();
                SimpleLogger.Log($"Got {guilds.Count} guilds");
                Debug.WriteLine($"Got {guilds.Count} guilds");

                listBoxGuilds.Items.Clear();
                foreach (var g in guilds)
                {
                    listBoxGuilds.Items.Add(new GuildItem
                    {
                        Id = g.id,
                        Name = g.name
                    });
                }
                SimpleLogger.Log("PopulateGuilds completed");
                Debug.WriteLine("PopulateGuilds completed");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"PopulateGuilds error: {ex.Message}");
                Debug.WriteLine($"PopulateGuilds error: {ex.Message}");
                MessageBox.Show($"Failed to load servers: {ex.Message}");
            }
        }
        private void chatBox_TextChanged(object sender, EventArgs e)
        {

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


        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void logOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TokenStorage.DeleteToken();
            Application.Restart();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var about = new AboutForm())
            {
                about.ShowDialog(this);
            }
        }

        private void exportChatToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var preferencesForm = new PreferencesForm())
            {
                if (preferencesForm.ShowDialog(this) == DialogResult.OK)
                {
                    _preferences = UserPreferences.Load();
                    MessageBox.Show("Preferences saved. Changes will apply to new messages.", "Preferences", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void connectionStatusLabel_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
