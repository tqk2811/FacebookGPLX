using FacebookGPLX.Common;
using FacebookGPLX.Data;
using KAutoHelper;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tesseract;
using TqkLibrary.Adb;
using TqkLibrary.Media.Images;
using TqkLibrary.Net.Captcha;
using TqkLibrary.Net.RentCodeCo;
using TqkLibrary.SeleniumSupport;

namespace FacebookGPLX
{
  internal class CheckPointException : Exception
  {
  }

  internal enum AdsResult
  {
    NotFound,
    Failed,
    Success
  }

  internal delegate void LogCallback(string log);

  internal class ChromeProfile : BaseChromeProfile
  {
    private static readonly Regex regex_smsCode = new Regex("\\d{6}");
    private readonly string ProfilePath;
    private string UA = null;
    public readonly string ProfileName;

    public event LogCallback LogEvent;

    private RentCodeCheckOrderResults RencodeLastOder = null;
    private readonly BaseAdb adb;

    public ChromeProfile(string ProfileName, string deviceId) : base(Extensions.ChromeDrivePath)
    {
      this.ProfileName = ProfileName;
      this.ProfilePath = Extensions.ChromeProfilePath + "\\" + ProfileName;
      if (!string.IsNullOrEmpty(deviceId)) this.adb = new BaseAdb(deviceId, Extensions.AdbPath);
    }

    private ChromeOptions InitChromeOptions(string proxy = null, string extensionPath = null)
    {
      ChromeOptions options = new ChromeOptions();
      UA = UserAgent.GetRandom();
      options.AddArguments("--user-agent=" + UA);
      options.AddArguments("--disable-notifications");
      options.AddArguments("--disable-web-security");
      options.AddArguments("--disable-blink-features");
      options.AddArguments("--disable-blink-features=AutomationControlled");
      options.AddArguments("--disable-infobars");
      options.AddArguments("--ignore-certificate-errors");
      options.AddArguments("--allow-running-insecure-content");
      options.AddArguments("--user-data-dir=" + ProfilePath);
      options.AddAdditionalCapability("useAutomationExtension", false);
      options.AddExcludedArgument("enable-automation");
      //disable ask password
      options.AddUserProfilePreference("credentials_enable_service", false);
      options.AddUserProfilePreference("profile.password_manager_enabled", false);
      if (!string.IsNullOrEmpty(extensionPath)) options.AddExtensions(extensionPath);
      if (!string.IsNullOrEmpty(proxy)) options.AddArguments("--proxy-server=" + string.Format("http://{0}", proxy));
      return options;
    }

    private void DelayWeb() => Delay(SettingData.Setting.DelayWebMin, SettingData.Setting.DelayWebMax);

    private void DelayStep() => Delay(SettingData.Setting.DelayStepMin, SettingData.Setting.DelayStepMax);

    public void WriteLog(string log)
    {
      log = ProfileName + ": " + log;
      LogEvent?.Invoke(log);
    }

