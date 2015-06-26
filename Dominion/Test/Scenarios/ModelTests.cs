﻿using System;
using System.Linq;
using gbd.Dominion.Contents.Cards;
using gbd.Dominion.Model;
using gbd.Dominion.Model.Cards;
using gbd.Dominion.Model.GameMechanics;
using gbd.Dominion.Model.Zones;
using gbd.Dominion.Test.Utilities;
using gbd.Dominion.Tools;
using gbd.Tools.NInject;
using Ninject;
using NUnit.Framework;

namespace gbd.Dominion.Test.Scenarios
{
    [TestFixture]
    public class ModelTests: BaseTest
    {

        public const int NB_CARDS_IN_DEFAULT_DECK = 10;

        [Test]
        public void Deck()
        {
            var deck = IoC.Kernel.Get<IDeck>();
            Assert.That(deck.Cards.Count, Is.EqualTo(10));
        }

        [Test]
        public void ShuffleDeck()
        {
            IoC.Kernel.ReBind<IDeck>().To<StartingDeck>();

            var deck = IoC.Kernel.Get<IDeck>();
            var library = deck.ShuffleDiscardToLibrary();

            Assert.That(deck.Cards.Count, Is.EqualTo(10));
            Assert.That(library.Cards.Count(), Is.EqualTo(10));
        }

   


        [TestCase(0, 0, false, 5, 5, 0)]
        [TestCase(5, 2, false, 8, 0, 2)]
        [TestCase(5, 2, true, 8, 2, 0)]
        public void DiscardFromHand(int draw, int discard, bool shuffle, int expectInHand, int expectInLib, int expectInDiscard)
        {
            IoC.Kernel.ReBind<IDeck>().To<StartingDeck>();

            var player = IoC.Kernel.Get<IPlayer>();
            player.Ready();

            player.Draw(draw);
            player.ChooseAndDiscard(discard);

            if (shuffle)
                player.Deck.ShuffleDiscardToLibrary();

            Assert.That(player.Deck.Cards.Count, Is.EqualTo(NB_CARDS_IN_DEFAULT_DECK));

            Assert.That(player.Deck.Hand.Cards.Count, Is.EqualTo(expectInHand));
            Assert.That(player.Deck.DiscardPile.Cards.Count, Is.EqualTo(expectInDiscard));
            Assert.That(player.Deck.Library.Cards.Count, Is.EqualTo(expectInLib));
        }


        [TestCase(0,5,5)]
        [TestCase(1,6,4)]
        [TestCase(2,7,3)]
        [TestCase(5,10,0)]
        public void PlayerDraw(int amountToDraw, int expectedInHand, int expectedInLibrary)
        {
            var player = IoC.Kernel.Get<Player>();
            player.Ready();


            Assert.That(player.Deck.Cards.Count(), Is.EqualTo(NB_CARDS_IN_DEFAULT_DECK));
            Assert.That(player.Deck.Library.Cards.Count(), Is.EqualTo(5));

            player.Draw(amountToDraw);

            Assert.That(player.Deck.Cards.Count(), Is.EqualTo(NB_CARDS_IN_DEFAULT_DECK));
            Assert.That(player.Deck.Library.Cards.Count(), Is.EqualTo(expectedInLibrary));
            Assert.That(player.Deck.Hand.Cards.Count(), Is.EqualTo(expectedInHand));

        }

        [ExpectedException(typeof(NotEnoughCardsException))]
        [Test]
        public void PlayerDrawAllDeckPlusOne()
        {
            IoC.Kernel.ReBind<IDeck>().To<StartingDeck>();

            var player = IoC.Kernel.Get<Player>();
            player.Ready();


            player.Draw(6);
        }

       

        [TestCase(0, 0)]
        [TestCase(3, 0)]
        [TestCase(0, 3)]
        [TestCase(10, 0)]
        [TestCase(7, 9)]
        public void CountVictory(int estates, int provinces)
        {
            IoC.Kernel.Unbind<ICard>();

            int numberOfFillCards = Math.Max(10 - estates - provinces, 3);
            int expectedScore = 1*estates + 6*provinces;

            IoC.Kernel.BindMultipleTimesTo<ICard, Estate>(estates).WhenAnyAncestorOfType<Estate, IDeck>();
            IoC.Kernel.BindMultipleTimesTo<ICard, Province>(provinces).WhenAnyAncestorOfType<Province, IDeck>();    
            IoC.Kernel.BindMultipleTimesTo<ICard, Copper>(numberOfFillCards).WhenAnyAncestorOfType<Copper, IDeck>();    

            var player = IoC.Kernel.Get<IPlayer>();

            Assert.That(player.CurrentScore, Is.EqualTo(expectedScore));

            player.Receive(IoC.Kernel.Get<Estate>());
            Assert.That(player.CurrentScore, Is.EqualTo(expectedScore + 1 ));


            player.Receive(IoC.Kernel.Get<Duchy>());
            Assert.That(player.CurrentScore, Is.EqualTo(expectedScore + 4));

            player.Receive(IoC.Kernel.Get<Province>());
            Assert.That(player.CurrentScore, Is.EqualTo(expectedScore + 10));

        }




