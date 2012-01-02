using SignalR.Hubs;

namespace SignalRTicTacToe.Web.Code
{
    public class TicTacToeSignalRClientUpdater : ITicTacToeClientUpdater
    {
        private static dynamic Clients
        {
            get { return Hub.GetClients<TicTacToeHub>(); }
        }

        public void BroadcastMessage(string message)
        {
            Clients.addMessage(message);
        }

        public void ResetGame()
        {
            Clients.reset();
        }

        public void SendMessage(string clientId, string message)
        {
            Clients[clientId].addMessage(message);
        }

        public void UpdateSpectators(int count)
        {
            Clients.updateSpectators(count);
        }

        public void UpdateSquare(int row, int col, string mark)
        {
            Clients.updateSquare(mark, row, col);
        }

        public void UpdateSquare(int row, int col, string mark, string clientId)
        {
            Clients[clientId].updateSquare(mark, row, col);
        }
    }
}