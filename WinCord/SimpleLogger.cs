using System;
using System.IO;

namespace WinCord
{
    public static class SimpleLogger
    {
        private static readonly bool IsEnabled = false;
        
        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "WinCord_Debug.txt"
        );

        public static void Log(string message)
        {
            if (!IsEnabled) return;
            
            try
            {
                string logMessage = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
                File.AppendAllText(LogPath, logMessage + Environment.NewLine);
            }
            catch
            {
            }
        }

        public static void Clear()
        {
            if (!IsEnabled) return;
            
            try
            {
                if (File.Exists(LogPath))
                {
                    File.Delete(LogPath);
                }
            }
            catch
            {
            }
        }

        public static string GetLogPath()
        {
            return LogPath;
        }
    }
}