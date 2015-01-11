using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Tree
{
    class TreeSyncData
    {
        // this address needs tto be filled from the listtreenode object
        //which requires to store the address also in the listtreenode
        //objects
        //needs to be done
        public Hash address;
        public ListTreeNode LTN;
        public bool getAll;

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
