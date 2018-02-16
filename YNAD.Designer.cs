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
            this.Question = new MetroFramework.Controls.MetroTextBox();
            this.Select = new MetroFramework.Controls.MetroButton();
            this.metroButton1 = new MetroFramework.Controls.MetroButton();
            this.metroButton2 = new MetroFramework.Controls.MetroButton();
            this.SuspendLayout();
            // 
            // Question
            // 
            // 
            // 
            // 
            this.Question.CustomButton.Image = null;
            this.Question.CustomButton.Location = new System.Drawing.Point(596, 2);
            this.Question.CustomButton.Name = "";
            this.Question.CustomButton.Size = new System.Drawing.Size(105, 105);
            this.Question.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.Question.CustomButton.TabIndex = 1;
            this.Question.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.Question.CustomButton.UseSelectable = true;
            this.Question.CustomButton.Visible = false;
            this.Question.Lines = new string[0];
            this.Question.Location = new System.Drawing.Point(23, 63);
            this.Question.MaxLength = 32767;
            this.Question.Multiline = true;
            this.Question.Name = "Question";
            this.Question.PasswordChar = '\0';
            this.Question.ReadOnly = true;
            this.Question.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.Question.SelectedText = "";
            this.Question.SelectionLength = 0;
            this.Question.SelectionStart = 0;
            this.Question.ShortcutsEnabled = true;
            this.Question.Size = new System.Drawing.Size(704, 110);
            this.Question.Style = MetroFramework.MetroColorStyle.Purple;
            this.Question.TabIndex = 11;
            this.Question.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.Question.UseSelectable = true;
            this.Question.UseStyleColors = true;
            this.Question.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.Question.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
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
            this.Controls.Add(this.Question);
            this.Name = "YNAD";
            this.Style = MetroFramework.MetroColorStyle.Purple;
            this.Text = "YNAD";
            this.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.Load += new System.EventHandler(this.YNAD_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private MetroFramework.Controls.MetroTextBox Question;
        private MetroFramework.Controls.MetroButton Select;
        private MetroFramework.Controls.MetroButton metroButton1;
        private MetroFramework.Controls.MetroButton metroButton2;
    }
}