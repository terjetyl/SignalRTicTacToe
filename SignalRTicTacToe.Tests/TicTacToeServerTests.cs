using Moq;
using Moq.Language.Flow;
using Moq.Sequences;
using NUnit.Framework;
using SignalRTicTacToe.Web.Code;

namespace SignalRTicTacToe.Tests
{
    // Connect
        // Start game when X and O have been assigned
        // Notify users when game starts
    // PlaceMark
        // Do not allow if game not started

    // When game ends, winner swaps with other player
    // When game ends, loser is replaced by spectator

    [TestFixture]
    public class TicTacToeServerTests
    {
        private const string Client1 = "Client1";
        private const string Client2 = "Client2";
        private const string Client3 = "Spectator1";
        private const string Client4 = "Spectator2";

        protected Mock<ITicTacToeClientUpdater> clientUpdater;
        protected Mock<ITicTacToe> game;
        protected Mock<IClientManager> clientManager;

        protected TicTacToeServer server;

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
            game.Setup(g => g.Status).Returns(state);

            game.Raise(g => g.GameCompleted += null, game.Object);

            clientUpdater.Verify(x => x.BroadcastMessage(message), Times.Once());
        }

        private void Client2IsPlayerO()
        {
            clientManager.Setup(_ => _.GetClientRole(Client2)).Returns(ClientRole.PlayerO);
        }

        private void Client1IsPlayerX()
        {
            clientManager.Setup(_ => _.GetClientRole(Client1)).Returns(ClientRole.PlayerX);
        }

        [SetUp]
        public void SetUp()
        {
            clientManager = new Mock<IClientManager>();
            clientUpdater = new Mock<ITicTacToeClientUpdater>();
            game = new Mock<ITicTacToe>();

            server = new TicTacToeServer(game.Object, clientManager.Object, clientUpdater.Object);
        }

        [Test]
        public void WhenPlayerConnects_AssignRole()
        {
            server.Connect("Client1");

            clientManager.Verify(_ => _.AssignRole("Client1"), Times.Once());
        }

        [Test]
        public void WhenPlayerXAssigned_SendMessageToX()
        {
            clientManager.Raise(_ => _.PlayerXAssigned += null, clientManager.Object, Client1);
            clientUpdater.Verify(x => x.SendMessage(Client1, "You are X's."), Times.Once());
        }

        [Test]
        public void WhenPlayerOAssigned_SendMessageToO()
        {
            clientManager.Raise(_ => _.PlayerOAssigned += null, clientManager.Object, Client2);
            clientUpdater.Verify(x => x.SendMessage(Client2, "You are O's."), Times.Once());
        }

        [Test]
        public void WhenSpectatorAssigned_SendMessageToSpectator()
        {
            clientManager.Raise(_ => _.SpectatorAssigned += null, clientManager.Object, Client3);
            clientUpdater.Verify(x => x.SendMessage(Client3, "You are a spectator."), Times.Once());
        }

        [Test]
        public void WhenSpectatorArrives_UpdateSpectatorCount()
        {
            const int spectators = 1;
            clientManager.SetupGet(_ => _.SpectatorCount).Returns(spectators);

            clientManager.Raise(_ => _.SpectatorAssigned += null, clientManager.Object, Client3);

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
        public void WhenClientDisconnects_Unassign()
        {
            server.Disconnect(Client1);

            clientManager.Verify(_ => _.Unassign(Client1), Times.Once());
        }

        [Test]
        public void WhenXDisconnects_ResetGame()
        {
            Client1IsPlayerX();

            server.Disconnect(Client1);

            game.Verify(_ => _.Reset());
            clientUpdater.Verify(_ => _.ResetGame());
        }

        [Test]
        public void WhenODisconnects_ResetGame()
        {
            Client2IsPlayerO();

            server.Disconnect(Client2);

            game.Verify(_ => _.Reset());
            clientUpdater.Verify(_ => _.ResetGame());
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
                clientManager.Setup(_ => _.Unassign(Client3)).InSequence();
                clientUpdater.Setup(_ => _.UpdateSpectators(spectators)).InSequence();

                server.Disconnect(Client3);
            }
        }

        [Test]
        public void WhenGameIsCompleted_AndPlayerOWins_PlayerXShouldBeMadeASpectator()
        {
            Client1IsPlayerX();
            Client2IsPlayerO();

            game.SetupGet(_ => _.Status).Returns(GameState.OWins);

            game.Raise(_ => _.GameCompleted += null, game);

            clientManager.Verify(_ => _.RotateRoleOutWithSpectator(ClientRole.PlayerX));
        }
    }

    internal static class TicTacToeMockTestExtensions
    {
        public static IReturnsResult<ITicTacToe> NextTurnFor(this Mock<ITicTacToe> game, PlayerType player)
        {
            return game.SetupGet(x => x.CurrentTurn).Returns(player);
        }
    }
}
