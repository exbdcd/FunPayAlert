using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

using Leaf.xNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using FunPayAlert.Forms;
using FunPayAlert.Helpers;
using System.Collections.Generic;

namespace FunPayAlert
{
    public static class Global
    {
        public static _Settings Settings = new _Settings();
        public static _VK VK = new _VK();
        public static _FP FP = new _FP();

        #region Forms

        private static FPLoginForm fpLoginForm;
        private static MainForm mainForm = new MainForm();
        private static SetupForm_One setupForm_One = new SetupForm_One();
        private static SetupForm_Two setupForm_Two = new SetupForm_Two();

        public static FPLoginForm GetFPLoginForm()
        {
            if (fpLoginForm == null)
            {
                WinInetHelper.SupressCookiePersist();
                WinInetHelper.EndBrowserSession();
                fpLoginForm = new FPLoginForm();
            }

            return fpLoginForm;
        }

        public static MainForm GetMainForm()
        {
            return mainForm;
        }

        public static SetupForm_One GetSetupForm_One()
        {
            return setupForm_One;
        }

        public static SetupForm_Two GetSetupForm_Two()
        {
            return setupForm_Two;
        }

        #endregion

        #region Funcs

        private static string currentProcessPath = "";
        public static string GetCurrentProcessPath()
        {
            if (currentProcessPath.Length == 0)
                currentProcessPath = Process.GetCurrentProcess().MainModule.FileName.Replace(Process.GetCurrentProcess().MainModule.ModuleName, "");

            return currentProcessPath;
        }

        public static void FixWebBrowser()
        {
            var appName = System.IO.Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION",
                  appName, 11000, Microsoft.Win32.RegistryValueKind.DWord);
        }

        public static void ShowError(string error, bool exitApp = false)
        {
            MessageBox.Show(error, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (exitApp)
                Application.Exit();
        }

        public static bool AskToQuit(string msg)
        {
            DialogResult dialogResult = MessageBox.Show(msg, "Выйти?", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                GetMainForm().Close();
                return true;
            }

            return false;
        }

        public static bool isFPLogined(string page)
        {
            if (page.IndexOf("<a href=\"https://funpay.ru/orders/\">Покупки") != -1 && page.IndexOf("<a href=\"https://funpay.ru/chat/\">Сообщения") != -1)
                return true;

            return false;
        }

        public static string GetCookieValue(Uri uri, string key)
        {
            var rtnValue = "";
            var findKey = key + "=";

            var cookieContainer = WinInetHelper.GetUriCookieContainer(uri);

            foreach (var oneCookie in cookieContainer.GetCookies(uri))
            {
                var cookieText = oneCookie.ToString();
                if (cookieText.Length > findKey.Length && cookieText.Substring(0, findKey.Length) == findKey)
                {
                    rtnValue = cookieText.Substring(findKey.Length);
                    break;
                }
            }
            return rtnValue;
        }

        public static bool ParseFPData(string page, out string ordersTag, out string chatTag, out int userId, out string csrf)
        {
            ordersTag = "";
            chatTag = "";
            csrf = "";
            userId = -1;

            if (page.Length == 0)
                return false;

            // find csrf
            var csrfPattern = "<input type=\"hidden\" name=\"csrf_token\" value=\"";
            var csrfFirstIndex = page.IndexOf(csrfPattern);
            if (csrfFirstIndex == -1)
                return false;

            csrfFirstIndex += csrfPattern.Length;

            var csrfSecondIndex = page.IndexOf('"', csrfFirstIndex);
            if (csrfSecondIndex == -1)
                return false;

            csrf = page.Substring(csrfFirstIndex, csrfSecondIndex - csrfFirstIndex);

            // find div
            var divPattern = "<div id=\"live-counters\"";
            var divIndex = page.IndexOf(divPattern);
            if (divIndex == -1)
                return false;

            // parse orders tag
            var ordersTagPattern = "data-orders=\"";
            var ordersTagFirstIndex = page.IndexOf(ordersTagPattern, divIndex);
            if (ordersTagFirstIndex == -1)
                return false;

            ordersTagFirstIndex += ordersTagPattern.Length;

            var ordersTagSecondIndex = page.IndexOf('"', ordersTagFirstIndex);
            if (ordersTagSecondIndex == -1)
                return false;

            ordersTag = page.Substring(ordersTagFirstIndex, ordersTagSecondIndex - ordersTagFirstIndex);
 
            //  parse chat tag
            var chatTagPattern = "data-chat=\"";
            var chatTagFirstIndex = page.IndexOf(chatTagPattern, divIndex);
            if (chatTagFirstIndex == -1)
                return false;

            chatTagFirstIndex += chatTagPattern.Length;

            var chatTagSecondIndex = page.IndexOf('"', chatTagFirstIndex);
            if (chatTagSecondIndex == -1)
                return false;

            chatTag = page.Substring(chatTagFirstIndex, chatTagSecondIndex - chatTagFirstIndex);

            //  parse fp userId
            var userIdPattern = "data-id=\"";
            var userIdFirstIndex = page.IndexOf(userIdPattern, divIndex);
            if (userIdFirstIndex == -1)
                return false;

            userIdFirstIndex += userIdPattern.Length;

            var userIdSecondIndex = page.IndexOf('"', userIdFirstIndex);
            if (userIdSecondIndex == -1)
                return false;

            var buf = page.Substring(userIdFirstIndex, userIdSecondIndex - userIdFirstIndex);
            return int.TryParse(buf, out userId);
        }

        public static string GetFPJsonStr(int userId, string ordersTag, string chatTag, bool data = true)
        {
            return "[{\"type\":\"orders_counters\",\"id\":\"" + userId + "\",\"tag\":\"" + ordersTag + "\",\"data\":" + data.ToString().ToLower() + "},{\"type\":\"chat_counter\",\"id\":\"" + userId + "\",\"tag\":\"" + chatTag + "\",\"data\":" + data.ToString().ToLower() + "}]";
        }

        public static void ParseFPResponse(string response)
        {
            if (response.Length == 0)
                return;

            try 
            {
                FPObjects fpObjects = JsonConvert.DeserializeObject<FPObjects>(response);
                foreach(var fpObject in fpObjects.objects)
                {
                    if(fpObject.type == "chat_counter")
                    {
                        FP.SetChatCounterTag(fpObject.tag);

                        if (fpObject.data.counter > 0)
                        {
                            var vkMsg = string.Format("[✉] НОВОЕ СООБЩЕНИЕ [✉]\n\nВам пришло новое сообщение на FunPay\nКоличество непрочитанных диалогов: {0}", fpObject.data.counter);
                            VK.SendMessage(vkMsg);
                        }
                    }
                    else if (fpObject.type == "orders_counters")
                    {
                        FP.SetOrdersCounterTag(fpObject.tag);

                        if (fpObject.data.seller > 0)
                        {
                            var vkMsg = string.Format("[💰] НОВЫЙ ЗАКАЗ [💰]\n\nУ вас сделали заказ на FunPay\nКоличество незавершенных заказов: {0}", fpObject.data.seller);
                            VK.SendMessage(vkMsg);
                        }
                    }
                }
            }
            catch
            {
            }
        }
        #endregion
    }

