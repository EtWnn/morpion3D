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
            Match match1 = new Match();
            Vector3 position = new Vector3();
            int x = 0;
            int y = 0;
            int z = 0;
            match1.afficher(match1.MatricePlateau);
            while (!match1.FinJeu)
            {
                Console.WriteLine("C'est le tour de {0}", match1.Mode);
                Console.WriteLine("Quelle est la coordonnee x (0,1 ou 2) de la position que vous voulez jouer ? (La couche)");
                x = (int.Parse(Console.ReadLine()));
                position.X = x;
                Console.WriteLine("Quelle est la coordonnee y (0,1 ou 2) de la position que vous voulez jouer ? (la ligne) ");
                y = (int.Parse(Console.ReadLine()));
                position.Y = y;
                Console.WriteLine("Quelle est la coordonnee z (0,1 ou 2) de la position que vous voulez jouer ? (la colonne)");
                z = (int.Parse(Console.ReadLine()));
                position.Z = z;
                match1.Jouer(position);

                byte[] StatusGame = Serialize.SerializationMatchStatus(match1);

                Console.WriteLine("Plateau");
                match1.afficher(match1.MatricePlateau);
                Console.WriteLine("Fin du jeu : {0}", match1.FinJeu);
            }

            Console.ReadKey();
        }
    }
}
