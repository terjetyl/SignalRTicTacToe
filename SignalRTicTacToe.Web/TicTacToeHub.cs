using System;
using SignalR.Hubs;

namespace SignalRTicTacToe.Web
{
    public class TicTacToeHub : Hub, IDisconnect
    {
        private readonly TicTacToeServer server = TicTacToeServer.Instance;

        public void Connect()
        {
            server.Connect(Context.ClientId);
        }

        public void Disconnect()
        {
            server.Disconnect(Context.ClientId);
        }

        public void PlaceMarkOn(int row, int col)
        {
            server.PlaceMarkOn(Context.ClientId, row, col);
        }

        public char[,] GetBoardState()
        {
            throw new NotImplementedException();
        }
    }
}