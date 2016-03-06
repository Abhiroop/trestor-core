using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetWallet.WalletOperations
{
    class UserList
    {
         string _UserName;
         string _LastLoginTime;


         public string UserName { get { return _UserName; } }
         public string LastLoginTime { get { return _LastLoginTime; } }

        public UserList(string UserName, string LastLoginTime)
        {
            _UserName = UserName;
            _LastLoginTime = LastLoginTime;
        }
    }
}
