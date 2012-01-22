using System;

namespace SignalRTicTacToe.Web.Code
{
    public interface ITicTacToe
    {
        GameState Status { get; }
        PlayerType CurrentTurn { get; }

        event EventHandler GameCompleted;
        
        PlayerType GetSquareState(int row, int col);
        void PlaceX(int row, int col);
        void PlaceO(int row, int col);
        void Reset();
    }
}