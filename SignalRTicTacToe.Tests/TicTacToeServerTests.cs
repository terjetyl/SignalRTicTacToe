using System;
using NUnit.Framework;
using SignalRTicTacToe.Web;

namespace SignalRTicTacToe.Tests
{
    // Connect
        // When first connects, assign as x
        // When second connects, assign as o
        // when third or later, assign as spectator
        // When user is assigned as X, notdify user
        // When user is assigned as O, notify user
        // When user is assigned as Spectator, notify user
        // If X is unassigned, assign as x
        // If O is unassigned, assign as o
        // Start game when X and O have been assigned
        // Notify users when game starts
    // Disconnect
        // When X disconnects, assign next spectator as X
        // When O disconnects, assign next spectator as O
        // When X disconnects, reset game
        // When O, disconnect, reset game
    // PlaceMark
        // Do not allow if game not started
        // Ignore if not their turn

    // When game ends, reset after 10 seconds
    // When game ends, winner swaps with other player
    // When game ends, loser is replaced by spectator

    [TestFixture]
    public class TicTacToeServerTests
    {
        [Test]
        public void Murloc()
        {
            TicTacToeServer server = new TicTacToeServer();
            server.Connect();
            server.PlayerXConnected += (sender) => Assert.Pass();
            Assert.Fail();
        }
    }

    public class TicTacToeServer
    {
        public void Connect()
        {
            throw new System.NotImplementedException();
        }

        public event Action<object> PlayerXConnected;
    }
}
