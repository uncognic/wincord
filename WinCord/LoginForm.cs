using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinCord
{
    public partial class LoginForm : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        private const int EM_SETCUEBANNER = 0x1501;
        public string Guild { get; private set; }
        public LoginForm()
        {
            InitializeComponent();
            SendMessage(textBoxToken.Handle, EM_SETCUEBANNER, 0, "Enter your token...");
            SendMessage(guildIdBox.Handle, EM_SETCUEBANNER, 0, "Enter server ID...");
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            string token = textBoxToken.Text.Trim();
            if (string.IsNullOrEmpty(token) )
            {
                MessageBox.Show("Please enter a token.");
                return;
            }
            string guild = guildIdBox.Text.Trim();
            if (string.IsNullOrEmpty(guild))
            {
                MessageBox.Show("Please enter a server ID.");
                return;
            }
            this.Tag = token;
            this.Guild = guild;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
