using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Helpers
{
    abstract class Printer
    {
        protected virtual void Print(string message)
        {
            DisplayUtils.Display(message);
        }
    }
}
