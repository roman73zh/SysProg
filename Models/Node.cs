using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1.Models
{
    internal class Node : TreeElement
    {
        public TreeElement? a, b, prev;

        public String type;

        public Node(TreeElement prev, String type = "Empty node") : base(prev)
        {
            a = null;
            b = null;
            this.prev = prev;
            this.type = type;
        }

        override public String ToString()
        {
            return type;
        }
    }
}
