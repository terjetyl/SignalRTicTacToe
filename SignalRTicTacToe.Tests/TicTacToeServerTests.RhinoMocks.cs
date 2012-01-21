using System;
using NUnit.Framework;
using Rhino.Mocks;
using SignalRTicTacToe.Web.Code;

namespace SignalRTicTacToe.Tests
{
    internal static class ClientManagerRhinoMockExtensions
    {
        public static void RaiseClientRoleAssigned(this IClientManager clientManager, string clientId, ClientRole role)
        {
            clientManager.Raise(_ => _.ClientRoleAssigned += null,
                clientManager,
                new ClientRoleAssignment { ClientId = clientId, Role = role });
        }
    }

    [TestFixture]
    public class TicTacToeServerTests_RhinoMocks
    {
        private ITicTacToe game;
        private IClientManager clientManager;
        private ITicTacToeClientUpdater clientUpdater;
        private TicTacToeServer server;

        #region Helper Methods

        private void VerifyGameIsResetWhen(Action action)
        {
            game.Expect(_ => _.Reset()).Repeat.Once();
            clientUpdater.Expect(_ => _.ResetGame()).Repeat.Once();

            action.Invoke();

            game.VerifyAllExpectations();
            clientUpdater.VerifyAllExpectations();
        }

        private void XPlacesMarkOn(int row, int col)
        {
            clientManager.Stub(_ => _.GetClientRole("client1")).Return(ClientRole.PlayerX);
            game.Stub(_ => _.CurrentTurn).Return(PlayerType.X);
            
            server.PlaceMark("client1", row, col);
        }

        private void OPlacesMarkOn(int row, int col)
        {
            game.Stub(_ => _.CurrentTurn).Return(PlayerType.O);
            clientManager.Stub(_ => _.GetClientRole("client2")).Return(ClientRole.PlayerO);

            server.PlaceMark("client2", row, col);
        }

        private void VerifyThatASpecificMessageBroadcastedWhenGameCompleted(string message, GameState state)
        {
            clientUpdater.Expect(_ => _.BroadcastMessage(message)).Repeat.Once();
            game.Stub(_ => _.Status).Return(state);

            game.Raise(_ => _.GameCompleted += null, game);

            clientUpdater.VerifyAllExpectations();
        }

        #endregion

        [SetUp]
        public void SetUp()
        {
            game = MockRepository.GenerateMock<ITicTacToe>();
            clientManager = MockRepository.GenerateMock<IClientManager>();
            clientUpdater = MockRepository.GenerateMock<ITicTacToeClientUpdater>();
            server = new TicTacToeServer(
                            game,
                            clientManager,
                            clientUpdater);
        }

        [Test]
        public void WhenPlayerConnects_AssignToNextAvailableRole()
        {
            clientManager.Expect(_ => _.AssignToNextAvailableRole("client1")).Repeat.Once();

            server.Connect("client1");
            
            clientManager.VerifyAllExpectations();
        }

        [Test]
        public void WhenPlayerXAssigned_SendMessageConfirmingRoleAsX()
        {
            clientUpdater.Expect(_ => _.SendMessage("client1", "You are X's.")).Repeat.Once();

            clientManager.RaiseClientRoleAssigned("client1", ClientRole.PlayerX);

            clientUpdater.VerifyAllExpectations();
        }

        [Test]
        public void WhenPlayerXAssigned_NotifyClients()
        {
            clientUpdater.Expect(_ => _.BroadcastMessage("Player X is ready."));

            clientManager.RaiseClientRoleAssigned("", ClientRole.PlayerX);

            clientUpdater.VerifyAllExpectations();
        }

        [Test]
        public void WhenPlayerXAssigned_ResetGame()
        {
            VerifyGameIsResetWhen(() =>
                {
                    clientManager.RaiseClientRoleAssigned("", ClientRole.PlayerX);
                });
        }

        [Test]
        public void WhenPlayerOAssign_SendMessageConfirmingRoleAsO()
        {
            clientUpdater.Expect(_ => _.SendMessage("client2", "You are O's.")).Repeat.Once();

            clientManager.RaiseClientRoleAssigned("client2", ClientRole.PlayerO);

            clientUpdater.VerifyAllExpectations();
        }

        [Test]
        public void WhenPlayerOAssigned_NotifyClients()
        {
            clientUpdater.Expect(_ => _.BroadcastMessage("Player O is ready."));

            clientManager.RaiseClientRoleAssigned("", ClientRole.PlayerO);

            clientUpdater.VerifyAllExpectations();
        }

