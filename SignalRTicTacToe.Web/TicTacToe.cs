using System;

namespace SignalRTicTacToe.Web
{
    public enum GameState
    {
        InProgress,
        XWins,
        OWins,
        Draw
    }

    public enum PlayerType
    {
        None,
        X,
        O
    }

    public class OutOfTurnException : InvalidOperationException
    {
    }

    public interface ITicTacToe
    {
        GameState Status { get; }
        PlayerType CurrentTurn { get; }

        event TicTacToe.GameCompletedDelegate GameCompleted;
        
        PlayerType GetSquareState(int row, int col);
        void PlaceX(int row, int col);
        void PlaceO(int row, int col);
        void Reset();
    }

    public class TicTacToe : ITicTacToe
    {
        private readonly PlayerType[,] _squares = new PlayerType[3, 3];

        public delegate void GameCompletedDelegate(object sender);

        public TicTacToe()
        {
            Reset();
            // Default anonymous delegate, so we do not have to check for null when invoking the event.
            GameCompleted += (sender) => { };
        }

        public GameState Status { get; private set; }

        public PlayerType CurrentTurn { get; private set; }

        public event GameCompletedDelegate GameCompleted;

        public PlayerType GetSquareState(int row, int col)
        {
            return _squares[row, col];
        }

        public void PlaceX(int row, int col)
        {
            PlaceMark(PlayerType.X, row, col);
        }

        public void PlaceO(int row, int col)
        {
            PlaceMark(PlayerType.O, row, col);
        }

        private void PlaceMark(PlayerType mark, int row, int col)
        {
            if (CurrentTurn != mark)
            {
                throw new OutOfTurnException();
            }

            if (row < 0 || row > 2)
            {
                throw new ArgumentOutOfRangeException("row");
            }
            if (col < 0 || col > 2)
            {
                throw new ArgumentOutOfRangeException("col");
            }

            if (_squares[row, col] != PlayerType.None)
            {
                throw new InvalidOperationException("A mark has already been placed at that position.");
            }

            _squares[row, col] = mark;

            CheckIfLastMoveEndsGame(row, col);
            ChangeTurns();
        }

        private void CheckIfLastMoveEndsGame(int row, int col)
        {
            if (IsThreeInARow(row) || IsThreeInAColumn(col) || IsThreeInADiagonal())
            {
                Status = (CurrentTurn == PlayerType.X) ? GameState.XWins : GameState.OWins;
                GameCompleted(this);
            }
            else if (IsDraw())
            {
                Status = GameState.Draw;
                GameCompleted(this);
            }
        }

        private bool IsThreeInARow(int row)
        {
            return _squares[row, 0] == _squares[row, 1] && _squares[row, 1] == _squares[row, 2];
        }

        private bool IsThreeInAColumn(int col)
        {
            return _squares[0, col] == _squares[1, col] && _squares[1, col] == _squares[2, col];
        }

        private bool IsThreeInADiagonal()
        {
            return _squares[1, 1] != PlayerType.None
                && ((_squares[0, 0] == _squares[1, 1] && _squares[1, 1] == _squares[2, 2])
                    || (_squares[0, 2] == _squares[1, 1] && _squares[1, 1] == _squares[2, 0]));
        }

        private bool IsDraw()
        {
            bool draw = true;
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    if (_squares[r, c] != PlayerType.None)
                        continue;
                    
                    draw = false;
                    break;
                }
                if (!draw)
                    break;
            }
            return draw;
        }

        private void ChangeTurns()
        {
            if (Status == GameState.InProgress)
            {
                switch (CurrentTurn)
                {
                    case PlayerType.X:
                        CurrentTurn = PlayerType.O;
                        break;
                    case PlayerType.O:
                        CurrentTurn = PlayerType.X;
                        break;
                }
            }
            else
            {
                CurrentTurn = PlayerType.None;
            }
        }

        public void Reset()
        {
            ClearSquares();
            CurrentTurn = PlayerType.X;
            Status = GameState.InProgress;
        }

        private void ClearSquares()
        {
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    _squares[r, c] = PlayerType.None;
                }
            }
        }
    }
}