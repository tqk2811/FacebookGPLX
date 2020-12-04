using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.Queues.TaskQueues;

namespace FacebookGPLX
{
  class ItemQueue : IQueue
  {












    #region IQueue
    public bool IsPrioritize => false;

    public bool ReQueue => false;

    public void Cancel()
    {
      throw new NotImplementedException();
    }

    public bool CheckEquals(IQueue queue)
    {
      throw new NotImplementedException();
    }

    public Task DoWork()
    {
      throw new NotImplementedException();
    }
    #endregion
  }
}
