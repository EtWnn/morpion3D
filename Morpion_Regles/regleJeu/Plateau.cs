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
        public static void afficher(int[,,] tab)
        {
            int size = tab.GetLength(0);
            string aff = "";
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        aff += tab[x, y, z].ToString() + '\t';
                    }
                    aff += '\n';
                }
                aff += "\n\n";
            }
            Console.WriteLine(aff);
        }

        public static T[] Flatten<T>(T[,,] arr, int size)
        {
        T[] arrFlattened = new T[size * size * size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
                {
                for (int k=0; k<size;k++)
                    {
                        var test = arr[k, j,i];
                        arrFlattened[k+size * (j+size * i)] = arr[k, j, i];
                    }
                }
        }
        return arrFlattened;
        }
        
        public static T[,,] Expand<T>(T[] arr, int size)
        {
        T[,,] arrExpanded = new T[size, size,size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
                {
                for (int k=0; k<size;k++)
                    {
                        arrExpanded[k,j,i] = arr[k+size * (j+size * i)];
                    }
                }
        }
        return arrExpanded;
        }

    }
}
