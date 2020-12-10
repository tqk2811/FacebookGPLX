using FacebookGPLX.Common;
using FacebookGPLX.Data;
using FacebookGPLX.UI.ViewModels;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TqkLibrary.Net.Facebook;
using TqkLibrary.Queues.TaskQueues;

namespace FacebookGPLX.UI
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    readonly TaskQueue<ItemQueue> taskQueue = new TaskQueue<ItemQueue>()
    {
      MaxRun = 0
    };
    readonly MainWindowViewModel mainWindowViewModel;


    public MainWindow()
    {
      Directory.CreateDirectory(Extensions.OutputPath);
      Directory.CreateDirectory(Extensions.ChromeProfilePath);
      Directory.CreateDirectory(Extensions.ImageSuccess);
      SettingData.Load();
      UserAgent.Load(Extensions.ExeFolderPath + "\\UAs.txt");
      mainWindowViewModel = new MainWindowViewModel(this.Dispatcher);
      this.DataContext = mainWindowViewModel;
      InitializeComponent();
      taskQueue.Dispatcher = this.Dispatcher;
      taskQueue.RunRandom = false;
      taskQueue.OnRunComplete += TaskQueue_OnRunComplete;
      taskQueue.OnQueueComplete += TaskQueue_OnQueueComplete;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      //ChromeProfile chromeProfile = new ChromeProfile("Test");
      //chromeProfile.TestCaptcha();

      //AccountDatas.Add(new AccountData() { UserName = "100056998035708", PassWord = "THmedia@8888", TwoFA = "6Y6GJOEPKVHZ7E6OQ3PMEPFNZDZ54QS4" });//acc bi checkpoint
      //AccountDatas.Add(new AccountData() { UserName = "100056944147952", PassWord = "THmedia@8888", TwoFA = "T7YWYAXWTQBIAAKHC3JK2Z66SME3LDJX" });//khang nghi thanh cong
      //AccountDatas.Add(new AccountData() { UserName = "100057260070746", PassWord = "THmedia@8888", TwoFA = "SO6IOLMOEU6OHWLGPMHSWHHDL4SK3M7P" });//khang nghi thanh cong
      //AccountDatas.Add(new AccountData() { UserName = "100056844197022", PassWord = "THmedia@8888", TwoFA = "WSC7SNSSITUHHY6KW7HKQYJ4D56MGFG5" });//email
      //AccountDatas.Add(new AccountData() { UserName = "100056744687657", PassWord = "THmedia@8888", TwoFA = "324EJVUN5JLFDPF6ULLKZH7CADEVSABH" });//den phone
      //AccountDatas.Add(new AccountData() { UserName = "100056858118683", PassWord = "THmedia@8888", TwoFA = "4OFJGEBYL5XRQGFXBUUYOA7WWUFGRE4L" });//khang nghi thanh cong
      //AccountDatas.Add(new AccountData() { UserName = "100057003068751", PassWord = "THmedia@8888", TwoFA = "I7JSBVRE34WVH2OHM5SWYPL2UDO3FWU7" });//khang nghi thanh cong
      //AccountDatas.Add(new AccountData() { UserName = "100056762778410", PassWord = "THmedia@8888", TwoFA = "DXTJY2N3G7XXBN67FFH5PBEUZXCJVFOJ" });//khang nghi thanh cong
      //AccountDatas.Add(new AccountData() { UserName = "100056873588504", PassWord = "THmedia@8888", TwoFA = "632CZQ6QEMCVSMGNELOQSVE4Y534H6BQ" });//email
      //AccountDatas.Add(new AccountData() { UserName = "100056964578087", PassWord = "THmedia@8888", TwoFA = "JBTJW2BJSJZMJLTMDTEANMMZWFBSOAJQ" });//khang nghi thanh cong
      //AccountDatas.Add(new AccountData() { UserName = "100056777286563", PassWord = "THmedia@8888", TwoFA = "5KB5A5CSPISJVS5LPBBZZEZ4JVGIBWCL" });//khang nghi thanh cong
      AccountDatas.Add(new AccountData() { UserName = "100055071842022", PassWord = "THmedia@8386", TwoFA = "RKALCRLC3SUZCIVIAZ6I5SZTPWBJEOIV" });
      mainWindowViewModel.AccountCount = AccountDatas.Count;
    }

    //async void Acc1()
    //{
    //  ChromeProfile chromeProfile = new ChromeProfile("Test Profile");
    //  //chromeProfile.ResetProfileData();
    //  chromeProfile.OpenChrome();
    //  //chromeProfile.RunLogin(new AccountData() { UserName= "100056998035708", PassWord= "THmedia@8888",TwoFA = "6Y6GJOEPKVHZ7E6OQ3PMEPFNZDZ54QS4" });
    //  //string token = chromeProfile.GetToken();
    //  //string token = "EAAGNO4a7r2wBAI96BN3TNH46ORo7Yrc1egzTtMCGvbiKZC9NhILJZCR1yRrQ4u7Icsc9xcgVc56fojAaNnQbr8mTsencXyxn2fkr7N8NRnOjXT6sZBRBIQyVqYxYhm587wfKTLQyGtrdWtqxmwGzwNFWYTW2ZB9fTBW6QnDZCL8a3jjqSMz0oPnNVGfs7zn0ZD";
    //  FacebookApi facebookApi = new FacebookApi();
    //  //Bitmap image = await facebookApi.PictureBitMap(token);
    //  //Bitmap image = (Bitmap)Bitmap.FromFile("D:\\test.png");
    //  //Bitmap image2 = image.DrawGPLX("Nguyễn Văn A","21-10-1995");
    //  //image2.Save("D:\\out.png");
    //  //string userinfo = await facebookApi.UserInfo(token, "birthday,name,id");
    //  //dynamic json = JsonConvert.DeserializeObject(userinfo);
    //  //string birthday = json.birthday;
    //  //string name = json.name;
    //  //string id = json.id;
    //  //chromeProfile.RunAdsManager("100056998035708");
    //  //chromeProfile.CloseChrome();
    //}

    //async void Acc2()
    //{
    //  //100056844197022
    //  //100056762778410
    //  //100056873588504
    //  //100056964578087
    //  try
    //  {
    //    ChromeProfile chromeProfile = new ChromeProfile("Test Profile 2");
    //    chromeProfile.ResetProfileData();
    //    chromeProfile.OpenChrome();
    //    chromeProfile.RunLogin(new AccountData() { UserName = "100056777286563", PassWord = "THmedia@8888", TwoFA = "5KB5A5CSPISJVS5LPBBZZEZ4JVGIBWCL" });
    //    string token = chromeProfile.GetToken();
    //    //string token = "";
    //    FacebookApi facebookApi = new FacebookApi();
    //    Bitmap image = await facebookApi.PictureBitMap(token);
    //    string userinfo = await facebookApi.UserInfo(token, "birthday,name,id");
    //    dynamic json = JsonConvert.DeserializeObject(userinfo);
    //    string birthday = json.birthday;
    //    string name = json.name;
    //    string id = json.id;
    //    Bitmap fake = image.DrawGPLX(name, birthday);
    //    fake.Save("D:\\test.png");
    //    chromeProfile.RunAdsManager(id, "D:\\test.png", Task.FromResult(0));
    //    //chromeProfile.CloseChrome();
    //  }
    //  catch (Exception ex)
    //  {
    //    if (ex is AggregateException ae) ex = ae.InnerException;
    //    MessageBox.Show(ex.Message + ex.StackTrace, ex.GetType().FullName);
    //  }
    //}



    #region taskQueue

    private void TaskQueue_OnQueueComplete(ItemQueue queue)
    {
      //throw new NotImplementedException();
    }

    private void TaskQueue_OnRunComplete()
    {
      ItemQueue.ResultFailed?.Close();
      ItemQueue.ResultSuccess?.Close();
      ItemQueue.ResultError?.Close();
      ItemQueue.ResultFailed = null;
      ItemQueue.ResultSuccess = null;
      ItemQueue.ResultError = null;
    }
    #endregion

    #region Button
    readonly List<AccountData> AccountDatas = new List<AccountData>();
    private void BT_LoadAccounts_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.InitialDirectory = Extensions.ExeFolderPath;
      openFileDialog.Filter = "txt file|*.txt|all file|*.*";
      if (openFileDialog.ShowDialog() == true)
      {
        AccountDatas.Clear();
        AccountDatas.AddRange(AccountData.LoadFromTxt(openFileDialog.FileName));
        mainWindowViewModel.AccountCount = AccountDatas.Count;
      }
    }

    readonly List<string> ProxysData = new List<string>();
    private void BT_LoadProxy_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.InitialDirectory = Extensions.ExeFolderPath;
      openFileDialog.Filter = "txt file|*.txt|all file|*.*";
      if (openFileDialog.ShowDialog() == true)
      {
        ProxysData.Clear();
        ProxysData.AddRange(File.ReadAllLines(openFileDialog.FileName).Where(x => !string.IsNullOrWhiteSpace(x)));
        mainWindowViewModel.ProxyCount = ProxysData.Count;
      }
    }

    private void BT_Stop_Click(object sender, RoutedEventArgs e)
    {
      taskQueue.ShutDown();
    }


    private void BT_Run_Click(object sender, RoutedEventArgs e)
    {
      if(taskQueue.MaxRun == 0 && taskQueue.RunningCount == 0)
      {
        ItemQueue.RunFlag = true;
        ItemQueue.AccountsQueue.Clear();
        ItemQueue.ProxysQueue.Clear();
        AccountDatas.ForEach(x => ItemQueue.AccountsQueue.Enqueue(x));
        ProxysData.ForEach(x => ItemQueue.ProxysQueue.Enqueue(x));
        for (int i = 0; i < mainWindowViewModel.MaxRun; i++)
        {
          ItemQueue itemQueue = new ItemQueue("Profile_" + i, mainWindowViewModel.LogCallback);
          taskQueue.Add(itemQueue);
        }
        ItemQueue.ResultSuccess = new StreamWriter(Extensions.OutputPath + "\\UpGPLX_success.txt", true);
        ItemQueue.ResultError = new StreamWriter(Extensions.OutputPath + "\\UpGPLX_error.txt", true);
        taskQueue.MaxRun = mainWindowViewModel.MaxRun;
      }
    }

    private void BT_Check_Click(object sender, RoutedEventArgs e)
    {
      if (taskQueue.MaxRun == 0 && taskQueue.RunningCount == 0)
      {
        ItemQueue.RunFlag = false;
        ItemQueue.ProxysQueue.Clear();
        ItemQueue.AccountsQueue.Clear();
        AccountDatas.ForEach(x => ItemQueue.AccountsQueue.Enqueue(x));
        ProxysData.ForEach(x => ItemQueue.ProxysQueue.Enqueue(x));
        for (int i = 0; i < mainWindowViewModel.MaxRun; i++)
        {
          ItemQueue itemQueue = new ItemQueue("Profile_" + i, mainWindowViewModel.LogCallback);
          taskQueue.Add(itemQueue);
        }
        ItemQueue.ResultFailed = new StreamWriter(Extensions.OutputPath + "\\CheckGPLX_failed.txt", true);
        ItemQueue.ResultSuccess = new StreamWriter(Extensions.OutputPath + "\\CheckGPLX_success.txt", true);
        taskQueue.MaxRun = mainWindowViewModel.MaxRun;
      }
    }
    #endregion

    private void BT_StopNext_Click(object sender, RoutedEventArgs e)
    {
      ItemQueue.AccountsQueue.Clear();
    }
  }
}
