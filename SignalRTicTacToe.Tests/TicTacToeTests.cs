using System;
using NUnit.Framework;
using SignalRTicTacToe.Web;

namespace SignalRTicTacToe.Tests
{
    [TestFixture]
    public class TicTacToeTests
    {
        private TicTacToe ticTacToe;

        private void AssertAllSquaresAreEmpty()
        {
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    Assert.AreEqual(PlayerType.None, ticTacToe.GetSquareState(row, col));
                }
            }
        }

        private void PlaceX(int row, int col)
        {
            ticTacToe.PlaceX(row, col);
        }

        private void PlaceO(int row, int col)
        {
            ticTacToe.PlaceO(row, col);
        }

        private void XWinsWithThreeInARow()
        {
            PlaceX(0, 0);
            PlaceO(1, 0);
            PlaceX(0, 1);
            PlaceO(1, 1);
            PlaceX(0, 2);
        }

        private void XWinsWithThreeInAColumn()
        {
            PlaceX(0, 0);
            PlaceO(1, 1);
            PlaceX(1, 0);
            PlaceO(1, 2);
            PlaceX(2, 0);
        }

        private void OWinsWithThreeInADiagonal()
        {
            PlaceX(0, 0);
            PlaceO(0, 2);
            PlaceX(0, 1);
            PlaceO(1, 1);
            PlaceX(1, 0);
            PlaceO(2, 0);
        }

        private void PlayUntilDraw()
        {
            PlaceX(0, 0);
            PlaceO(1, 0);
            PlaceX(0, 1);
            PlaceO(0, 2);
            PlaceX(2, 0);
            PlaceO(1, 1);
            PlaceX(1, 2);
            PlaceO(2, 1);
            PlaceX(2, 2);
        }

        private void PassIfGameCompleted(Action playGame)
        {
            ticTacToe.GameCompleted += (sender) => Assert.Pass();

            playGame.Invoke();

            Assert.Fail();
        }

        private void PlayCompleteGameAndReset()
        {
            PlayUntilDraw();

            ticTacToe.Reset();
        }

        [SetUp]
        public void SetUp()
        {
            ticTacToe = new TicTacToe();
        }

        [Test]
        public void TestAllSquaresAreInitializedAsEmpty()
        {
            AssertAllSquaresAreEmpty();
        }

        [Test]
        public void TestGetSquareStateReturnsProperState()
        {
            PlaceX(1, 1);
            PlaceO(2, 2);

            Assert.AreEqual(PlayerType.X, ticTacToe.GetSquareState(1, 1));
            Assert.AreEqual(PlayerType.O, ticTacToe.GetSquareState(2, 2));
        }

        [Test]
        public void TestXStartsFirst()
        {
            Assert.AreEqual(PlayerType.X, ticTacToe.CurrentTurn);
        }

        [Test]
        [ExpectedException(typeof(OutOfTurnException))]
        public void TestODoesNotMoveFirst()
        {
            PlaceO(0, 0);
        }

        [Test]
        [ExpectedException(typeof(OutOfTurnException))]
        public void TestXCannotTakeTwoConsecutiveTurns()
        {
            PlaceX(0, 0);
            PlaceX(1, 1);
        }

        [Test]
        [ExpectedException(typeof(OutOfTurnException))]
        public void TestOCannotTakeTwoConsecutiveTurns()
        {
            PlaceX(0, 0);
            PlaceO(1, 1);
            PlaceO(1, 2);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestMarkCannotBePlacedOnSameSquareTwice()
        {
            PlaceX(0, 0);
            PlaceO(1, 1);
            PlaceX(0, 0);
        }

        [Test]
        public void TestThreeInARowWins()
        {
            XWinsWithThreeInARow();

            Assert.AreEqual(GameState.XWins, ticTacToe.Status);
        }

        [Test]
        public void TestThreeInAColumnWins()
        {
            XWinsWithThreeInAColumn();

            Assert.AreEqual(GameState.XWins, ticTacToe.Status);
        }

        [Test]
        public void TestThreeInADiagonalWins()
        {
            OWinsWithThreeInADiagonal();

            Assert.AreEqual(GameState.OWins, ticTacToe.Status);
        }

        [Test]
        public void TestDraw()
        {
            PlayUntilDraw();

            Assert.AreEqual(GameState.Draw, ticTacToe.Status);
        }

        [Test]
        public void WhenGameIsWon_NotifyGameCompleted()
        {
            PassIfGameCompleted(XWinsWithThreeInARow);
        }

        [Test]
        public void WhenDrawOccurs_NotifyGameCompleted()
        {
            PassIfGameCompleted(PlayUntilDraw);
        }

        [Test]
        public void SquaresAreEmptyAfterReset()
        {
            PlayCompleteGameAndReset();

            AssertAllSquaresAreEmpty();
        }

        [Test]
        public void PlayerXGoesNextAfterReset()
        {
            PlayCompleteGameAndReset();

            Assert.AreEqual(PlayerType.X, ticTacToe.CurrentTurn);
        }

        [Test]
        public void GamesBackInProgressAfterReset()
        {
            PlayCompleteGameAndReset();

            Assert.AreEqual(GameState.InProgress, ticTacToe.Status);
        }
    }
}
