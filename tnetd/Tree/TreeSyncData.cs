using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Tree
{
    class TreeSyncData
    {
        private ListTreeNode LTN;
        private bool getAll;

        public TreeSyncData(ListTreeNode LTN, bool getAll)
        {
            this.LTN = LTN;
            this.getAll = getAll;
        }

        public void setGetAll(bool getAll)
        {
            this.getAll = getAll;
        }
    }
}
