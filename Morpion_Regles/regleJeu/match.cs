using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;


namespace regleJeu
{
    // L'etat d'une case du morpion
    public enum Case
    {
        Vide = 0,
        MotifJoueur1 = 1,
        MotifJoueur2 = 2,
        SurbrillanceJoueur1 = 3,
        SurbrillanceJoueur2 = 4
    };

    public enum ModeJeu
    {
        Joueur1,
        Joueur2,
        Player1Won,
        Player2Won,
        NoneWon,
    }

    [Serializable]
    public class Match
    {
     
        public ModeJeu Mode { get; set; } //Joueur qui peut jouer 
        public bool FinJeu { get; set; } = false; //booleen pour declarer la fin de la partie

        [XmlIgnore]
        public int[,,] MatricePlateau { get; set; } //La matrice du plateau

        [XmlArray("MatricePlateau")]
        public int[] ReadingsDto 
        { 
        get { return Flatten(MatricePlateau); }
        set { MatricePlateau = Expand(value); }
        }
        

        private const int TAILLEPLATEAU = 3; // Dimension du plateau
        private List<Vector3> PositionsJoueesJoueur1 { get; set; } = new List<Vector3>();
        private List<Vector3> PositionsJoueesJoueur2 { get; set; } = new List<Vector3>();

        public Match()
        {
            MatricePlateau = Plateau.generationPlateau(TAILLEPLATEAU);
            Mode =ModeJeu.Joueur1;
        }
        
        //Methode pour mettre a jour le morpion en fonction de l'action d un joueur
        public void Jouer(Vector3 positionJouee)
        {
            if (MatricePlateau[(int)positionJouee.X, (int)positionJouee.Y, (int)positionJouee.Z] == (int)Case.Vide)
            {
                List<Vector3> listePositionsGagnantes = new List<Vector3>();
                listePositionsGagnantes = CombinaisonGagnante(positionJouee);
                if (listePositionsGagnantes.Count==0)
                {
                    PlacerJeton(positionJouee, false);
                }
                else
                {
                   foreach (Vector3 position in listePositionsGagnantes)
                    {
                        PlacerJeton(position, true);
                    }
                }
                CalculFinJeu();
            }
            else
            {
                // a definir
            }
        }
        private void CalculFinJeu()
        {
            if (PositionsJoueesJoueur1.Count + PositionsJoueesJoueur2.Count == TAILLEPLATEAU * TAILLEPLATEAU * TAILLEPLATEAU)
            {
                Console.WriteLine("fin fin a cause du calcul fin jeu");
                FinJeu = true;
                Mode = ModeJeu.NoneWon;
            }
        }
        private List<Vector3> CombinaisonGagnante( Vector3 nouvellePosition)
        {
            bool combinaisonGagnante = false;
            List<Vector3> positionsJoueesJoueur = new List<Vector3>();
            List<Vector3> listePositionsGagnantes = new List<Vector3>();
            if (Mode== ModeJeu.Joueur1)
            {
                positionsJoueesJoueur = PositionsJoueesJoueur1;
            }
            else if (Mode == ModeJeu.Joueur2)
            {
                positionsJoueesJoueur = PositionsJoueesJoueur2;
            }
            if (positionsJoueesJoueur.Count != 0)
            { 
            for (int i=0; i<positionsJoueesJoueur.Count; i++)
            {
                for (int j=i+1; j < positionsJoueesJoueur.Count; j++)
                {
                    combinaisonGagnante = Alignement(nouvellePosition, positionsJoueesJoueur[i], positionsJoueesJoueur[j]);
                    if (combinaisonGagnante)
                    {
                        listePositionsGagnantes.Add(nouvellePosition);
                        listePositionsGagnantes.Add(positionsJoueesJoueur[i]);
                        listePositionsGagnantes.Add(positionsJoueesJoueur[j]);
                        FinJeu = true;
                        break;
                    }
                }
                if (combinaisonGagnante)
                {
                    break;
                }
            }
            }
            return listePositionsGagnantes;
        }
        private void PlacerJeton(Vector3 position, bool combinaisonGagnante)
        {
            if (Mode == ModeJeu.Joueur1 ||Mode == ModeJeu.Player1Won)
            {
                if (combinaisonGagnante)
                {
                    MatricePlateau[(int)position.X, (int)position.Y, (int)position.Z] = (int)Case.SurbrillanceJoueur1;
                    Mode = ModeJeu.Player1Won;
                }
                else
                {
                    MatricePlateau[(int)position.X, (int)position.Y, (int)position.Z] = (int)Case.MotifJoueur1;
                    Mode = ModeJeu.Joueur2;
                }
                PositionsJoueesJoueur1.Add(position);
            }
            else if (Mode == ModeJeu.Joueur2||Mode == ModeJeu.Player2Won)
            {
                if (combinaisonGagnante)
                {
                    MatricePlateau[(int)position.X, (int)position.Y, (int)position.Z] = (int)Case.SurbrillanceJoueur2;
                    Mode = ModeJeu.Player2Won;
                }
                else
                {
                    MatricePlateau[(int)position.X, (int)position.Y, (int)position.Z] = (int)Case.MotifJoueur2;
                    Mode = ModeJeu.Joueur1;
                }
                PositionsJoueesJoueur2.Add(position);
            }
        }
        private Boolean Alignement(Vector3 point1, Vector3 point2, Vector3 point3)
        {
            Boolean aGagne = false;
            Vector3 vector1 = point1 - point2;
            Vector3 vector2 = point1 - point3;
            Vector3 produitVectoriel = Vector3.Cross(vector1, vector2);
            Vector3 vecteurNul = new Vector3(0, 0, 0);
            if (produitVectoriel == vecteurNul )
            {
                aGagne = true;
            }
            return aGagne;
        }

        public static void afficher(int[,,] tab)
        {
            string aff = "";
            for (int x = 0; x < TAILLEPLATEAU; x++)
            {
                for (int y = 0; y < TAILLEPLATEAU; y++)
                {
                    for (int z = 0; z < TAILLEPLATEAU; z++)
                    {
                        aff += tab[x, y, z].ToString() + '\t';
                    }
                    aff += '\n';
                }
                aff += "\n\n";
            }
            Console.WriteLine(aff);
        }
        public static T[] Flatten<T>(T[,,] arr)
        {
        T[] arrFlattened = new T[TAILLEPLATEAU * TAILLEPLATEAU * TAILLEPLATEAU];
        for (int i = 0; i < TAILLEPLATEAU; i++)
        {
            for (int j = 0; j < TAILLEPLATEAU; j++)
                {
                for (int k=0; k<TAILLEPLATEAU;k++)
                    {
                        var test = arr[k, j,i];
                        arrFlattened[k+TAILLEPLATEAU * (j+TAILLEPLATEAU * i)] = arr[k, j, i];
                    }
                }
        }
        return arrFlattened;
        }
        
        public static T[,,] Expand<T>(T[] arr)
        {
        T[,,] arrExpanded = new T[TAILLEPLATEAU, TAILLEPLATEAU,TAILLEPLATEAU];
        for (int i = 0; i < TAILLEPLATEAU; i++)
        {
            for (int j = 0; j < TAILLEPLATEAU; j++)
                {
                for (int k=0; k<TAILLEPLATEAU;k++)
                    {
                        arrExpanded[k,j,i] = arr[k+TAILLEPLATEAU * (j+TAILLEPLATEAU * i)];
                    }
                }
        }
        return arrExpanded;
        }

    }
}
