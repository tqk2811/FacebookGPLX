using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FacebookGPLX.Common
{
  public static class UserAgent
  {
    static readonly List<string> Oss = new List<string>();
    static readonly List<string> Uas = new List<string>();
    static readonly Random rd = new Random();
    public static void Load(string filePath)
    {
      if (File.Exists(filePath))
      {
        var lines = File.ReadAllLines(filePath);
        int i = 1;
        for (; i < lines.Count(); i++)
        {
          if (lines[i].StartsWith("#")) break;
          Oss.Add(lines[i]);
        }
        for (i++; i < lines.Count(); i++)
        {
          Uas.Add(lines[i]);
        }
      }
      else MessageBox.Show(filePath, "File Not Found");
    }
    public static string GetRandom() => Uas[rd.Next(Uas.Count)].Replace("{os}", Oss[rd.Next(Oss.Count)]);
  }
}
