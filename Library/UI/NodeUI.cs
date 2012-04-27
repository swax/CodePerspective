using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XLibrary
{
    public class NodeUI
    {
        public int ID;
        public XNodeIn XNode;

        public NodeUI(XNodeIn xNode)
        {
            ID = xNode.ID;
            XNode = xNode;
        }
    }
}
