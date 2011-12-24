using System;
using SignalR.Hubs;

namespace SignalRTicTacToe.Web
{
    public class TicTacToeHub : Hub, IDisconnect
    {
        public void Connect()
        {
        }

        public void Disconnect()
        {
        }

        public void PlaceMarkOn(int row, int col)
        {
        }
    }
}