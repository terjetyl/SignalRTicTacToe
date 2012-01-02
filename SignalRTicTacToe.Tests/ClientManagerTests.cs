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

        private void AssignNextAvailableRoles(params string[] clientIds)
        {
            foreach (var clientId in clientIds)
            {
                clientManager.AssignToNextAvailableRole(clientId);
            }
        }

        [SetUp]
        public void SetUp()
        {
            clientManager = new ClientManager();
        }

        [Test]
        public void WhenPlayerXHasNotBeenAssigned_AssignToPlayerXRole()
        {
            AssignNextAvailableRoles(Client1);
            Assert.AreEqual(ClientRole.PlayerX, clientManager.GetClientRole(Client1));
        }

        [Test]
        public void WhenPlayerXIsAssigned_ClientRoleAssignedEventIsInvoked()
        {
            ClientRoleAssignment roleAssignment = null;
            clientManager.ClientRoleAssigned += (sender, assignment) => roleAssignment = assignment;

            AssignNextAvailableRoles(Client1);

            Assert.AreEqual(Client1, roleAssignment.ClientId);
            Assert.AreEqual(ClientRole.PlayerX, roleAssignment.Role);
        }

        [Test]
        public void WhenSecondClientCallsAssignRole_AssignToPlayerORole()
        {
            AssignNextAvailableRoles(Client1, Client2);
            Assert.AreEqual(ClientRole.PlayerO, clientManager.GetClientRole(Client2));
        }

        [Test]
        public void WhenPlayerOIsAssigned_ClientRoleAssignedEventIsInvoked()
        {
            ClientRoleAssignment lastRoleAssignment = null;
            clientManager.ClientRoleAssigned += (sender, assignment) => lastRoleAssignment = assignment;

            AssignNextAvailableRoles("FirstClient", Client2);

            Assert.AreEqual(Client2, lastRoleAssignment.ClientId);
            Assert.AreEqual(ClientRole.PlayerO, lastRoleAssignment.Role);
        }

        [Test]
        public void WhenThirdClientCallsAssignRole_AssignToSpectatorRole()
        {
            AssignNextAvailableRoles(Client1, Client2, Client3);
            Assert.AreEqual(ClientRole.Spectator, clientManager.GetClientRole(Client3));
        }

        [Test]
        public void WhenSpectatorIsAssigned_ClientRoleAssignedEventIsInvoked()
        {
            ClientRoleAssignment lastRoleAssignment = null;
            clientManager.ClientRoleAssigned += (sender, assignment) => lastRoleAssignment = assignment;

            AssignNextAvailableRoles("FirstClient", "SecondClient", Client3);

            Assert.AreEqual(Client3, lastRoleAssignment.ClientId);
            Assert.AreEqual(ClientRole.Spectator, lastRoleAssignment.Role);
        }

        [Test]
        public void TestSpectatorCount()
        {
            AssignNextAvailableRoles(Client1, Client2, Client3);

            Assert.AreEqual(1, clientManager.SpectatorCount);
        }

        [Test]
        public void WhenPlayerXIsUnassigned_ClientShouldNoLongerHaveARole()
        {
            AssignNextAvailableRoles(Client1);

            clientManager.RemoveClient(Client1);

            Assert.AreEqual(ClientRole.None, clientManager.GetClientRole(Client1));
        }

        [Test]
        public void WhenPlayerXIsUnassigned_TheFirstSpectatorShouldTakeTheirPlace()
        {
            AssignNextAvailableRoles(Client1, Client2, Client3);

            clientManager.RemoveClient(Client1);

            Assert.AreEqual(ClientRole.PlayerX, clientManager.GetClientRole(Client3));
        }

        [Test]
        public void WhenPlayerOIsUnassigned_TheFirstSpectatorShouldTakeTheirPlace()
        {
            AssignNextAvailableRoles(Client1, Client2, Client3);

            clientManager.RemoveClient(Client2);

            Assert.AreEqual(ClientRole.PlayerO, clientManager.GetClientRole(Client3));
        }

        [Test]
        public void WhenPlayerOIsUnassigned_ClientShouldNoLongerHaveARole()
        {
            AssignNextAvailableRoles(Client1, Client2);

            clientManager.RemoveClient(Client2);

            Assert.AreEqual(ClientRole.None, clientManager.GetClientRole(Client2));
        }

        [Test]
        public void WhenSpectatorIsUnassigned_ClientShouldNoLongerHaveARole()
        {
            AssignNextAvailableRoles(Client1, Client2, Client3);

            clientManager.RemoveClient(Client3);

            Assert.AreEqual(ClientRole.None, clientManager.GetClientRole(Client3));
        }

        [Test]
        public void WhenSpectatorIsUnassigned_SpectatorCountShouldBeDecrementedByOne()
        {
            AssignNextAvailableRoles(Client1, Client2, Client3);

            Assert.AreEqual(1, clientManager.SpectatorCount);

            clientManager.RemoveClient(Client3);

            Assert.AreEqual(0, clientManager.SpectatorCount);
        }

        [Test]
        public void WhenRotatingRoles_AndNoSpectators_SwapPlayerXandO()
        {
            AssignNextAvailableRoles(Client1, Client2);

            Assert.AreEqual(ClientRole.PlayerX, clientManager.GetClientRole(Client1), "Client1 should initially be Player X.");
            Assert.AreEqual(ClientRole.PlayerO, clientManager.GetClientRole(Client2), "Client2 should initially be Player O.");

            clientManager.RotateRolesKeepingAsPlayer(ClientRole.PlayerX);

            Assert.AreEqual(ClientRole.PlayerO, clientManager.GetClientRole(Client1), "Client1 should be switched to Player O.");
            Assert.AreEqual(ClientRole.PlayerX, clientManager.GetClientRole(Client2), "Client2 should be switched to Player X.");
        }

        [Test]
        public void WhenRotatingRolesKeepingX_AndSpectatorsExist_ReplaceXWithSpectatorAndChangeXToO()
        {
            AssignNextAvailableRoles(Client1, Client2, Client3);

            Assert.AreEqual(ClientRole.PlayerX, clientManager.GetClientRole(Client1), "Client1 should initially be Player X.");
            Assert.AreEqual(ClientRole.PlayerO, clientManager.GetClientRole(Client2), "Client2 should initially be Player O.");
            Assert.AreEqual(ClientRole.Spectator, clientManager.GetClientRole(Client3), "Client3 should initially be a spectator.");

            clientManager.RotateRolesKeepingAsPlayer(ClientRole.PlayerX);

            Assert.AreEqual(ClientRole.PlayerO, clientManager.GetClientRole(Client1), "Client1 should be switched to Player O.");
            Assert.AreEqual(ClientRole.Spectator, clientManager.GetClientRole(Client2), "Client2 should be switched to a spectator.");
            Assert.AreEqual(ClientRole.PlayerX, clientManager.GetClientRole(Client3), "Client3 should be switcher to Player X.");
        }
    }
}
