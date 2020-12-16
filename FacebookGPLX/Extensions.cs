using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacebookGPLX
{
  internal static class Extensions
  {
    public static string ExeFolderPath { get; } = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
    public static string ChromeDrivePath { get; } = ExeFolderPath + "\\ChromeDrive";
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
  }
}