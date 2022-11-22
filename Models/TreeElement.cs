using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1.Models
{
    internal class TreeElement
    {
        public TreeElement prev;
        public TreeElement(TreeElement prev)
        {
            this.prev = prev;
        }

        override public String ToString()
        {
            return "";
        }
    }
}