    public void RunLogin(AccountData accountData)
    {
      if (IsOpenChrome)
      {
        WriteLog("Start Run Login");
        chromeDriver.Navigate().GoToUrl("https://www.facebook.com");
        DelayWeb();

        var eles = chromeDriver.FindElements(By.CssSelector("button[data-cookiebanner='accept_button']"));
        if (eles.Count > 0)
        {
          eles.First().Click();
          DelayStep();
        }

        eles = chromeDriver.FindElements(By.Id("email"));
        if (eles.Count == 0) throw new ChromeAutoException("FindElements By.Id email");
        eles.First().Click();
        WriteLog("UserName: " + accountData.UserName);
        eles.First().SendKeys(accountData.UserName);
        DelayStep();

        eles = chromeDriver.FindElements(By.Id("pass"));
        if (eles.Count == 0) throw new ChromeAutoException("FindElements By.Id pass");
        eles.First().Click();
        WriteLog("PassWord: " + accountData.PassWord);
        eles.First().SendKeys(accountData.PassWord);
        DelayStep();

        eles = chromeDriver.FindElements(By.Id("u_0_b"));
        if (eles.Count == 0) throw new ChromeAutoException("FindElements By.Id u_0_b");
        eles.First().Click();
        DelayWeb();

        TwoFactorAuthNet.TwoFactorAuth twoFactorAuth = new TwoFactorAuthNet.TwoFactorAuth();
        eles = chromeDriver.FindElements(By.Id("approvals_code"));
        if (eles.Count > 0)
        {
          eles.First().Click();
          string facode = twoFactorAuth.GetCode(accountData.TwoFA);
          WriteLog("2FA: " + facode);
          eles.First().SendKeys(facode);

          eles = chromeDriver.FindElements(By.CssSelector("button[id='checkpointSubmitButton'][name='submit[Continue]']"));
          if (eles.Count == 0) throw new ChromeAutoException("FindElements CssSelector button[id='checkpointSubmitButton'][name='submit[Continue]']");
          eles.First().Click();
          DelayWeb();
        }

        eles = chromeDriver.FindElements(By.CssSelector("button[id='checkpointSubmitButton'][name='submit[Continue]']"));
        if (eles.Count > 0)
        {
          eles.First().Click();
          DelayWeb();
        }

        eles = chromeDriver.FindElements(By.CssSelector("button[id='checkpointSubmitButton'][name='submit[Continue]']"));
        if (eles.Count > 0)
        {
          eles.First().Click();
          DelayWeb();
        }

        eles = chromeDriver.FindElements(By.CssSelector("button[id='checkpointSubmitButton'][name='submit[Continue]']"));
        if (eles.Count > 0)
        {
          eles.First().Click();
          DelayWeb();
        }

        eles = chromeDriver.FindElements(By.CssSelector("button[id='checkpointSubmitButton'][name='submit[This was me]']"));
        if (eles.Count > 0)
        {
          eles.First().Click();
          DelayWeb();
        }

        eles = chromeDriver.FindElements(By.CssSelector("button[id='checkpointSubmitButton'][name='submit[Continue]']"));
        if (eles.Count > 0)
        {
          eles.First().Click();
          DelayWeb();
        }

        eles = chromeDriver.FindElements(By.CssSelector("button[id='checkpointSubmitButton'][name='submit[Continue]']"));
        if (eles.Count > 0)
        {
          eles.First().Click();
          DelayWeb();
        }

        if (chromeDriver.Url.Contains("www.facebook.com/checkpoint/")) throw new CheckPointException();
        eles = chromeDriver.FindElements(By.CssSelector("button[id='checkpointSubmitButton'][name='submit[Log Out]']"));
        if (eles.Count > 0) throw new ChromeAutoException("Đăng nhập thất bại, chưa xác nhận danh tính button[id='checkpointSubmitButton'][name='submit[Log Out]']");
      }
      else throw new ChromeAutoException("Chrome is not open");
    }

    public void RunAdsManager(string imagePath, Task task, ProxyHelper proxyHelper = null)
    {
      if (IsOpenChrome)
      {
        WriteLog("Check Account Quality");
        chromeDriver.Navigate().GoToUrl("https://www.facebook.com/accountquality/");
        DelayWeb();

        var eles = chromeDriver.FindElements(By.CssSelector("button[target='_blank']"));
        if (eles.Count == 0) chromeDriver.FindElements(By.CssSelector("a[type='button'][target='_blank']"));
        if (eles.Count == 0) throw new ChromeAutoException("button[target='_blank'] and a[type='button'][target='_blank'] not found, Không tìm thấy nút kháng nghị");
        eles.First().Click();
        DelayWeb();
        if (!chromeDriver.Url.Contains("www.facebook.com/checkpoint/")) throw new ChromeAutoException("Url not Contains www.facebook.com/checkpoint");

        while (true)
        {
          CheckSomethingWentWrong();
          CheckEmail();
          ClickContinue();
          ClickCaptcha();
          GetOtpSms();
          if (UploadImageFile(task, imagePath))
          {
            ClickContinue();
            return;
          }
        }
      }
      else throw new ChromeAutoException("Chrome is not open");
    }

    public AdsResult Check()
    {
      chromeDriver.Navigate().GoToUrl("https://www.facebook.com/accountquality/");
      DelayWeb();
      //tam giac cham than : -webkit-mask-position 0px -197px

      if (chromeDriver.FindElements(By.CssSelector("div[class='jwy3ehce'][style*='0px -292px;']")).Count == 0)
        return AdsResult.Failed;
      else return AdsResult.Success;
    }

