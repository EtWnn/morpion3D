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
            Plateau.afficher(match1.MatricePlateau);
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
                Plateau.afficher(match1.MatricePlateau);
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
                Plateau.afficher(match1.MatricePlateau);
                Console.WriteLine("\n ----------------------");
                Console.ReadKey();
            }
            Console.WriteLine("\n ----------------------");
            Console.WriteLine("Fin du jeu : {0}", match1.FinJeu);
            Console.WriteLine("Statut Plateau Fin Jeu");
            Plateau.afficher(match1.MatricePlateau);
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
            Plateau.afficher(match1.MatricePlateau);
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

        public static void RandomGameWithSerializationOfOneGameStatus(int numberRoundSerialization)
        {
            Random rnd = new Random();
            Match match1 = new Match();
            Match match2 = new Match();
            Vector3 position = new Vector3();
            int compt = 0;
            bool Match2Created = false;
            bool error = false;
            int taillePlateau = 3;
            while (match1.FinJeu != true)
            {
            position.X = rnd.Next(0, 3);
            position.Y = rnd.Next(0, 3);
            position.Z = rnd.Next(0, 3);
            match1.Jouer(position);
            if (compt == numberRoundSerialization)
            {
                byte[] data = Serialize.SerializationMatchStatus(match1);
                match2 = Serialize.DeserializationMatchStatus(data);
                Match2Created = true;
                for (int x = 0; x < taillePlateau; x++)
                {
                    for (int y = 0; y < taillePlateau; y++)
                    {
                        for (int z = 0; z < taillePlateau; z++)
                        {
                            if (match1.MatricePlateau[x,y,z] != match2.MatricePlateau[x,y,z])
                            {
                                Console.WriteLine("Erreur de plateau");
                                Console.WriteLine("Plateau match 1");
                                Plateau.afficher(match1.MatricePlateau);
                                Console.WriteLine("Plateau match 2");
                                Plateau.afficher(match2.MatricePlateau);
                                error =true;
                            }
                            
                        }
                    }
                }
                if (match1.Mode != match2.Mode)
                {
                    Console.WriteLine("Erreur de mode");
                    Console.WriteLine("Mode match 1 : {0}", match1.Mode);
                    Console.WriteLine("Mode match 2 : {0}", match2.Mode);
                    error =true;
                }
                if (!error)
                {
                Console.WriteLine("Serialization suceed");
                }
            }
            compt++;
            }
            if (!Match2Created)
            {
                Console.WriteLine("Pas assez de tours joues.\n Nombre de tours joues : {0}. \n Numero du tour auquel un match parallele devait etre cree : {1}.", compt, numberRoundSerialization);
            }
            Console.ReadKey();
        }

        public static bool TestSerializationOfOnePosition()
        {
            Random rnd = new Random();
            Vector3 position = new Vector3();
            Vector3 position2 = new Vector3();
            bool error = false;
            position.X = rnd.Next(0, 3);
            position.Y = rnd.Next(0, 3);
            position.Z = rnd.Next(0, 3);
            byte[] data = Serialize.SerializationPositionPlayed(position);
            position2=Serialize.DeserializationPositionPlayed(data);
            if (position.X != position2.X)
                {
                Console.WriteLine("Erreur de la coordonee X");
                error = true;
                }
            if (position.Y != position2.Y)
                {
                Console.WriteLine("Erreur de la coordonee Y");
                error = true;
                }
            if (position.Z != position2.Z)
                {
                Console.WriteLine("Erreur de la coordonee Z");
                error = true;
                }
            return (!error);
           
        }
    }
}
