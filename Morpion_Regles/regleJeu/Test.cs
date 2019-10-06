using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace regleJeu
{
    public static class Test
    {
        public static void LaunchGame2Players()
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
                Console.WriteLine("Plateau");
                match1.afficher(match1.MatricePlateau);
                Console.WriteLine("Fin du jeu : {0}", match1.FinJeu);
            }
            Console.ReadKey();
        }

        public static void LaunchRandomGameStepByStep()
        {
            Random rnd = new Random();
            Match match1 = new Match();
            Vector3 position = new Vector3();
            while (!match1.FinJeu)
            {
                position.X = rnd.Next(0, 3);
                position.Y = rnd.Next(0, 3);
                position.Z = rnd.Next(0, 3);
                Console.WriteLine("\n ----------------------");
                Console.WriteLine("Le {0} joue la position couche : {1}, ligne : {2}, colonne : {3}", match1.Mode, position.X, position.Y, position.Z);
                match1.Jouer(position);
                Console.WriteLine("Statut Plateau");
                match1.afficher(match1.MatricePlateau);
                Console.WriteLine("\n ----------------------");
                Console.ReadKey();
            }
            Console.WriteLine("\n ----------------------");
            Console.WriteLine("Fin du jeu : {0}", match1.FinJeu);
            Console.WriteLine("Statut Plateau Fin Jeu");
            match1.afficher(match1.MatricePlateau);
            Console.WriteLine("\n ----------------------");
            Console.ReadKey();
        }
        public static void LaunchRandomGameShowFinalState()
        {
            Random rnd = new Random();
            Match match1 = new Match();
            Vector3 position = new Vector3();
            while (!match1.FinJeu)
            {
                position.X = rnd.Next(0, 3);
                position.Y = rnd.Next(0, 3);
                position.Z = rnd.Next(0, 3);
                match1.Jouer(position);
                
            }
            Console.WriteLine("\n ----------------------");
            Console.WriteLine("Fin du jeu : {0}", match1.FinJeu);
            Console.WriteLine("Mode : {0}", match1.Mode);
            Console.WriteLine("Statut Plateau Fin Jeu");
            match1.afficher(match1.MatricePlateau);
            Console.WriteLine("\n ----------------------");
            Console.ReadKey();
        }
        public static bool GamePlayer1Win()
        {
            bool test = true;
            Match match1 = new Match();
            Vector3 position = new Vector3();
            // Player 1 plays
            position.X = 1;
            position.Y = 1;
            position.Z = 1;
            match1.Jouer(position);
            // Player 2 plays
            position.X = 0;
            position.Y = 0;
            position.Z = 1;
            match1.Jouer(position);
            // Player 1 plays
            position.X = 0;
            position.Y = 1;
            position.Z = 1;
            match1.Jouer(position);
            // Player 2 plays
            position.X = 0;
            position.Y = 0;
            position.Z = 0;
            match1.Jouer(position);
            // Player 1 plays
            position.X = 2;
            position.Y = 1;
            position.Z = 1;
            match1.Jouer(position);
            if (match1.FinJeu != true)
            {
                Console.WriteLine("erreur de declaration de fin de jeu");
                test = false;
            }
            if (match1.Mode != ModeJeu.Player1Won)
            {
                Console.WriteLine("erreur de declaration de gagnant");
                test = false;
            }
            return test;
            
        }
        public static void LaunchRandomGameWithSerialization()
        {
            Random rnd = new Random();
            Match match1 = new Match();
            Vector3 position = new Vector3();
            while (!match1.FinJeu)
            {
                position.X = rnd.Next(0, 3);
                position.Y = rnd.Next(0, 3);
                position.Z = rnd.Next(0, 3);
                Console.WriteLine("\n ----------------------");
                Console.WriteLine("Le {0} joue la position couche : {1}, ligne : {2}, colonne : {3}", match1.Mode, position.X, position.Y, position.Z);
                match1.Jouer(position);
                Console.WriteLine("Statut Plateau");
                match1.afficher(match1.MatricePlateau);
                byte[] data = Serialize.SerializationMatchStatus(match1);
                Console.WriteLine("\n ----------------------");
                Console.ReadKey();
            }
            Console.WriteLine("\n ----------------------");
            Console.WriteLine("Fin du jeu : {0}", match1.FinJeu);
            Console.WriteLine("Statut Plateau Fin Jeu");
            match1.afficher(match1.MatricePlateau);
            Console.WriteLine("\n ----------------------");
            Console.ReadKey();
        }
    }
}