    public string GetToken()
    {
      if (IsOpenChrome)
      {
        WriteLog("Get Access Token");
        chromeDriver.Navigate().GoToUrl("https://business.facebook.com/business_locations/?nav_source=mega_menu");
        DelayWeb();
        string html = chromeDriver.PageSource;
        int start_index = html.IndexOf("EAAG");
        if (start_index > 0)
        {
          int end_index = html.IndexOf('"', start_index);
          return html.Substring(start_index, end_index - start_index);
        }
        else throw new ChromeAutoException("Token not found");
      }
      else throw new ChromeAutoException("Chrome is not open");
    }

    public void OpenChrome(int index, string proxy = null, string extensionPath = null)
    {
      if (!string.IsNullOrEmpty(extensionPath))
      {
        WriteLog("Open chrome and login proxy with extension: " + proxy);
        OpenChrome(InitChromeOptions(null, extensionPath));
      }
      else if (!string.IsNullOrEmpty(proxy))
      {
        WriteLog("Open chrome with proxy: " + proxy);
        OpenChrome(InitChromeOptions(proxy, null));
      }
      else
      {
        WriteLog("Open chrome");
        OpenChrome(InitChromeOptions(null, null));
      }

      var pos = ChromeLocationHelper.GetPos(index);
      chromeDriver.Manage().Window.Size = pos.Size;// new Size(800, 500);
      chromeDriver.Manage().Window.Position = pos.Location;// new Point(0, 0);
    }

    public void ClearCookies()
    {
      if (IsOpenChrome)
      {
        WriteLog("Clear Cookies");
        chromeDriver.Manage().Cookies.DeleteAllCookies();
        if (chromeDriver.HasWebStorage)
        {
          WriteLog("Clear WebStorage");
          chromeDriver.WebStorage.LocalStorage.Clear();
          chromeDriver.WebStorage.SessionStorage.Clear();
        }
        //"var c = document.cookie.split('; '); for (i in c) document.cookie =/^[^=]+/.exec(c[i])[0] + '=; Path=/; Expires=Thu, 01 Jan 1970 00:00:01 GMT;';"
        chromeDriver.ExecuteScript("localStorage.clear();sessionStorage.clear();");
      }
    }

    public new void CloseChrome() => base.CloseChrome();

    public void Stop() => tokenSource?.Cancel();

    private void ClickContinue()
    {
      var eles = chromeDriver.FindElements(By.CssSelector("div[role='button']:not([type='file'])"));
      if (eles.Count != 0)
      {
        try
        {
          eles.Last().Click();
          DelayWeb();
        }
        catch (Exception) { }
      }
    }

    private void CheckEmail()
    {
      var eles = chromeDriver.FindElements(By.CssSelector("input[name='email']"));
      if (eles.Count > 0) throw new ChromeAutoException("Bị đòi xác nhận email");
    }

    private void CheckSomethingWentWrong()
    {
      //if(chromeDriver.PageSource.Contains("Sorry, something went wrong"))
      //{
      //  chromeDriver.Navigate().GoToUrl(chromeDriver.Url);
      //  DelayWeb();
      //}
    }

    private void ClickCaptcha()
    {
      var eles = chromeDriver.FindElements(By.CssSelector("iframe[src='/common/referer_frame.php']"));
      if (eles.Count > 0)
      {
        ResolveCaptcha_New(eles.First());
        ClickContinue();
      }
    }

    #region SMS

