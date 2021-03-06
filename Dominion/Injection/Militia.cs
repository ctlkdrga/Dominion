﻿using gbd.Dominion.Contents;
using gbd.Dominion.Model.Cards;

namespace gbd.Dominion.Injection
{
    public class Militia: Card, ICard
    {
        public Militia(ICardMechanics mechanics, GameExtension ext, Include inc) 
            : base(mechanics, ext, inc) { }

        public override ICardMechanics Mechanics { get; protected set; }
    }
}