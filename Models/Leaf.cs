using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1.Models
{
    internal class Leaf : TreeElement
    {
        public Lexem.Types type;
        int id;
        public String content;

        public Leaf(Node prev, Lexem.Types type, int id, string content = "") : base(prev)
        {
            this.type = type;
            this.content = content;
            this.id = id;
        }

        override public String ToString()
        {
            return content;
        }
    }
}
