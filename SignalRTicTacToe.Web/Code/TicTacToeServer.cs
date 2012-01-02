using System;
using System.Threading;
using System.Threading.Tasks;

namespace SignalRTicTacToe.Web.Code
{
    // TODO: Uh, thread-safety?
    public class TicTacToeServer
    {
        private const int ResetWaitTimeInSeconds = 5;

        private static readonly Lazy<TicTacToeServer> _instance = new Lazy<TicTacToeServer>(CreateDefault);

        public static TicTacToeServer Instance
        {
            get { return _instance.Value;  }
        }

        private readonly IClientManager _clientManager;
        private readonly ITicTacToeClientUpdater _clientUpdater;
        private readonly ITicTacToe _ticTacToeGame;

        public TicTacToeServer(ITicTacToe ticTacToeGame, IClientManager clientManager, ITicTacToeClientUpdater clientUpdater)
        {
            _ticTacToeGame = ticTacToeGame;
            _ticTacToeGame.GameCompleted += OnGameCompleted;

            _clientManager = clientManager;
            _clientManager.ClientRoleAssigned += OnClientRoleAssigned;

            _clientUpdater = clientUpdater;
        }

        public static TicTacToeServer CreateDefault()
        {
            return new TicTacToeServer(new TicTacToe(), new ClientManager(), new TicTacToeSignalRClientUpdater());
        }

        public void Connect(string clientId)
        {
            SendAllSquaresState(clientId);
            _clientManager.AssignToNextAvailableRole(clientId);
        }

        private void SendAllSquaresState(string clientId)
        {
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    var square = _ticTacToeGame.GetSquareState(r, c);
                    if (square != PlayerType.None)
                    {
                        _clientUpdater.UpdateSquare(r, c, square == PlayerType.X ? "X" : "O", clientId);
                    }
                }
            }
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

        protected void OnGameCompleted(object sender)
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

            // TODO: Cover with unit tests.
            ResetGameAfterDelay();
        }

        private void ResetGameAfterDelay()
        {
            Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(ResetWaitTimeInSeconds*1000);
                    if (_ticTacToeGame.Status == GameState.XWins || _ticTacToeGame.Status == GameState.OWins)
                    {
                        _clientManager.RotateRolesKeepingAsPlayer(_ticTacToeGame.Status == GameState.XWins ? ClientRole.PlayerX : ClientRole.PlayerO);
                    }
                    ResetGame();
                });
        }

        private void UpdateSpectatorCount()
        {
            _clientUpdater.UpdateSpectators(_clientManager.SpectatorCount);
        }

        private void ResetGame()
        {
            _ticTacToeGame.Reset();
            _clientUpdater.ResetGame();
        }

        protected void OnClientRoleAssigned(object sender, ClientRoleAssignment assignment)
        {
            switch (assignment.Role)
            {
                case ClientRole.PlayerX:
                    ResetGame();
                    _clientUpdater.BroadcastMessage("Player X is ready.");
                    break;
                case ClientRole.PlayerO:
                    ResetGame();
                    _clientUpdater.BroadcastMessage("Player O is ready.");
                    break;
                case ClientRole.Spectator:
                    UpdateSpectatorCount();
                    break;
            } 
            _clientUpdater.SendMessage(assignment.ClientId, GetClientAssigmentMessage(assignment.Role));
        }

        private string GetClientAssigmentMessage(ClientRole role)
        {
            switch (role)
            {
                case ClientRole.PlayerX:
                    return "You are X's.";
                case ClientRole.PlayerO:
                    return "You are O's.";
                case ClientRole.Spectator:
                    return "You are a spectator.";
                default:
                    throw new InvalidOperationException(String.Format("There is no assignment message for role '{0}'.", role.ToString()));
            }
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

            if (clientRole == ClientRole.PlayerX || clientRole == ClientRole.PlayerO)
            {
                _clientUpdater.BroadcastMessage(clientRole == ClientRole.PlayerX ? "Player X has left." : "Player O has left.");
            }

            _clientManager.RemoveClient(clientId);

            if (clientRole == ClientRole.Spectator)
            {
                UpdateSpectatorCount();
            }
        }
    }
}