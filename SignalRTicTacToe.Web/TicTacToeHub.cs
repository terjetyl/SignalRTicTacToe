using System;
using SignalR.Hubs;

namespace SignalRTicTacToe.Web
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
        }

        public void PlaceMarkOn(int row, int col)
        {
            _server.PlaceMark(Context.ClientId, row, col);
        }
    }
}