        [TestCase(10,0,10)]
        [TestCase(10,10,15)]
        [TestCase(1,10,3)]
        public void LibraryReshuffle(int librarySize, int discardSize, int pick)
        {
            IoC.Kernel.Unbind<ICard>();
            IoC.Kernel.BindMultipleTimes<ICard>(librarySize).To<ICard, TestCard>()
                .WhenAnyAncestorOfType<TestCard, ILibrary>();
            //IoC.Kernel.BindMultipleTimes<ICard>(discardSize).To<ICard, TestCard>()
            //    .WhenAnyAncestorOfType<TestCard, IDiscardPile>();


            var deck = IoC.Kernel.Get<IDeck>();
            deck.Ready();

            Assert.That(deck.CardCountByZone, Is.EqualTo(
                new CardRepartition(librarySize,0,discardSize,0)));

            deck.Library.MoveCardsTo(deck.Hand, pick);

            Assert.That(deck.CardCountByZone, Is.EqualTo(
                new CardRepartition(librarySize + discardSize - pick, pick, 0, 0)));
        }

        [TestCase(1, 1, 1, 1)]
        [TestCase(2, 2, 2, 2)]
        [TestCase(10, 50, 500, 5000)]
        public void InjectIntoDeckComponents_Tools(int inLib, int inHand, int inDisc, int inBf)
        {
            IoC.Kernel.Unbind<ICard>();

            IoC.Kernel.BindMultipleTimes<ICard>(inLib).To<ICard, Copper>()
                .WhenAnyAncestorOfType<Copper, ILibrary>();

            IoC.Kernel.BindMultipleTimes<ICard>(inHand).To<ICard, Silver>()
                .WhenAnyAncestorOfType<Silver, IHand>();

            IoC.Kernel.BindMultipleTimes<ICard>(inDisc).To<ICard, Gold>()
                .WhenAnyAncestorOfType<Gold, IDiscardPile>();

            IoC.Kernel.BindMultipleTimes<ICard>(inBf).To<ICard, Estate>()
                .WhenAnyAncestorOfType<Estate, IBattleField>();


            InjectIntoDeckComponents_InternalCheck(inLib, inHand, inDisc, inBf);

        }

        [TestCase(1, 1, 1, 1)]
        [TestCase(2, 2, 2, 2)]
        [TestCase(10, 50, 500, 5000)]
        public void InjectIntoDeckComponents_Linq(int inLib, int inHand, int inDisc, int inBf)
        {
            IoC.Kernel.Unbind<ICard>();

            IoC.Kernel.BindMultipleTimes<ICard>(inLib).To<ICard, Copper>()
                .Select(c => c.WhenAnyAncestorOfType<Copper, ILibrary>()).ToList();
            
            
            IoC.Kernel.BindMultipleTimes<ICard>(inHand).To<ICard, Silver>()
                 .Select(c => c.WhenAnyAncestorOfType<Silver, IHand>()).ToList();
            

            IoC.Kernel.BindMultipleTimes<ICard>(inDisc).To<ICard, Gold>()
                 .Select(c => c.WhenAnyAncestorOfType<Gold, IDiscardPile>()).ToList();
            

            IoC.Kernel.BindMultipleTimes<ICard>(inBf).To<ICard, Estate>()
                 .Select(c => c.WhenAnyAncestorOfType<Estate, IBattleField>()).ToList();
            


            InjectIntoDeckComponents_InternalCheck(inLib, inHand, inDisc, inBf);

        }


