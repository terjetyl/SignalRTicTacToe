namespace SignalRTicTacToe.Web.Code
{
    public interface ITicTacToeClientUpdater
    {
        void BroadcastMessage(string message);
        void ResetGame();
        void SendMessage(string clientId, string message);
        void UpdateSpectators(int count);
        void UpdateSquare(int row, int col, string mark);
    }
}