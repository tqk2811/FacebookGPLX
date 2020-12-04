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
  class SettingData
  {
    public int MaxRun { get; set; }









    public static void Load()
    {
      if(Setting == null)
      {
        timer.Elapsed += Timer_Elapsed;
        if (File.Exists(Extensions.SettingPath)) Setting = JsonConvert.DeserializeObject<SettingData>(File.ReadAllText(Extensions.SettingPath));
        else
        {
          Setting = new SettingData();
          Save();
        }
      }
    }
    private static void Timer_Elapsed(object sender, ElapsedEventArgs e) => File.WriteAllText(Extensions.SettingPath, JsonConvert.SerializeObject(Setting));
    public static void Save()
    {
      timer.Stop();
      timer.Start();
    }
    public static SettingData Setting { get; private set; }
    static readonly Timer timer = new Timer(500);
  }
}