        [TestCase(1, 1, 1, 1)]
        [TestCase(2, 2, 2, 2)]
        [TestCase(10, 50, 500, 5000)]
        public void InjectIntoDeckComponents_Foreach(int inLib, int inHand, int inDisc, int inBf)
        {
            IoC.Kernel.Unbind<ICard>();

            foreach (var syntax in IoC.Kernel.BindMultipleTimes<ICard>(inLib).To<ICard, Copper>())
                syntax.WhenAnyAncestorOfType<Copper, ILibrary>();


            foreach (var syntax in IoC.Kernel.BindMultipleTimes<ICard>(inHand).To<ICard, Silver>())
                 syntax.WhenAnyAncestorOfType<Silver, IHand>();


            foreach (var syntax in IoC.Kernel.BindMultipleTimes<ICard>(inDisc).To<ICard, Gold>())
                 syntax.WhenAnyAncestorOfType<Gold, IDiscardPile>();


            foreach (var syntax in IoC.Kernel.BindMultipleTimes<ICard>(inBf).To<ICard, Estate>())
                syntax.WhenAnyAncestorOfType<Estate, IBattleField>();



            InjectIntoDeckComponents_InternalCheck(inLib, inHand, inDisc, inBf);

        }


        [TestCase(1,1,1,1)]
        [TestCase(2,2,2,2)]
        [TestCase(10, 50, 500, 5000)]
        public void InjectIntoDeckComponents_Loop(int inLib, int inHand, int inDisc, int inBf)
        {
            IoC.Kernel.Unbind<ICard>();

            for (int i = 0; i < inLib; i++)
                IoC.Kernel.Bind<ICard>().To<Copper>().WhenAnyAncestorOfType<Copper, ILibrary>();

            for (int i = 0; i < inHand; i++)
                IoC.Kernel.Bind<ICard>().To<Silver>().WhenAnyAncestorOfType<Silver, IHand>();

            for (int i = 0; i < inDisc; i++)
                IoC.Kernel.Bind<ICard>().To<Gold>().WhenAnyAncestorOfType<Gold, IDiscardPile>();

            for (int i = 0; i < inBf; i++)
                IoC.Kernel.Bind<ICard>().To<Estate>().WhenAnyAncestorOfType<Estate, IBattleField>();


            InjectIntoDeckComponents_InternalCheck(inLib, inHand, inDisc, inBf);
        }

        private static void InjectIntoDeckComponents_InternalCheck(int inLib, int inHand, int inDisc, int inBf)
        {
            var lib = IoC.Kernel.Get<ILibrary>();
            var hand = IoC.Kernel.Get<IHand>();
            var discard = IoC.Kernel.Get<IDiscardPile>();
            var bf = IoC.Kernel.Get<IBattleField>();

            Assert.That(lib.Cards.Count, Is.EqualTo(inLib));
            Assert.That(hand.Cards.Count, Is.EqualTo(inHand));
            Assert.That(discard.Cards.Count, Is.EqualTo(inDisc));
            Assert.That(bf.Cards.Count, Is.EqualTo(inBf));


            var deck = new TestDeck(lib, discard, bf, hand);

            Assert.That(deck.CardCountByZone, Is.EqualTo(new CardRepartition(inLib, inHand, inDisc, inBf)));
        }

  

        [Test]
        public void LibraryCanReshuffle()
        {
            var player = IoC.Kernel.Get<IPlayer>();
            player.Deck.Ready();

            Assert.That(player.Deck.CardCountByZone, Is.EqualTo(new CardRepartition(10,0,0,0)));

            player.Deck.Library.MoveCardsTo(player.Deck.DiscardPile, 5);
            Assert.That(player.Deck.CardCountByZone, Is.EqualTo(new CardRepartition(5, 0, 5, 0)));

            player.Deck.Library.MoveCardsTo(player.Deck.Hand, 6);
            Assert.That(player.Deck.CardCountByZone, Is.EqualTo(new CardRepartition(4, 6, 0, 0)));
        }

        [ExpectedException(typeof(NotEnoughCardsException))]
        [Test]
        public void DiscardCannotReshuffle()
        {
            var player = IoC.Kernel.Get<IPlayer>();
            player.Deck.Ready();

            player.Deck.Library.MoveCardsTo(player.Deck.DiscardPile, 5);
            player.Deck.DiscardPile.MoveCardsTo(player.Deck.Hand, 6);
        }

        [ExpectedException(typeof(NotEnoughCardsException))]
        [Test]
        public void HandCannotReshuffle()
        {
            var player = IoC.Kernel.Get<IPlayer>();
            player.Deck.Ready();

            player.Deck.Hand.MoveCardsTo(player.Deck.DiscardPile, 5);
        }

