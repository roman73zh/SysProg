using System;

namespace Lab1.Examples
{
    internal class Test1
    {
        public Test1()
        {
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 20; j++)
                    Console.WriteLine(" i = {0:D}, j = {1:D}", i, j);
            }
        }

    }
}
