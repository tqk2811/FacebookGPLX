using FacebookGPLX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TqkLibrary.Queues.TaskQueues;

namespace FacebookGPLX
{
  class ItemQueue : IQueue
  {
    public static readonly Queue<AccountData> AccountsQueue = new Queue<AccountData>();
    readonly ChromeProfile chromeProfile;
    public ItemQueue(ChromeProfile chromeProfile)
    {
      this.chromeProfile = chromeProfile;
    }


    void Work()
    {
      while(true)
      {
        AccountData accountData = AccountsQueue.Dequeue();
        if (accountData == null) break;

        try
        {
          chromeProfile.OpenChrome();
          chromeProfile.RunLogin(accountData);
        }
        finally
        {
          chromeProfile.CloseChrome();
          chromeProfile.ResetProfileData();
        }
      }
    }







    #region IQueue
    public bool IsPrioritize => false;
    public bool ReQueue => false;
    public void Cancel() => chromeProfile.Stop();

    public bool CheckEquals(IQueue queue)
    {
      return this.Equals(queue);
    }

    public Task DoWork()=> Task.Factory.StartNew(Work, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    #endregion
  }
}
