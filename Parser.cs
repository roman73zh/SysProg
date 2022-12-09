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

        public Leaf panic (String reason = "")
        {
            Console.Write("PAAAAANIIIIIC!!!!");
            if (reason != "")
                Console.WriteLine($"\t причина: {reason}");
            return new Leaf(null, Lexem.Types.ParsingError, 0, "Error state");
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

        private int priority(Lexem lexem)
        {
            String str = lexem.desc;
            if (str == "mul_operation" || str == "div_operation" || str == "mod_operation")
                return 3;
            else if (str == "sum_operation" || str == "sub_operation")
                return 2;
            else return 1;
        }


        private TreeElement parseIntegerExpression(Node prev)
        {
            TreeElement localRoot = null;
            List<Lexem> outLexems = new List<Lexem>();
            Stack<Lexem> st = new Stack<Lexem> ();
            int openedScopes = 0;
            do
            {
                if (peekLexem().type == Lexem.Types.Operation && peekLexem().desc.Contains("comparison"))
                    break;
                Lexem lexem = nextLexem();
                if (lexem == null)
                    break;
                if (lexem.type == Lexem.Types.Variable || lexem.type == Lexem.Types.Constant || lexem.type == Lexem.Types.Identifier || lexem.type == Lexem.Types.Keyword)
                {
                    if ((lexem.type == Lexem.Types.Variable || lexem.type == Lexem.Types.Identifier) && (!variables.Any(v => v.name == lexem.desc)))
                        panic("Необъявленная переменная");
                    if (lexem.type == Lexem.Types.Variable && variables.FirstOrDefault(v => v.name == lexem.desc).dataType == "boolean")
                        panic("Недопустимое использование переменной типа bool");
                    if (lexem.type == Lexem.Types.Keyword && (lexem.desc == "true" || lexem.desc == "false"))
                        return panic("Нельзя использовать логические значения в численных выражениях");
                    outLexems.Add(lexem);
                } else if (lexem.type == Lexem.Types.Delimeter && lexem.desc == "(")
                {
                    st.Push(lexem);
                    openedScopes++;
                } else if (lexem.type == Lexem.Types.Delimeter && lexem.desc == ")")
                {
                    Lexem temp = st.Pop();
                    while(temp.type != Lexem.Types.Delimeter && temp.desc != "(")
                    {
                        outLexems.Add(temp);
                        temp = st.Pop();
                    }
                    openedScopes--;
                } else
                {
                    if (st.Count == 0 || priority(st.Peek()) < priority(lexem)){
                        st.Push(lexem);
                    }
                    else
                    {
                        Lexem temp = lexem;
                        while (!(st.Count == 0 || priority(st.Peek()) < priority(temp)))
                        {
                            outLexems.Add(st.Pop());
                        }
                        st.Push(lexem);
                    }
                }
            }
            while (peekLexem().type != Lexem.Types.Delimeter || !(peekLexem().desc == ";" || peekLexem().desc == "," || peekLexem().desc == ")" && openedScopes == 0));
            while (st.Count > 0)
                outLexems.Add(st.Pop());
            Stack<TreeElement> treeStack = new Stack<TreeElement> ();
            foreach (var lexem in outLexems)
            {
                if (lexem.type == Lexem.Types.Constant || lexem.type == Lexem.Types.Variable || lexem.type == Lexem.Types.Identifier)
                {
                    treeStack.Push(new Leaf(null, lexem.type, lexem.id, lexem.desc));
                } else
                {
                    if (treeStack.Count() > 1)
                    {
                        TreeElement b = treeStack.Pop();
                        TreeElement a = treeStack.Pop();
                        Node temp = new Node(null, lexem.desc);
                        b.prev = temp;
                        a.prev = temp;
                        temp.a = a;
                        temp.b = b;
                        treeStack.Push(temp);
                    } else
                    {
                        panic("Некорректный синтаксис арифметического выражения");
                    }
                }
            }
            if (treeStack.Count != 1)
            {
                panic("Некорректный синтаксис арифметического выражения");
            } else
            {
                localRoot = treeStack.Pop();
                localRoot.prev = prev;
            }
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

        private TreeElement parseBoolExpression(Node prev)
        {
            TreeElement localRoot = null;
            if (peekLexem().type == Lexem.Types.Keyword && (peekLexem().desc == "true" || peekLexem().desc == "false"))
            {
                nextLexem();
                localRoot = new Leaf(prev, Lexem.Types.Keyword, currentLexem().id, currentLexem().desc);
                if (peekLexem().type != Lexem.Types.Delimeter)
                    panic("Данные после конца условного выражения");
            } else
            {
                TreeElement a = parseIntegerExpression(prev);
                if (peekLexem().type == Lexem.Types.Delimeter)
                {
                    return a;
                }
                Lexem op = nextLexem();
                TreeElement b = parseIntegerExpression(prev);
                localRoot = new Node(prev, op.desc);
                ((Node)localRoot).a = a;
                ((Node)localRoot).b = b;
                a.prev = localRoot;
                b.prev = localRoot;
            }
            return localRoot;
        }

        private Node parseAssign(Node prev)
        {
            Node localRoot = new Node(prev, "Assign node");
            localRoot.a = new Leaf(localRoot, Lexem.Types.Variable, variables.FindIndex(v => v.name == prevLexem().desc), prevLexem().desc);
            if (variables.FirstOrDefault(v => v.name == prevLexem().desc).dataType == "boolean")
            {
                localRoot.b = parseBoolExpression(localRoot);
            } else
            {
                localRoot.b = parseIntegerExpression(localRoot);
            }
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
                localRoot.a = parseBoolExpression(localRoot);
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
