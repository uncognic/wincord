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
    public partial class PreferencesForm : Form
    {
        private UserPreferences _preferences;

        public PreferencesForm()
        {
            InitializeComponent();
        }


        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void PreferencesForm_Load(object sender, EventArgs e)
        {
            _preferences = UserPreferences.Load();

            if (_preferences.ChatStyle == UserPreferences.MessageStyle.Spaced)
            {
                radioButtonSpaced.Checked = true;
            }
            else
            {
                radioButtonIRC.Checked = true;
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (radioButtonSpaced.Checked)
            {
                _preferences.ChatStyle = UserPreferences.MessageStyle.Spaced;
            }
            else
            {
                _preferences.ChatStyle = UserPreferences.MessageStyle.IRC;
            }

            _preferences.Save();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void radioButtonIRC_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
