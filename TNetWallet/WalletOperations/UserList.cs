using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetWallet.WalletOperations
{
    class UserList
    {
        public string UserName{set; get;}
        public string LastLoginTime { set; get; }

        public UserList(string UserName, string LastLoginTime)
        {
            this.UserName = UserName;
            this.LastLoginTime = LastLoginTime;
        }
    }
}
