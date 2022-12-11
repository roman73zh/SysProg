using Lab1.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1
{
    internal class Generator
    {
        private Node? root;
        private String output;
        private int nestingLevel;
        
        public Generator()
        {
            root = null;
            output = "";
        }

        public void loadTree(Node tree)
        {
            root = tree;
        }

        private void tab()
        {
            for (int i = 0; i < nestingLevel; i++)
                output += "    ";
        }

        private void processElement(TreeElement? element)
        {
            if (element == null)
                return;
            if (element.GetType() == typeof(Node))
                processNode((Node)element);
            else if (element.GetType() == typeof(Leaf))
                processLeaf((Leaf)element);
        }

        private void processNode(Node current)
        {
            if (current.type == "Sequence node")
            {
                processElement(current.a);
                processElement(current.b);
            } else if (current.type == "Declaration node")
            {
                if (current.b != null && current.b.GetType() == typeof(Node))
                    processElement(current.b);
            } else if (current.type == "assign_operation")
            {
                processElement(current.a);
                output += "= ";
                processElement(current.b);
                output += "\n";
            } else if (current.type.Contains("operation") || current.type.Contains("comparison"))
            {
                processElement(current.a);
                output += Consts.Constants.pythonOperations[current.type] + " ";
                processElement(current.b);
                output += "\n";
            }


        }

        private void processLeaf(Leaf current)
        {
            if (current.type == Lexem.Types.Variable || current.type == Lexem.Types.Identifier || current.type == Lexem.Types.Constant)
                output += current.content + " ";
        }

        public String process()
        {
            output = "";
            nestingLevel = 0;
            processElement(root);
            return output;
        }
    }
}
