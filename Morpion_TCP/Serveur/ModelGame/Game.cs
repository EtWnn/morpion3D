using System;
using System.Collections.Generic;
using System.Numerics;
using System.Xml.Serialization;

namespace Serveur.ModelGame
{

    public enum Cell
    {
        Empty = 0,
        Player1Pattern = 1,
        Player2Pattern = 2,
        HighlightPlayer1 = 3,
        HighlightPlayer2 = 4
    };

    public enum GameMode
    {
        Player1,
        Player2,
        Player1Won,
        Player2Won,
        NoneWon,
    }

    [Serializable]
    public class Game
    {

        public GameMode Mode { get; set; } //Joueur qui peut jouer 
        public bool EndGame { get; set; } = false; //booleen pour declarer la fin de la partie

        [XmlIgnore]
        public int[,,] GameBoardMatrix { get; set; } //La matrice du plateau

        [XmlArray("GameBoardMatrix")]
        public int[] GameBoardMatrixDto
        {
            get { return GameBoard.Flatten(GameBoardMatrix, GAMEBOARDSIZE); }
            set { GameBoardMatrix = GameBoard.Expand(value, GAMEBOARDSIZE); }
        }

        private const int GAMEBOARDSIZE = 3; // Dimension du plateau
        private List<Vector3> PlayedPositionsPlayer1 = new List<Vector3>();
        private List<Vector3> PlayedPositionsPlayer2 = new List<Vector3>();
        public int IdPlayer1 { get; set; }
        public int IdPlayer2 { get; set; }

        // Constructeur par defaut sans parametres pour le serializeur XML
        public Game()
        {
            GameBoardMatrix = GameBoard.gameBoardGeneration(GAMEBOARDSIZE);
            Mode = GameMode.Player1;
        }
        /*public Game( int idClient1, int idClient2 )
        {
            Random rnd = new Random();
            GameBoardMatrix = GameBoard.gameBoardGeneration(GAMEBOARDSIZE);
            Mode = GameMode.Player1;
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
            
        }*/

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

        //Methode pour mettre a jour le morpion en fonction de l'action d un joueur
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
            else
            {
                // a definir
            }
        }

        private void CalculationEndGame()
        {
            if (PlayedPositionsPlayer1.Count + PlayedPositionsPlayer2.Count == GAMEBOARDSIZE * GAMEBOARDSIZE * GAMEBOARDSIZE)
            {
                EndGame = true;
                Mode = GameMode.NoneWon;
            }
        }

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
