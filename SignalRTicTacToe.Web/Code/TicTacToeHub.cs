using SignalR.Hubs;

namespace SignalRTicTacToe.Web.Code
{
    public class TicTacToeHub : Hub, IDisconnect
    {
        private readonly TicTacToeServer _server;

        public TicTacToeHub()
        {
            _server = TicTacToeServer.Instance;
        }

        public void Connect()
        {
            _server.Connect(Context.ClientId);
        }

        public void Disconnect()
        {
            _server.Disconnect(Context.ClientId);
        }

        public void PlaceMarkOn(int row, int col)
        {
            _server.PlaceMark(Context.ClientId, row, col);
        }
    }
}