using System.Windows.Forms;

namespace FunPayAlert.Forms
{
    public partial class SetupForm_Two : Form
    {
        public SetupForm_Two()
        {
            InitializeComponent();
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://regvk.com/id/");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://vk.com/club177568550");
        }

        private void SetupForm_Two_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !Global.AskToQuit("Прервать настройку и выйти?");
        }

        private void textBox1_TextChanged(object sender, System.EventArgs e)
        {
            if(int.TryParse(textBox1.Text, out Global.Settings.vk_user_id))
            {
                buttonTestMsg.Enabled = true;
                buttonContinue.Enabled = true;
                Global.VK.SetUserID(Global.Settings.vk_user_id);
            }
            else
            {
                buttonTestMsg.Enabled = false;
                buttonContinue.Enabled = false;
            }
        }

        private void buttonTestMsg_Click(object sender, System.EventArgs e)
        {
            var code = Global.VK.SendMessage("test! 👌🏻");
            if (code == 0)
                MessageBox.Show("Сообщение отправлено!");
            else if (code == -1)
                MessageBox.Show("Сообщение не отправлено.\n\nРазрешите сообществу отправлять вам сообщения");
            else
                MessageBox.Show("Сообщение не отправлено. HTTP Error: " + code);
        }

        private void buttonContinue_Click(object sender, System.EventArgs e)
        {
            Global.GetMainForm().UpdateWorker();
            Global.GetMainForm().CustomShow();
            Global.Settings.SaveSettings();
            Hide();
        }
    }
}
