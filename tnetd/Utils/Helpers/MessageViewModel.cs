
 // @Author: Arpan Jati
 // @Date: 30 August 2015

using System.Collections.ObjectModel;

namespace TNetD.Helpers
{
    class MessageViewModel
    {

        ObservableCollection<DisplayMessageType> logMessages = new ObservableCollection<DisplayMessageType>();
        ObservableCollection<DisplayMessageType> statusMessages = new ObservableCollection<DisplayMessageType>();
        
        public ObservableCollection<DisplayMessageType> LogMessages
        {
            get
            {
                return logMessages;
            }
        }

        public ObservableCollection<DisplayMessageType> StatusMessages
        {
            get
            {
                return statusMessages;
            }
        }

        public void ProcessSkips()
        {
            try {

                if (statusMessages.Count > 1000)
                {
                    for (int i = 0; i < 200; i++)
                        statusMessages.RemoveAt(0);                    
                }

                if (logMessages.Count > 1000)
                {
                    for (int i = 0; i < 200; i++)
                        logMessages.RemoveAt(0);
                }
            }
            catch { }              
        }

    }
}
