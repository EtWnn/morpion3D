using System;

namespace Serveur.ModelGame
{
    static class GameBoard
    {

        public static int[,,] gameBoardGeneration(int gameBoardSize)
        {
            int[,,] plateau = new int[gameBoardSize, gameBoardSize, gameBoardSize];
            for (int i = 0; i < gameBoardSize; i++)
            {
                for (int j = 0; j < gameBoardSize; j++)
                {
                    for (int k = 0; k < gameBoardSize; k++)
                    {
                        plateau[i, j, k] = (int)Cell.Empty;
                    }
                }
            }
            return plateau;
        }
        public static void display(int[,,] tab)
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
                    for (int k = 0; k < size; k++)
                    {
                        var test = arr[k, j, i];
                        arrFlattened[k + size * (j + size * i)] = arr[k, j, i];
                    }
                }
            }
            return arrFlattened;
        }

        public static T[,,] Expand<T>(T[] arr, int size)
        {
            T[,,] arrExpanded = new T[size, size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    for (int k = 0; k < size; k++)
                    {
                        arrExpanded[k, j, i] = arr[k + size * (j + size * i)];
                    }
                }
            }
            return arrExpanded;
        }

    }
}
