using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinCord
{
    internal class ThemeManager
    {
        public enum Theme
        {
            Light,
            Dark
        }

        public static class DarkTheme
        {
            public static readonly Color Background = Color.FromArgb(54, 57, 63);
            public static readonly Color SecondaryBackground = Color.FromArgb(47, 49, 54);
            public static readonly Color TertiaryBackground = Color.FromArgb(64, 68, 75);
            public static readonly Color TextPrimary = Color.FromArgb(220, 221, 222);
            public static readonly Color TextSecondary = Color.FromArgb(185, 187, 190);
            public static readonly Color AccentColor = Color.FromArgb(88, 101, 242);
            public static readonly Color BorderColor = Color.FromArgb(32, 34, 37);
        }

        public static class LightTheme
        {
            public static readonly Color Background = SystemColors.Control;
            public static readonly Color SecondaryBackground = Color.White;
            public static readonly Color TertiaryBackground = Color.FromArgb(240, 240, 240);
            public static readonly Color TextPrimary = Color.Black;
            public static readonly Color TextSecondary = Color.FromArgb(79, 84, 92);
            public static readonly Color AccentColor = Color.FromArgb(88, 101, 242);
            public static readonly Color BorderColor = Color.FromArgb(200, 200, 200);
        }

        public static void ApplyTheme(Form form, Theme theme)
        {
            if (theme == Theme.Dark)
            {
                ApplyDarkTheme(form);
            }
            else
            {
                ApplyLightTheme(form);
            }
        }
        private static void ApplyDarkTheme(Control control)
        {
            control.BackColor = DarkTheme.Background;
            control.ForeColor = DarkTheme.TextPrimary;

            if (control is MenuStrip menuStrip)
            {
                menuStrip.BackColor = DarkTheme.SecondaryBackground;
                menuStrip.ForeColor = DarkTheme.TextPrimary;
                menuStrip.Renderer = new DarkMenuRenderer();
            }
            else if (control is TextBox textBox)
            {
                textBox.BackColor = DarkTheme.TertiaryBackground;
                textBox.ForeColor = DarkTheme.TextPrimary;
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (control is RichTextBox richTextBox)
            {
                richTextBox.BackColor = DarkTheme.TertiaryBackground;
                richTextBox.ForeColor = DarkTheme.TextPrimary;
            }
            else if (control is ListBox listBox)
            {
                listBox.BackColor = DarkTheme.SecondaryBackground;
                listBox.ForeColor = DarkTheme.TextPrimary;
                listBox.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (control is Button button)
            {
                button.BackColor = DarkTheme.TertiaryBackground;
                button.ForeColor = DarkTheme.TextPrimary;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = DarkTheme.BorderColor;
            }
            else if (control is Label label)
            {
                label.ForeColor = DarkTheme.TextPrimary;
                if (label.BackColor != Color.Transparent)
                {
                    label.BackColor = DarkTheme.Background;
                }
            }
            else if (control is Panel panel)
            {
                panel.BackColor = DarkTheme.SecondaryBackground;
            }

            foreach (Control child in control.Controls)
            {
                ApplyDarkTheme(child);
            }
        }
        private static void ApplyLightTheme(Control control)
        {
            control.BackColor = LightTheme.Background;
            control.ForeColor = LightTheme.TextPrimary;

            if (control is MenuStrip menuStrip)
            {
                menuStrip.BackColor = LightTheme.SecondaryBackground;
                menuStrip.ForeColor = LightTheme.TextPrimary;
                menuStrip.Renderer = new ToolStripProfessionalRenderer();
            }
            else if (control is TextBox textBox)
            {
                textBox.BackColor = LightTheme.SecondaryBackground;
                textBox.ForeColor = LightTheme.TextPrimary;
            }
            else if (control is RichTextBox richTextBox)
            {
                richTextBox.BackColor = LightTheme.SecondaryBackground;
                richTextBox.ForeColor = LightTheme.TextPrimary;
            }
            else if (control is ListBox listBox)
            {
                listBox.BackColor = LightTheme.SecondaryBackground;
                listBox.ForeColor = LightTheme.TextPrimary;
            }
            else if (control is Button button)
            {
                button.BackColor = LightTheme.Background;
                button.ForeColor = LightTheme.TextPrimary;
                button.FlatStyle = FlatStyle.Standard;
            }

            foreach (Control child in control.Controls)
            {
                ApplyLightTheme(child);
            }
        }
        private class DarkMenuRenderer : ToolStripProfessionalRenderer
        {
            public DarkMenuRenderer() : base(new DarkColorTable()) { }
        }
        private class DarkColorTable : ProfessionalColorTable
        {
            public override Color MenuItemSelected => DarkTheme.TertiaryBackground;
            public override Color MenuItemSelectedGradientBegin => DarkTheme.TertiaryBackground;
            public override Color MenuItemSelectedGradientEnd => DarkTheme.TertiaryBackground;
            public override Color MenuItemBorder => DarkTheme.BorderColor;
            public override Color MenuBorder => DarkTheme.BorderColor;
            public override Color MenuItemPressedGradientBegin => DarkTheme.SecondaryBackground;
            public override Color MenuItemPressedGradientEnd => DarkTheme.SecondaryBackground;
            public override Color ImageMarginGradientBegin => DarkTheme.SecondaryBackground;
            public override Color ImageMarginGradientMiddle => DarkTheme.SecondaryBackground;
            public override Color ImageMarginGradientEnd => DarkTheme.SecondaryBackground;
            public override Color ToolStripDropDownBackground => DarkTheme.SecondaryBackground;
        }
    }
}
