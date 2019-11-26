using System;
using System.Collections.Generic;
using System.Numerics;
using System.Xml.Serialization;

namespace MyClient.ModelGame
{
    // ---- Enums ---

    /// <summary>
    /// Enumeration of the values/entities which can filled a cubelet of the GameBoard matrix
    /// </summary>
    public enum Cell
    {
        Empty = 0,
        Player1Pattern = 1,
        Player2Pattern = 2,
        HighlightPlayer1 = 3,
        HighlightPlayer2 = 4
    };

    /// <summary>
    /// Enumeration of the status of the Game
    /// </summary>
    public enum GameMode
    {
        /// <summary> In Game : Player 1 turn to play </summary>
        Player1,
        /// <summary> In Game : Player 2 turn to play </summary>
        Player2,
        /// <summary> Game ended : Player 1 won </summary>
        Player1Won,
        /// <summary> Game ended : Player 2 won </summary>
        Player2Won,
        /// <summary> Game ended : None won </summary>
        NoneWon,
    }

    /// <summary>
    /// <para>Handle the whole game party</para>
    /// </summary>
    [Serializable]
    public class Game
    {

        // ---- Public fields/properties ----

        /// <summary> See <see cref="GameMode"/> enum </summary>
        public GameMode Mode { get; set; }

        /// <summary> True if the Game has ended, false otherwise </summary>
        public bool EndGame { get; set; } = false;

        /// <summary>
        /// <para>Matrix representing the GameBoard of the tic-tac-toe</para>
        /// <para>The matrix can be filled with the values of the <see cref="Cell"/> enum</para>
        /// </summary>
        [XmlIgnore]
        public int[,,] GameBoardMatrix { get; set; }

        /// <summary>
        /// <para>Transform the matrix of the GameBoard in a list</para>
        /// <para>Needed for the serialisation of the object</para>
        /// </summary>
        [XmlArray("GameBoardMatrix")]
        public int[] GameBoardMatrixDto
        {
            get { return GameBoard.Flatten(GameBoardMatrix, GAMEBOARDSIZE); }
            set { GameBoardMatrix = GameBoard.Expand(value, GAMEBOARDSIZE); }
        }

        /// <summary> Client Id assigned to Player 1 </summary>
        public int IdPlayer1 { get; set; }
        /// <summary> Client Id assigned to Player 2 </summary>
        public int IdPlayer2 { get; set; }

        // ---- Private fields/properties ----

        private const int GAMEBOARDSIZE = 3; // Dimension du plateau
        /// <summary> List of positions played by Player 1 since the begining of the game </summary>
        private List<Vector3> PlayedPositionsPlayer1 = new List<Vector3>();
        /// <summary> List of positions played by Player 2 since the begining of the game </summary>
        private List<Vector3> PlayedPositionsPlayer2 = new List<Vector3>();

        // ---- Public methods ----

        /// <summary>
        /// <para>Initiate a Game instance</para>
        /// <para>Create an empty <see cref="GameBoardMatrix"/></para>
        /// <para>Set the <see cref="GameMode"/> as Player 1 turn to play</para>
        /// </summary>
        public Game()
        {
            GameBoardMatrix = GameBoard.gameBoardGeneration(GAMEBOARDSIZE);
            Mode = GameMode.Player1;
        }


        /// <summary>
        /// <para>Update the tic-tac-toe gameboard matrix <see cref="GameBoardMatrix"/>according to a player's action</para>
        /// <para>A player's action is the position described described by the <see cref="Vector3"/> <paramref name="playedPosition"/> that he wants to played</para>
        /// </summary>
        /// <param name="playedPosition"></param>
        /// <param name="idClient"></param>
        public void Play(Vector3 playedPosition, int idClient)
        {
            if (GameBoardMatrix[(int)playedPosition.X, (int)playedPosition.Y, (int)playedPosition.Z] == (int)Cell.Empty && CanPlay(idClient))
            {
                List<Vector3> WinningPositionsList = new List<Vector3>();
                WinningPositionsList = WinningCombinaison(playedPosition);
                if (WinningPositionsList.Count == 0)
                {
                    PlaceToken(playedPosition, false);
                }
                else
                {
                    foreach (Vector3 position in WinningPositionsList)
                    {
                        PlaceToken(position, true);
                    }
                }
                CalculationEndGame();
            }
        }

