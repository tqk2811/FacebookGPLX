using FacebookGPLX.Data;
using FacebookGPLX.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using TqkLibrary.Queues.TaskQueues;

namespace FacebookGPLX.UI
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    readonly MainWindowViewModel mainWindowViewModel;
    public MainWindow()
    {
      SettingData.Load();
      mainWindowViewModel = new MainWindowViewModel();
      this.DataContext = mainWindowViewModel;
      InitializeComponent();
      taskQueue.Dispatcher = this.Dispatcher;
      taskQueue.RunRandom = false;
      taskQueue.OnRunComplete += TaskQueue_OnRunComplete;
      taskQueue.OnQueueComplete += TaskQueue_OnQueueComplete;
    }

    #region taskQueue
    readonly TaskQueue<ItemQueue> taskQueue = new TaskQueue<ItemQueue>();
    private void TaskQueue_OnQueueComplete(ItemQueue queue)
    {
      //throw new NotImplementedException();
    }

    private void TaskQueue_OnRunComplete()
    {
      //throw new NotImplementedException();
    }
    #endregion


    #region Button
    private void BT_Run_Click(object sender, RoutedEventArgs e)
    {

    }
    #endregion
  }
}
