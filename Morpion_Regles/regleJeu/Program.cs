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
            bool test = Test.TestSerializationOfOnePosition();
            Console.WriteLine(test.ToString());
            Console.ReadKey();
            Test.LaunchRandomGameShowFinalState();
            Console.ReadKey();
            Test.RandomGameWithSerializationOfOneGameStatus(10);
            Console.ReadKey();

        }
    }
}
