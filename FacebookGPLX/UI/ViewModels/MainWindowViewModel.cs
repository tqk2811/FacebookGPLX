using FacebookGPLX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using TqkLibrary.WpfUi;
using TqkLibrary.WpfUi.ObservableCollection;
namespace FacebookGPLX.UI.ViewModels
{
  class MainWindowViewModel : BaseViewModel 
  {
    readonly Dispatcher dispatcher;
    public MainWindowViewModel(Dispatcher dispatcher)
    {
      this.dispatcher = dispatcher;
    }

    public void LogCallback(string text) => dispatcher.Invoke(() => Logs.Add(text));



    int _AccountCount = 0;
    public int AccountCount
    {
      get { return _AccountCount; }
      set { _AccountCount = value; NotifyPropertyChange(); }
    }

    int _ProxyCount = 0;
    public int ProxyCount
    {
      get { return _ProxyCount; }
      set { _ProxyCount = value; NotifyPropertyChange(); }
    }

    public LimitObservableCollection<string> Logs { get; } = new LimitObservableCollection<string>()
    {
      Limit = 500,
      IsInsertTop = true,
      LogPath = Extensions.ExeFolderPath + "\\logs.txt"
    };

    #region Save
    public int MaxRun
    {
      get { return SettingData.Setting.MaxRun; }
      set { SettingData.Setting.MaxRun = value; SettingData.Save(); NotifyPropertyChange(); }
    }
    public int DelayStepMin
    {
      get { return SettingData.Setting.DelayStepMin; }
      set { SettingData.Setting.DelayStepMin = value; SettingData.Save(); NotifyPropertyChange(); }
    }
    public int DelayStepMax
    {
      get { return SettingData.Setting.DelayStepMax; }
      set { SettingData.Setting.DelayStepMax = value; SettingData.Save(); NotifyPropertyChange(); }
    }
    public int DelayWebMin
    {
      get { return SettingData.Setting.DelayWebMin; }
      set { SettingData.Setting.DelayWebMin = value; SettingData.Save(); NotifyPropertyChange(); }
    }
    public int DelayWebMax
    {
      get { return SettingData.Setting.DelayWebMax; }
      set { SettingData.Setting.DelayWebMax = value; SettingData.Save(); NotifyPropertyChange(); }
    }
    public int DelayFbCheck
    {
      get { return SettingData.Setting.DelayFbCheck; }
      set { SettingData.Setting.DelayFbCheck = value; SettingData.Save(); NotifyPropertyChange(); }
    }
    public string RentCodeKey
    {
      get { return SettingData.Setting.RentCodeKey; }
      set { SettingData.Setting.RentCodeKey = value; SettingData.Save(); NotifyPropertyChange(); }
    }

    #endregion
  }
}