    private void GetOtpSms()
    {
      var eles = chromeDriver.FindElements(By.Name("phone"));
      if (eles.Count > 0)
      {
        WriteLog("Get Code From Phone Number");
        int phoneTry = 0;
        while (phoneTry++ < SettingData.Setting.ReTryCount)
        {
          eles = chromeDriver.FindElements(By.Name("phone"));
          string phoneNumber = GetPhoneNumber();
          if (string.IsNullOrEmpty(phoneNumber))
          {
            WriteLog("Get Phone Number Failed, Try again");
            DelayStep();
            continue;
          }

          WriteLog("Phone Number: " + phoneNumber);
          eles.First().SendKeys(phoneNumber);

          eles = chromeDriver.FindElements(By.CssSelector("div[aria-label='Send Code']"));
          if (eles.Count == 0) throw new ChromeAutoException("FindElements By.CssSelector div[aria-label='Send Code']");
          eles.First().Click();
          DelayWeb();

          string code = string.Empty;
          int reTry = 30;
          while (reTry-- != 0)
          {
            code = GetSms();
            if (!string.IsNullOrEmpty(code)) break;
            else DelayStep();
          }
          if (string.IsNullOrEmpty(code))
          {
            WriteLog("Get code sms timeout, try again");
            eles = chromeDriver.FindElements(By.CssSelector("div[role='button'][aria-label][tabindex='0']"));
            if (eles.Count == 0) throw new ChromeAutoException("FindElements By.CssSelector div[role='button'][aria-label][tabindex='0']");
            eles.First().Click();
            DelayWeb();
            continue;
          }

          eles = chromeDriver.FindElements(By.CssSelector("input[autocomplete='one-time-code']"));
          if (eles.Count == 0) throw new ChromeAutoException("FindElements By.CssSelector input[autocomplete='one-time-code']");
          WriteLog("Sms code: " + code);
          eles.First().SendKeys(code);

          eles = chromeDriver.FindElements(By.CssSelector("div[aria-label='Next']"));
          if (eles.Count == 0) throw new ChromeAutoException("FindElements By.CssSelector div[aria-label='Next']");
          eles.First().Click();
          DelayWeb();
          return;
        }
        throw new ChromeAutoException($"RentCode Lấy sms thất bại với {phoneTry} lần thử");
      }
    }

    private string GetPhoneNumber()
    {
      switch (SettingData.Setting.SmsService)
      {
        case SmsService.Rencode: return GetRencodePhone();
        case SmsService.Saferum: return GetSafeumPhone();
        default: return string.Empty;
      }
    }

    private string GetSms()
    {
      switch (SettingData.Setting.SmsService)
      {
        case SmsService.Rencode: return GetRencodeSms();
        case SmsService.Saferum: return GetSafeumSms();
        default: return string.Empty;
      }
    }

    private string GetRencodePhone()
    {
      RentCode rentCode = new RentCode(SettingData.Setting.RentCodeKey);
      RentCodeResult rentCodeResult = rentCode.Request(1, false, RentCode.NetworkProvider.Viettel).Result;
      if (rentCodeResult.Id == null) return string.Empty;
      DelayWeb();
      DelayWeb();
      RentCodeCheckOrderResults rentCodeCheckOrderResults = rentCode.Check(rentCodeResult).Result;
      if (!rentCodeCheckOrderResults.Success || string.IsNullOrEmpty(rentCodeCheckOrderResults.PhoneNumber)) return string.Empty;
      else
      {
        RencodeLastOder = rentCodeCheckOrderResults;
        return "+84" + rentCodeCheckOrderResults.PhoneNumber.Substring(1);
      }
    }

    private string GetSafeumPhone()
    {
      string userName = Extensions.RandomString(8, 15);
      string passWord = Extensions.RandomString(7);
      adb.DisableApk("com.safeum.android");
      adb.EnableApk("com.safeum.android");
      adb.ClearApk("com.safeum.android");
      adb.OpenApk("com.safeum.android", "im.sum.viewer.login.LoginActivity");
      DelayStep();
      //adb.ScreenShot("D:\\temp\\file.png");
      adb.TapByPercent(0.5, 0.935);
      adb.InputText(userName);
      adb.Key(TqkLibrary.Adb.ADBKeyEvent.KEYCODE_TAB);
      adb.InputText(passWord);
      adb.Key(TqkLibrary.Adb.ADBKeyEvent.KEYCODE_TAB);
      adb.InputText(passWord);
      adb.TapByPercent(0.5, 0.5);
      DelayStep();
      adb.TapByPercent(0.5, 0.24375);//begin to use
      DelayWeb();
      while (true)
      {
        using (Bitmap shoot = adb.ScreenShot())
        {
          Point? point = ImageScanOpenCV.FindOutPoint(shoot, Properties.Resources.setting_icon, 0.75);
          if (point != null)
          {
            adb.TapByPercent(0.74167, 0.107);//setting
            break;
          }
        }
      }
      using Bitmap bitmap = adb.ScreenShot();
      adb.TapByPercent(0.3167, 0.107);
      using Bitmap crop = bitmap.CropImage(new Rectangle() { X = 240, Y = 340, Width = 180, Height = 70 });
      //crop.Save("D:\\temp\\crop.png");
      using TesseractEngine tesseractEngine = new TesseractEngine(Extensions.ExeFolderPath, "eng", EngineMode.Default);
      tesseractEngine.DefaultPageSegMode = PageSegMode.SparseText;
      using Page page = tesseractEngine.Process(crop);
      string text = "+" + page.GetText().Replace(" ", "").Trim();
      return text;
    }

