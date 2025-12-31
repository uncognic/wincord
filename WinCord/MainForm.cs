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

    public partial class MainForm : Form
    {
        private DiscordClient _discord;
        private string _currentChannelId;
        public MainForm(string token, string channel)
        {
            InitializeComponent();
            _discord = new DiscordClient(token);
            _currentChannelId = channel;
        }
        private async Task SendMessage()
        {
            string message = textBox1.Text.Trim();

            if (string.IsNullOrEmpty(message))
                return;

            textBox1.Clear();

            try
            {
                await _discord.SendMessage(_currentChannelId, message);
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
    }
}
