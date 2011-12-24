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

        private void XMoves(int row, int col)
        {
            ticTacToe.PlaceX(row, col);
        }

        private void OMoves(int row, int col)
        {
            ticTacToe.PlaceO(row, col);
        }

        private void XWinsWithThreeInARow()
        {
            XMoves(0, 0);
            OMoves(1, 0);
            XMoves(0, 1);
            OMoves(1, 1);
            XMoves(0, 2);
        }

        private void Draw()
        {
            XMoves(0, 0);
            OMoves(1, 0);
            XMoves(0, 1);
            OMoves(0, 2);
            XMoves(2, 0);
            OMoves(1, 1);
            XMoves(1, 2);
            OMoves(2, 1);
            XMoves(2, 2);
        }

        private void PassIfGameCompleted()
        {
            ticTacToe.GameCompleted += sender => Assert.Pass();
        }

        private void PlayCompleteGameAndReset()
        {
            Draw();
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
            XMoves(1, 1);
            OMoves(2, 2);

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
            OMoves(0, 0);
        }

        [Test]
        [ExpectedException(typeof(OutOfTurnException))]
        public void TestXCannotTakeTwoConsecutiveTurns()
        {
            XMoves(0, 0);
            XMoves(1, 1);
        }

        [Test]
        [ExpectedException(typeof(OutOfTurnException))]
        public void TestOCannotTakeTwoConsecutiveTurns()
        {
            XMoves(0, 0);
            OMoves(1, 1);
            OMoves(1, 2);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestMarkCannotBePlacedOnSameSquareTwice()
        {
            XMoves(0, 0);
            OMoves(1, 1);
            XMoves(0, 0);
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
            XMoves(0, 0);
            OMoves(1, 1);
            XMoves(1, 0);
            OMoves(1, 2);
            XMoves(2, 0);

            Assert.AreEqual(GameState.XWins, ticTacToe.Status);
        }

        [Test]
        public void TestThreeInADiagonalWins()
        {
            XMoves(0, 0);
            OMoves(0, 2);
            XMoves(0, 1);
            OMoves(1, 1);
            XMoves(1, 0);
            OMoves(2, 0);

            Assert.AreEqual(GameState.OWins, ticTacToe.Status);
        }

        [Test]
        public void TestDraw()
        {
            Draw();

            Assert.AreEqual(GameState.Draw, ticTacToe.Status);
        }

        [Test]
        public void WhenGameIsWon_NotifyGameCompleted()
        {
            PassIfGameCompleted();

            XWinsWithThreeInARow();

            Assert.Fail();
        }

        [Test]
        public void WhenDrawOccurs_NotifyGameCompleted()
        {
            PassIfGameCompleted();

            Draw();

            Assert.Fail();
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
