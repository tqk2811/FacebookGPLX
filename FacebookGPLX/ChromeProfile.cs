using FacebookGPLX.Common;
using FacebookGPLX.Data;
using KAutoHelper;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using PInvoke;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TqkLibrary.Media.Images;
using TqkLibrary.Net.Captcha;
using TqkLibrary.Net.RentCodeCo;
using TqkLibrary.SeleniumSupport;

namespace FacebookGPLX
{
  enum AdsResult
  {
    NotFound,
    Failed,
    Success
  }

  delegate void LogCallback(string log);
  class ChromeProfile : BaseChromeProfile
  {
    static readonly Regex regex_smsCode = new Regex("\\d{6}");
    readonly string ProfilePath;
    string UA = null;
    public readonly string ProfileName;
    public event LogCallback LogEvent;

    public ChromeProfile(string ProfileName) : base(Extensions.ChromeDrivePath)
    {
      this.ProfileName = ProfileName;
      this.ProfilePath = Extensions.ChromeProfilePath + "\\" + ProfileName;
    }

    ChromeOptions InitChromeOptions(string proxy = null,string extensionPath = null)
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
      if (!string.IsNullOrEmpty(extensionPath)) options.AddExtensions(extensionPath);
      if (!string.IsNullOrEmpty(proxy)) options.AddArguments("--proxy-server=" + string.Format("http://{0}", proxy));
      return options;
    }

    void DelayWeb() => Delay(SettingData.Setting.DelayWebMin, SettingData.Setting.DelayWebMax);
    void DelayStep() => Delay(SettingData.Setting.DelayStepMin, SettingData.Setting.DelayStepMax);

    public void WriteLog(string log)
    {
      log = ProfileName + ": " + log;
      LogEvent?.Invoke(log);
    }

