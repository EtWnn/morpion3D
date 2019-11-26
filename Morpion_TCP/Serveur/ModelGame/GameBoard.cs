using System;

namespace Serveur.ModelGame
{
    /// <summary>
    /// <para>Group the fonctionnalities of the GameBoard</para>
    /// </summary>
    static class GameBoard
    {
        // ---- Static Public methods ----

        /// <summary>
        /// <para>Initiate the GameBoard</para>
        /// <para>Matrix of dimension <paramref name="gameBoardSize"/>^3 </para>
        /// <para>Filled with <see cref="Cell.Empty"/> </para>
        /// </summary>
        /// <param name="gameBoardSize"></param>
        /// <returns></returns>
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
        
        /// <summary>
        /// Transform a matrix of dimension <paramref name="size"/>^3 in a list of dimension <paramref name="size"/>*3
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="size"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Transform a list of dimension <paramref name="size"/>*3 in a matrix of dimension <paramref name="size"/>^3 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="size"></param>
        /// <returns></returns>
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
