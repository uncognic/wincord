using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        private string guildId; 
        private ClientWebSocket _ws;
        public MainForm(string token, string guild)
        {
            InitializeComponent();
            _discord = new DiscordClient(token);
            guildId = guild;
            StartWebSocket(token);
        }

        private void AddMessage(string author, string content)
        {
            chatBox.AppendText($"{author}: {content}\n");
        }
        private async Task SendMessage()
        {
            string message = textBox1.Text.Trim();

            if (string.IsNullOrEmpty(message))
                return;

            textBox1.Clear();

            try
            {
                await _discord.SendMessage(guildId, message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Send failed");
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                _ = SendMessage();
            }
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            _ = SendMessage();
        }
        private int? _lastSequence = null;
        private async void StartWebSocket(string token)
        {
            _ws = new ClientWebSocket();
            await _ws.ConnectAsync(new Uri("wss://gateway.discord.gg/?v=9&encoding=json"), CancellationToken.None);

            var identify = new
            {
                op = 2,
                d = new
                {
                    token = token,
                    properties = new { 
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
                var buffer = new byte[8192];
                int heartbeatInterval = 0;

                while (_ws.State == WebSocketState.Open)
                {
                    var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
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
                                if (channelId == guildId)
                                {
                                    var author = obj["d"]["author"]["username"].ToString();
                                    var content = obj["d"]["content"].ToString();
                                    AddMessage(author, content);
                                }
                            }
                        }
                        catch { }
                    }
                }
            });
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
                catch {}

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
        
        private void MainForm_Load(object sender, EventArgs e)
        {
            _ = PopulateChannels(guildId);
        }

        private void listBoxChannels_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (listBoxChannels.SelectedItem is ChannelItem selected)
            {
                guildId = selected.Id;
                chatBox.Clear();
            }
        }
    }
}