        [ExpectedException(typeof(NotEnoughCardsException))]
        [Test]
        public void BAttlefieldCannotReshuffle()
        {
            var player = IoC.Kernel.Get<IPlayer>();
            player.Deck.Ready();

            player.Deck.Library.MoveCardsTo(player.Deck.BattleField, 5);
            player.Deck.BattleField.MoveCardsTo(player.Deck.Hand, 6);
        }

        [ExpectedException(typeof(NotEnoughCardsException))]
        [Test]
        public void SupplyCannotReshuffle()
        {
            var player = IoC.Kernel.Get<IPlayer>();
            player.Deck.Ready();

            IoC.Kernel.Unbind<ISupplyPile>();
            IoC.Kernel.Bind<ISupplyPile>().To<SupplyPile>();
            var supply = IoC.Kernel.Get<ISupplyPile>();
            supply.Ready();

            supply.MoveCardsTo(player.Deck, 11);
        }



        [Test]
        public void ReshuffleWhenEmpty_FullCase()
        {
            IoC.Kernel.ReBind<IDeck>().To<TestDeck>();

            var player = IoC.Kernel.Get<Player>();
            player.Ready();

            Assert.That(player.Deck.CardCountByZone, Is.EqualTo(new CardRepartition(5, 5, 0, 0)));

            player.Deck.Hand.MoveCardsTo(player.Deck.DiscardPile, 3);
            Assert.That(player.Deck.CardCountByZone, Is.EqualTo(new CardRepartition(5,2,3,0)));

            player.Draw(5);
            Assert.That(player.Deck.CardCountByZone, Is.EqualTo(new CardRepartition(0, 7, 3, 0)));

            player.ChooseAndDiscard(3);
            Assert.That(player.Deck.CardCountByZone, Is.EqualTo(new CardRepartition(0, 4, 6, 0)));

            player.Draw(3);
            Assert.That(player.Deck.CardCountByZone, Is.EqualTo(new CardRepartition(3, 7, 0, 0)));

        }


        [Test, ExpectedException(typeof(NotEnoughCardsException))]
        public void ExceptionWhenDrawFromEmptyDeck()
        {
            IoC.Kernel.ReBind<IDeck>().To<StartingDeck>();

            var player = IoC.Kernel.Get<Player>();
            player.Ready();

            Assert.That(player.Deck.CardCountByZone, Is.EqualTo(new CardRepartition(5, 5, 0, 0)));

            player.Draw(5);
            Assert.That(player.Deck.CardCountByZone, Is.EqualTo(new CardRepartition(0, 10, 0, 0)));

            player.Draw(2);

        }


        [Test]
        public void CardGet()
        {
            TestCard.ResetCounters();
            IoC.Kernel.Rebind<ICardShuffler>().To<CardShuffleBySorting>();

            var player = IoC.Kernel.Get<Player>();
            // player.Ready();

            var sample = player.Deck.Library.Get(1, Position.Top).ToList();

            Assert.That(sample.Count(), Is.EqualTo(1));
            Assert.That(((TestCard)sample.First()).Index, Is.EqualTo(0));
            Assert.That(((TestCard)sample.Last()).Index, Is.EqualTo(0));

            sample = player.Deck.Library.Get(5, Position.Top).ToList();

            Assert.That(sample.Count(), Is.EqualTo(5));
            Assert.That(((TestCard)sample.First()).Index, Is.EqualTo(0));
            Assert.That(((TestCard)sample.Last()).Index, Is.EqualTo(4));

            sample = player.Deck.Library.Get(1, Position.Bottom).ToList();

            Assert.That(sample.Count(), Is.EqualTo(1));
            Assert.That(((TestCard)sample.First()).Index, Is.EqualTo(9));
            Assert.That(((TestCard)sample.Last()).Index, Is.EqualTo(9));

            sample = player.Deck.Library.Get(4, Position.Bottom).ToList();

            Assert.That(sample.Count(), Is.EqualTo(4));
            Assert.That(((TestCard)sample.First()).Index, Is.EqualTo(6));
            Assert.That(((TestCard)sample.Last()).Index, Is.EqualTo(9));
        }


  

        [Test, ExpectedException(typeof(NotEnoughCardsException))]
        public void GetTooManyCardsFromZone()
        {
            IoC.Kernel.ReBind<IDeck>().To<TestDeck>();

            var deck = IoC.Kernel.Get<IDeck>();
            deck.Ready();

            Assert.That(deck.Cards.Count, Is.EqualTo(10));

            deck.Library.Get(11);

        }
        
    }
}