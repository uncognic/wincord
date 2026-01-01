using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinCord
{
    class DiscordClient : IDisposable
    {
        private readonly HttpClient _http;

        public DiscordClient(string token)
        {
            try
            {
                _http = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(30)
                };
                _http.DefaultRequestHeaders.Add("Authorization", token);
                _http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HttpClient initialization failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public void Dispose()
        {
            _http?.Dispose();
        }

        public async Task SendMessage(string channelId, string content)
        {
            var payload = new { content };
            string json = JsonConvert.SerializeObject(payload);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _http.PostAsync(
                    $"https://discord.com/api/v9/channels/{channelId}/messages",
                    data
                );

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Invalid token. Please log in again.");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new UnauthorizedAccessException("You don't have permission to send messages in this channel.");
                }
                else if (response.StatusCode == (System.Net.HttpStatusCode)429)
                {
                    throw new InvalidOperationException("Rate limited. Please wait before sending more messages.");
                }

                response.EnsureSuccessStatusCode();
            }
            catch (TaskCanceledException)
            {
                throw new TimeoutException("Request timed out. Check your internet connection.");
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Network error: {ex.Message}", ex);
            }
        }
      

        public async Task<List<Channel>> GetChannels(string guildId)
        {
            try
            {
                var response = await _http.GetAsync($"https://discord.com/api/v9/guilds/{guildId}/channels");
                
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Invalid token. Please log in again.");
                }
                
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Channel>>(json);
            }
            catch (TaskCanceledException)
            {
                throw new TimeoutException("Request timed out.");
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Network error: {ex.Message}", ex);
            }
        }

        public class Channel
        {
            public string id { get; set; }
            public string name { get; set; }
            public int type { get; set; }

        }
        public async Task<List<Message>> GetMessages(string channelId, int limit = 50)
        {
            try
            {
                var response = await _http.GetAsync($"https://discord.com/api/v9/channels/{channelId}/messages?limit={limit}");
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Invalid token. Please log in again.");
                }
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Message>>(json);
            }
            catch (TaskCanceledException)
            {
                throw new TimeoutException("Request timed out.");
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Network error: {ex.Message}", ex);
            }
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
        public class Guild
        {
            public string id { get; set; }
            public string name { get; set; }
        }
        public async Task<List<Guild>> GetGuilds()
        {
            var response = await _http.GetAsync("https://discord.com/api/v9/users/@me/guilds");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Guild>>(json);
        }
        public class User
        {
            public string id { get; set; }
            public string username { get; set; }
            public string discriminator { get; set; }
            public string avatar { get; set; }
            public string global_name { get; set; }
        }
        public async Task<System.Drawing.Image> GetUserAvatar(string userId, string avatarHash)
        {
            try
            {
                string avatarUrl;
                
                if (string.IsNullOrEmpty(avatarHash))
                {
                    Random random = new Random();
                    int avatarIndex = random.Next(0, 6);
                    avatarUrl = $"https://cdn.discordapp.com/embed/avatars/{avatarIndex}.png";
                }
                else
                {
                    string extension = avatarHash.StartsWith("a_") ? "gif" : "png";
                    avatarUrl = $"https://cdn.discordapp.com/avatars/{userId}/{avatarHash}.{extension}?size=128";
                }

                var response = await _http.GetAsync(avatarUrl);
                response.EnsureSuccessStatusCode();

                var imageBytes = await response.Content.ReadAsByteArrayAsync();
               
                using (var ms = new System.IO.MemoryStream(imageBytes))
                {
                    var tempImage = System.Drawing.Image.FromStream(ms);
                    return new System.Drawing.Bitmap(tempImage);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load avatar: {ex.Message}");
                return null;
            }
        }

        public async Task<User> GetCurrentUser()
        {
            var response = await _http.GetAsync("https://discord.com/api/v9/users/@me");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<User>(json);
        }
    }
}
