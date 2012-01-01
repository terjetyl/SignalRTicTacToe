using System;
using System.Threading;
using System.Threading.Tasks;

namespace SignalRTicTacToe.Web.Code
{
    public class TicTacToeServer
    {
        private const int ResetWaitTimeInSeconds = 5;

        private static readonly Lazy<TicTacToeServer> _instance = new Lazy<TicTacToeServer>(() => new TicTacToeServer());

        public static TicTacToeServer Instance
        {
            get { return _instance.Value;  }
        }

        private readonly IClientManager _clientManager;
        private readonly ITicTacToeClientUpdater _clientUpdater;
        private readonly ITicTacToe _ticTacToeGame;

        public TicTacToeServer()
            : this(new TicTacToe(), new ClientManager(), new TicTacToeSignalRClientUpdater())
        {
        }

        public TicTacToeServer(ITicTacToe ticTacToeGame, IClientManager clientManager, ITicTacToeClientUpdater clientUpdater)
        {
            _ticTacToeGame = ticTacToeGame;
            _ticTacToeGame.GameCompleted += OnGameCompleted;

            _clientManager = clientManager;
            _clientManager.PlayerXAssigned += OnPlayerXAssigned;
            _clientManager.PlayerOAssigned += OnPlayerOAssigned;
            _clientManager.SpectatorAssigned += OnSpectatorAssigned;

            _clientUpdater = clientUpdater;
        }

        public void Connect(string clientId)
        {
            _clientManager.AssignRole(clientId);
        }

        public void PlaceMark(string clientId, int row, int col)
        {
            if (IsPlayerX(clientId) && IsPlayerXTurn())
            {
                _ticTacToeGame.PlaceX(row, col);
                _clientUpdater.UpdateSquare(row, col, "X");
            }
            else if (IsPlayerO(clientId) && IsPlayerOTurn())
            {
                _ticTacToeGame.PlaceO(row, col);
                _clientUpdater.UpdateSquare(row, col, "O");
            }
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
            ResetGameAfterDelay();
        }

        private void ResetGameAfterDelay()
        {
            Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(ResetWaitTimeInSeconds*1000);
                    _ticTacToeGame.Reset();
                    _clientUpdater.ResetGame();
                });
        }

        private void OnPlayerXAssigned(object sender, string clientId)
        {
            _clientUpdater.SendMessage(clientId, "You are X's.");
        }

        private void OnPlayerOAssigned(object sender, string clientId)
        {
            _clientUpdater.SendMessage(clientId, "You are O's.");
        }

        private void OnSpectatorAssigned(object sender, string clientId)
        {
            _clientUpdater.SendMessage(clientId, "You are a spectator.");
            _clientUpdater.UpdateSpectators(_clientManager.SpectatorCount);
        }

        private bool IsPlayerX(string clientId)
        {
            return _clientManager.GetClientRole(clientId) == ClientRole.PlayerX;
        }

        private bool IsPlayerO(string clientId)
        {
            return _clientManager.GetClientRole(clientId) == ClientRole.PlayerO;
        }

        private bool IsPlayerXTurn()
        {
            return _ticTacToeGame.CurrentTurn == PlayerType.X;
        }

        private bool IsPlayerOTurn()
        {
            return _ticTacToeGame.CurrentTurn == PlayerType.O;
        }

        public void Disconnect(string clientId)
        {
            ClientRole clientRole = _clientManager.GetClientRole(clientId);

            _clientManager.Unassign(clientId);

            switch (clientRole)
            {
                case ClientRole.PlayerO:
                case ClientRole.PlayerX:
                    _ticTacToeGame.Reset();
                    _clientUpdater.ResetGame();
                    break;
                case ClientRole.Spectator:
                    _clientUpdater.UpdateSpectators(_clientManager.SpectatorCount);
                    break;
            }
        }
    }
}