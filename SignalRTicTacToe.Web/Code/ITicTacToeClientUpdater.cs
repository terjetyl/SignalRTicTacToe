namespace SignalRTicTacToe.Web.Code
{
    /// <summary>
    /// Facility for sending updates to the clients.
    /// </summary>
    public interface ITicTacToeClientUpdater
    {
        void BroadcastMessage(string message);
        void ResetGame();
        void SendMessage(string clientId, string message);
        void UpdateSpectators(int count);
        void UpdateSquare(int row, int col, string mark);
    }
}