    public class _Settings
    {

        #region Consts

        public const string settingsFileName = "settings.ini";
        public const int defaultUpdateTimer = 5000;

        #endregion

        #region Vars

        // fpAlert
        public bool fp_alert_enabled;
        public int fp_alert_updateTimer;

        // VK
        public int vk_user_id;

        // FP Cookies
        public string cookie_PHPSESSID;
        public string cookie_golden_key;

        // Settings file
        private IniFile settingsFile;

        #endregion

        public _Settings()
        {
            ResetSettings();
        }
        public bool LoadSettings()
        {
            var path = Global.GetCurrentProcessPath() + "//" + settingsFileName;
            settingsFile = new IniFile(path);

            if (!File.Exists(path))
            {
                ResetSettings();
                SaveSettings();
                return false;
            }

            try
            {
                fp_alert_enabled = bool.Parse(settingsFile.IniReadValue("fp_alert", "enabled"));
                fp_alert_updateTimer = int.Parse(settingsFile.IniReadValue("fp_alert", "updateTimer"));

                if (fp_alert_updateTimer < defaultUpdateTimer)
                    fp_alert_updateTimer = defaultUpdateTimer;

                vk_user_id = int.Parse(settingsFile.IniReadValue("vk", "user_id"));

                cookie_PHPSESSID = settingsFile.IniReadValue("cookie", "PHPSESSID");
                cookie_golden_key = settingsFile.IniReadValue("cookie", "golden_key");

                if (vk_user_id == -1 || cookie_PHPSESSID.Length == 0 || cookie_golden_key.Length == 0)
                    return false;
            }
            catch
            {
                Global.ShowError("Ошибка при загрузке файла настроек!", true);
                return false;
            }

            return true;
        }

        public void ResetSettings()
        {
            fp_alert_enabled = false;
            fp_alert_updateTimer = defaultUpdateTimer;

            vk_user_id = -1;

            cookie_PHPSESSID = "";
            cookie_golden_key = "";
        }

