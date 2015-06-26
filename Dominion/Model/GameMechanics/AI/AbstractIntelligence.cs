﻿using System.Collections.Generic;
using gbd.Dominion.Model.Cards;

namespace gbd.Dominion.Model.GameMechanics.AI
{
    public abstract class AbstractIntelligence: IIntelligence
    {

        public Player Player { get; protected set; }



        public void Init(Player player)
        {
            Player = player;
        }
        
        
        public abstract IEnumerable<ICard> ChooseAndDiscard(int amount);

    }
}