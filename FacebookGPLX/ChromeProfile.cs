using FacebookGPLX.Common;
using FacebookGPLX.Data;
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
using TqkLibrary.Media.Images;
using TqkLibrary.Net.Captcha;
using TqkLibrary.Net.PhoneNumberApi.RentCodeCo;
using TqkLibrary.Net.PhoneNumberApi.ChoThueSimCodeCom;
using TqkLibrary.Net.PhoneNumberApi.OtpSimCom;
using TqkLibrary.Net.PhoneNumberApi.SimThueCom;
using TqkLibrary.SeleniumSupport;
using TqkLibrary.Net.Captcha.TwoCaptchaCom;
using System.Threading;

namespace FacebookGPLX
{
  internal class CheckPointException : Exception
  {
  }

  internal class AdsException : Exception
  {
    public AdsException()
    {
    }

    public AdsException(string message) : base(message)
    {
    }
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

    public ChromeProfile(string ProfileName) : base(Extensions.ChromeDriverPath)
    {
      this.ProfileName = ProfileName;
      this.ProfilePath = Extensions.ChromeProfilePath + "\\" + ProfileName;
    }

    private ChromeOptions InitChromeOptions(string proxy = null, string extensionPath = null)
    {
      lock(regex_smsCode)
      {
        if (!File.Exists(Extensions.ChromeDriverPath + "\\chromedriver.exe")) Extensions.Setting.Setting.ChromeVer = 0;
        Extensions.Setting.Setting.ChromeVer = ChromeDriverUpdater.Download(Extensions.ChromeDriverPath, Extensions.Setting.Setting.ChromeVer).Result;
        Extensions.Setting.Save();
      }

      ChromeOptions options = new ChromeOptions();
      UA = UserAgent.GetRandom();
      if (string.IsNullOrWhiteSpace(UA)) throw new Exception("UA is null");
      options.AddArguments("--no-sandbox");
      options.AddArguments("--user-agent=" + UA);
      WriteLog("Using UA:" + UA);
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

    private void DelayWeb() => Delay(Extensions.Setting.Setting.DelayWebMin, Extensions.Setting.Setting.DelayWebMax);

    private void DelayStep() => Delay(Extensions.Setting.Setting.DelayStepMin, Extensions.Setting.Setting.DelayStepMax);

    public void WriteLog(string log)
    {
      log = ProfileName + ": " + log;
      LogEvent?.Invoke(log);
    }

    public void RunLogin(AccountData accountData)
    {
      if (IsOpenChrome)
      {
        WriteLog("Start Run Login");//id=ssrb_top_nav_start is logged
        chromeDriver.Navigate().GoToUrl("https://www.facebook.com");
        WaitUntil(By.TagName("body"), ElementsExists, true, 500, 30000);

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

        eles = chromeDriver.FindElements(By.Id("pass"));
        if (eles.Count == 0) throw new ChromeAutoException("FindElements By.Id pass");
        eles.First().Click();
        WriteLog("PassWord: " + accountData.PassWord);
        eles.First().SendKeys(accountData.PassWord);

        eles = chromeDriver.FindElements(By.Name("login"));
        if (eles.Count == 0) throw new ChromeAutoException("FindElements By.Name login");
        eles.First().Click();
        WaitUntil(By.TagName("body"), ElementsExists,true, 500, 30000);

        eles = chromeDriver.FindElements(By.Id("pass"));
        if (eles.Count > 0)
        {
          eles.First().Click();
          WriteLog("PassWord: " + accountData.PassWord);
          eles.First().SendKeys(accountData.PassWord);

          eles = chromeDriver.FindElements(By.Id("loginbutton"));
          if (eles.Count == 0) throw new ChromeAutoException("FindElements By.Id loginbutton");
          eles.First().Click();
          WaitUntil(By.TagName("body"), ElementsExists, true, 500, 30000);
        }

        if (chromeDriver.FindElements(By.Id("pass")).Count > 0) throw new ChromeAutoException("Đăng nhập thất bại, mật khẩu bị đổi hoặc không đúng.");

        eles = chromeDriver.FindElements(By.Id("approvals_code"));
        if (eles.Count > 0)
        {
          TwoFactorAuthNet.TwoFactorAuth twoFactorAuth = new TwoFactorAuthNet.TwoFactorAuth();
          eles.First().Click();
          string facode = twoFactorAuth.GetCode(accountData.TwoFA);
          WriteLog("2FA: " + facode);
          eles.First().SendKeys(facode);

          eles = chromeDriver.FindElements(By.CssSelector("button[id='checkpointSubmitButton']"));
          if (eles.Count == 0) throw new ChromeAutoException("FindElements CssSelector button[id='checkpointSubmitButton']");
          eles.First().Click();
          WaitUntil(By.TagName("body"), ElementsExists, true, 500, 30000);
        }

        eles = chromeDriver.FindElements(By.CssSelector("button[id='checkpointSubmitButton']"));
        if (eles.Count > 0)
        {
          eles.First().Click();
          WaitUntil(By.TagName("body"), ElementsExists, true, 500, 30000);
        }

        eles = chromeDriver.FindElements(By.CssSelector("button[id='checkpointSubmitButton']"));
        if (eles.Count > 0)
        {
          eles.First().Click();
          WaitUntil(By.TagName("body"), ElementsExists, true, 500, 30000);
        }

        eles = chromeDriver.FindElements(By.CssSelector("button[id='checkpointSubmitButton']"));
        if (eles.Count > 0)
        {
          eles.First().Click();
          WaitUntil(By.TagName("body"), ElementsExists, true, 500, 30000);
        }

        eles = chromeDriver.FindElements(By.CssSelector("button[id='checkpointSubmitButton']"));
        if (eles.Count > 0)
        {
          eles.First().Click();
          WaitUntil(By.TagName("body"), ElementsExists, true, 500, 30000);
        }

        eles = chromeDriver.FindElements(By.CssSelector("button[id='checkpointSubmitButton']"));
        if (eles.Count > 0)
        {
          eles.First().Click();
          WaitUntil(By.TagName("body"), ElementsExists, true, 500, 30000);
        }

        eles = chromeDriver.FindElements(By.CssSelector("button[id='checkpointSubmitButton']"));
        if (eles.Count > 0)
        {
          eles.First().Click();
          WaitUntil(By.TagName("body"), ElementsExists, true, 500, 30000);
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
        WaitUntil(By.TagName("body"), ElementsExists, true, 500, 30000);
        DelayStep();
        var eles = chromeDriver.FindElements(By.CssSelector("button[target='_blank']"));
        if (eles.Count == 0) eles = chromeDriver.FindElements(By.CssSelector("a[type='button'][target='_blank']"));
        if (eles.Count == 0)
          throw new AdsException("Không tìm thấy nút kháng nghị");
        eles.First().Click();
        WaitUntil(By.TagName("body"), ElementsExists, true, 500, 30000);
        var token = this.Token;
        DelayWeb();
        if (!chromeDriver.Url.Contains("www.facebook.com/checkpoint/"))
          throw new ChromeAutoException("Url not Contains www.facebook.com/checkpoint");

        task.Wait();

        for (int i = 0; i < 2; i++)
        {
          int TryTimes = 0;
          while (TryTimes++ < 6)
          {
            //CheckSomethingWentWrong();
            CheckEmail();
            ClickContinue();
            ClickCaptcha();
            GetOtpSms();
            if (UploadImageFile(task, imagePath))
            {
              ClickContinue();
              return;
            }
            WriteLog($"Try Times {TryTimes}");
          }
          WriteLog($"SomethingWentWrong");
          chromeDriver.Navigate().GoToUrl(chromeDriver.Url);
          DelayWeb();
        }
        throw new ChromeAutoException("Auto failed");
      }
      else throw new ChromeAutoException("Chrome is not open");
    }

    public void RunAdsManager2(string imagePath, string password, Task task, ProxyHelper proxyHelper = null)
    {
      if (IsOpenChrome)
      {
        WriteLog("Check Account Quality");
        chromeDriver.Navigate().GoToUrl("https://www.facebook.com/accountquality/");
        WaitUntil(By.TagName("body"), ElementsExists, true, 500, 30000);
        DelayStep();
        var eles = chromeDriver.FindElements(By.CssSelector("button[target='_blank']"));
        if (eles.Count == 0) eles = chromeDriver.FindElements(By.CssSelector("a[type='button'][target='_blank']"));
        if (eles.Count == 0)
          throw new AdsException("Không tìm thấy nút kháng nghị");
        eles.First().Click();
        WaitUntil(By.TagName("body"), ElementsExists, true, 500, 30000);
        DelayWeb();
        if (!chromeDriver.Url.Contains("www.facebook.com/checkpoint/"))
          throw new ChromeAutoException("Url not Contains www.facebook.com/checkpoint");
        task.Wait();

        bool smsSuccess = false;
        try
        {
          for (int i = 0; i < 2; i++)
          {
            int TryTimes = 0;
            while (TryTimes++ < 6)
            {
              CheckEmail();
              ClickContinue();
              ClickCaptcha();
              if (GetOtpSms())
              {
                smsSuccess = true;
                break;
              }

              WriteLog($"Try Times {TryTimes}");
            }
            if (smsSuccess) break;
            else
            {
              WriteLog($"SomethingWentWrong");
              chromeDriver.Navigate().GoToUrl(chromeDriver.Url);
              DelayWeb();
            }
          }
          if (!smsSuccess) throw new ChromeAutoException("Auto failed");


          chromeDriver.Navigate().GoToUrl("https://www.facebook.com/accountquality/");
          WaitUntil(By.TagName("body"), ElementsExists, true, 500, 30000);
          DelayStep();
          eles = chromeDriver.FindElements(By.CssSelector("button[target='_blank']"));
          if (eles.Count == 0) eles = chromeDriver.FindElements(By.CssSelector("a[type='button'][target='_blank']"));
          if (eles.Count == 0)
            throw new AdsException("Không tìm thấy nút kháng nghị 2");
          eles.First().Click();
          WaitUntil(By.TagName("body"), ElementsExists, true, 500, 30000);
          DelayWeb();
          if (!chromeDriver.Url.Contains("www.facebook.com/checkpoint/"))
            throw new ChromeAutoException("Url not Contains www.facebook.com/checkpoint 2");


          for (int i = 0; i < 2; i++)
          {
            int TryTimes = 0;
            while (TryTimes++ < 6)
            {
              CheckEmail();
              ClickContinue();
              ClickCaptcha();
              if (UploadImageFile(task, imagePath))
              {
                ClickContinue();
                return;
              }
              WriteLog($"Try Times {TryTimes}");
            }
          }
        }
        finally
        {
          if (smsSuccess) 
            Func(password);
        }
      }
      else throw new ChromeAutoException("Chrome is not open");
    }

    void Func(string password)
    {
      chromeDriver.Navigate().GoToUrl("https://www.facebook.com/settings?tab=mobile");
      DelayWeb();
      //var body = WaitUntil(By.TagName("body"), ElementsExists, true, 500, 30000).First();
      //var popup = chromeDriver.FindElements(By.CssSelector("div[role='dialog'] div[role='button'][class*='synb87wq']")).FirstOrDefault();
      //if (popup != null)
      //{
      //  popup.Click();
      //  DelayStep();
      //}

      //var popup2 = chromeDriver.FindElements(By.CssSelector("div[role='dialog'] div[role='button'][class*='synb87wq']"));
      //if (popup2.Count > 1)
      //{
      //  popup2.Last().Click();
      //  DelayStep();
      //}
      //body.SendKeys(Keys.Escape);

      using (FrameSwitch frameSwitch = FrameSwitch(WaitUntil(By.TagName("iframe[src*='https://www.facebook.com/settings?']"), ElementsExists, true, 500, 30000).First()))
      {
        var SettingsPage_Content = WaitUntil(By.TagName("div[id='SettingsPage_Content']"), ElementsExists, true, 500, 30000).First();

        var a_e = WaitUntil(SettingsPage_Content, By.TagName("span a[role='button']"), ElementsExists, true, 500, 30000).First();
        JsClick(a_e);
        DelayWeb();

        var dialog = WaitUntil(By.TagName("div[role='dialog']:not([class*='uiToggleFlyout'])"), ElementsExists, true, 500, 30000).First();
        var checkbox = WaitUntil(dialog, By.TagName("input[type='checkbox']"), ElementsExists, true, 500, 30000).First();
        JsClick(checkbox);
        var submit = WaitUntil(dialog, By.TagName("button[type*='submit']"), ElementsExists, true, 500, 30000).First();
        JsClick(submit);
        DelayStep();


        var pass = chromeDriver.FindElements(By.CssSelector("input[type='password']"));
        if (pass.Count > 0)
        {
          pass.First().SendKeys(password);
          submit = WaitUntil(dialog, By.TagName("button[type='submit']"), ElementsExists, true, 500, 30000).First();
          JsClick(submit);
        }

        DelayWeb();
      }
    }


    public AdsResult Check()
    {
      chromeDriver.Navigate().GoToUrl("https://www.facebook.com/accountquality/");
      WaitUntil(By.TagName("body"), ElementsExists);
      DelayWeb();
      //tam giac cham than : -webkit-mask-position 0px -197px

      if (chromeDriver.FindElements(By.CssSelector("div[class*='jwy3ehce'][style*='0px -292px;']")).Count == 0)
        return AdsResult.Failed;
      else return AdsResult.Success;
    }

    public string GetToken()
    {
      if (IsOpenChrome)
      {
        WriteLog("Get Access Token");
        chromeDriver.Navigate().GoToUrl("https://business.facebook.com/business_locations/?nav_source=mega_menu");
        WaitUntil(By.TagName("body"), ElementsExists);
        string html = chromeDriver.PageSource;
        int start_index = html.IndexOf("EAAG");
        if (start_index > 0)
        {
          int end_index = html.IndexOf('"', start_index);
          return html.Substring(start_index, end_index - start_index);
        }
        else throw new ChromeAutoException("AccessToken not found");
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

    public new void Stop() => tokenSource?.Cancel();

    private void ClickContinue()
    {
      var eles = chromeDriver.FindElements(By.CssSelector("div[data-pagelet='root'] div[role='button']:not([type='file'])"));
      if (eles.Count != 0)
      {
        var ele = eles.Last();
        string ele_class = ele.GetAttribute("class");
        if (!ele_class.Contains("rj84mg9z"))//disable mouse rj84mg9z
        {
          try
          {
            WriteLog("Click Continue");
            ele.Click();
            DelayWeb();
          }
          catch (Exception) { }
        }
        //else WriteLog($"Continue: Enabled {ele.Enabled}, Displayed {ele.Displayed}");
      }
    }

    private void CheckEmail()
    {
      var eles = chromeDriver.FindElements(By.CssSelector("input[name='email']"));
      if (eles.Count > 0) throw new ChromeAutoException("Bị đòi xác nhận email");
    }

    private void ClickCaptcha()
    {
      var eles = chromeDriver.FindElements(By.CssSelector("div[data-pagelet='root'] iframe[src='/common/referer_frame.php']"));
      if (eles.Count > 0)
      {
        WriteLog("Resolve Captcha");
        ResolveCaptcha(eles.First());
        ClickContinue();
      }
    }

    #region SMS

    private bool GetOtpSms()
    {
      var eles = chromeDriver.FindElements(By.Name("phone"));
      if (eles.Count > 0)
      {
        WriteLog("Get Code From Phone Number");
        int phoneTry = 0;
        while (phoneTry++ < Extensions.Setting.Setting.ReTryCount)
        {
          eles = WaitUntil(By.Name("phone"), ElementsExists);
          string phoneNumber = GetPhoneNumber();
          if (string.IsNullOrEmpty(phoneNumber))
          {
            WriteLog("Get Phone Number Failed, Try again");
            DelayStep();
            continue;
          }

          WriteLog("Phone Number: " + phoneNumber);

          try { eles.First().Clear(); } catch (Exception) { }

          eles.First().SendKeys(phoneNumber);

          ClickContinue();

          string code = string.Empty;
          using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(130000))
          {
            while(string.IsNullOrEmpty(code) && !cancellationTokenSource.IsCancellationRequested)
            {
              try
              {
                code = GetSms();
              }
              catch (Exception ex2)
              {
                WriteLog("Get code sms Exception: " + ex2.Message);
                break;
              }
              if (!string.IsNullOrEmpty(code)) break;
              else DelayStep();
            }
          }
          if (string.IsNullOrEmpty(code))
          {
            WriteLog("Get code sms timeout/Exception, try again");
            WaitUntil(By.CssSelector("div[role='button'][aria-label][tabindex='0']"), ElementsExists).First().Click();
            DelayWeb();
            continue;
          }

          WaitUntil(By.CssSelector("input[autocomplete='one-time-code']"), ElementsExists).First().SendKeys(code);
          WriteLog("Sms code: " + code);

          WaitUntil(By.CssSelector("div[role='button'][class*='s1i5eluu']"), ElementsExists).First().Click();
          DelayWeb();
          return true;
        }
        throw new ChromeAutoException($"RentCode Lấy sms thất bại với {phoneTry} lần thử");
      }
      return false;
    }

    private string GetPhoneNumber()
    {
      switch (Extensions.Setting.Setting.SmsService)
      {
        case SmsService.Rencode: return GetRencodePhone();
        case SmsService.ChoThueSim: return GetChoThueSimCodePhone();
        case SmsService.OtpSim: return GetOtpSimPhone();
        case SmsService.SimThue: return GetSimThueComPhone();
        default: return string.Empty;
      }
    }

    private string GetSms()
    {
      switch (Extensions.Setting.Setting.SmsService)
      {
        case SmsService.Rencode: return GetRencodeSms();
        case SmsService.ChoThueSim: return GetChoThueSimCodeSms();
        case SmsService.OtpSim: return GetOtpSimSms();
        case SmsService.SimThue: return GetSimThueComSms();
        default: return string.Empty;
      }
    }

    #region Rencode

    private RentCodeResult rentCodeResult = null;

    private string GetRencodePhone()
    {
      RentCodeApi rentCode = new RentCodeApi(Extensions.Setting.Setting.RentCodeKey);
      RentCodeResult rentCodeResult = rentCode.Request(1, false, NetworkProvider.Viettel).Result;
      if (rentCodeResult.Id == null || rentCodeResult.Success != true)
      {
        WriteLog($"RentCodeResult Message: {rentCodeResult?.Message}");
        return string.Empty;
      }
      DelayWeb();
      DelayWeb();
      RentCodeCheckOrderResults rentCodeCheckOrderResults = rentCode.Check(rentCodeResult).Result;
      if (!rentCodeCheckOrderResults.Success || string.IsNullOrEmpty(rentCodeCheckOrderResults.PhoneNumber)) return string.Empty;
      else
      {
        this.rentCodeResult = rentCodeResult;
        return "+84" + rentCodeCheckOrderResults.PhoneNumber.Substring(1);
      }
    }

    private string GetRencodeSms()
    {
      RentCodeApi rentCode = new RentCodeApi(Extensions.Setting.Setting.RentCodeKey);
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

    #endregion Rencode

    #region OtpSim

    private BaseResult<PhoneRequestResult> OtpSimBaseResult = null;

    private string GetOtpSimPhone()
    {
      OtpSimApi otpSimApi = new OtpSimApi(Extensions.Setting.Setting.OtpSimKey);
      var phone = otpSimApi.PhonesRequest(new DataService() { Id = 7 }).Result;
      if (phone.Success && phone.StatusCode == StatusCode.Success)
      {
        OtpSimBaseResult = phone;
        return "+84" + phone.Data.PhoneNumber;
      }
      else
      {
        WriteLog("GetOtpSimPhone: " + phone.StatusCode + ", " + phone.Message);
        return string.Empty;
      }
    }

    private string GetOtpSimSms()
    {
      OtpSimApi otpSimApi = new OtpSimApi(Extensions.Setting.Setting.OtpSimKey);
      var message = otpSimApi.GetPhoneMessage(OtpSimBaseResult.Data).Result;
      if (message.Success && message.StatusCode == StatusCode.Success)
      {
        return message.Data.Messages?.FirstOrDefault().Otp;
      }
      else
      {
        WriteLog("GetOtpSimSms: " + message.StatusCode + ", " + message.Message);
        return string.Empty;
      }
    }

    #endregion OtpSim

    #region ChoThueSimCode

    private BaseResult<ResponseCodeGetPhoneNumber, PhoneNumberResult> ChoThueSimCodePhone = null;

    private string GetChoThueSimCodePhone()
    {
      ChoThueSimCodeApi choThueSimCodeApi = new ChoThueSimCodeApi(Extensions.Setting.Setting.ChoThueSimKey);
      var phone = choThueSimCodeApi.GetPhoneNumber(1001).Result;
      if (phone.ResponseCode == ResponseCodeGetPhoneNumber.Success)
      {
        ChoThueSimCodePhone = phone;
        return phone.Result.Number;
      }
      else
      {
        WriteLog("GetChoThueSimCodePhone: " + phone.ResponseCode + ", " + phone.Msg);
        return string.Empty;
      }
    }

    private string GetChoThueSimCodeSms()
    {
      ChoThueSimCodeApi choThueSimCodeApi = new ChoThueSimCodeApi(Extensions.Setting.Setting.ChoThueSimKey);
      var message = choThueSimCodeApi.GetMessage(ChoThueSimCodePhone.Result).Result;
      if (message.ResponseCode == ResponseCodeMessage.Success)
      {
        return message.Result.Code;
      }
      else
      {
        WriteLog("GetChoThueSimCodeSms: " + message.ResponseCode + ", " + message.Msg);
        return string.Empty;
      }
    }

    #endregion ChoThueSimCode

    #region SimThueCom

    private RequestResult SimThueComPhone = null;

    private string GetSimThueComPhone()
    {
      SimThueApi simThueApi = new SimThueApi(Extensions.Setting.Setting.SimThueKey);
      var request = simThueApi.CreateRequest(new ServiceResult() { Id = 9 }).Result;
      if (request.Success)
      {
        DelayWeb();
        var phone = simThueApi.CheckRequest(request).Result;
        if (phone.Success)
        {
          SimThueComPhone = request;
          return "+84" + phone.Number.Value.ToString();
        }
        else WriteLog("GetSimThueComPhone: " + phone.Message);
      }
      else WriteLog("GetSimThueComPhone: " + request.Message);
      return string.Empty;
    }

    private string GetSimThueComSms()
    {
      SimThueApi simThueApi = new SimThueApi(Extensions.Setting.Setting.SimThueKey);
      var phone = simThueApi.CheckRequest(SimThueComPhone).Result;
      if (phone.Success)
      {
        var split = phone?.Sms?.FirstOrDefault().Split('|');
        if (split != null && split.Length == 3)
        {
          Match match = regex_smsCode.Match(split.Last());
          if (match.Success) return match.Value;
        }
      }
      else WriteLog("GetSimThueComSms: " + phone.Message);
      return string.Empty;
    }

    #endregion SimThueCom

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
      DelayWeb();
      return true;
    }

    private void ResolveCaptcha(IWebElement iframe)
    {
      using (FrameSwitch frameSwitch = FrameSwitch(iframe))
      {
        var ele = WaitUntil(By.CssSelector("div[class*='g-recaptcha']"), ElementsExists).First();
        string data_sitekey = ele.GetAttribute("data-sitekey");

        TwoCaptchaApi twoCaptcha = new TwoCaptchaApi(Extensions.Setting.Setting.TwoCaptchaKey);
        int retry = 0;
        while (retry++ < Extensions.Setting.Setting.ReTryCount)
        {
          var res = twoCaptcha.RecaptchaV2(data_sitekey, "https://attachment.fbsbx.com/captcha/recaptcha/iframe/").Result;
          if (res.CheckState() == TwoCaptchaState.Success)
          {
            WriteLog("twoCaptcha.RecaptchaV2: " + res.request);
            var result = twoCaptcha.WaitResponseJsonCompleted(res.request, tokenSource.Token).Result;
            if (result.CheckState() == TwoCaptchaState.Success)
            {
              WriteLog("twoCaptcha.Resolve: Success");
              chromeDriver.ExecuteScript($"document.getElementById('g-recaptcha-response').innerHTML='{result.request}';successCallback('{result.request}');");
              return;
            }
            else
            {
              WriteLog($"TwoCaptcha Failed: {result.request}, Retry:" + retry);
              continue;
            }
          }
        }
        throw new ChromeAutoException($"TwoCaptcha Thất bại với {retry} lần thử");
      }
    }
  }
}