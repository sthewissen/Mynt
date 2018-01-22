using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.BackTester.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                BackTester.Program.WriteIntro();
                System.Console.WriteLine();
                System.Console.WriteLine();
                BackTester.Program.PresentMenuToUser();
            }
            catch (Exception ex)
            {
                BackTester.Program.WriteColoredLine($"\t{ex.Message}", ConsoleColor.Red);
                System.Console.ReadLine();
            }
        }
    }
}
