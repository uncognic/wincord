using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Authentication.ExtendedProtection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinCord
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                SimpleLogger.Clear();
                SimpleLogger.Log("========== WinCord Starting ==========");
                SimpleLogger.Log($"OS: {Environment.OSVersion}");
                SimpleLogger.Log($".NET Version: {Environment.Version}");

                SimpleLogger.Log("Setting SecurityProtocol to TLS 1.2...");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                ServicePointManager.DefaultConnectionLimit = 10;
                ServicePointManager.Expect100Continue = false;
                
                SimpleLogger.Log("Setting up SSL certificate validation callback for Windows 7...");
                ServicePointManager.ServerCertificateValidationCallback = 
                    new RemoteCertificateValidationCallback(ValidateServerCertificate);
                SimpleLogger.Log("SSL certificate validation callback set");
                
                SimpleLogger.Log("Enabling visual styles...");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                Application.ThreadException += Application_ThreadException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                SimpleLogger.Log("Exception handlers registered");

                SimpleLogger.Log("Loading token...");
                string token = TokenStorage.LoadToken();

                if (string.IsNullOrEmpty(token))
                {
                    SimpleLogger.Log("No token found, showing login form...");
                    using (var login = new LoginForm())
                    {
                        if (login.ShowDialog() == DialogResult.OK)
                        {
                            token = login.Tag?.ToString();
                            SimpleLogger.Log("Token obtained from login");
                        }
                        else
                        {
                            SimpleLogger.Log("Login cancelled");
                            return;
                        }
                    }
                }
                else
                {
                    SimpleLogger.Log("Token loaded from storage");
                }

                SimpleLogger.Log("Creating MainForm...");
                var mainForm = new MainForm(token);
                SimpleLogger.Log("MainForm created successfully");
                
                SimpleLogger.Log("Starting Application.Run...");
                Application.Run(mainForm);
                
                SimpleLogger.Log("Application closed normally");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"FATAL ERROR: {ex.GetType().Name}: {ex.Message}");
                SimpleLogger.Log($"Stack trace: {ex.StackTrace}");
                
                MessageBox.Show(
                    $"WinCord failed to start:\n\n{ex.Message}\n\nLog saved to Desktop: WinCord_Debug.txt\n\nStack trace:\n{ex.StackTrace}",
                    "Startup Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        private static bool ValidateServerCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            SimpleLogger.Log($"Certificate validation - Errors: {sslPolicyErrors}");
            
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            SimpleLogger.Log($"Certificate Subject: {certificate.Subject}");
            SimpleLogger.Log($"Certificate Issuer: {certificate.Issuer}");

            if (certificate.Subject.Contains("discord.gg") || 
                certificate.Subject.Contains("discord.com") ||
                certificate.Subject.Contains("discordapp.com"))
            {
                SimpleLogger.Log("Accepting Discord certificate despite policy errors (Windows 7 compatibility)");
                return true;
            }

            SimpleLogger.Log("Rejecting certificate with policy errors");
            return false;
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            SimpleLogger.Log($"Thread Exception: {e.Exception.GetType().Name}: {e.Exception.Message}");
            SimpleLogger.Log($"Stack trace: {e.Exception.StackTrace}");
            
            MessageBox.Show(
                $"An error occurred:\n\n{e.Exception.Message}\n\nCheck WinCord_Debug.txt on Desktop\n\nStack trace:\n{e.Exception.StackTrace}",
                "Application Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                SimpleLogger.Log($"Unhandled Exception: {ex.GetType().Name}: {ex.Message}");
                SimpleLogger.Log($"Stack trace: {ex.StackTrace}");
                
                MessageBox.Show(
                    $"Fatal error:\n\n{ex.Message}\n\nCheck WinCord_Debug.txt on Desktop\n\nStack trace:\n{ex.StackTrace}",
                    "Fatal Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
