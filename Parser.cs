using Lab1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Lab1
{
    internal class Parser
    {
        private List<Lexem> lexems;
        Node root;
        private List<Variable> variables;

        int lexemIndex;

        public void panic (String reason = "")
        {
            Console.Write("PAAAAANIIIIIC!!!!");
            if (reason != "")
                Console.WriteLine($"\t причина: {reason}");
        }

        public void loadLexems(List<Lexem> lexems)
        {
            root = new Node(null, "Sequence node");
            variables = new List<Variable>();
            this.lexems = lexems;
            lexemIndex = 0;
        }

        private Lexem nextLexem()
        {
            return lexems[lexemIndex++];
        }

        private Lexem currentLexem()
        {
            return lexems[lexemIndex - 1];
        }

        private Lexem prevLexem()
        {
            return lexems[lexemIndex - 2];
        }

        private Lexem peekLexem()
        {
            return lexems[lexemIndex];
        }

        private TreeElement parseExpression(Node prev)
        {
            TreeElement localRoot = null;
            do
            {
                Lexem lexem = nextLexem();
                localRoot = new Leaf((Node)prev, Lexem.Types.Constant, lexem.id, lexem.id.ToString());
            }
            while (peekLexem().type != Lexem.Types.Delimeter);
            return localRoot;
        }

        private TreeElement parseCondition(Node prev)
        {
            TreeElement localRoot = null;
            localRoot = new Leaf((Node)prev, Lexem.Types.Operation, 0, "Condition node (");
            do
            {
                Lexem lexem = nextLexem();
                ((Leaf)localRoot).content += lexem.desc + " ";
            }
            while (peekLexem().type != Lexem.Types.Delimeter);
            ((Leaf)localRoot).content += ")";
            return localRoot;
        }

        private Node parseAssign(Node prev)
        {
            Node localRoot = new Node(prev, "Assign node");
            localRoot.a = new Leaf(localRoot, Lexem.Types.Variable, variables.FindIndex(v => v.name == prevLexem().desc), prevLexem().desc);
            localRoot.b = parseExpression(localRoot);
            return localRoot;
        }

        private Node parseDeclaration(Node prev, Lexem dataType = null)
        {
            Node localRoot = new Node(prev, "Declaration node");
            Node result = localRoot;
            Lexem lexem = (dataType == null ? nextLexem() : dataType);
            String varType = lexem.desc;
            localRoot.a = new Leaf(localRoot, Lexem.Types.DataType, lexem.id, varType);
            lexem = nextLexem();
            if (lexem.type != Lexem.Types.Variable && lexem.type != Lexem.Types.Identifier)
                panic("Ожидалась переменная");
            if (variables.Any(v => v.name.Equals(lexem.desc)))
            {
                panic("Повторное объявление переменной");
            }
            else
            {
                variables.Add(new Variable(variables.Count, varType, lexem.desc));
            }
            if (peekLexem().type == Lexem.Types.Delimeter)
            {
                localRoot.b = new Leaf(localRoot, Lexem.Types.Variable, variables.Count - 1, lexem.desc);
            }
            else if (peekLexem().type == Lexem.Types.Operation)
            {
                if ((currentLexem().type != Lexem.Types.Variable && currentLexem().type != Lexem.Types.Identifier) || peekLexem().type != Lexem.Types.Operation || nextLexem().id != Consts.Constants.operators["="].Item1)
                    panic("Ожидалось присвоение");
                localRoot.b = parseAssign(localRoot);
            }
            return result;
        }

        private Node parseInc(Node prev)
        {
            Node localRoot = new Node(prev, "Inc node");
            localRoot.a = new Leaf(localRoot, Lexem.Types.Variable, variables.FindIndex(v => v.name == prevLexem().desc), prevLexem().desc);
            localRoot.b = new Leaf((Node)prev, Lexem.Types.Constant, 1, 1.ToString());
            return localRoot;
        }

        private Node parseSingleStmt(Node prev)
        {
            Node localRoot = null;
            if (currentLexem().type == Lexem.Types.Variable)
            {
                if (!variables.Any(v => v.name == currentLexem().desc))
                    panic("Необъявленная переменная");
                if (peekLexem().type == Lexem.Types.Operation && peekLexem().id == Consts.Constants.operators["="].Item1)
                {
                    nextLexem();
                    localRoot = parseAssign(prev);
                } else if (peekLexem().type == Lexem.Types.Operation && peekLexem().id == Consts.Constants.operators["++"].Item1)
                {
                    nextLexem();
                    localRoot = parseInc(prev);
                }
            }
            else
            {
                panic("Необъявленная переменная");
            }
            return localRoot;
        }

        private Node parseFor(Node prev)
        {
            Node localRoot = new Node(prev, "While node");
            Node result = localRoot;
            nextLexem();
            if (peekLexem().type != Lexem.Types.Delimeter || peekLexem().desc != "(")
            {
                panic("Неправильно объявлен for - ожидалось открытие скобки");
            }
            nextLexem();
            if (peekLexem().type == Lexem.Types.DataType)
            {
                Lexem dataType = peekLexem();
                localRoot.type = "Sequence node";
                localRoot.a = parseDeclaration(localRoot);
                localRoot.b = new Node(localRoot, "While node");
                localRoot = (Node)localRoot.b;
                while (peekLexem().type == Lexem.Types.Delimeter && peekLexem().desc == ",")
                {
                    nextLexem();
                    localRoot.type = "Sequence node";
                    localRoot.a = parseDeclaration(localRoot, dataType);
                    localRoot.b = new Node(localRoot, "While node");
                    localRoot = (Node)localRoot.b;
                }
            }
            if (peekLexem().type == Lexem.Types.Delimeter && peekLexem().desc == ";")
            {
                nextLexem();
            } else
            {
                panic("Неправильно объявлен for - ошибка в разделителях");
            }
            if (peekLexem().type != Lexem.Types.Delimeter)
                localRoot.a = parseCondition(localRoot);
            else
                localRoot.a = new Leaf((Node)prev, Lexem.Types.Operation, 0, "True node");
            if (peekLexem().type == Lexem.Types.Delimeter && peekLexem().desc == ";")
            {
                nextLexem();
            }
            else
            {
                panic("Неправильно объявлен for - ошибка в разделителях");
            }
            if (peekLexem().type == Lexem.Types.Delimeter && peekLexem().desc == ")")
            {
                nextLexem();
            } else
            {
                nextLexem();
                localRoot.b = new Node(localRoot, "Sequence node");
                localRoot = (Node)localRoot.b;
                localRoot.b = parseSingleStmt(localRoot);
                if (peekLexem().type == Lexem.Types.Delimeter && peekLexem().desc == ")")
                    nextLexem();
                else
                {
                    panic("Неправильно объявлен for - ожидалось закрытие скобки");
                }
            }

            if (localRoot.a != null)
                localRoot.b = parseLine(localRoot);
            else
                localRoot.a = parseLine(localRoot);
                
            return result;
        }

        private Node parseBlock(Node prev)
        {
            Node currentNode = new Node(prev, "Sequence node");
            Node result = currentNode;
            Lexem lexem = peekLexem();
            while (lexem.type != Lexem.Types.Delimeter || lexem.desc != "}")
            {
                if (lexem.type == Lexem.Types.Delimeter && lexem.desc == ";")
                {
                    nextLexem();
                }
                else
                {
                    currentNode.a = parseLine(currentNode);
                    currentNode.b = new Node(currentNode, "Sequence node");
                    currentNode = (Node)currentNode.b;
                }
                lexem = peekLexem();
            }
            nextLexem();
            currentNode = (Node)currentNode.prev;
            currentNode.b = null;
            return result;
        }

        private Node parseLine(Node prev)
        {
            Node currentNode = prev;
            Lexem lexem = peekLexem();

            if (lexem.type == Lexem.Types.DataType)
            {
                currentNode = parseDeclaration(currentNode);
            }
            else if (lexem.type == Lexem.Types.Keyword && lexem.desc == "for")
            {
                currentNode = parseFor(currentNode);
            }
            else if (lexem.type == Lexem.Types.Variable || lexem.type == Lexem.Types.Identifier)
            {
                nextLexem();
                currentNode = parseSingleStmt(currentNode);
            }
            else if (lexem.type == Lexem.Types.Delimeter && lexem.desc == "{")
            {
                nextLexem();
                currentNode = parseBlock(currentNode);
            }
            else
            {
                panic("Ошибка парсинга");
                nextLexem();
                currentNode = null;
            }
            return currentNode;
        }

        public void proceed()
        {
            Node currentLexem = root;

            while (lexemIndex < lexems.Count)
            {
                Lexem lexem = peekLexem();
                if (lexem.type == Lexem.Types.Delimeter && lexem.desc == ";")
                {
                    nextLexem();
                } else
                {
                    currentLexem.a = parseLine(currentLexem);
                    currentLexem.b = new Node(currentLexem, "Sequence node");
                    currentLexem = (Node)currentLexem.b;
                }
            }
            currentLexem = (Node)currentLexem.prev;
            currentLexem.b = null;
        }

        public Node getTree()
        {
            return root;
        }
    }
}
