using System;
using Moq;
using Moq.Language.Flow;
using Moq.Sequences;
using NUnit.Framework;
using SignalRTicTacToe.Web.Code;

namespace SignalRTicTacToe.Tests
{
    [TestFixture]
    public class TicTacToeServerTests
    {
        private const string Client1 = "Client1";
        private const string Client2 = "Client2";
        private const string Client3 = "Spectator1";

        protected Mock<ITicTacToeClientUpdater> clientUpdater;
        protected Mock<ITicTacToe> game;
        protected Mock<IClientManager> clientManager;

        protected TicTacToeServer server;

        #region Helper Methods

        private void XIsPlacedOnRow1Column1()
        {
            clientManager.Setup(_ => _.GetClientRole(Client1)).Returns(ClientRole.PlayerX);
            game.NextTurnFor(PlayerType.X);

            server.PlaceMark(Client1, 1, 1);
        }

        private void OIsPlacedOnRow0Column0()
        {
            clientManager.Setup(_ => _.GetClientRole(Client2)).Returns(ClientRole.PlayerO);
            game.NextTurnFor(PlayerType.O);

            server.PlaceMark(Client2, 0, 0);
        }

        private void VerifyThatSpecificMessageBroadcastedWhenGameCompleted(string message, GameState state)
        {
            game.Raise(g => g.GameCompleted += null, game.Object, new GameCompletedEventArgs(state));

            clientUpdater.Verify(x => x.BroadcastMessage(message), Times.Once());
        }

        private void Client1IsPlayerX()
        {
            clientManager.Setup(_ => _.GetClientRole(Client1)).Returns(ClientRole.PlayerX);
        }

        private void Client2IsPlayerO()
        {
            clientManager.Setup(_ => _.GetClientRole(Client2)).Returns(ClientRole.PlayerO);
        }

        private void VerifyGameIsReset()
        {
            game.Verify(_ => _.Reset());
            clientUpdater.Verify(_ => _.ResetGame());
        }

        #endregion

        [SetUp]
        public void SetUp()
        {
            clientManager = new Mock<IClientManager>();
            clientUpdater = new Mock<ITicTacToeClientUpdater>();
            game = new Mock<ITicTacToe>();

            server = new TicTacToeServer(game.Object, clientManager.Object, clientUpdater.Object);
        }

        [Test]
        public void WhenPlayerConnects_AssignToNextAvailableRole()
        {
            server.Connect(Client1);
            clientManager.Verify(_ => _.AssignToNextAvailableRole(Client1), Times.Once());
        }

        [Test]
        public void WhenPlayerXAssigned_SendMessageConfirmingRoleAsX()
        {
            clientManager.RaiseClientRoleAssigned(Client1, ClientRole.PlayerX);
            clientUpdater.Verify(x => x.SendMessage(Client1, "You are X's."), Times.Once());
        }

        [Test]
        public void WhenPlayerXAssigned_NotifyClients()
        {
            clientManager.RaiseClientRoleAssigned("Any Client", ClientRole.PlayerX);
            clientUpdater.Verify(_ => _.BroadcastMessage("Player X is ready."));
        }

        [Test]
        public void WhenPlayerXAssigned_ResetGame()
        {
            clientManager.RaiseClientRoleAssigned("Any Client", ClientRole.PlayerX);
            VerifyGameIsReset();
        }

        [Test]
        public void WhenPlayerOAssigned_SendMessageConfirmingRoleAsO()
        {
            clientManager.RaiseClientRoleAssigned(Client2, ClientRole.PlayerO);
            clientUpdater.Verify(x => x.SendMessage(Client2, "You are O's."), Times.Once());
        }

        [Test]
        public void WhenPlayerOAssigned_NotifyClients()
        {
            clientManager.RaiseClientRoleAssigned("Any Client", ClientRole.PlayerO);
            clientUpdater.Verify(_ => _.BroadcastMessage("Player O is ready."));
        }

        [Test]
        public void WhenPlayerOAssigned_ResetGame()
        {
            clientManager.RaiseClientRoleAssigned("Any Client", ClientRole.PlayerO);
            VerifyGameIsReset();
        }

        [Test]
        public void WhenSpectatorAssigned_SendMessageConfirmingRoleAsSpectator()
        {
            clientManager.RaiseClientRoleAssigned(Client3, ClientRole.Spectator);
            clientUpdater.Verify(x => x.SendMessage(Client3, "You are a spectator."), Times.Once());
        }

