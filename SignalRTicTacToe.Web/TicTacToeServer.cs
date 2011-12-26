using System;
using System.Threading;
using System.Threading.Tasks;

namespace SignalRTicTacToe.Web
{
    public class TicTacToeServer
    {
        private static readonly Lazy<TicTacToeServer> _instance = new Lazy<TicTacToeServer>(() => new TicTacToeServer());

        public static TicTacToeServer Instance
        {
            get { return _instance.Value;  }
        }

        private readonly ITicTacToeClientUpdater _clientUpdater;
        private readonly ITicTacToe _ticTacToeGame;
        private string _playerX;
        private string _playerO;
        private int _spectatorCount = 0;

        public TicTacToeServer()
            : this(new TicTacToe(), new TicTacToeSignalRClientUpdater())
        {
        }

        public TicTacToeServer(ITicTacToe ticTacToeGame, ITicTacToeClientUpdater clientUpdater)
        {
            _ticTacToeGame = ticTacToeGame;
            _ticTacToeGame.GameCompleted += OnGameCompleted;

            _clientUpdater = clientUpdater;
        }

        private bool IsPlayerXUnassigned
        {
            get { return _playerX == null; }
        }

        private bool IsPlayerOUnassigned
        {
            get { return _playerO == null; }
        }

        private void OnGameCompleted(object sender)
        {
            switch (_ticTacToeGame.Status)
            {
                case GameState.XWins:
                    _clientUpdater.BroadcastMessage("X Wins!");
                    break;
                case GameState.OWins:
                    _clientUpdater.BroadcastMessage("O Wins!");
                    break;
                case GameState.Draw:
                    _clientUpdater.BroadcastMessage("Game is a draw.");
                    break;
            }

            Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(5 * 1000);
                    _ticTacToeGame.Reset();
                    _clientUpdater.ResetGame();
                });
        }

        public void Connect(string clientId)
        {
            if (IsPlayerXUnassigned)
            {
                _playerX = clientId;
                _clientUpdater.SendMessage(clientId, "You are X's.");
            }
            else if (IsPlayerOUnassigned)
            {
                _playerO = clientId;
                _clientUpdater.SendMessage(clientId, "You are O's.");
            }
            else
            {
                _clientUpdater.SendMessage(clientId, "You are a spectator.");

                _spectatorCount++;
                _clientUpdater.UpdateSpectators(_spectatorCount);
            }
        }

        public void PlaceMark(string clientId, int row, int col)
        {
            if (IsPlayerX(clientId) && IsPlayerXTurn)
            {
                _ticTacToeGame.PlaceX(row, col);
                _clientUpdater.UpdateSquare(row, col, "X");
            }
            else if (IsPlayerO(clientId) && IsPlayerOTurn)
            {
                _ticTacToeGame.PlaceO(row, col);
                _clientUpdater.UpdateSquare(row, col, "O");
            }
        }

        private bool IsPlayerX(string clientId)
        {
            return clientId == _playerX;
        }

        private bool IsPlayerXTurn
        {
            get { return _ticTacToeGame.CurrentTurn == PlayerType.X; }
        }

        private bool IsPlayerO(string clientId)
        {
            return clientId == _playerO;
        }

        private bool IsPlayerOTurn
        {
            get { return _ticTacToeGame.CurrentTurn == PlayerType.O; }
        }
    }
}