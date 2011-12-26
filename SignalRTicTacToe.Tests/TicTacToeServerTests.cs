using System;
using System.Collections.Generic;
using System.Linq;
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
        private List<string> clients;

        protected Mock<ITicTacToeClientUpdater> clientUpdater;
        protected Mock<ITicTacToe> game;

        protected TicTacToeServer server;

        private string PlayerX
        {
            get { return clients[0]; }
        }

        private string PlayerO
        {
            get { return clients[1]; }
        }

        private void ConnectPlayerXAndO()
        {
            if (clients.Any())
                throw new InvalidOperationException("Player X and O are already connected.");
            ConnectMany(2);
        }

        private void Connect()
        {
            string clientId = (clients.Count + 1).ToString();
            clients.Add(clientId);

            server.Connect(clientId);
        }

        private void ConnectMany(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Connect();
            }
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
            clients = new List<string>();

            clientUpdater = new Mock<ITicTacToeClientUpdater>();
            game = new Mock<ITicTacToe>();
            server = new TicTacToeServer(game.Object, clientUpdater.Object);

            ConnectPlayerXAndO();
        }

        [Test]
        public void WhenFirstClientArrives_AssignToX()
        {
            clientUpdater.Verify(x => x.SendMessage(clients[0], "You are X's."), Times.Once());
        }

        [Test]
        public void WhenSecondClientArrives_AssignToO()
        {
            clientUpdater.Verify(x => x.SendMessage(clients[1], "You are O's."), Times.Once());
        }

        [Test]
        public void WhenAnyClientAfterTheSecondArrives_DesignateAsSpectator()
        {
            ConnectMany(2);

            clientUpdater.Verify(x => x.SendMessage(clients[2], "You are a spectator."), Times.Once());
            clientUpdater.Verify(x => x.SendMessage(clients[3], "You are a spectator."), Times.Once());
        }

        [Test]
        public void WhenSpectatorArrives_UpdateSpectatorCount()
        {
            ConnectMany(2);

            clientUpdater.Verify(x => x.UpdateSpectators(1), Times.Once());
            clientUpdater.Verify(x => x.UpdateSpectators(2), Times.Once());
        }

        [Test]
        public void WhenPlayerXPlacesMark_OnTheirTurn_PlaceX()
        {
            NextTurnIsFor(PlayerType.X);
            
            server.PlaceMark(PlayerX, 1, 1);

            game.Verify(x => x.PlaceX(1, 1));
        }

        [Test]
        public void WhenPlayerOPlacesMark_OnTheirTurn_PlaceO()
        {
            NextTurnIsFor(PlayerType.O);

            server.PlaceMark(PlayerO, 0, 0);

            game.Verify(x => x.PlaceO(0, 0), Times.Once());
        }

        [Test]
        public void WhenPlayerXPlacesMark_OnTheirTurn_UpdateClientSquares()
        {
            NextTurnIsFor(PlayerType.X);

            server.PlaceMark(PlayerX, 1, 1);

            clientUpdater.Verify(x => x.UpdateSquare(1, 1, "X"), Times.Once());
        }

        [Test]
        public void WhenPlayerOPlacesMark_OnTheirTurn_UpdateClientSquares()
        {
            NextTurnIsFor(PlayerType.O);

            server.PlaceMark(PlayerO, 0, 0);

            clientUpdater.Verify(x => x.UpdateSquare(0, 0, "O"), Times.Once());
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
