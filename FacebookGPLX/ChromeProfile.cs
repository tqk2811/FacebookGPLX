using FacebookGPLX.Common;
using FacebookGPLX.Data;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Drawing;
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
      string ua = UserAgent.GetRandom();
      options.AddArguments("--user-agent=" + ua);
      options.AddArguments("--disable-notifications");
      options.AddArguments("--disable-web-security");
      options.AddArguments("--allow-running-insecure-content");
      options.AddArguments("--user-data-dir=" + ProfilePath);
      if (!string.IsNullOrEmpty(extensionPath)) options.AddExtensions(extensionPath);
      if (!string.IsNullOrEmpty(proxy)) options.AddArguments("--proxy-server=" + string.Format("http://{0}", proxy));
      return options;
    }

    void DelayWeb() => Delay(5000, 7000);
    void DelayStep() => Delay(1000, 2000);
    public void WriteLog(string log)
    {
      log = ProfileName + ": " + log;
      LogEvent?.Invoke(log);
      //Console.WriteLine(log);
    }

    public void RunLogin(AccountData accountData)
    {
      if(IsOpenChrome)
      {
        chromeDriver.Navigate().GoToUrl("https://www.facebook.com");
        DelayWeb();

        var eles = chromeDriver.FindElements(By.Id("email"));
        if (eles.Count == 0) throw new ChromeAutoException("FindElements By.Id email");
        eles.First().Click();
        eles.First().SendKeys(accountData.UserName);
        DelayStep();

        eles = chromeDriver.FindElements(By.Id("pass"));
        if (eles.Count == 0) throw new ChromeAutoException("FindElements By.Id pass");
        eles.First().Click();
        eles.First().SendKeys(accountData.PassWord);
        DelayStep();

        eles = chromeDriver.FindElements(By.Id("u_0_b"));
        if (eles.Count == 0) throw new ChromeAutoException("FindElements By.Id u_0_b");
        eles.First().Click();
        DelayWeb();

        TwoFactorAuthNet.TwoFactorAuth twoFactorAuth = new TwoFactorAuthNet.TwoFactorAuth();
        twoFactorAuth.GetCode(accountData.TwoFA);

        eles = chromeDriver.FindElements(By.Id("approvals_code"));
        if (eles.Count == 0) throw new ChromeAutoException("FindElements By.Id approvals_code");
        eles.First().Click();
        eles.First().SendKeys(twoFactorAuth.GetCode(accountData.TwoFA));


        eles = chromeDriver.FindElements(By.Id("checkpointSubmitButton"));
        if (eles.Count == 0) throw new ChromeAutoException("FindElements By.Id checkpointSubmitButton");
        eles.First().Click();

        eles = chromeDriver.FindElements(By.Id("checkpointSubmitButton"));
        if (eles.Count > 0) eles.First().Click();

        DelayWeb();
      }
    }

    public AdsResult RunAdsManager(string uid,string imagePath, Task task,string proxy = null)
    {
      if(IsOpenChrome)
      {
        chromeDriver.Navigate().GoToUrl("https://www.facebook.com/accountquality/" + uid);
        DelayWeb();

        var eles = chromeDriver.FindElements(By.CssSelector("button[target='_blank']"));
        if (eles.Count == 0)
        {
          WriteLog("Không thấy nút kháng");
          return AdsResult.NotFound;
        }
        eles.First().Click();
        DelayWeb();
        if (!chromeDriver.Url.Contains("www.facebook.com/checkpoint/")) throw new ChromeAutoException("Url not Contains www.facebook.com/checkpoint");

        eles = chromeDriver.FindElements(By.CssSelector("div[aria-label='Continue']"));
        if (eles.Count != 0)
        {
          eles.First().Click();
          DelayWeb();
        }

        eles = chromeDriver.FindElements(By.CssSelector("iframe[src='/common/referer_frame.php']"));
        if(eles.Count > 0)
        {
          chromeDriver.SwitchTo().Frame(eles.First());

          eles = chromeDriver.FindElements(By.ClassName("g-recaptcha"));
          if (eles.Count == 0) throw new ChromeAutoException("FindElements By.ClassName g-recaptcha");
          string data_sitekey = eles.First().GetAttribute("data-sitekey");

          WriteLog("Solve Captcha");
          SolveCaptcha(data_sitekey, proxy);
          chromeDriver.SwitchTo().ParentFrame();
        }

        eles = chromeDriver.FindElements(By.CssSelector("div[aria-label='Continue']"));
        if (eles.Count != 0)
        {
          eles.First().Click();
          DelayWeb();
        }

        eles = chromeDriver.FindElements(By.CssSelector("input[name='email']"));
        if (eles.Count > 0) throw new ChromeAutoException("Bị đòi xác nhận email");

        WriteLog("Get Code From Phone Number");
        GetCodeFromPhone();

        eles = chromeDriver.FindElements(By.CssSelector("input[name='email']"));
        if (eles.Count > 0) throw new ChromeAutoException("Bị đòi xác nhận email");
        //if (eles.Count == 0) throw new ChromeAutoException("FindElements By.ClassName div[aria-label='Continue']");

        eles = chromeDriver.FindElements(By.CssSelector("input[type='file']"));
        if (eles.Count == 0) throw new ChromeAutoException("FindElements By.CssSelector input[type='file']");
        task.Wait();
        eles.First().SendKeys(imagePath);
        DelayWeb();

        eles = chromeDriver.FindElements(By.CssSelector("div[aria-label='Continue']"));
        if (eles.Count == 0) throw new ChromeAutoException("FindElements By.CssSelector div[aria-label='Continue']");
        eles.First().Click();
        DelayWeb();
        DelayWeb();
        DelayWeb();
        DelayWeb();
        DelayWeb();
        chromeDriver.Navigate().GoToUrl("https://www.facebook.com/accountquality/" + uid);
        DelayWeb();

        if(chromeDriver.PageSource.Contains(">Account Restricted</span>"))
          return AdsResult.Failed;
        else return AdsResult.Success;
      }
      throw new ChromeAutoException("Chrome is not open");
    }

    void SolveCaptcha(string data_sitekey,string proxy = null)
    {
      bool flag_again = false;
      TwoCaptcha twoCaptcha = new TwoCaptcha(SettingData.Setting.TwoCaptchaKey);
      do
      {
        string data = twoCaptcha.RecaptchaV2(data_sitekey, chromeDriver.Url, proxy , string.IsNullOrEmpty(proxy) ? null : "HTTP").Result;
        if (data.IndexOf("OK|") != 0)
        {
          if (data.Contains("ERROR_NO_SLOT_AVAILABLE"))
          {
            flag_again = true;
            Delay(2000, 3000);
            continue;
          }
          else throw new Exception("2captcha Exception: " + data);
        }
        data = data.Substring(3);
        TwoCaptchaResponse twoCaptchaResponse = null;
        bool flag = false;
        bool flag_while = true;
        while (flag_while)
        {
          TwoCaptchaResponse.Wait(tokenSource.Token);
          twoCaptchaResponse = twoCaptcha.GetResponseJson(data).Result;
          switch (twoCaptchaResponse.CheckState())
          {
            case TwoCaptchaState.NotReady:
              WriteLog("TwoCaptcha: NotReady");
              continue;
            case TwoCaptchaState.Success: 
              flag = false; 
              flag_while = false; 
              break;
            case TwoCaptchaState.Error:
              if (twoCaptchaResponse.request.Contains("ERROR_CAPTCHA_UNSOLVABLE"))
              {
                WriteLog("TwoCaptcha: ERROR_CAPTCHA_UNSOLVABLE");
                Delay(2000, 3000);
                flag = true;
                flag_while = false;
                flag_again = true;
                break;
              }
              else throw new ChromeAutoException("TwoCaptcha: " + twoCaptchaResponse.request);
          }
        }
        if (flag) continue;

        var eles = chromeDriver.FindElements(By.Id("g-recaptcha-response"));
        if (eles.Count == 0) throw new ChromeAutoException("FindElements By.Id g-recaptcha-response");
        chromeDriver.ExecuteScript($"document.getElementById('g-recaptcha-response').innerHTML=\"{twoCaptchaResponse.request}\";___grecaptcha_cfg.clients[0].A.A.callback();");

        flag_again = false;
      }
      while (flag_again);
    }

    void SolveCaptcha_Old()
    {
      var eles = chromeDriver.FindElements(By.CssSelector("iframe[src*='https://www.google.com/recaptcha/api2/anchor']"));
      if (eles.Count == 0) throw new ChromeAutoException("FindElements: By.CssSelector iframe[src*='https://www.google.com/recaptcha/api2/anchor']");
      chromeDriver.SwitchTo().Frame(eles.First());

      eles = chromeDriver.FindElements(By.Id("recaptcha-anchor"));
      if (eles.Count == 0) throw new ChromeAutoException("FindElements: By.Id recaptcha-anchor");
      eles.First().Click();
      DelayStep();

      eles = chromeDriver.FindElements(By.ClassName("rc-imageselect-desc-no-canonical"));
      string q = eles.First().Text;

      eles = chromeDriver.FindElements(By.Id("rc-imageselect-target"));
      Bitmap bitmap = chromeDriver.GetScreenshot().AsByteArray.ToBitMap();



      TwoCaptcha twoCaptcha = new TwoCaptcha(SettingData.Setting.TwoCaptchaKey);
      
      //twoCaptcha.ReCaptchaV2_old()
    }


    public void GetCodeFromPhone()
    {
      var eles = chromeDriver.FindElements(By.Name("phone"));
      if (eles.Count == 0) throw new ChromeAutoException("FindElements By.Name phone");

      //get phone from api
      RentCode rentCode = new RentCode(SettingData.Setting.RentCodeKey);
      RentCodeResult rentCodeResult = rentCode.Request(null, null, RentCode.NetworkProvider.Viettel).Result;

      DelayStep();
      RentCodeCheckOrderResults rentCodeCheckOrderResults = rentCode.Check(rentCodeResult).Result;//get phonenumber
      if (!rentCodeCheckOrderResults.Success) throw new ChromeAutoException("RentCode Phone Number Failed: " + rentCodeCheckOrderResults.ToString());
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
        Delay(2000,2000);
        rentCodeCheckOrderResults = rentCode.Check(rentCodeResult).Result;
        if(rentCodeCheckOrderResults.Success && rentCodeCheckOrderResults.Messages?.Count > 0)
        {
          foreach(var sms in rentCodeCheckOrderResults.Messages)
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
      if (string.IsNullOrEmpty(code)) throw new ChromeAutoException("Get code sms failed");

      
      eles = chromeDriver.FindElements(By.CssSelector("input[autocomplete='one-time-code']"));
      if (eles.Count == 0) throw new ChromeAutoException("FindElements By.CssSelector input[autocomplete='one-time-code']");
      WriteLog("Sms code: " + code);
      eles.First().SendKeys(code);

      eles = chromeDriver.FindElements(By.CssSelector("div[aria-label='Next']"));
      if (eles.Count == 0) throw new ChromeAutoException("FindElements By.CssSelector div[aria-label='Next']");
      eles.First().Click();
      DelayWeb();
    }

    public string GetToken()
    {
      if (IsOpenChrome)
      {
        chromeDriver.Navigate().GoToUrl("https://business.facebook.com/business_locations/?nav_source=mega_menu");
        DelayWeb();
        string html = chromeDriver.PageSource;
        int start_index = html.IndexOf("EAAG");
        if(start_index > 0)
        {
          int end_index = html.IndexOf('"', start_index);
          return html.Substring(start_index, end_index - start_index);
        }
      }
      return null;
    }






    public void OpenChrome(string proxy = null, string extensionPath = null)
    {
      OpenChrome(InitChromeOptions(proxy, extensionPath));
      chromeDriver.Manage().Window.Size = new System.Drawing.Size(1366, 768);
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
  }
}
