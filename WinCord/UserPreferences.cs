using System;
using System.IO;
using Newtonsoft.Json;

namespace WinCord
{
    public class UserPreferences
    {
        public enum MessageStyle
        {
            Spaced,
            IRC
        }

        public MessageStyle ChatStyle { get; set; } = MessageStyle.Spaced;

        private static readonly string PreferencesPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "preferences.json"
        );

        public static UserPreferences Load()
        {
            try
            {
                if (File.Exists(PreferencesPath))
                {
                    string json = File.ReadAllText(PreferencesPath);
                    return JsonConvert.DeserializeObject<UserPreferences>(json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load preferences: {ex.Message}");
            }

            return new UserPreferences();
        }

        public void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(PreferencesPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(PreferencesPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save preferences: {ex.Message}");
            }
        }
    }
}