        [Test]
        public void WhenSpectatorIsAssigned_UpdateSpectatorCount()
        {
            const int spectators = 1;
            clientManager.SetupGet(_ => _.SpectatorCount).Returns(spectators);

            clientManager.RaiseClientRoleAssigned(Client3, ClientRole.Spectator);

            clientUpdater.Verify(x => x.UpdateSpectators(spectators), Times.Once());
        }

        [Test]
        public void WhenPlayerXPlacesMark_AndIsXsTurn_PlaceX()
        {
            XIsPlacedOnRow1Column1();

            game.Verify(x => x.PlaceX(1, 1), Times.Once());
        }

        [Test]
        public void WhenPlayerXPlacesMark_AndIsXsTurn_UpdateSquareForX()
        {
            XIsPlacedOnRow1Column1();

            clientUpdater.Verify(_ => _.UpdateSquare(1, 1, "X"));
        }

        [Test]
        public void WhenPlayerOPlacesMark_AndIsOsTurn_PlaceO()
        {
            OIsPlacedOnRow0Column0();

            game.Verify(_ => _.PlaceO(0, 0), Times.Once());
        }

        [Test]
        public void WhenPlayerOPlacesMark_AndIsOsTurn_UpdateSquareForO()
        {
            OIsPlacedOnRow0Column0();

            clientUpdater.Verify(_ => _.UpdateSquare(0, 0, "O"), Times.Once());
        }

        [Test]
        public void WhenGameCompleted_AndXWins_BroadcastXWinsToAll()
        {
            VerifyThatSpecificMessageBroadcastedWhenGameCompleted("X Wins!", GameState.XWins);
        }

        [Test]
        public void WhenGameComplete_AndOWins_BroadcastOWinsToAll()
        {
            VerifyThatSpecificMessageBroadcastedWhenGameCompleted("O Wins!", GameState.OWins);
        }

        [Test]
        public void WhenGameComplete_AndDraw_BroadcastDrawToAll()
        {
            VerifyThatSpecificMessageBroadcastedWhenGameCompleted("Game is a draw.", GameState.Draw);
        }

        [Test]
        [Ignore]
        public void WhenGameComplete_ResetAfterFiveSeconds()
        {
            // I don't know how to test this.
        }

        [Test]
        public void WhenPlayerXDisconnects_NotifyOtherClients()
        {
            Client1IsPlayerX();

            server.Disconnect(Client1);

            clientUpdater.Verify(_ => _.BroadcastMessage("Player X has left."));
        }

        [Test]
        public void WhenPlayerODisconnects_NotifyOtherClients()
        {
            Client2IsPlayerO();

            server.Disconnect(Client2);

            clientUpdater.Verify(_ => _.BroadcastMessage("Player O has left."));
        }

        [Test]
        public void WhenClientDisconnects_Unassign()
        {
            server.Disconnect(Client1);

            clientManager.Verify(_ => _.RemoveClient(Client1), Times.Once());
        }

        [Test]
        public void WhenSpectatorDisconnects_UpdateSpectators()
        {
            const int spectators = 0;
            clientManager.SetupGet(_ => _.SpectatorCount).Returns(spectators);
            clientManager.Setup(_ => _.GetClientRole(Client3)).Returns(ClientRole.Spectator);

            server.Disconnect(Client3);

            clientUpdater.Verify(_ => _.UpdateSpectators(spectators));
        }

        [Test]
        public void WhenSpectatorDisconnects_ClientMustBeUnassignedBeforeUpdateSpectators()
        {
            const int spectators = 0;
            clientManager.SetupGet(_ => _.SpectatorCount).Returns(spectators);
            clientManager.Setup(_ => _.GetClientRole(Client3)).Returns(ClientRole.Spectator);

            using (Sequence.Create())
            {
                clientManager.Setup(_ => _.RemoveClient(Client3)).InSequence();
                clientUpdater.Setup(_ => _.UpdateSpectators(spectators)).InSequence();

                server.Disconnect(Client3);
            }
        }
    }

    internal static class MockTicTacToeExtensions
    {
        public static IReturnsResult<ITicTacToe> NextTurnFor(this Mock<ITicTacToe> game, PlayerType player)
        {
            return game.SetupGet(x => x.CurrentTurn).Returns(player);
        }
    }

    internal static class MockClientManagerExtensions
    {
        public static void RaiseClientRoleAssigned(this Mock<IClientManager> clientManager, string clientId, ClientRole role)
        {
            clientManager.Raise(_ => _.ClientRoleAssigned += null
                                    , clientManager.Object
                                    , new ClientRoleAssignedArgs(clientId, role));
        }
    }
}
