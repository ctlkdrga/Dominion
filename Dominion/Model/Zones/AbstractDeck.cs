﻿using System;
using System.Collections.Generic;
using System.Linq;
using gbd.Dominion.Injection;
using gbd.Dominion.Model.Cards;
using gbd.Dominion.Model.GameMechanics;
using Ninject;

namespace gbd.Dominion.Model.Zones
{
    public abstract class AbstractDeck: IDeck
    {
  

        [Inject]
        protected AbstractDeck(ILibrary lib, IDiscardPile discard, IBattleField field, IHand hand)
        {
            DiscardPile = discard;
            Library = lib;
            BattleField = field;
            Hand = hand;
        }
        
        
        public IHand Hand { get; protected set; }
        public IDiscardPile DiscardPile { get; protected set; }
        public ILibrary Library { get; protected set; }

        public IBattleField BattleField { get; protected set; }

        public IList<ICard> Cards
        {
            get
            {
                var toreturn = new List<ICard>();
                toreturn.AddRange(Library.Cards);
                toreturn.AddRange(Hand.Cards);
                toreturn.AddRange(DiscardPile.Cards);
                toreturn.AddRange(BattleField.Cards);

                return toreturn.AsReadOnly();
            }

            set { throw new NotImplementedException(); }
        }

        public IEnumerable<ICard> Get(int amount, Position positionFrom = Position.Top)
        {
            throw new InvalidOperationException("Cannot Get cards from a deck. Try from Library instead.");
        }

        public void SortCards(Func<ICard, IComparable> comparer)
        {
            throw new InvalidOperationException("Cannot sort cards from a deck. Try sorting the library instead.");
        }

        public int TotalCardsAvailable
        {
            get { return Cards.Count; }
        }


        public int Score
        {
            get { return Cards.Sum(card => card.Mechanics.VictoryPoints); }
        }

        public CardRepartition CardCountByZone
        {
            get
            {
                return new CardRepartition(Library.Cards.Count, Hand.Cards.Count, DiscardPile.Cards.Count, BattleField.Cards.Count);
            }
        }


        public void EndOfTurnCleanup()
        {
            // TODO: implement StayInPlayOnce

            BattleField.Cards.ToList().ForEach(c => c.Attributes.Clear());

            BattleField.Cards.MoveTo(DiscardPile);
            Hand.Cards.MoveTo(DiscardPile);

            Library.MoveCardsTo(Hand, 5);
        }

        public ILibrary ShuffleDiscardToLibrary()
        {
            DiscardPile.Cards.MoveTo(Library);
            IoC.Kernel.Get<ICardShuffler>().Shuffle(Library);

            return Library;
        }

        public IZone Get(ZoneChoice zone)
        {
            switch (zone)
            {
                case ZoneChoice.Discard:
                    return DiscardPile;

                case ZoneChoice.Library:
                    return Library;

                case ZoneChoice.Hand:
                    return Hand;

                case ZoneChoice.Play:
                    return BattleField;

                default:
                    throw new InvalidOperationException();
            }
        }


        public void Ready()
        {
            DiscardPile.Ready();
            Hand.Ready();
            BattleField.Ready();

            DiscardPile.MoveCardsTo(Library, DiscardPile.Cards.Count);
            Hand.MoveCardsTo(Library, Hand.Cards.Count);
            BattleField.MoveCardsTo(Library, BattleField.Cards.Count);

            Library.Ready(this);
        }

        public override string ToString()
        {
            return String.Format("{0} # {1} with {2}",   
                    this.GetType().Name,
                    this.GetHashCode(),
                    this.CardCountByZone);
        }
    }
}
