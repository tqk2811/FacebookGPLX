using System.Collections.Generic;
using System.IO;

namespace FacebookGPLX.Data
{
  class AccountData
  {
    public string UserName { get; set; }
    public string PassWord { get; set; }
    public string TwoFA { get; set; }
    public string AccessToken { get; set; }
    public string Cookies { get; set; }

    public override string ToString() => $"{UserName}|{PassWord}|{TwoFA}|{AccessToken}|{Cookies}";

    public static List<AccountData> LoadFromTxt(string filePath)
    {
      List<AccountData> accountDatas = new List<AccountData>();
      if (File.Exists(filePath))
      {
        foreach(var line in File.ReadAllLines(filePath))
        {
          var line_data = line.Split('|');
          if(line_data.Length >= 3)
          {
            AccountData accountData = new AccountData()
            {
              UserName = line_data[0].Trim(),
              PassWord = line_data[1].Trim(),
              TwoFA = line_data[2].Trim()
            };
            if (line_data.Length >= 4) accountData.AccessToken = line_data[3].Trim();
            if (line_data.Length >= 5) accountData.Cookies = line_data[4].Trim();

            accountDatas.Add(accountData);
          }  
        }
      }
      return accountDatas;
    }
  }
}
