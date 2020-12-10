using FacebookGPLX.Common;
using FacebookGPLX.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TqkLibrary.Net.Facebook;
using TqkLibrary.Queues.TaskQueues;
using TqkLibrary.SeleniumSupport;

namespace FacebookGPLX
{
  class ItemQueue : IQueue
  {
    public static bool RunFlag { get; set; }
    public static readonly Queue<AccountData> AccountsQueue = new Queue<AccountData>();
    public static readonly Queue<string> ProxysQueue = new Queue<string>();
    public static StreamWriter ResultSuccess { get; set; }
    public static StreamWriter ResultFailed { get; set; }
    public static StreamWriter ResultError { get; set; }
    readonly ChromeProfile chromeProfile;
    public ItemQueue(string ProfileName, LogCallback logCallback)
    {
      this.chromeProfile = new ChromeProfile(ProfileName);
      this.chromeProfile.LogEvent += logCallback;
    }


    void Work()
    {
      while(true)
      {
        AccountData accountData = null;
        try
        {
          accountData = AccountsQueue.Dequeue();
          if (accountData == null) break;
#if DEBUG
          chromeProfile.OpenChrome();
          chromeProfile.RunAdsManager("", Task.FromResult(0), null);
          return;
#endif
          chromeProfile.WriteLog("Reset Profile");
          chromeProfile.ResetProfileData();

          ProxyHelper proxyHelper = null;
          if (ProxysQueue.Count > 0)
          {
            proxyHelper = new ProxyHelper(ProxysQueue.Dequeue());
            if (proxyHelper.IsLogin)//extension
            {
              chromeProfile.WriteLog("Open chrome and login proxy by extension: " + proxyHelper.Proxy);
              string ext_path = Extensions.ChromeProfilePath + "\\" + chromeProfile.ProfileName + ".zip";
              ProxyLoginExtension.GenerateExtension(ext_path, proxyHelper.Host, proxyHelper.Port, proxyHelper.UserName, proxyHelper.PassWord);
              chromeProfile.OpenChrome(null, ext_path);
            }
            else
            {
              chromeProfile.WriteLog("Open chrome with proxy: " + proxyHelper.Proxy);
              chromeProfile.OpenChrome(proxyHelper.Proxy, null);
            }
          }
          else
          {
            chromeProfile.WriteLog("Open chrome");
            chromeProfile.OpenChrome();
          }

          chromeProfile.RunLogin(accountData);
          string access_token = chromeProfile.GetToken();
          chromeProfile.WriteLog("Access Token: " + access_token);


          string imagePath = Extensions.ChromeProfilePath + "\\" + chromeProfile.ProfileName + ".png";
          string birthday = null;
          string name = null;
          string id = null;
          Task task = Task.Factory.StartNew(() =>
          {
            chromeProfile.WriteLog("Download & Edit Avatar");

            FacebookApi facebookApi = new FacebookApi();
            string userinfo = facebookApi.UserInfo(access_token, "birthday,name,id").Result;
            dynamic json = JsonConvert.DeserializeObject(userinfo);
            id = json.id;
            birthday = json.birthday;
            name = json.name;

            using (Bitmap image = facebookApi.PictureBitMap(access_token).Result)
            {
              using (Bitmap fake = image.DrawGPLX(name, birthday))
              {
                fake.Save(imagePath, ImageFormat.Png);
              }
            }
            chromeProfile.WriteLog("Download & Edit Avatar Completed");
          }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

          chromeProfile.RunAdsManager(imagePath, task, proxyHelper);
          File.Copy(imagePath, Extensions.ImageSuccess + $"\\{id}.png", true);
          ResultSuccess?.WriteLine(accountData);
          ResultSuccess?.Flush();
        }
        catch(OperationCanceledException)
        {
          return;
        }
        catch(ChromeAutoException cae)
        {
          chromeProfile.SaveHtml(Extensions.DebugData + $"\\{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}_{accountData?.UserName}.html");
          ResultError?.WriteLine(accountData + "| -> " + cae.Message);
          ResultError?.Flush();
          chromeProfile.WriteLog("ChromeAutoException:" + cae.Message);
        }
        catch(Exception ex)
        {
          chromeProfile.SaveHtml(Extensions.DebugData + $"\\{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}_{accountData?.UserName}.html");
          if (ex is AggregateException ae) ex = ae.InnerException;
          ResultError?.WriteLine(accountData + "| -> " + ex.Message);
          ResultError?.Flush();
          chromeProfile.WriteLog(ex.GetType().FullName+ ":" + ex.Message + ex.StackTrace);
        }
        finally
        {
          chromeProfile.WriteLog("Close chrome");
          chromeProfile.CloseChrome();
        }
      }
    }

    void WorkCheck()
    {
      while (true)
      {
        AccountData accountData = AccountsQueue.Dequeue();
        if (accountData == null) break;

        try
        {
          chromeProfile.ResetProfileData();
          //string proxy = null;
          if (ProxysQueue.Count > 0)
          {
            ProxyHelper proxyHelper = new ProxyHelper(ProxysQueue.Dequeue());
            if (proxyHelper.IsLogin)//extension
            {
              string ext_path = Extensions.ChromeProfilePath + "\\" + chromeProfile.ProfileName + ".zip";
              ProxyLoginExtension.GenerateExtension(ext_path, proxyHelper.Host, proxyHelper.Port, proxyHelper.UserName, proxyHelper.PassWord);
              chromeProfile.OpenChrome(null, ext_path);
            }
            else chromeProfile.OpenChrome(proxyHelper.Proxy, null);//set to chrome option
            //proxy = proxyHelper.Gen();
          }
          else chromeProfile.OpenChrome();

          chromeProfile.RunLogin(accountData);

          AdsResult result = chromeProfile.Check();
          StreamWriter streamWriter = null;
          switch (result)
          {
            case AdsResult.Success:
              streamWriter = ResultSuccess;
              break;
            case AdsResult.Failed:
              streamWriter = ResultFailed;
              break;
          }
          streamWriter?.WriteLine(accountData);
          streamWriter?.Flush();
        }
        catch (OperationCanceledException)
        {
          return;
        }
        catch (ChromeAutoException cae)
        {
          ResultError?.WriteLine(accountData + " -> " + cae.Message);
          ResultError?.Flush();
          chromeProfile.WriteLog("ChromeAutoException:" + cae.Message);
        }
        catch (Exception ex)
        {
          if (ex is AggregateException ae) ex = ae.InnerException;
          ResultError?.WriteLine(accountData + " -> " + ex.Message);
          ResultError?.Flush();
          chromeProfile.WriteLog(ex.GetType().FullName + ":" + ex.Message + ex.StackTrace);
        }
        finally
        {
          chromeProfile.CloseChrome();
        }
      }
    }

#region IQueue
    public bool IsPrioritize => false;
    public bool ReQueue => false;
    public void Cancel() => chromeProfile.Stop();

    public bool CheckEquals(IQueue queue)
    {
      return this.Equals(queue);
    }

    public Task DoWork()
    {
      if (RunFlag) return Task.Factory.StartNew(Work, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
      else return Task.Factory.StartNew(WorkCheck, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }
#endregion
  }
}
