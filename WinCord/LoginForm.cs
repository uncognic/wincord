using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinCord
{
    public partial class LoginForm : Form
    {
        public string ChannelId { get; private set; }
        public LoginForm()
        {
            InitializeComponent();
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
