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
        public string ChannelId { get; private set; }
        public LoginForm()
        {
            InitializeComponent();
            SendMessage(textBoxToken.Handle, EM_SETCUEBANNER, 0, "Enter your token...");
            SendMessage(channelIdBox.Handle, EM_SETCUEBANNER, 0, "Enter channel ID...");
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            string token = textBoxToken.Text.Trim();
            if (string.IsNullOrEmpty(token) )
            {
                MessageBox.Show("Please enter a token.");
                return;
            }
            string channel = channelIdBox.Text.Trim();
            if (string.IsNullOrEmpty(channel))
            {
                MessageBox.Show("Please enter a channel ID.");
                return;
            }
            this.Tag = token;
            this.ChannelId = channel;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
