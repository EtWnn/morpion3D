using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace regleJeu
{
    static class Plateau
    {

        public static int[,,] generationPlateau(int taillePlateau)
        {
            int[,,] plateau = new int[taillePlateau, taillePlateau, taillePlateau];
            for (int i = 0; i < taillePlateau; i++)
            {
                for (int j = 0; j < taillePlateau; j++)
                {
                    for (int k = 0; k < taillePlateau; k++)
                    {
                        plateau[i, j, k] = (int)Case.Vide;
                    }
                }
            }
            return plateau;
        }
    }
}
