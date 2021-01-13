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
using TqkLibrary.Media.Images;

namespace FacebookGPLX.UI
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private readonly TaskQueue<ItemQueue> taskQueue = new TaskQueue<ItemQueue>()
    {
      MaxRun = 0
    };

    private readonly MainWindowViewModel mainWindowViewModel;

    public MainWindow()
    {
      //DateTime dateTime = new DateTime(2021, 2, 1);
      //if (DateTime.Now > dateTime) throw new Exception("");

      Directory.CreateDirectory(Extensions.OutputPath);
      Directory.CreateDirectory(Extensions.ChromeProfilePath);
      Directory.CreateDirectory(Extensions.ImageSuccess);
      Directory.CreateDirectory(Extensions.DebugData);
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
#if DEBUG
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
      //AccountDatas.Add(new AccountData() { UserName = "100055071842022", PassWord = "THmedia@8386", TwoFA = "RKALCRLC3SUZCIVIAZ6I5SZTPWBJEOIV" });
      //AccountDatas.Add(new AccountData() { UserName = "100058850012762", PassWord = "THmedia@8386", TwoFA = "DCTKJCN3AS3OW4ZAIFRYMJCZC3XGBP4K" });//khang nghi thanh cong
      //AccountDatas.Add(new AccountData() { UserName = "100055013798404", PassWord = "THmedia@1102", TwoFA = "IVJK3MXYHXV5E3CK4HOB4QQKWJYVNOCJ" });//ko tim thay nut khang
      //AccountDatas.Add(new AccountData() { UserName = "100055194077117", PassWord = "THmedia@1102", TwoFA = "FURCC74PVQKETIQ5DVNNGLEZK2DILICN" });//acc bi checkpoint
      //AccountDatas.Add(new AccountData() { UserName = "100055170846395", PassWord = "THmedia@8888", TwoFA = "4YIN6HQ5OFACUXTHCPRALYDVWMUA4VPG" });//acc bi checkpoint
      //AccountDatas.Add(new AccountData() { UserName = "100055533753481", PassWord = "THmedia@8888", TwoFA = "NZO6R7AWWKYXYI6GGOQTMUVBDYQSPGIZ" });//acc bi checkpoint
      //AccountDatas.Add(new AccountData() { UserName = "100058311460546", PassWord = "THmedia@8386", TwoFA = "VWUOHHI6N4ZMJWUMUWBWMQNH6GZJRE3O" });//acc bi checkpoint
      //AccountDatas.Add(new AccountData() { UserName = "100058236025901", PassWord = "THmedia@8386", TwoFA = "2D7BZCUSOYZAGGQ5YWS6UJ42ZBPM6UM3" });//ko tim thay nut khang
      //AccountDatas.Add(new AccountData() { UserName = "100058368257568", PassWord = "THmedia@8386", TwoFA = "ZLJ2RHT47XO7PR2C7BR3I3FXZERVMAVQ" });//da gui khang
      //AccountDatas.Add(new AccountData() { UserName = "100058285010119", PassWord = "THmedia@8386", TwoFA = "O3HTL4QXZPD7EY2D2OIRDIQKFNWQYWIE" });//ko tim thay nut khang
      //AccountDatas.Add(new AccountData() { UserName = "100058712403429", PassWord = "HDvia@6666", TwoFA = "Y6YBXJWXMVYNRZTLH7TZONYGAIUQXO2B" });//not 2FA, mail
      //AccountDatas.Add(new AccountData() { UserName = "100058765803739", PassWord = "HDvia@8888", TwoFA = "OHFOX5CQXKJGSX7ZXTV3XYIDILBGAGQA" });//sms
      AccountDatas.Add(new AccountData() { UserName = "100058713244059", PassWord = "HDvia@8888", TwoFA = "3KJETIRIJT4VUD6UBOBEXPDNQFQJ3LTX" });
      //ProxysData.Add("217.163.29.98:35335:phuonglazy:bluecrazy");
      //mainWindowViewModel.AccountCount = AccountDatas.Count;
      //Bitmap bitmap = (Bitmap)Bitmap.FromFile("D:\\c.png");
      //ImageHelper.DrawGPLX(bitmap, "Nguyễn Văn Anh", "24/12/1990").Save("D:\\test.png");
      //TwoFactorAuthNet.TwoFactorAuth twoFactorAuth = new TwoFactorAuthNet.TwoFactorAuth();
      //string facode = twoFactorAuth.GetCode("T2YARRP5LO5LHOEZLSQCEULIK3BEGCVE");
#endif
    }

    #region taskQueue

    private void TaskQueue_OnQueueComplete(Task task, ItemQueue queue)
    {
      //throw new NotImplementedException();
    }

    private void TaskQueue_OnRunComplete()
    {
      taskQueue.MaxRun = 0;
      ItemQueue.ResultCheckPoint?.Close();
      ItemQueue.ResultFailed?.Close();
      ItemQueue.ResultSuccess?.Close();
      ItemQueue.ResultError?.Close();
      ItemQueue.ResultCheckPoint = null;
      ItemQueue.ResultFailed = null;
      ItemQueue.ResultSuccess = null;
      ItemQueue.ResultError = null;
      mainWindowViewModel.LogCallback("Run Completed");
    }

    #endregion taskQueue

    #region Button

    private readonly List<AccountData> AccountDatas = new List<AccountData>();

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

    private readonly List<string> ProxysData = new List<string>();

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
      using (StreamWriter streamWriter = new StreamWriter(Extensions.OutputPath + "\\DataChuaChay_Stop.txt", true))
      {
        ItemQueue.AccountsQueue.ToList().ForEach(x => streamWriter.WriteLine(x));
      }
    }

    private void BT_Run_Click(object sender, RoutedEventArgs e)
    {
      if (taskQueue.MaxRun == 0 && taskQueue.RunningCount == 0)
      {
        try
        {
          if (Directory.Exists(Extensions.ChromeProfilePath)) Directory.Delete(Extensions.ChromeProfilePath, true);
          Directory.CreateDirectory(Extensions.ChromeProfilePath);
        }
        catch (Exception ex)
        {
          MessageBox.Show(ex.Message + ex.StackTrace, ex.GetType().FullName);
          return;
        }
        ChromeLocationHelper.Init();
        ItemQueue.RunFlag = true;
        ItemQueue.AccountsQueue.Clear();
        ItemQueue.ProxysQueue.Clear();
        ItemQueue.StopLogAcc = false;
        AccountDatas.ForEach(x => ItemQueue.AccountsQueue.Enqueue(x));
        ProxysData.ForEach(x => ItemQueue.ProxysQueue.Enqueue(x));
        List<string> devices = new List<string>();

        for (int i = 0; i < mainWindowViewModel.MaxRun; i++)
        {
          ItemQueue itemQueue = new ItemQueue(mainWindowViewModel.LogCallback);
          taskQueue.Add(itemQueue);
        }
        ItemQueue.ResultCanAds = new StreamWriter(Extensions.OutputPath + "\\UpGPLX_KhongCoNutKhang.txt", true);
        ItemQueue.ResultCheckPoint = new StreamWriter(Extensions.OutputPath + "\\UpGPLX_checkpoint.txt", true);
        ItemQueue.ResultSuccess = new StreamWriter(Extensions.OutputPath + "\\UpGPLX_success.txt", true);
        ItemQueue.ResultError = new StreamWriter(Extensions.OutputPath + "\\UpGPLX_error.txt", true);
        taskQueue.MaxRun = mainWindowViewModel.MaxRun;
      }
    }

    private void BT_Check_Click(object sender, RoutedEventArgs e)
    {
      if (taskQueue.MaxRun == 0 && taskQueue.RunningCount == 0)
      {
        try
        {
          if (Directory.Exists(Extensions.ChromeProfilePath)) Directory.Delete(Extensions.ChromeProfilePath, true);
          Directory.CreateDirectory(Extensions.ChromeProfilePath);
        }
        catch (Exception ex)
        {
          MessageBox.Show(ex.Message + ex.StackTrace, ex.GetType().FullName);
          return;
        }
        ChromeLocationHelper.Init();
        ItemQueue.StopLogAcc = false;
        ItemQueue.RunFlag = false;
        ItemQueue.ProxysQueue.Clear();
        ItemQueue.AccountsQueue.Clear();
        AccountDatas.ForEach(x => ItemQueue.AccountsQueue.Enqueue(x));
        ProxysData.ForEach(x => ItemQueue.ProxysQueue.Enqueue(x));
        for (int i = 0; i < mainWindowViewModel.MaxRun; i++)
        {
          ItemQueue itemQueue = new ItemQueue(mainWindowViewModel.LogCallback);
          taskQueue.Add(itemQueue);
        }
        ItemQueue.ResultCheckPoint = new StreamWriter(Extensions.OutputPath + "\\CheckGPLX_checkpoint.txt", true);
        ItemQueue.ResultFailed = new StreamWriter(Extensions.OutputPath + "\\CheckGPLX_failed.txt", true);
        ItemQueue.ResultSuccess = new StreamWriter(Extensions.OutputPath + "\\CheckGPLX_success.txt", true);
        ItemQueue.ResultError = new StreamWriter(Extensions.OutputPath + "\\CheckGPLX_error.txt", true);
        taskQueue.MaxRun = mainWindowViewModel.MaxRun;
      }
    }

    private void BT_StopNext_Click(object sender, RoutedEventArgs e)
    {
      ItemQueue.StopLogAcc = true;
      using (StreamWriter streamWriter = new StreamWriter(Extensions.OutputPath + "\\DataChuaChay_StopNext.txt", true))
      {
        ItemQueue.AccountsQueue.ToList().ForEach(x => streamWriter.WriteLine(x));
      }
    }

    #endregion Button
  }
}