using FacebookGPLX.Data;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.Net.Captcha;
using TqkLibrary.SeleniumSupport;

namespace FacebookGPLX
{
  class ChromeProfile : BaseChromeProfile
  {
    readonly string ProfilePath;
    public ChromeProfile(string ProfileName) : base(Extensions.ChromeDrivePath)
    {
      this.ProfilePath = Extensions.ChromeProfilePath + "\\" + ProfileName;
    }

    ChromeOptions InitChromeOptions(string proxy = null)
    {
      ChromeOptions options = new ChromeOptions();
      //options.AddArguments("--user-agent=" + Ip6UA);
      options.AddArguments("--disable-notifications");
      options.AddArguments("--disable-web-security");
      options.AddArguments("--allow-running-insecure-content");
      options.AddArguments("--user-data-dir=" + ProfilePath);
      if (!string.IsNullOrEmpty(proxy)) options.AddArguments("--proxy-server=" + string.Format("http://{0}", proxy));
      return options;
    }

    void DelayWeb() => Delay(5000, 7000);
    void DelayStep() => Delay(1000, 2000);


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

    public bool RunAdsManager(string uid)
    {
      if(IsOpenChrome)
      {
        chromeDriver.Navigate().GoToUrl("https://www.facebook.com/accountquality/" + uid);
        DelayWeb();

        var eles = chromeDriver.FindElements(By.CssSelector("button[target='_blank']"));
        if (eles.Count == 0) return false;
        eles.First().Click();
        DelayWeb();
        if (!chromeDriver.Url.Contains("www.facebook.com/checkpoint/")) throw new ChromeAutoException("Url not Contains www.facebook.com/checkpoint");




        //eles = chromeDriver.FindElements(By.CssSelector("div[aria-label='Continue']"));
        //if (eles.Count == 0) throw new ChromeAutoException("FindElements By.CssSelector div[aria-label='Continue']");
        //eles.First().Click();
        //DelayWeb();

        eles = chromeDriver.FindElements(By.TagName("iframe"));
        if (eles.Count == 0) throw new ChromeAutoException("FindElements By.TagName iframe");
        chromeDriver.SwitchTo().Frame(eles.First());

        eles = chromeDriver.FindElements(By.ClassName("g-recaptcha"));
        if (eles.Count == 0) throw new ChromeAutoException("FindElements By.ClassName g-recaptcha");
        string data_sitekey = eles.First().GetAttribute("data-sitekey");

        string data = TwoCaptcha.SolveRecaptchaV2("d86e5ffd18ee2fe0c74b848722d7e24e", data_sitekey, chromeDriver.Url).Result;
        if (data.IndexOf("OK|") != 0) throw new Exception();
        data = data.Substring(3);
        TwoCaptchaResponse twoCaptchaResponse = null;
        bool flag = true;
        while (flag)
        {
          TwoCaptchaResponse.Wait(tokenSource.Token);
          twoCaptchaResponse = TwoCaptcha.GetResponseJson(data, "d86e5ffd18ee2fe0c74b848722d7e24e").Result;
          switch(twoCaptchaResponse.CheckState())
          {
            case TwoCaptchaState.NotReady: continue;
            case TwoCaptchaState.Success: flag = false; break;
            case TwoCaptchaState.Error: throw new ChromeAutoException("TwoCaptcha: " + twoCaptchaResponse.request);
          }
        }

        eles = chromeDriver.FindElements(By.Id("g-recaptcha-response"));
        if (eles.Count == 0) throw new ChromeAutoException("FindElements By.Id g-recaptcha-response");
        chromeDriver.ExecuteScript($"document.getElementById('g-recaptcha-response').innerHTML=\"{twoCaptchaResponse.request}\";");
        //submit

        

      }
      return false;
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






    public void OpenChrome()
    {
      OpenChrome(InitChromeOptions());
      chromeDriver.Manage().Window.Size = new System.Drawing.Size(1000, 720);
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
