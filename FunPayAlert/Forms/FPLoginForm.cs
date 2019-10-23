using System;
using System.Windows.Forms;

namespace FunPayAlert.Forms
{
    public partial class FPLoginForm : Form
    {
        public FPLoginForm()
        {
            InitializeComponent();
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            webBrowser.Refresh();
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            webBrowser.GoBack();
        }

        private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webBrowser.Url.Host == "funpay.ru")
            {
                var page = webBrowser.DocumentText;

                if (Global.isFPLogined(page))
                {
                    Global.Settings.cookie_PHPSESSID = Global.GetCookieValue(webBrowser.Url, "PHPSESSID");
                    Global.Settings.cookie_golden_key = Global.GetCookieValue(webBrowser.Url, "golden_key");
                    Global.Settings.SaveSettings();

                    Global.GetSetupForm_One().Hide();
                    Global.GetSetupForm_Two().Show();
                    Close();
                }
            }
        }

        private void FPLoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
        }
    }
}
