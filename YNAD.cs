using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework.Forms;

namespace Tweak_Installer
{
    public partial class YNAD : MetroForm
    {
        public int result = 0;
        public YNAD(string question)
        {
            InitializeComponent();
            Question.Text = question;
        }

        private void YNAD_Load(object sender, EventArgs e)
        {

        }

        private void Select_Click(object sender, EventArgs e)
        {
            result = 1;
            Close();
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            result = 2;
            Close();
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            result = 3;
            Close();
        }
    }
}
