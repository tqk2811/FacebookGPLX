using FacebookGPLX.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.WpfUi;
namespace FacebookGPLX
{
  static class Extensions
  {
    public static string ExeFolderPath { get; } = Directory.GetCurrentDirectory();
    public static string ChromeDriverPath { get; } = ExeFolderPath + "\\ChromeDrive";
    public static string ChromeProfilePath { get; } = ExeFolderPath + "\\Profiles";
    public static string OutputPath { get; } = ExeFolderPath + "\\Output";
    public static string ImageSuccess { get; } = OutputPath + "\\ImageSuccess";
    public static string DebugData { get; } = ExeFolderPath + "\\DebugData";
    public static string AdbPath { get; } = ExeFolderPath + "\\Adb\\adb.exe";

    private static readonly Random rd = new Random();

    public static string RandomString(int length)
    {
      const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
      return new string(Enumerable.Repeat(chars, length).Select(s => s[rd.Next(s.Length)]).ToArray());
    }

    public static string RandomString(int min, int max)
    {
      const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
      return new string(Enumerable.Repeat(chars, rd.Next(min, max + 1)).Select(s => s[rd.Next(s.Length)]).ToArray());
    }

    public static SaveSettingData<SettingData> Setting { get; } = new SaveSettingData<SettingData>(WpfUiExtensions.ExeFolderPath + "\\Setting.json");
  }
}