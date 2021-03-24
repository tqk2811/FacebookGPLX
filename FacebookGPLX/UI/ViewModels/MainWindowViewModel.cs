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
  enum SmsService
  {
    None = 0,
    Rencode = 1,
    OtpSim = 2,
    SimThue = 3,
    ChoThueSim = 4
  }
  class MainWindowViewModel : BaseViewModel
  {
    public MainWindowViewModel(Dispatcher dispatcher) : base(dispatcher)
    {
    }

    public void LogCallback(string text)
    {
      dispatcher.Invoke(() => Logs.Add(text));
    }

    private int _AccountCount = 0;

    public int AccountCount
    {
      get { return _AccountCount; }
      set { _AccountCount = value; NotifyPropertyChange(); }
    }

    private int _ProxyCount = 0;

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

    private int _DevicesCount = 0;

    public int DevicesCount
    {
      get { return _DevicesCount; }
      set { _DevicesCount = value; NotifyPropertyChange(); }
    }

    public List<ComboBoxViewModel> CbbData { get; } = new List<ComboBoxViewModel>()
    {
      new ComboBoxViewModel(TypeRun.V1),
      new ComboBoxViewModel(TypeRun.V2)
    };


    #region Save

    public int MaxRun
    {
      get { return Extensions.Setting.Setting.MaxRun; }
      set { Extensions.Setting.Setting.MaxRun = value; Extensions.Setting.Save(); NotifyPropertyChange(); }
    }

    public int DelayStepMin
    {
      get { return Extensions.Setting.Setting.DelayStepMin; }
      set { Extensions.Setting.Setting.DelayStepMin = value; Extensions.Setting.Save(); NotifyPropertyChange(); }
    }

    public int DelayStepMax
    {
      get { return Extensions.Setting.Setting.DelayStepMax; }
      set { Extensions.Setting.Setting.DelayStepMax = value; Extensions.Setting.Save(); NotifyPropertyChange(); }
    }

    public int DelayWebMin
    {
      get { return Extensions.Setting.Setting.DelayWebMin; }
      set { Extensions.Setting.Setting.DelayWebMin = value; Extensions.Setting.Save(); NotifyPropertyChange(); }
    }

    public int DelayWebMax
    {
      get { return Extensions.Setting.Setting.DelayWebMax; }
      set { Extensions.Setting.Setting.DelayWebMax = value; Extensions.Setting.Save(); NotifyPropertyChange(); }
    }

    public int ReTryCount
    {
      get { return Extensions.Setting.Setting.ReTryCount; }
      set { Extensions.Setting.Setting.ReTryCount = value; Extensions.Setting.Save(); NotifyPropertyChange(); }
    }

    public string RentCodeKey
    {
      get { return Extensions.Setting.Setting.RentCodeKey; }
      set { Extensions.Setting.Setting.RentCodeKey = value; Extensions.Setting.Save(); NotifyPropertyChange(); }
    }

    public string TwoCaptchaKey
    {
      get { return Extensions.Setting.Setting.TwoCaptchaKey; }
      set { Extensions.Setting.Setting.TwoCaptchaKey = value; Extensions.Setting.Save(); NotifyPropertyChange(); }
    }

    public string OtpSimKey
    {
      get { return Extensions.Setting.Setting.OtpSimKey; }
      set { Extensions.Setting.Setting.OtpSimKey = value; Extensions.Setting.Save(); NotifyPropertyChange(); }
    }

    public string SimThueKey
    {
      get { return Extensions.Setting.Setting.SimThueKey; }
      set { Extensions.Setting.Setting.SimThueKey = value; Extensions.Setting.Save(); NotifyPropertyChange(); }
    }

    public string ChoThueSimKey
    {
      get { return Extensions.Setting.Setting.ChoThueSimKey; }
      set { Extensions.Setting.Setting.ChoThueSimKey = value; Extensions.Setting.Save(); NotifyPropertyChange(); }
    }

    public SmsService SmsService
    {
      get { return Extensions.Setting.Setting.SmsService; }
      set { Extensions.Setting.Setting.SmsService = value; Extensions.Setting.Save(); NotifyPropertyChange(); }
    }


    public TypeRun TypeRun
    {
      get { return Extensions.Setting.Setting.TypeRun; }
      set { Extensions.Setting.Setting.TypeRun = value; Extensions.Setting.Save(); NotifyPropertyChange(); }
    }
    #endregion Save
  }
}