using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SignalR.Hubs;

namespace SignalRTicTacToe.Web
{
    // TODO: Thread-safety
    public class TicTacToeServer
    {
        private static readonly Lazy<TicTacToeServer> _instance = new Lazy<TicTacToeServer>(() => new TicTacToeServer());

        public static TicTacToeServer Instance { get { return _instance.Value; } }

        private readonly TicTacToe _game;
        private readonly IList<string> _spectators = new List<string>();

        public TicTacToeServer()
        {
            _game = new TicTacToe();
            _game.GameCompleted += OnGameCompleted;
        }

        public string PlayerX { get; private set; }

        public string PlayerO { get; private set; }

        public void Connect(string clientId)
        {
            if (PlayerX == null)
            {
                PlayerX = clientId;
                SendMessage(clientId, "You are X's.");
            }
            else if (PlayerO == null)
            {
                PlayerO = clientId;
                SendMessage(clientId, "You are O's.");
            }
            else
            {
                _spectators.Add(clientId);
                UpdateSpectators();

                SendMessage(clientId, "You are a spectator.");
            }

            SendClientGridState(clientId);
        }

        private void SendClientGridState(string clientId)
        {
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    var squareState = _game.GetSquareState(r, c);
                    if (squareState != PlayerType.None)
                    {
                        UpdateSquare(clientId, squareState.ToString(), r, c);
                    }
                }
            }
        }

        private void SendMessage(string clientId, string message)
        {
            var clients = Hub.GetClients<TicTacToeHub>();
            clients[clientId].addMessage(message);
        }

        private void UpdateSpectators()
        {
            var clients = Hub.GetClients<TicTacToeHub>();
            clients.updateSpectators(_spectators.Count);
        }

        public void Disconnect(string clientId)
        {
            if (PlayerX == clientId)
            {
                PlayerX = _spectators.FirstOrDefault();

                if (PlayerX != null)
                {
                    _spectators.Remove(PlayerX);
                    SendMessage(PlayerX, "You are now X's.");
                }
            }
            else if (PlayerO == clientId)
            {
                PlayerO = _spectators.FirstOrDefault();

                if (PlayerO != null)
                {
                    _spectators.Remove(PlayerO);
                    SendMessage(PlayerO, "You are now O's.");
                }
            }
            else
            {
                _spectators.Remove(clientId);
            }
            UpdateSpectators();
        }

        public void PlaceMarkOn(string clientId, int row, int col)
        {
            if (clientId == PlayerX && _game.CurrentTurn == PlayerType.X)
            {
                _game.PlaceX(row, col);
                UpdateSquare("X", row, col);
            }
            else if (clientId == PlayerO && _game.CurrentTurn == PlayerType.O)
            {
                _game.PlaceO(row, col);
                UpdateSquare("O", row, col);
            }
        }

        private void UpdateSquare(string mark, int row, int col)
        {
            Hub.GetClients<TicTacToeHub>().updateSquare(mark, row, col);
        }

        private void UpdateSquare(string clientId, string mark, int row, int col)
        {
            Hub.GetClients<TicTacToeHub>()[clientId].updateSquare(mark, row, col);
        }

        private void OnGameCompleted(object sender)
        {
            BroadcastMessage(GetEndOfGameMessage());

            // TODO: Getting lazy.  Will clean this up later.
            Task.Factory.StartNew(() =>
                {
                    System.Threading.Thread.Sleep(10000);
                    _game.Reset();
                    Hub.GetClients<TicTacToeHub>().reset();
                });
        }

        private void BroadcastMessage(string message)
        {
            Hub.GetClients<TicTacToeHub>().addMessage(message);
        }

        private string GetEndOfGameMessage()
        {
            string message = null;

            switch (_game.Status)
            {
                case GameState.XWins:
                    message = "X Wins!";
                    break;
                case GameState.OWins:
                    message = "O Wins!";
                    break;
                case GameState.Draw:
                    message = "It's a draw.";
                    break;
            }
            return message;
        }
    }
}