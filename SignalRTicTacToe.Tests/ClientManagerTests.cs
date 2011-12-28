using NUnit.Framework;
using SignalRTicTacToe.Web.Code;

namespace SignalRTicTacToe.Tests
{
    [TestFixture]
    public class ClientManagerTests
    {
        private const string Client1 = "1";
        private const string Client2 = "2";
        private const string Client3 = "3";

        protected ClientManager clientManager;

        private void AssignRoles(params string[] clientIds)
        {
            foreach (var clientId in clientIds)
            {
                clientManager.AssignRole(clientId);
            }
        }

        [SetUp]
        public void SetUp()
        {
            clientManager = new ClientManager();
        }

        [Test]
        public void WhenFirstClientCallsAssignRole_AssignToPlayerXRole()
        {
            AssignRoles(Client1);
            Assert.AreEqual(ClientRole.PlayerX, clientManager.GetClientRole(Client1));
        }

        [Test]
        public void TestPlayerXAssignedEventIsInvoked()
        {
            string playerX = null;
            clientManager.PlayerXAssigned += (sender, id) => playerX = id;

            AssignRoles(Client1);

            Assert.AreEqual(Client1, playerX);
        }

        [Test]
        public void WhenSecondClientCallsAssignRole_AssignToPlayerORole()
        {
            AssignRoles(Client1, Client2);
            Assert.AreEqual(ClientRole.PlayerO, clientManager.GetClientRole(Client2));
        }

        [Test]
        public void TestPlayerOAssignedEventIsInvoked()
        {
            string playerO = null;
            clientManager.PlayerOAssigned += (sender, id) => playerO = id;

            AssignRoles(Client1, Client2);

            Assert.AreEqual(Client2, playerO);
        }

        [Test]
        public void WhenThirdClientCallsAssignRole_AssignToSpectatorRole()
        {
            AssignRoles(Client1, Client2, Client3);
            Assert.AreEqual(ClientRole.Spectator, clientManager.GetClientRole(Client3));
        }

        [Test]
        public void TestSpectatorAssignedIsInvoked()
        {
            string spectatorId = null;
            clientManager.SpectatorAssigned += (sender, id) => spectatorId = id;

            AssignRoles(Client1, Client2, Client3);
            
            Assert.AreEqual(Client3, spectatorId);
        }
    }
}
