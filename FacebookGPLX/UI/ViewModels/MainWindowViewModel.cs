using FacebookGPLX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.WpfUi;

namespace FacebookGPLX.UI.ViewModels
{
  class MainWindowViewModel : BaseViewModel 
  {
    public MainWindowViewModel()
    {

    }





    int _AccountCount = 0;
    public int AccountCount
    {
      get { return _AccountCount; }
      set { _AccountCount = value; NotifyPropertyChange(); }
    }

    #region Save
    public int MaxRun
    {
      get { return SettingData.Setting.MaxRun; }
      set { SettingData.Setting.MaxRun = value; SettingData.Save(); NotifyPropertyChange(); }
    }







    #endregion
  }
}
