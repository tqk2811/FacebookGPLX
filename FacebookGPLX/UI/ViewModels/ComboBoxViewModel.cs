using FacebookGPLX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacebookGPLX.UI.ViewModels
{
  enum TypeRun
  {
    V1 = 0,
    V2 = 1,
  }
  class ComboBoxViewModel
  {
    public ComboBoxViewModel(TypeRun typeRun)
    {
      this.TypeRun = typeRun;
    }

    public TypeRun TypeRun { get; }
  }
}
