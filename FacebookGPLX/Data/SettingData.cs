using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace FacebookGPLX.Data
{
  internal enum SmsService
  {
    None = 0,
    Rencode = 1,
    OtpSim = 2,
    SimThue = 3,
    ChoThueSim = 4
  }

  internal class SettingData
  {
    public int MaxRun { get; set; } = 1;

    public string TwoCaptchaKey { get; set; }
#if DEBUG
      = "25fbc812b40b10686f4dc98105bbf62f";

#endif

    public string RentCodeKey { get; set; }
#if DEBUG
      = "JMAWHXwulem2fVh0vTShzGLKmzBq3YN1vu30qOXWoYYv";

#endif
    public string OtpSimKey { get; set; }
    public string SimThueKey { get; set; }
    public string ChoThueSimKey { get; set; }

    public int DelayStepMin { get; set; } = 2000;
    public int DelayStepMax { get; set; } = 3000;
    public int DelayWebMin { get; set; } = 6000;
    public int DelayWebMax { get; set; } = 8000;
    public int ReTryCount { get; set; } = 5;
    public SmsService SmsService { get; set; } = SmsService.Rencode;

    private static readonly string SettingPath = Extensions.ExeFolderPath + "\\Setting.json";

    public static void Load()
    {
      if (Setting == null)
      {
        timer.Elapsed += Timer_Elapsed;
        if (File.Exists(SettingPath)) Setting = JsonConvert.DeserializeObject<SettingData>(File.ReadAllText(SettingPath));
        else
        {
          Setting = new SettingData();
          Save();
        }
      }
    }

    private static void Timer_Elapsed(object sender, ElapsedEventArgs e) => File.WriteAllText(SettingPath, JsonConvert.SerializeObject(Setting));

    public static void Save()
    {
      timer.Stop();
      timer.Start();
    }

    public static SettingData Setting { get; private set; }
    private static readonly Timer timer = new Timer(500);
  }
}