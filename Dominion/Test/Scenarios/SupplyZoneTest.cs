﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public class SupplyZoneTest: BaseTest
    {

        [SetUp]
        public new void SetUp()
        {
            base.SetUp();
        }




        [Test]
        public void SupplyZone()
        {
            IoC.Kernel.ReBind<ISupplyPile>().To<TestSupplyPile>();
            IoC.Kernel.ReBind<ISupplyZone>().To<TestSupplyZone>();
            
            IoC.Kernel.ReBind<ICollection<IPlayer>>()
                .ToConstructor(x => new List<IPlayer>(x.Inject<IList<IPlayer>>()));


            var player = IoC.Kernel.Get<IPlayer>();
            var supply = IoC.Kernel.Get<ISupplyZone>();

            Assert.That(player.Deck.Cards.Count, Is.EqualTo(10));
            Assert.That(supply.Cards.Count, Is.EqualTo(100));



        }


        [Test]
        public void SupplyPile()
        {

            var pile = IoC.Kernel.Get<ISupplyPile>();
            var deck = IoC.Kernel.Get<IDeck>();

            Assert.That(deck.Cards.Count, Is.EqualTo(10));
            Assert.That(pile.Cards.Count, Is.EqualTo(10));
        }



        [Test]
        public void MoveCardFromSupply()
        {

            var pile = IoC.Kernel.Get<ISupplyPile>();
            var player = IoC.Kernel.Get<IPlayer>();

            Assert.That(player.Deck.Cards.Count, Is.EqualTo(10));
            Assert.That(pile.Cards.Count, Is.EqualTo(10));


            Game.MoveCards(pile, player.DiscardPile);

            Assert.That(player.Deck.Cards.Count, Is.EqualTo(11));
            Assert.That(pile.Cards.Count, Is.EqualTo(9));
        }


        [Test]
        public void EnoughCardsForPlayableSupplyZone()
        {
            var classes = Assembly.GetExecutingAssembly().GetTypes();

            var cards = classes.Where(t => typeof(SelectableCard).IsAssignableFrom(t)
                                                               && t.IsInterface == false
                                                               && t.IsAbstract == false);


            Assert.That(cards.Count(), Is.GreaterThanOrEqualTo(10));
        }

        




    }
}
