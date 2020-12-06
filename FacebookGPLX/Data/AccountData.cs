using System.Collections.Generic;
using System.IO;

namespace FacebookGPLX.Data
{
  class AccountData
  {
    public string UserName { get; set; }
    public string PassWord { get; set; }
    public string TwoFA { get; set; }

    public override string ToString()
    {
      return $"{UserName}|{PassWord}|{TwoFA}";
    }

    public static List<AccountData> LoadFromTxt(string filePath)
    {
      List<AccountData> accountDatas = new List<AccountData>();
      if (File.Exists(filePath))
      {
        foreach(var line in File.ReadAllLines(filePath))
        {
          var line_data = line.Split('|');
          if(line_data.Length == 3)
          {
            accountDatas.Add(new AccountData()
            {
              UserName = line_data[0],
              PassWord = line_data[1],
              TwoFA = line_data[2]
            });
          }  
        }
      }
      return accountDatas;
    }

    public static List<AccountData> LoadFromExcel(string filePath)
    {
      List<AccountData> accountDatas = new List<AccountData>();
      if (File.Exists(filePath))
      {

      }
      return accountDatas;
    }
  }
}
