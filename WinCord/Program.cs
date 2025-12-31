using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string token = null;
            string channel = null;

            using (var login = new LoginForm())
            {
                if (login.ShowDialog() == DialogResult.OK)
                {
                    token = login.Tag?.ToString();
                    channel = login.Guild?.ToString();
                }
                else
                {
                    return;
                }
            }
            Application.Run(new MainForm(token, channel));

        }
    }
}
