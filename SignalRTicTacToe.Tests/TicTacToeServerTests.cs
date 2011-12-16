using NUnit.Framework;
using SignalRTicTacToe.Web;

namespace SignalRTicTacToe.Tests
{
    [TestFixture]
    public class TicTacToeServerTests
    {
        private int clientCount;

        private TicTacToeServer server;

        // Games starts when player O arrives        
        // Game ends when player X or O wins or draws.

        private void Connect()
        {
            server.Connect((++clientCount).ToString());
        }

        private void ConnectMany(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Connect();
            }
        }

        [SetUp]
        public void SetUp()
        {
            clientCount = 0;
            server = new TicTacToeServer();
        }

        [Test]
        public void FirstUserIsAssignedAsPlayerX()
        {
            Connect();

            Assert.AreEqual("1", server.PlayerX);
        }

        [Test]
        public void SecondUserIsAssignedAsPlayerO()
        {
            ConnectMany(2);

            Assert.AreEqual("2", server.PlayerO);
        }

        [Test]
        public void WhenPlayerXDisconnectsReplaceWithFirstSpectator()
        {
            ConnectMany(3);

            server.Disconnect("1");

            Assert.AreEqual("3", server.PlayerX);
        }

        [Test]
        public void WhenPlayerODisconnectsReplaceWithFirstSpectator()
        {
            ConnectMany(3);

            server.Disconnect("2");

            Assert.AreEqual("3", server.PlayerO);
        }
    }
}
