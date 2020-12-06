using FacebookGPLX.Common;
using FacebookGPLX.Data;
using FacebookGPLX.UI.ViewModels;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
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
    readonly TaskQueue<ItemQueue> taskQueue = new TaskQueue<ItemQueue>();
    readonly MainWindowViewModel mainWindowViewModel;


    public MainWindow()
    {
      SettingData.Load();
      UserAgent.Load(Extensions.ExeFolderPath + "\\UAs.txt");
      mainWindowViewModel = new MainWindowViewModel();
      this.DataContext = mainWindowViewModel;
      InitializeComponent();
      taskQueue.Dispatcher = this.Dispatcher;
      taskQueue.RunRandom = false;
      taskQueue.OnRunComplete += TaskQueue_OnRunComplete;
      taskQueue.OnQueueComplete += TaskQueue_OnQueueComplete;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      AccountDatas.Add(new AccountData() { UserName = "100056858118683", PassWord = "THmedia@8888", TwoFA = "4OFJGEBYL5XRQGFXBUUYOA7WWUFGRE4L" });
        //Task.Factory.StartNew(Acc2, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
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
      ItemQueue.ResultNotFound?.Close();
      ItemQueue.ResultSuccess?.Close();
      ItemQueue.ResultError?.Close();
      ItemQueue.ResultFailed = null;
      ItemQueue.ResultNotFound = null;
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
    private void BT_Run_Click(object sender, RoutedEventArgs e)
    {
      taskQueue.ShutDown();
      ItemQueue.AccountsQueue.Clear();
      AccountDatas.ForEach(x => ItemQueue.AccountsQueue.Enqueue(x));
      for (int i = 0; i < mainWindowViewModel.MaxRun; i++)
      {
        ItemQueue itemQueue = new ItemQueue("Profile_" + i, LogCallback);
        taskQueue.Add(itemQueue);
      }
      ItemQueue.ResultFailed = new System.IO.StreamWriter(Extensions.ExeFolderPath + "\\result_failed.txt",true);
      ItemQueue.ResultNotFound = new System.IO.StreamWriter(Extensions.ExeFolderPath + "\\result_notFound.txt", true);
      ItemQueue.ResultSuccess = new System.IO.StreamWriter(Extensions.ExeFolderPath + "\\result_success.txt", true);
      ItemQueue.ResultError = new System.IO.StreamWriter(Extensions.ExeFolderPath + "\\result_error.txt", true);
      taskQueue.MaxRun = mainWindowViewModel.MaxRun;
    }

    #endregion
    void LogCallback(string text)
    {
      Console.WriteLine(text);
    }
  }
}
