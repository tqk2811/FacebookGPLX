using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacebookGPLX.Common
{
  class ProxyHelper
  {
    public readonly bool IsLogin;
    public readonly string Proxy;
    public readonly string Host;
    public readonly string Port;
    public readonly string UserName;
    public readonly string PassWord;

    public ProxyHelper(string proxy)
    {
      if (string.IsNullOrEmpty(proxy)) throw new ArgumentNullException(nameof(proxy));
      string[] split = proxy.Split(':');
      if (split.Count() == 2)
      {
        Host = split[0];
        Port = split[1];
        IsLogin = false;
      }
      else if (split.Count() == 4)
      {
        Host = split[0];
        Port = split[1];
        UserName = split[2];
        PassWord = split[3];
        IsLogin = true;
      }
      else throw new ArgumentException(nameof(proxy));
      Proxy = proxy;
    }

    public string Gen() => IsLogin ? $"{UserName}:{PassWord}@{Host}:{Port}" : $"{Host}:{Port}";
  }
}
