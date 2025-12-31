using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinCord
{
    class DiscordClient
    {
        private readonly HttpClient _http;

        public DiscordClient(string token)
        {
            _http = new HttpClient();
            _http.DefaultRequestHeaders.Add("Authorization", token);
            _http.DefaultRequestHeaders.Add("User-Agent", "Wincord");
        }

        public async Task SendMessage(string channelId, string content)
        {
            var payload = new { content };
            string json = JsonConvert.SerializeObject(payload);

            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(
                $"https://discord.com/api/v9/channels/{channelId}/messages",
                data
            );

            response.EnsureSuccessStatusCode();
        }
      

        public async Task<List<Channel>> GetChannels(string guildId)
        {
            var response = await _http.GetAsync($"https://discord.com/api/v9/guilds/{guildId}/channels");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Channel>>(json);
        }

        public class Channel
        {
            public string id { get; set; }
            public string name { get; set; }
            public int type { get; set; }
        }
        public async Task<List<Message>> GetMessages(string channelId, int limit = 50)
        {
            var response = await _http.GetAsync($"https://discord.com/api/v9/channels/{channelId}/messages?limit={limit}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Message>>(json);
        }
        public class Message
        {
            public string id { get; set; }
            public Author author { get; set; }
            public string content { get; set; }
            [JsonProperty("timestamp")]
            public string timestampRaw { get; set; }

            [JsonIgnore]
            public DateTime timestamp => DateTime.Parse(timestampRaw);
        }
        public class Author
        {
            public string id { get; set; }
            public string username { get; set; }
        }
    }
}
