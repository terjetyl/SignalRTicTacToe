namespace SignalRTicTacToe.Web
{
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
}