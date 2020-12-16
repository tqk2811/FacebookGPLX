using FacebookGPLX.Common;
using FacebookGPLX.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Tesseract;
using TqkLibrary.Net.Facebook;
using TqkLibrary.Queues.TaskQueues;
using TqkLibrary.SeleniumSupport;

namespace FacebookGPLX
{
  internal class ItemQueue : IQueue
  {
    public static bool RunFlag { get; set; }
    public static readonly Queue<AccountData> AccountsQueue = new Queue<AccountData>();
    public static bool StopLogAcc { get; set; } = false;
    public static readonly Queue<string> ProxysQueue = new Queue<string>();
    public static StreamWriter ResultSuccess { get; set; }
    public static StreamWriter ResultFailed { get; set; }
    public static StreamWriter ResultError { get; set; }
    public static StreamWriter ResultCheckPoint { get; set; }
    public static List<string> AndroidDevices { get; } = new List<string>();

    private readonly ChromeProfile chromeProfile;
    private readonly int profile_index = -1;

    public ItemQueue(int profile_index, LogCallback logCallback, string deviceId)
    {
      this.profile_index = profile_index;
      this.chromeProfile = new ChromeProfile("Profile_" + profile_index, deviceId);
      this.chromeProfile.LogEvent += logCallback;
    }

    private void Work()
    {
      try
      {
        while (true)
        {
          AccountData accountData = null;

          try
          {
            lock (AccountsQueue)
            {
              if (StopLogAcc || AccountsQueue.Count == 0) return;
              accountData = AccountsQueue.Dequeue();
            }
#if DEBUG
            chromeProfile.OpenChrome(0);
            chromeProfile.RunAdsManager("", Task.FromResult(0), null);
            return;
#endif
            ProxyHelper proxyHelper = null;
            if (ProxysQueue.Count > 0)
            {
              proxyHelper = new ProxyHelper(ProxysQueue.Dequeue());
              if (proxyHelper.IsLogin)//extension
              {
                string ext_path = Extensions.ChromeProfilePath + "\\" + chromeProfile.ProfileName + ".zip";
                ProxyLoginExtension.GenerateExtension(ext_path, proxyHelper.Host, proxyHelper.Port, proxyHelper.UserName, proxyHelper.PassWord);
                chromeProfile.OpenChrome(profile_index, proxyHelper.Proxy, ext_path);
              }
              else chromeProfile.OpenChrome(profile_index, proxyHelper.Proxy, null);
            }
            else chromeProfile.OpenChrome(profile_index);

            //chromeProfile.ClearCookies();

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
              DateTime dateTime = DateTime.ParseExact(birthday, "MM/dd/yyyy", CultureInfo.CurrentCulture);

              using (Bitmap image = facebookApi.PictureBitMap(access_token).Result)
              {
                using (Bitmap fake = image.DrawGPLX(name, dateTime.ToString("dd/MM/yyyy"))) fake.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);
              }
              chromeProfile.WriteLog("Download & Edit Avatar Completed");
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            chromeProfile.RunAdsManager(imagePath, task, proxyHelper);
            ResultSuccess?.WriteLine(accountData);
            ResultSuccess?.Flush();
            File.Copy(imagePath, Extensions.ImageSuccess + $"\\{id}.png", true);
          }
          catch (OperationCanceledException)
          {
            return;
          }
          catch (CheckPointException)
          {
            ResultCheckPoint?.WriteLine(accountData);
            ResultCheckPoint?.Flush();
            chromeProfile.WriteLog("CheckPointException:" + accountData);
          }
          catch (ChromeAutoException cae)
          {
            ResultError?.WriteLine($"{accountData}| -> {cae.Message}");
            ResultError?.Flush();
            chromeProfile.WriteLog("ChromeAutoException:" + cae.Message);
            try
            {
              chromeProfile.SaveHtml(Extensions.DebugData + $"\\{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}_{accountData?.UserName}.html");
            }
            catch (Exception)
            {
            }
          }
          catch (Exception ex)
          {
            if (ex is AggregateException ae) ex = ae.InnerException;
            ResultError?.WriteLine($"{accountData}| -> {ex.Message}");
            ResultError?.Flush();
            chromeProfile.WriteLog(ex.GetType().FullName + ":" + ex.Message + ex.StackTrace);
            try
            {
              chromeProfile.SaveHtml(Extensions.DebugData + $"\\{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}_{accountData?.UserName}.html");
            }
            catch (Exception)
            {
            }
          }
          finally
          {
            chromeProfile.ClearCookies();
            chromeProfile.CloseChrome();
            chromeProfile.WriteLog("Close chrome");
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message + ex.StackTrace, ex.GetType().FullName);
      }
    }

    private void WorkCheck()
    {
      while (true)
      {
        AccountData accountData = null;

        try
        {
          lock (AccountsQueue)
          {
            if (StopLogAcc || AccountsQueue.Count == 0) return;
            accountData = AccountsQueue.Dequeue();
          }

          //string proxy = null;
          if (ProxysQueue.Count > 0)
          {
            ProxyHelper proxyHelper = new ProxyHelper(ProxysQueue.Dequeue());
            if (proxyHelper.IsLogin)//extension
            {
              string ext_path = Extensions.ChromeProfilePath + "\\" + chromeProfile.ProfileName + ".zip";
              ProxyLoginExtension.GenerateExtension(ext_path, proxyHelper.Host, proxyHelper.Port, proxyHelper.UserName, proxyHelper.PassWord);
              chromeProfile.OpenChrome(profile_index, proxyHelper.Proxy, ext_path);
            }
            else chromeProfile.OpenChrome(profile_index, proxyHelper.Proxy, null);//set to chrome option
            //proxy = proxyHelper.Gen();
          }
          else chromeProfile.OpenChrome(profile_index);

          //chromeProfile.ClearCookies();

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
        catch (CheckPointException)
        {
          ResultCheckPoint?.WriteLine(accountData);
          ResultCheckPoint?.Flush();
          chromeProfile.WriteLog("CheckPointException:" + accountData);
        }
        catch (ChromeAutoException cae)
        {
          ResultError?.WriteLine(accountData + "| -> " + cae.Message);
          ResultError?.Flush();
          chromeProfile.WriteLog("ChromeAutoException:" + cae.Message);
        }
        catch (Exception ex)
        {
          if (ex is AggregateException ae) ex = ae.InnerException;
          ResultError?.WriteLine(accountData + "| -> " + ex.Message);
          ResultError?.Flush();
          chromeProfile.WriteLog(ex.GetType().FullName + ":" + ex.Message + ex.StackTrace);
        }
        finally
        {
          chromeProfile.ClearCookies();
          chromeProfile.CloseChrome();
          chromeProfile.WriteLog("Close chrome");
        }
      }
    }

    #region IQueue

    public bool IsPrioritize => false;
    public bool ReQueue => false;

    public void Cancel() => chromeProfile.Stop();

    public bool CheckEquals(IQueue queue) => this.Equals(queue);

    public Task DoWork()
    {
      if (RunFlag) return Task.Factory.StartNew(Work, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
      else return Task.Factory.StartNew(WorkCheck, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    #endregion IQueue
  }
}