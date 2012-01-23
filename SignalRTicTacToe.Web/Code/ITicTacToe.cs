using System;

namespace SignalRTicTacToe.Web.Code
{
    public class GameCompletedEventArgs : EventArgs
    {
        public GameCompletedEventArgs(GameState gameState)
        {
            GameState = gameState;
        }

        public GameState GameState { get; set; }
    }

    public interface ITicTacToe
    {
        GameState Status { get; }
        PlayerType CurrentTurn { get; }

        event EventHandler<GameCompletedEventArgs> GameCompleted;
        
        PlayerType GetSquareState(int row, int col);
        void PlaceX(int row, int col);
        void PlaceO(int row, int col);
        void Reset();
    }
}