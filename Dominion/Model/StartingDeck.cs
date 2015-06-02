using System;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using org.gbd.Dominion.Contents;
using org.gbd.Dominion.Model.Cards;
using org.gbd.Dominion.Tools;

namespace org.gbd.Dominion.Model
{
    public class StartingDeck : Deck, IDeck
    {
        public StartingDeck()
        {
            // TODO/ Ultimately this should be sent through NInject (and this class obsoleted?)

            for (var i = 0; i < 7; i++)
                DiscardPile.Cards.Add(new Copper());

            for (var i = 0; i < 3; i++)
                DiscardPile.Cards.Add(new Estate());

        }


    }
}