        /// <summary>
        /// Randomly assign a client ID to a player number
        /// </summary>
        /// <param name="idClient1"></param>
        /// <param name="idClient2"></param>
        public void SpecifyPlayersID(int idClient1, int idClient2)
        {
            Random rnd = new Random();

            if (rnd.Next(0, 2) < 1)
            {
                IdPlayer1 = idClient1;
                IdPlayer2 = idClient2;
            }
            else
            {
                IdPlayer1 = idClient2;
                IdPlayer2 = idClient1;
            }
        }

        // ---- Public methods ----

        /// <summary>
        /// Return true if it is the turn of client which the id in argument to play, false otherwise
        /// </summary>
        /// <param name="idCLient"></param>
        /// <returns></returns>
        private bool CanPlay(int idCLient)
        {
            if (idCLient == IdPlayer1 && Mode == GameMode.Player1)
            {
                return true;
            }
            else if (idCLient == IdPlayer2 && Mode == GameMode.Player2)
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// Update the <see cref="GameMode"/> to NoneWon if all the cubelets are filled
        /// </summary>
        private void CalculationEndGame()
        {
            if (PlayedPositionsPlayer1.Count + PlayedPositionsPlayer2.Count == GAMEBOARDSIZE * GAMEBOARDSIZE * GAMEBOARDSIZE)
            {
                EndGame = true;
                Mode = GameMode.NoneWon;
            }
        }

        /// <summary>
        /// Return a list of positions which forms a winning combinaison
        /// </summary>
        /// <param name="newPosition"></param>
        /// <returns></returns>
        private List<Vector3> WinningCombinaison(Vector3 newPosition)
        {
            bool winningCombinaison = false;
            List<Vector3> playedPositions = new List<Vector3>();
            List<Vector3> winningPositionsList = new List<Vector3>();

            if (Mode == GameMode.Player1)
            {
                playedPositions = PlayedPositionsPlayer1;
            }
            else if (Mode == GameMode.Player2)
            {
                playedPositions = PlayedPositionsPlayer2;
            }

            if (playedPositions.Count != 0)
            {
                for (int i = 0; i < playedPositions.Count; i++)
                {
                    for (int j = i + 1; j < playedPositions.Count; j++)
                    {
                        winningCombinaison = Alignment(newPosition, playedPositions[i], playedPositions[j]);
                        if (winningCombinaison)
                        {
                            winningPositionsList.Add(newPosition);
                            winningPositionsList.Add(playedPositions[i]);
                            winningPositionsList.Add(playedPositions[j]);
                            EndGame = true;
                            break;
                        }
                    }
                    if (winningCombinaison)
                    {
                        break;
                    }
                }
            }
            return winningPositionsList;
        }

        /// <summary>
        /// Add the last token played in the tic-tac-toe matrix <see cref="GameBoardMatrix"/> with the correct pattern and add its position to the list of played position by the player
        /// </summary>
        /// <param name="position"></param>
        /// <param name="winningCombinaison"></param>
        private void PlaceToken(Vector3 position, bool winningCombinaison)
        {
            if (Mode == GameMode.Player1 || Mode == GameMode.Player1Won)
            {
                if (winningCombinaison)
                {
                    GameBoardMatrix[(int)position.X, (int)position.Y, (int)position.Z] = (int)Cell.HighlightPlayer1;
                    Mode = GameMode.Player1Won;
                }
                else
                {
                    GameBoardMatrix[(int)position.X, (int)position.Y, (int)position.Z] = (int)Cell.Player1Pattern;
                    Mode = GameMode.Player2;
                }
                PlayedPositionsPlayer1.Add(position);
            }
            else if (Mode == GameMode.Player2 || Mode == GameMode.Player2Won)
            {
                if (winningCombinaison)
                {
                    GameBoardMatrix[(int)position.X, (int)position.Y, (int)position.Z] = (int)Cell.HighlightPlayer2;
                    Mode = GameMode.Player2Won;
                }
                else
                {
                    GameBoardMatrix[(int)position.X, (int)position.Y, (int)position.Z] = (int)Cell.Player2Pattern;
                    Mode = GameMode.Player1;
                }
                PlayedPositionsPlayer2.Add(position);
            }
        }

        /// <summary>
        /// Return true if the tree <see cref="Vector3"/> are aligned, false otherwise
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="point3"></param>
        private Boolean Alignment(Vector3 point1, Vector3 point2, Vector3 point3)
        {
            Boolean hasWin = false;
            Vector3 vector1 = point1 - point2;
            Vector3 vector2 = point1 - point3;
            Vector3 vectorialProduct = Vector3.Cross(vector1, vector2);
            Vector3 zeroVector = new Vector3(0, 0, 0);
            if (vectorialProduct == zeroVector)
            {
                hasWin = true;
            }
            return hasWin;
        }


    }
}