        [Test]
        public void WhenPlayerOAssigned_ResetGame()
        {
            VerifyGameIsResetWhen(() =>
                {
                    clientManager.RaiseClientRoleAssigned("", ClientRole.PlayerO);
                });
        }

        [Test]
        public void WhenSpectatorAssigned_SendMessageConfirmingRoleAsSpectator()
        {
            clientUpdater.Expect(_ => _.SendMessage("client3", "You are a spectator.")).Repeat.Once();

            clientManager.RaiseClientRoleAssigned("client3", ClientRole.Spectator);

            clientUpdater.VerifyAllExpectations();
        }

        [Test]
        public void WhenSpectatorIsAssigned_UpdateSpectatorCount()
        {
            clientManager.Stub(_ => _.SpectatorCount).Return(1);
            clientUpdater.Expect(_ => _.UpdateSpectators(1)).Repeat.Once();

            clientManager.RaiseClientRoleAssigned("client3", ClientRole.Spectator);

            clientUpdater.VerifyAllExpectations();
        }

        [Test]
        public void WhenPlayerXPlacesMark_AndIsXsTurn_PlaceX()
        {
            game.Expect(_ => _.PlaceX(1, 1)).Repeat.Once();

            XPlacesMarkOn(1, 1);

            game.VerifyAllExpectations();
        }

        [Test]
        public void WhenPlayerXPlacesMark_AndIsXsTurn_UpdateSquareForX()
        {
            game.Expect(_ => _.PlaceX(1, 1)).Repeat.Once();

            XPlacesMarkOn(1, 1);

            clientUpdater.VerifyAllExpectations();
        }

        [Test]
        public void WhenPlayerOPlacesMark_AndIsOsTurn_PlaceO()
        {
            game.Expect(_ => _.PlaceO(0, 0)).Repeat.Once();

            OPlacesMarkOn(0, 0);

            game.VerifyAllExpectations();
        }

        [Test]
        public void WhenPlayerOPlacesMark_AndIsOsTurn_UpdateSquareForO()
        {
            clientUpdater.Expect(_ => _.UpdateSquare(0, 0, "O")).Repeat.Once();

            OPlacesMarkOn(0, 0);

            clientUpdater.VerifyAllExpectations();
        }

        [Test]
        public void WhenGameCompleted_AndXWins_BroadcastXWinsToAll()
        {
            VerifyThatASpecificMessageBroadcastedWhenGameCompleted("X Wins!", GameState.XWins);
        }

        [Test]
        public void WhenGameComplete_AndOWins_BroadcastOWinsToAll()
        {
            VerifyThatASpecificMessageBroadcastedWhenGameCompleted("O Wins!", GameState.OWins);
        }

        [Test]
        public void WhenGameComplete_AndDraw_BroadcastDrawToAll()
        {
            VerifyThatASpecificMessageBroadcastedWhenGameCompleted("Game is a draw.", GameState.Draw);
        }

        [Test]
        public void WhenPlayerXDisconnects_NotifyOtherClients()
        {
            clientUpdater.Expect(_ => _.BroadcastMessage("Player X has left."));
            clientManager.Stub(_ => _.GetClientRole("client1")).Return(ClientRole.PlayerX);

            server.Disconnect("client1");

            clientUpdater.VerifyAllExpectations();
        }

        [Test]
        public void WhenPlayerODisconnects_NotifyOtherClients()
        {
            clientUpdater.Expect(_ => _.BroadcastMessage("Player O has left."));
            clientManager.Stub(_ => _.GetClientRole("client2")).Return(ClientRole.PlayerO);

            server.Disconnect("client2");

            clientUpdater.VerifyAllExpectations();
        }

        [Test]
        public void WhenClientDisconnects_Unassign()
        {
            clientManager.Expect(_ => _.RemoveClient("client1")).Repeat.Once();

            server.Disconnect("client1");

            clientManager.VerifyAllExpectations();
        }

        [Test]
        public void WhenSpectatorDisconnects_UpdateSpectators()
        {
            clientManager.Stub(_ => _.SpectatorCount).Return(0);
            clientManager.Stub(_ => _.GetClientRole("client3")).Return(ClientRole.Spectator);
            clientUpdater.Expect(_ => _.UpdateSpectators(0));

            server.Disconnect("client3");

            clientUpdater.VerifyAllExpectations();
        }

        [Test]
        [Ignore]
        public void WhenSpectatorDisconnects_ClientMustBeUnassignedBeforeUpdateSpectators()
        {
            // Not sure how to test this with Rhino Mocks.
        }
    }
}
