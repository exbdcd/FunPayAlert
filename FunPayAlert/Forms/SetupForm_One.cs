using System;
using System.Windows.Forms;

namespace FunPayAlert.Forms
{
    public partial class SetupForm_One : Form
    {
        public SetupForm_One()
        {
            InitializeComponent();
        }

        private void buttonAuthFP_Click(object sender, EventArgs e)
        {
            var form = Global.GetFPLoginForm();
            if(form == null)
            {
                Global.ShowError("Не удалось открыть FPLoginForm!");
                return;
            }

            form.ShowDialog();
        }

        private void StartForm_One_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !Global.AskToQuit("Прервать настройку и выйти?");
        }
    }
}
