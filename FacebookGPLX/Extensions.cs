using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacebookGPLX
{
  static class Extensions
  {
    public static string ExeFolderPath { get; } = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
    public static string ChromeDrivePath { get; } = ExeFolderPath + "\\ChromeDrive";
    public static string ChromeProfilePath { get; } = ExeFolderPath + "\\Profiles";
    public static string OutputPath { get; } = ExeFolderPath + "\\Output";
    public static string ImageSuccess { get; } = OutputPath + "\\ImageSuccess";
    public static string DebugData { get; } = ExeFolderPath + "\\DebugData";
    public static string AdbPath { get; } = ExeFolderPath + "\\Adb\\adb.exe";
  }
}
