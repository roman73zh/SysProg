using Lab1;
using Lab1.Models;
using System.Xml.Linq;

void printTree(TreeElement treeElement, String det = "", bool isSecond = false)
{
    if (det != "")
    {
        det = det.Substring(0, det.Length - 9);
        if (isSecond)
        {
            Console.WriteLine(det + "    -----" + treeElement.ToString());
            det += "         ";
        }
        else
        {
            if (treeElement is Node && ((Node)treeElement.prev).b != null)
            {
                Console.WriteLine(det + "    |----" + treeElement.ToString());
                det += "    |    ";
            }
            else
            {
                Console.WriteLine(det + "    |----" + treeElement.ToString());
                det += "         ";
            } 
        }
    }
    else
    {
        Console.WriteLine(treeElement.ToString());
    }
    det += "    |    ";
    if (treeElement is Node)
    {
        Node node = (Node)treeElement;
        if (node.a != null)
            printTree(node.a, det);
        if (node.b != null)
            printTree(node.b, det, true);
    }
}

Console.WriteLine("Hello, World!");
DirectoryInfo dirInfo = new DirectoryInfo("../../../Examples/");
FileInfo[] files = dirInfo.GetFiles("*.*");
LexemProc proc = new LexemProc();
Parser parser = new Parser();
foreach (FileInfo file in files)
{
    proc.LoadFile(file.ToString());
    Console.WriteLine("#############################################################################\nProcessing " + file.Name);
    proc.Process();
    parser.loadLexems(proc.GetLexems());
    parser.proceed();
    var tree = parser.getTree();
    printTree(tree);
    
    Console.WriteLine("\n++++++++++++++++++++++++++++++++++++\nLexems:");
    foreach (var lexem in proc.GetLexems())
    {
        Console.WriteLine(lexem.ToString());
    }
    Console.WriteLine("++++++++++++++++++++++++++++++++++++++\nVariables:");
    foreach (var variable in proc.GetVariables())
    {
        Console.WriteLine(variable.ToString());
    }
    Console.WriteLine("++++++++++++++++++++++++++++++++++++++\nIdentifiers:");
    foreach (var identifier in proc.GetIdentifiers())
    {
        Console.WriteLine(identifier.ToString());
    }
    Console.WriteLine("#############################################################################");
    proc.clear();
}