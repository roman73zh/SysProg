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
        private String pending;
        private int nestingLevel;
        private bool inExpression = false;
        
        public Generator()
        {
            root = null;
            output = "";
        }

        public void loadTree(Node tree)
        {
            root = tree;
        }

        private void tab(bool delayed = false)
        {
            if (!delayed)
                for (int i = 0; i < nestingLevel; i++)
                    output += "    ";
            else
                for (int i = 0; i < nestingLevel; i++)
                    pending += "    ";
        }

        private void releasePending()
        {
            output += pending;
            pending = "";
        }

        public void processElementBrakets(TreeElement? element)
        {
            if (element == null)
                return;
            if (element.GetType() == typeof(Leaf))
                processLeaf((Leaf)element, false);
            else if (element.GetType() == typeof(Node))
            {
                bool needBrakets = ((Node)element).type != "inc_operation" && ((Node)element).type != "dec_operation";
                if (needBrakets)
                    output += "( ";
                processNode((Node)element);
                if (needBrakets) 
                    output += ") ";
            }
        }

        private void processElement(TreeElement? element, bool delayed = false)
        {
            if (element == null)
                return;
            if (element.GetType() == typeof(Node))
                processNode((Node)element);
            else if (element.GetType() == typeof(Leaf))
                processLeaf((Leaf)element, delayed);
        }

        private void processNode(Node current)
        {
            if (current.type == "Sequence node")
            {
                processElement(current.a);
                releasePending();
                processElement(current.b);
                //releasePending();
            } else if (current.type == "Declaration node")
            {
                if (current.b != null && current.b.GetType() == typeof(Node))
                    processElement(current.b);
            } else if (current.type == "assign_operation")
            {
                tab();
                inExpression = true;
                processElement(current.a);
                output += "= ";
                processElement(current.b);
                output += "\n";
                inExpression = false;
            } else if (current.type == "inc_operation" || current.type == "dec_operation")
            {
                if (inExpression)
                {
                    processElement(current.a);
                    tab(true);
                    processElement(current.a, true);
                    pending += Consts.Constants.pythonOperations[current.type] + "\n";
                } else
                {
                    tab();
                    processElement(current.a);
                    output += Consts.Constants.pythonOperations[current.type] + "\n";
                }
            } else if (current.type == "inversion_operation")
            {
                output += Consts.Constants.pythonOperations[current.type] + " ";
                processElement(current.a);
            } else if (current.type.Contains("operation") || current.type.Contains("comparison"))
            {
                processElementBrakets(current.a);
                output += Consts.Constants.pythonOperations[current.type] + " ";
                processElementBrakets(current.b);
            } else if (current.type == "While node")
            {
                tab();
                nestingLevel++;
                output += "while ";
                inExpression = true;
                processElement(current.a);
                inExpression = false;
                output = output.Substring(0, output.Length - 1) + ":\n";
                releasePending();
                processElement(current.b);
                nestingLevel--;
            }


        }

        private void processLeaf(Leaf current, bool delayed)
        {
            if (!delayed && (current.type == Lexem.Types.Variable || current.type == Lexem.Types.Identifier || current.type == Lexem.Types.Constant))
                output += current.content + " ";
            else if (delayed && (current.type == Lexem.Types.Variable || current.type == Lexem.Types.Identifier || current.type == Lexem.Types.Constant))
                pending += current.content + " ";
            else if (current.type == Lexem.Types.Keyword && (current.content == "true" || current.content == "false"))
                output += (current.content == "true" ? "True " : "False ");
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
