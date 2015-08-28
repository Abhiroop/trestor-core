using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Helpers
{
    class MessageViewModel
    {

        BindingList<LogMessageType> LogMessages = new BindingList<LogMessageType>();

        //ObservableCollection<LogMessageType> LogMessages = new ObservableCollection<LogMessageType>();

        ObservableCollection<LogMessageType> StatusMessages = new ObservableCollection<LogMessageType>();





    }
}