        public void SaveSettings()
        {
            try
            {
                settingsFile.IniWriteValue("fp_alert", "enabled", fp_alert_enabled.ToString());
                settingsFile.IniWriteValue("fp_alert", "updateTimer", fp_alert_updateTimer.ToString());

                settingsFile.IniWriteValue("vk", "user_id", vk_user_id.ToString());

                settingsFile.IniWriteValue("cookie", "PHPSESSID", cookie_PHPSESSID);
                settingsFile.IniWriteValue("cookie", "golden_key", cookie_golden_key);
            }
            catch
            {
                Global.ShowError("Ошибка при сохранении файла настроек!", true);
            }
        }
    }

    public class _VK
    {
        #region Const

        public const string send_message_key = "your_code_here";

        #endregion

        #region Vars

        private int user_id;

        #endregion

        public _VK()
        {
            user_id = -1;
        }

        public void SetUserID(int id)
        {
            user_id = id;
        }

        public int SendMessage(string message)
        {
            try
            {
                using (var request = new HttpRequest())
                {
                    var requestParams = new RequestParams();
                    requestParams["message"] = message;
                    requestParams["user_id"] = user_id.ToString();
                    requestParams["access_token"] = send_message_key;
                    requestParams["v"] = "5.92";
                    requestParams["random_id"] = "0";

                    var response = request.Post("https://api.vk.com/method/messages.send", requestParams).ToString();
                    if (response.IndexOf("{\"response\":") == -1)
                        return -1;
                }
            }
            catch (HttpException ex)
            {
                return (int)ex.HttpStatusCode;
            }
            return 0;
        }
    }

    public class _FP
    {
        #region Vars

        private string PHPSESSID;
        private string golden_key;
        private string chat_counter_tag;
        private string orders_counter_tag;
        private string csrf_token;
        private int fp_user_id;

        #endregion

        public _FP()
        {
            PHPSESSID = "";
            golden_key = "";
            chat_counter_tag = "";
            orders_counter_tag = "";
            csrf_token = "";
            fp_user_id = -1;
        }

        public void SetCookies(string sessId, string goldenKey)
        {
            fp_user_id = -1;
            PHPSESSID = sessId;
            golden_key = goldenKey;
        }

        public void SetChatCounterTag(string tag)
        {
            chat_counter_tag = tag;
        }

        public void SetOrdersCounterTag(string tag)
        {
            orders_counter_tag = tag;
        }

        public void Process()
        {
            try
            {
                using (var request = new HttpRequest())
                {
                    request.ConnectTimeout = 10000;
                    request.ReadWriteTimeout = 10000;

                    request.Cookies = new CookieStorage();
                    request.Cookies.Set("PHPSESSID", PHPSESSID, "funpay.ru");
                    request.Cookies.Set("golden_key", golden_key, "funpay.ru");
                    request.UserAgent = Http.ChromeUserAgent();

                    if (fp_user_id == -1)
                    {
                        var response = request.Get("https://funpay.ru/account/balance").ToString();
                        if (!Global.isFPLogined(response))
                        {
                            Global.Settings.ResetSettings();
                            Global.Settings.SaveSettings();
                            Global.VK.SendMessage("[❗] Сессия устарела! Перезапустите бота [❗]");
                            Global.ShowError("Сессия устарела! Перезапустите бота", true);
                        }
                        else
                        {
                            if (Global.ParseFPData(response, out orders_counter_tag, out chat_counter_tag, out fp_user_id, out csrf_token))
                                Process();
                        }
                    }
                    else
                    {
                        var requestParams = new RequestParams();

                        requestParams["csrf_token"] = csrf_token;
                        requestParams["objects"] = Global.GetFPJsonStr(fp_user_id, orders_counter_tag, chat_counter_tag, true);
                        requestParams["request"] = false;

                        request.AddHeader("content-type", "application/x-www-form-urlencoded");
                        request.AddHeader("x-requested-with", "XMLHttpRequest");


                        var response = request.Post("https://funpay.ru/runner/", requestParams).ToString();
                        Global.ParseFPResponse(response);
                    }
                }
            }
            catch (HttpException ex)
            {
            }
        }
    }

    #region FP /runner/ Response Json Classes
    public class FPObjects
    {
        public List<FPObject> objects;
    }
    public class FPObject
    {
        public string type;
        public int id;
        public string tag;
        public FPObjectData data;
    }

    public class FPObjectData
    {
        public int counter;
        public int message;
        public int buyer;
        public int seller;
    }

    #endregion
}
