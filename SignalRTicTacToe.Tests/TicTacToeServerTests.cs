using Moq;
using NUnit.Framework;
using SignalRTicTacToe.Web.Code;

namespace SignalRTicTacToe.Tests
{
    // Connect
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
            NextTurnIsFor(PlayerType.X);

            server.PlaceMark(Client1, 1, 1);
        }

        private void OIsPlacedOnRow0Column0()
        {
            clientManager.Setup(_ => _.GetClientRole(Client2)).Returns(ClientRole.PlayerO);
            NextTurnIsFor(PlayerType.O);

            server.PlaceMark(Client2, 0, 0);
        }

        private void NextTurnIsFor(PlayerType player)
        {
            game.SetupGet(x => x.CurrentTurn).Returns(player);
        }

        private void VerifyThatSpecificMessageBroadcastedWhenGameCompleted(string message, GameState state)
        {
            game.Setup(g => g.Status).Returns(state);

            game.Raise(g => g.GameCompleted += null, game.Object);

            clientUpdater.Verify(x => x.BroadcastMessage(message), Times.Once());
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
            clientManager.Raise(_ => _.SpectatorAssigned += null, clientManager.Object, Client3);
            clientManager.Raise(_ => _.SpectatorAssigned += null, clientManager.Object, Client4);

            clientUpdater.Verify(x => x.UpdateSpectators(1), Times.Once());
            clientUpdater.Verify(x => x.UpdateSpectators(2), Times.Once());
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
    }
}
