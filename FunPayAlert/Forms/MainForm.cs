using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace FunPayAlert.Forms
{
    public partial class MainForm : Form
    {
        private double defaultOpacity;
        private bool customVisible = true;

        #region ContextMenu

        private void TrayMenuContext()
        {
            notifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            notifyIcon.ContextMenuStrip.Items.Add("Закрыть бота", null, TrayMenu_Exit);
        }

        void TrayMenu_Exit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public void notifyIconMsg(string msg)
        {
            notifyIcon.BalloonTipTitle = "FPAlert";
            notifyIcon.BalloonTipText = msg;
            notifyIcon.ShowBalloonTip(5000);
        }

        #endregion

        public MainForm()
        {
            InitializeComponent();
            TrayMenuContext();
        }

        public void UpdateWorker()
        {
            if (Global.Settings.fp_alert_enabled)
                StartWorker();
            else
                PauseWorker();
        }

        public void CustomHide()
        {
            defaultOpacity = Opacity;
            Opacity = 0;
            ShowInTaskbar = false;
            customVisible = false;
        }

        public void CustomShow()
        {
            notifyIcon.Visible = true;
            Opacity = defaultOpacity;
            ShowInTaskbar = true;
            customVisible = true;
            Show();
            Activate();
        }

        public void StartWorker()
        {
            labelStatus.Text = "Работает";
            labelStatus.ForeColor = Color.Green;
            buttonStartStop.Text = "Приостановить";
            notifyIcon.Text = "FPAlert -- Работает";

            Global.VK.SetUserID(Global.Settings.vk_user_id);
            Global.FP.SetCookies(Global.Settings.cookie_PHPSESSID, Global.Settings.cookie_golden_key);
            Global.Settings.fp_alert_enabled = true;
        }

        public void PauseWorker()
        {
            labelStatus.Text = "Приостановлен";
            labelStatus.ForeColor = Color.Red;
            buttonStartStop.Text = "Запустить";
            notifyIcon.Text = "FPAlert -- Приостановлен";
            Global.Settings.fp_alert_enabled = false;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Global.FixWebBrowser();
            backgroundWorker.RunWorkerAsync();

            if (!Global.Settings.LoadSettings())
            {
                CustomHide();
                Global.GetSetupForm_One().Show();
            }
            else
            {
                notifyIcon.Visible = true;
                UpdateWorker();
            }
        }

        private void buttonStartStop_Click(object sender, EventArgs e)
        {
            Global.Settings.fp_alert_enabled = !Global.Settings.fp_alert_enabled;
            UpdateWorker();
            Global.Settings.SaveSettings();
        }

        private void backgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while(true)
            {
                if(Global.Settings.fp_alert_enabled)
                {
                    Global.FP.Process();
                    Thread.Sleep(Global.Settings.fp_alert_updateTimer);
                }
                else
                    Thread.Sleep(100);
            }
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (customVisible)
                {
                    notifyIconMsg("FPAlert вернут в трей!");
                    CustomHide();
                }
                else
                    CustomShow();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                notifyIconMsg("FPAlert вернут в трей!");
                CustomHide();
                
            }
        }
    }
}
