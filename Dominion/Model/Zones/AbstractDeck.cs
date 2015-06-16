﻿using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using gbd.Dominion.Model.Cards;
using gbd.Dominion.Tools;
using gbd.Tools.Cli;

namespace gbd.Dominion.Model.Zones
{
    public abstract class AbstractDeck: IDeck
    {
        protected AbstractDeck(IDiscardPile discard,  IBattleField bf, IHand hand)
        {
            DiscardPile = discard;
            BattleField = bf;
            Hand = hand;
        }


        public IHand Hand { get; protected set; }
        public IDiscardPile DiscardPile { get; protected set; }
        public ILibrary Library { get; set; }
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


        public int CurrentScore
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

        [Obsolete] 
        public void Add(ICard card, CardsPile destination)
        {
            Add(card, destination, Position.Top);
        }

        [Obsolete]
        public void Add(ICard card, CardsPile destination, Position position)
        {
            switch (destination)
            {
                case CardsPile.Discard:
                    DiscardPile.Cards.Add(card);
                    break;

                case CardsPile.Hand:
                    Hand.Add(card);
                    break;

                case CardsPile.Library:
                    Library.Add(card, position);
                    break;

                default:
                    throw new InvalidOperationException();

            }
        }

        public void EndOfTurnCleanup()
        {
            foreach (var card in BattleField.Cards)
            {
                if (card.Attributes.Contains(CardAttribute.StayInPlayOnce))
                {
                    card.Attributes.Remove(CardAttribute.StayInPlayOnce);
                    continue;
                }

                DiscardPile.Cards.Add(card);
                BattleField.Cards.Remove(card);
            }

            BattleField.Cards.Clear();
        }

        public ILibrary ShuffleDiscardToLibrary()
        {
            foreach (var card in DiscardPile.Cards)
            {
                Library.Cards.Add(card);
            }
            Library.Cards.Shuffle();
            DiscardPile.Cards.Clear();

            return Library;
        }

        public void GetReadyToStartGame()
        {
            Library.Init(this);
        }

    }
}
