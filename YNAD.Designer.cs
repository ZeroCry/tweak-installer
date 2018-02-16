namespace Tweak_Installer
{
    partial class YNAD
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
            this.TweakList = new MetroFramework.Controls.MetroTextBox();
            this.Select = new MetroFramework.Controls.MetroButton();
            this.metroButton1 = new MetroFramework.Controls.MetroButton();
            this.metroButton2 = new MetroFramework.Controls.MetroButton();
            this.SuspendLayout();
            // 
            // TweakList
            // 
            // 
            // 
            // 
            this.TweakList.CustomButton.Image = null;
            this.TweakList.CustomButton.Location = new System.Drawing.Point(596, 2);
            this.TweakList.CustomButton.Name = "";
            this.TweakList.CustomButton.Size = new System.Drawing.Size(105, 105);
            this.TweakList.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.TweakList.CustomButton.TabIndex = 1;
            this.TweakList.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.TweakList.CustomButton.UseSelectable = true;
            this.TweakList.CustomButton.Visible = false;
            this.TweakList.Lines = new string[0];
            this.TweakList.Location = new System.Drawing.Point(23, 63);
            this.TweakList.MaxLength = 32767;
            this.TweakList.Multiline = true;
            this.TweakList.Name = "TweakList";
            this.TweakList.PasswordChar = '\0';
            this.TweakList.ReadOnly = true;
            this.TweakList.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.TweakList.SelectedText = "";
            this.TweakList.SelectionLength = 0;
            this.TweakList.SelectionStart = 0;
            this.TweakList.ShortcutsEnabled = true;
            this.TweakList.Size = new System.Drawing.Size(704, 110);
            this.TweakList.Style = MetroFramework.MetroColorStyle.Purple;
            this.TweakList.TabIndex = 11;
            this.TweakList.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.TweakList.UseSelectable = true;
            this.TweakList.UseStyleColors = true;
            this.TweakList.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.TweakList.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // Select
            // 
            this.Select.Location = new System.Drawing.Point(23, 179);
            this.Select.Name = "Select";
            this.Select.Size = new System.Drawing.Size(194, 23);
            this.Select.Style = MetroFramework.MetroColorStyle.Purple;
            this.Select.TabIndex = 12;
            this.Select.Text = "Yes";
            this.Select.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.Select.UseSelectable = true;
            this.Select.UseStyleColors = true;
            this.Select.Click += new System.EventHandler(this.Select_Click);
            // 
            // metroButton1
            // 
            this.metroButton1.Location = new System.Drawing.Point(223, 179);
            this.metroButton1.Name = "metroButton1";
            this.metroButton1.Size = new System.Drawing.Size(194, 23);
            this.metroButton1.Style = MetroFramework.MetroColorStyle.Purple;
            this.metroButton1.TabIndex = 13;
            this.metroButton1.Text = "No";
            this.metroButton1.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.metroButton1.UseSelectable = true;
            this.metroButton1.UseStyleColors = true;
            this.metroButton1.Click += new System.EventHandler(this.metroButton1_Click);
            // 
            // metroButton2
            // 
            this.metroButton2.Location = new System.Drawing.Point(533, 179);
            this.metroButton2.Name = "metroButton2";
            this.metroButton2.Size = new System.Drawing.Size(194, 23);
            this.metroButton2.Style = MetroFramework.MetroColorStyle.Purple;
            this.metroButton2.TabIndex = 14;
            this.metroButton2.Text = "Yes to All";
            this.metroButton2.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.metroButton2.UseSelectable = true;
            this.metroButton2.UseStyleColors = true;
            this.metroButton2.Click += new System.EventHandler(this.metroButton2_Click);
            // 
            // YNAD
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(750, 250);
            this.Controls.Add(this.metroButton2);
            this.Controls.Add(this.metroButton1);
            this.Controls.Add(this.Select);
            this.Controls.Add(this.TweakList);
            this.Name = "YNAD";
            this.Style = MetroFramework.MetroColorStyle.Purple;
            this.Text = "YNAD";
            this.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.Load += new System.EventHandler(this.YNAD_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private MetroFramework.Controls.MetroTextBox TweakList;
        private MetroFramework.Controls.MetroButton Select;
        private MetroFramework.Controls.MetroButton metroButton1;
        private MetroFramework.Controls.MetroButton metroButton2;
    }
}