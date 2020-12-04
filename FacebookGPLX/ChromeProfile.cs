using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


    public bool ResetProfileData()
    {
      if(!IsOpenChrome)
      {
        if (Directory.Exists(ProfilePath)) Directory.Delete(ProfilePath, true);
        return true;
      }
      else return false;
    }
  }
}