    public void RunLogin(AccountData accountData)
    {
      if(IsOpenChrome)
      {
        WriteLog("Start Run Login");
        chromeDriver.Navigate().GoToUrl("https://www.facebook.com");
        DelayWeb();

        var eles = chromeDriver.FindElements(By.Id("email"));
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
        string facode = twoFactorAuth.GetCode(accountData.TwoFA);

        eles = chromeDriver.FindElements(By.Id("approvals_code"));
        if (eles.Count > 0)
        {
          eles.First().Click();
          WriteLog("2FA: " + facode);
          eles.First().SendKeys(facode);

          eles = chromeDriver.FindElements(By.CssSelector("button[id='checkpointSubmitButton'][name='submit[Continue]']"));
          if (eles.Count == 0) throw new ChromeAutoException("");
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

        if (chromeDriver.Url.Contains("www.facebook.com/checkpoint/")) throw new ChromeAutoException("Login failed");
        eles = chromeDriver.FindElements(By.CssSelector("button[id='checkpointSubmitButton'][name='submit[Log Out]']"));
        if (eles.Count > 0) throw new ChromeAutoException("Đăng nhập thất bại, chưa xác nhận danh tính button[id='checkpointSubmitButton'][name='submit[Log Out]']");
      }
      else throw new ChromeAutoException("Chrome is not open");
    }

    public void RunAdsManager(string imagePath, Task task, ProxyHelper proxyHelper = null)
    {
      if(IsOpenChrome)
      {
        WriteLog("Check Account Quality");
        chromeDriver.Navigate().GoToUrl("https://www.facebook.com/accountquality/");
        DelayWeb();

        var eles = chromeDriver.FindElements(By.CssSelector("button[target='_blank']"));
        if (eles.Count == 0) throw new ChromeAutoException("button[target='_blank'] not found, Không tìm thấy nút kháng nghị");
        eles.First().Click();
        DelayWeb();
        if (!chromeDriver.Url.Contains("www.facebook.com/checkpoint/")) throw new ChromeAutoException("Url not Contains www.facebook.com/checkpoint");

        while(true)
        {
          CheckSomethingWentWrong();
          CheckEmail();
          ClickContinue();
          ClickCaptcha();
          GetOtpSms();
          if(UploadImageFile(task,imagePath))
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

      if (chromeDriver.PageSource.Contains(">Account Restricted</span>"))
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
        if(start_index > 0)
        {
          int end_index = html.IndexOf('"', start_index);
          return html.Substring(start_index, end_index - start_index);
        }
        else throw new ChromeAutoException("Token not found");
      }
      else throw new ChromeAutoException("Chrome is not open");
    }

    public void OpenChrome(string proxy = null, string extensionPath = null)
    {
      OpenChrome(InitChromeOptions(proxy, extensionPath));
      chromeDriver.Manage().Window.Size = new Size(1366, 900);
#if DEBUG
      chromeDriver.Manage().Window.Position = new Point(0, 0);
#endif
    }

    public bool ResetProfileData()
    {
      if(!IsOpenChrome)
      {
        if (Directory.Exists(ProfilePath)) Directory.Delete(ProfilePath, true);
        return true;
      }
      else return false;
    }

    public new void CloseChrome() => base.CloseChrome();
    public void Stop() => tokenSource?.Cancel();





    void ClickContinue()
    {
      var eles = chromeDriver.FindElements(By.CssSelector("div[aria-label='Continue']"));
      if (eles.Count != 0)
      {
        try
        {
          eles.First().Click();
          DelayWeb();
        }
        catch (Exception) { }
      }
    }
    void CheckEmail()
    {
      var eles = chromeDriver.FindElements(By.CssSelector("input[name='email']"));
      if (eles.Count > 0) throw new ChromeAutoException("Bị đòi xác nhận email");
    }
    void CheckSomethingWentWrong()
    {
      //if(chromeDriver.PageSource.Contains("Sorry, something went wrong"))
      //{
      //  chromeDriver.Navigate().GoToUrl(chromeDriver.Url);
      //  DelayWeb();
      //}
    }

    void ClickCaptcha()
    {
      var eles = chromeDriver.FindElements(By.CssSelector("iframe[src='/common/referer_frame.php']"));
      if (eles.Count > 0)
      {
#if DEBUG
        ResolveCaptcha_Old(eles.First());
#else
        WaitSolveCaptcha();
#endif
      }
    }

    void GetOtpSms()
    {
      var eles = chromeDriver.FindElements(By.Name("phone"));
      if (eles.Count >= 0)
      {
        WriteLog("Get Code From Phone Number");
        int phoneTry = 3;
        //get phone from api
        RentCode rentCode = new RentCode(SettingData.Setting.RentCodeKey);
        while (true)
        {
          RentCodeResult rentCodeResult = rentCode.Request(1, false, RentCode.NetworkProvider.Viettel).Result;          
          DelayWeb();
          RentCodeCheckOrderResults rentCodeCheckOrderResults = rentCode.Check(rentCodeResult).Result;//get phonenumber
          if (!rentCodeCheckOrderResults.Success)
          {
            if (phoneTry-- == 0) throw new ChromeAutoException("RentCode Phone Number Failed: " + rentCodeCheckOrderResults.ToString());
            else
            {
              chromeDriver.Navigate().GoToUrl(chromeDriver.Url);
              DelayWeb();
              continue;
            }
          }

          string phone = "+84" + rentCodeCheckOrderResults.PhoneNumber.Substring(1);
          WriteLog("Phone Number: " + phone);
          eles.First().SendKeys(phone);

          eles = chromeDriver.FindElements(By.CssSelector("div[aria-label='Send Code']"));
          if (eles.Count == 0) throw new ChromeAutoException("FindElements By.CssSelector div[aria-label='Send Code']");
          eles.First().Click();
          DelayWeb();

          //get code from api
          string code = string.Empty;
          int reTry = 30;
          while (reTry-- != 0)
          {
            Delay(2000, 2000);
            rentCodeCheckOrderResults = rentCode.Check(rentCodeResult).Result;
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
            if (!string.IsNullOrEmpty(code)) break;
          }
          if (string.IsNullOrEmpty(code))
          {
            if(phoneTry-- == 0) throw new ChromeAutoException("Get code sms failed");
            else
            {
              chromeDriver.Navigate().GoToUrl(chromeDriver.Url);
              DelayWeb();
              continue;
            }
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
      }
    }

    bool UploadImageFile(Task task,string imagePath)
    {
      var eles = chromeDriver.FindElements(By.CssSelector("input[type='file']"));
      if (eles.Count == 0) return false;
      task.Wait();
      if (task.IsFaulted) throw new ChromeAutoException("Xử lý ảnh bị lỗi: " + task.Exception.GetType().FullName + ": " + task.Exception.Message + task.Exception.StackTrace);
      eles.First().SendKeys(imagePath);
      DelayWeb();
      return true;
    }




    void WaitSolveCaptcha()
    {
      while(true)
      {
        var eles = chromeDriver.FindElements(By.CssSelector("iframe[src='/common/referer_frame.php']"));
        if (eles.Count > 0) DelayStep();
        else
        {
          DelayWeb();
          break;
        }
      }
    }

    static readonly object _lock = new object();
    void ResolveCaptcha_Old_1(IWebElement iframe)
    {
      var guid = Guid.NewGuid();
      chromeDriver.ExecuteScript($"document.title ='{guid.ToString()}'");
      Delay(1000, 1000);
      var process = System.Diagnostics.Process.GetProcessesByName("chrome").FirstOrDefault(p => p.MainWindowTitle.Contains(guid.ToString()));
      if (process == null) throw new ChromeAutoException("Can't find process chrome");
      IntPtr handle = process.MainWindowHandle;

      User32.ShowWindow(handle, User32.WindowShowStyle.SW_SHOW);
      AutoControl.MouseClick(AutoControl.GetGlobalPoint(handle, 430, 298));
      DelayStep();

      using MemoryStream memoryStream = new MemoryStream(chromeDriver.GetScreenshot().AsByteArray);
      Bitmap bitmap = (Bitmap)Bitmap.FromStream(memoryStream);
      bitmap.Save("D:\\chrome.png");
      using Bitmap q = bitmap.CropImage(new Rectangle() { X = 454, Y = 187, Width = 286, Height = 115 });
      q.Save("D:\\q.png");
      using Bitmap i = bitmap.CropImage(new Rectangle() { X = 454, Y = 320, Width = 292, Height = 292 });
      i.Save("D:\\i.png");
      TwoCaptcha twoCaptcha = new TwoCaptcha(SettingData.Setting.TwoCaptchaKey);
      var data = twoCaptcha.ReCaptchaV2_old(i, q, 4, 4).Result;
      if (data.StartsWith("OK|")) data = data.Substring(3);
      else throw new Exception("");

      do
      {
        var result = twoCaptcha.GetResponseJson(data).Result;
        //result.
      }
      while (true);



      //var image = KAutoHelper.CaptureHelper.CaptureWindow(handle);
      //image.Save("D:\\test.png",ImageFormat.Png);

    }

    void ResolveCaptcha_Old(IWebElement iframe)
    {
      chromeDriver.SwitchTo().Frame(iframe);
      var Iframe_anchor = chromeDriver.FindElement(By.CssSelector("iframe[src*='https://www.google.com/recaptcha/api2/anchor']"));
      var Iframe_bframe = chromeDriver.FindElement(By.CssSelector("iframe[src*='https://www.google.com/recaptcha/api2/bframe']"));
      chromeDriver.SwitchTo().Frame(Iframe_anchor);
      chromeDriver.FindElement(By.ClassName("recaptcha-checkbox")).Click();
      DelayStep();
      chromeDriver.SwitchTo().ParentFrame();

      chromeDriver.SwitchTo().Frame(Iframe_bframe);
      while(chromeDriver.FindElements(By.CssSelector("span[class*='recaptcha-checkbox'][aria-checked='false']")).Count > 0) SolveStep();
      chromeDriver.SwitchTo().ParentFrame();

      chromeDriver.SwitchTo().ParentFrame();
    }


    void SolveStep()
    {
      using MemoryStream memoryStream = new MemoryStream(chromeDriver.GetScreenshot().AsByteArray);
      Bitmap bitmap = (Bitmap)Bitmap.FromStream(memoryStream);
      bitmap.Save("D:\\chrome.png");
      var instructions = chromeDriver.FindElement(By.ClassName("rc-imageselect-instructions"));
      var challenge = chromeDriver.FindElement(By.ClassName("rc-imageselect-challenge"));
      var tds = chromeDriver.FindElements(By.TagName("tr"));
      using Bitmap i = bitmap.CropImage(new Rectangle()
      {
        X = instructions.Location.X + 447,
        Y = instructions.Location.Y + 196,
        Width = instructions.Size.Width,
        Height = instructions.Size.Height
      });
      using Bitmap c = bitmap.CropImage(new Rectangle()
      {
        X = challenge.Location.X + 447,
        Y = challenge.Location.Y + 196,
        Width = challenge.Size.Width,
        Height = challenge.Size.Height
      });
      i.Save("D:\\i.png");
      c.Save("D:\\c.png");

      TwoCaptcha twoCaptcha = new TwoCaptcha(SettingData.Setting.TwoCaptchaKey);
      string result = TwoCaptchaSolve(twoCaptcha, c, i, tds.Count);

      //click




      chromeDriver.FindElement(By.Id("recaptcha-verify-button")).Click();
    }


    string TwoCaptchaSolve(TwoCaptcha twoCaptcha,Bitmap c,Bitmap i,int size)
    {
      var data = twoCaptcha.ReCaptchaV2_old(c, i, size, size).Result;
      if (data.StartsWith("OK|")) data = data.Substring(3);
      else throw new Exception(data);

      while (true)
      {
        TwoCaptchaResponse.Wait(tokenSource.Token);
        var result = twoCaptcha.GetResponseJson(data).Result;
        switch(result.CheckState())
        {
          case TwoCaptchaState.Success: 
            return result.request;
          case TwoCaptchaState.NotReady: 
            continue;
          case TwoCaptchaState.Error: 
            throw new Exception(result.request);
        }
      }
    }
  }
}
