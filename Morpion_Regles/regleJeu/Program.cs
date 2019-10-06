using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace regleJeu
{
    class Program
    {
        static void Main(string[] args)
        {
           for (int i=0; i<100;i++)
           {
                bool test = Test.GamePlayer1Win();
                Console.WriteLine(test.ToString());
           }
           Console.ReadKey();
        }
    }
}
