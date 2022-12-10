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

        private void forwardLexem(int step = 1)
        {
            lexemIndex -= step; ;
        }

        private Lexem peekLexem()
        {
            return lexems[lexemIndex];
        }

        private int priorityFull(Lexem lexem)
        {
            String str = lexem.desc;
            if (str == "inc_operation" || str == "dec_operation")
                return 13;
            else if (str == "mul_operation" || str == "div_operation" || str == "mod_operation")
                return 12;
            else if (str == "sum_operation" || str == "sub_operation")
                return 11;
            else if (str == "more_comparison" || str == "less_comparison" || str == "more_equ_comparison" || str == "less_equ_comparison")
                return 9;
            else if (str == "equal_comparison" || str == "not_equal_comparison")
                return 8;
            else if (str == "bit_and_operator")
                return 7;
            else if (str == "bit_or_operator")
                return 6;
            else if (str == "and_operation")
                return 4;
            else if (str == "or_operation")
                return 3;
            else if (str == "assign_operation")
                return 1;
            else
                return 0;
        }

        private int getDataType(Lexem lexem)
        {
            if (lexem.type == Lexem.Types.Keyword && (lexem.desc == "true" || lexem.desc == "false"))
                return 1;
            else if (lexem.type == Lexem.Types.Operation && lexem.desc.Contains("comparison"))
                return 1;
            else if (lexem.type == Lexem.Types.Operation && (lexem.desc.Contains("and_operation") || lexem.desc.Contains("or_operation")))
                return 1;
            else if (lexem.type == Lexem.Types.Variable && variables.FirstOrDefault(v => v.name == lexem.desc).dataType == "boolean")
                return 1;
            return 2;
        }

        private TreeElement parseExpression(Node prev)
        {
            TreeElement localRoot = null;
            List<Lexem> outLexems = new List<Lexem>();
            Stack<Lexem> st = new Stack<Lexem>();
            int openedScopes = 0;
            do
            {
                Lexem lexem = nextLexem();
                if (lexem == null)
                    break;
                if (lexem.type == Lexem.Types.Variable || lexem.type == Lexem.Types.Constant || lexem.type == Lexem.Types.Identifier || lexem.type == Lexem.Types.Keyword)
                {
                    if ((lexem.type == Lexem.Types.Variable || lexem.type == Lexem.Types.Identifier) && (!variables.Any(v => v.name == lexem.desc)))
                        panic("Необъявленная переменная");
                    outLexems.Add(lexem);
                }
                else if (lexem.type == Lexem.Types.Delimeter && lexem.desc == "(")
                {
                    st.Push(lexem);
                    openedScopes++;
                }
                else if (lexem.type == Lexem.Types.Delimeter && lexem.desc == ")")
                {
                    Lexem temp = st.Pop();
                    while (temp.type != Lexem.Types.Delimeter && temp.desc != "(")
                    {
                        outLexems.Add(temp);
                        temp = st.Pop();
                    }
                    openedScopes--;
                }
                else
                {
                    if (st.Count == 0 || priorityFull(st.Peek()) < priorityFull(lexem))
                    {
                        st.Push(lexem);
                    }
                    else
                    {
                        Lexem temp = lexem;
                        while (!(st.Count == 0 || priorityFull(st.Peek()) < priorityFull(temp)))
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
            Stack<(TreeElement, int)> treeStack = new Stack<(TreeElement, int)>();
            foreach (var lexem in outLexems)
            {
                if (lexem.type == Lexem.Types.Constant || lexem.type == Lexem.Types.Variable || lexem.type == Lexem.Types.Identifier || lexem.type == Lexem.Types.Keyword)
                {
                    treeStack.Push((new Leaf(null, lexem.type, lexem.id, lexem.desc), getDataType(lexem)));
                }
                else
                {
                    
                    if (lexem.desc == "inc_operation" || lexem.desc == "dec_operation")
                    {
                        if (treeStack.Count() > 0)
                        {
                            var a = treeStack.Pop();
                            Node temp = new Node(null, lexem.desc);
                            int reqType = Consts.Constants.operators.FirstOrDefault(t => t.Value.Item2 == lexem.desc).Value.Item3;
                            a.Item1.prev = temp;
                            temp.a = a.Item1;
                            if (reqType > a.Item2)
                                panic("Ошибка приведения типов");
                            treeStack.Push((temp, getDataType(lexem)));
                        }
                        else
                        {
                            panic("Некорректный синтаксис арифметического выражения");
                        }
                    }
                    else
                    {
                        if (treeStack.Count() > 1)
                        {
                            var b = treeStack.Pop();
                            var a = treeStack.Pop();
                            Node temp = new Node(null, lexem.desc);
                            int reqType = Consts.Constants.operators.FirstOrDefault(t => t.Value.Item2 == lexem.desc).Value.Item3;
                            if (lexem.desc == "assign_operation")
                                reqType = a.Item2;
                            b.Item1.prev = temp;
                            a.Item1.prev = temp;
                            temp.a = a.Item1;
                            temp.b = b.Item1;
                            if (reqType > a.Item2 || reqType > b.Item2)
                                panic("Ошибка приведения типов");
                            treeStack.Push((temp, getDataType(lexem)));
                        }
                        else
                        {
                            panic("Некорректный синтаксис арифметического выражения");
                        }
                    }
                }
            }
            if (treeStack.Count != 1)
            {
                panic("Некорректный синтаксис арифметического выражения");
            }
            else
            {
                localRoot = treeStack.Pop().Item1;
                localRoot.prev = prev;
            }
            return localRoot;
        }

        private Node parseAssign(Node prev)
        {
            forwardLexem(2);
            TreeElement result = parseExpression(prev);
            if (result.GetType() == typeof(Node))
                return (Node)result;
            else
                return new Node(prev);
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

        private Node parseSingleStmt(Node prev)
        {
            Node localRoot = null;
            if (currentLexem().type == Lexem.Types.Variable)
            {
                if (!variables.Any(v => v.name == currentLexem().desc))
                    panic("Необъявленная переменная");
                if (peekLexem().type == Lexem.Types.Operation)
                {
                    nextLexem();
                    localRoot = parseAssign(prev);
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
                localRoot.a = parseExpression(localRoot);
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