    private string GetRencodeSms()
    {
      RentCodeResult rentCodeResult = null;
      RentCode rentCode = new RentCode(SettingData.Setting.RentCodeKey);
      var rentCodeCheckOrderResults = rentCode.Check(rentCodeResult).Result;
      string code = string.Empty;
      if (rentCodeCheckOrderResults.Success && rentCodeCheckOrderResults.Messages?.Count > 0)
      {
        foreach (var sms in rentCodeCheckOrderResults.Messages)
        {
          var match = regex_smsCode.Match(sms.Message);
          if (match.Success)
          {
            code = match.Value;
            break;
          }
        }
      }
      return code;
    }

    private string GetSafeumSms()
    {
      adb.TapByPercent(0.5, 0.1867);
      DelayStep();
      using Bitmap bitmap = adb.ScreenShot();
      using Bitmap crop = bitmap.CropImage(new Rectangle() { X = 0, Y = 850, Width = 720, Height = 360 });
      using TesseractEngine tesseractEngine = new TesseractEngine(Extensions.ExeFolderPath, "eng", EngineMode.Default);
      tesseractEngine.DefaultPageSegMode = PageSegMode.SparseText;
      using Page page = tesseractEngine.Process(crop);
      string text = page.GetText().Trim();
      ////
      return text;
    }

    #endregion SMS

    private bool UploadImageFile(Task task, string imagePath)
    {
      var eles = chromeDriver.FindElements(By.CssSelector("input[type='file']"));
      if (eles.Count == 0) return false;
      WriteLog("UploadImageFile");
      task.Wait();
      if (task.IsFaulted) throw new ChromeAutoException("Xử lý ảnh bị lỗi: " + task.Exception.GetType().FullName + ": " + task.Exception.Message + task.Exception.StackTrace);
      eles.First().SendKeys(imagePath);
      WriteLog("Upload Image: " + imagePath);
      DelayWeb();
      return true;
    }

    private void ResolveCaptcha_New(IWebElement iframe)
    {
      //string cookies = string.Join(";", chromeDriver.Manage().Cookies.AllCookies.Where(x => x.Domain.Contains(".facebook.com")).Select(x => $"{x.Name}:{x.Value}"));
      chromeDriver.SwitchTo().Frame(iframe);
      var eles = chromeDriver.FindElements(By.CssSelector("div[class*='g-recaptcha']"));
      if (eles.Count == 0) throw new Exception();
      string data_sitekey = eles.First().GetAttribute("data-sitekey");

      TwoCaptcha twoCaptcha = new TwoCaptcha(SettingData.Setting.TwoCaptchaKey);
      int retry = 0;
      while (retry++ < SettingData.Setting.ReTryCount)
      {
        string data = twoCaptcha.RecaptchaV2(data_sitekey, "https://attachment.fbsbx.com/captcha/recaptcha/iframe/").Result;
        if (data.StartsWith("OK|"))
        {
          var result = twoCaptcha.WaitResponseJsonCompleted(data.Substring(3), tokenSource.Token).Result;
          if (result.CheckState() == TwoCaptchaState.Success)
          {
            chromeDriver.ExecuteScript($"document.getElementById('g-recaptcha-response').innerHTML='{result.request}';successCallback('{result.request}');");
            chromeDriver.SwitchTo().ParentFrame();
            return;
          }
          else
          {
            WriteLog("TwoCaptcha Failed, Retry:" + retry);
            continue;
          }
        }
      }
      throw new ChromeAutoException($"TwoCaptcha Thất bại với {retry} lần thử");
    }
  }
}