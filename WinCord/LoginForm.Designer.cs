namespace WinCord
{
    partial class LoginForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBoxToken = new System.Windows.Forms.TextBox();
            this.loginButton = new System.Windows.Forms.Button();
            this.guildIdBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBoxToken
            // 
            this.textBoxToken.Location = new System.Drawing.Point(339, 146);
            this.textBoxToken.Name = "textBoxToken";
            this.textBoxToken.Size = new System.Drawing.Size(100, 20);
            this.textBoxToken.TabIndex = 0;
            this.textBoxToken.UseSystemPasswordChar = true;
            // 
            // loginButton
            // 
            this.loginButton.Location = new System.Drawing.Point(352, 201);
            this.loginButton.Name = "loginButton";
            this.loginButton.Size = new System.Drawing.Size(75, 23);
            this.loginButton.TabIndex = 1;
            this.loginButton.Text = "Login";
            this.loginButton.UseVisualStyleBackColor = true;
            this.loginButton.Click += new System.EventHandler(this.loginButton_Click);
            // 
            // guildIdBox
            // 
            this.guildIdBox.Location = new System.Drawing.Point(339, 175);
            this.guildIdBox.Name = "guildIdBox";
            this.guildIdBox.Size = new System.Drawing.Size(100, 20);
            this.guildIdBox.TabIndex = 2;
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.guildIdBox);
            this.Controls.Add(this.loginButton);
            this.Controls.Add(this.textBoxToken);
            this.Name = "LoginForm";
            this.Text = "Login";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxToken;
        private System.Windows.Forms.Button loginButton;
        private System.Windows.Forms.TextBox guildIdBox;
    }
}