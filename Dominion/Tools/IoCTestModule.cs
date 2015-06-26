﻿using gbd.Dominion.Contents.Cards;
using gbd.Dominion.Model;
using gbd.Dominion.Model.Cards;
using gbd.Dominion.Model.GameMechanics;
using gbd.Dominion.Model.GameMechanics.AI;
using gbd.Dominion.Model.Zones;
using gbd.Dominion.Test.Utilities;
using gbd.Tools.NInject;
using Ninject.Modules;

namespace gbd.Dominion.Tools
{
    public class IoCTestModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IIntelligence>().To<RandomAi>();
            Bind<IAi>().To<RandomAi>();
            Bind<ISupplyZone>().To<TestSupplyZone>();

            Bind<IDeck>().To<TestDeck>();
            this.Kernel.BindMultipleTimesTo<ICard, TestCard>(10).WhenAnyAncestorOfType<TestCard, ILibrary>();
            this.Kernel.BindMultipleTimesTo<ICard, TestCard>(10).WhenAnyAncestorOfType<TestCard, ISupplyPile>();


            this.Kernel.BindMultipleTimesTo<ISupplyPile, TestSupplyPile>(10);
            
            
            //this.Kernel.BindMultipleTimesTo<ICard, TestCard>(10);



            // This will be bound to CurrentPlayer
            //Bind<IPlayer>().To<Player>();

            //Kernel.Bind<ICardType>().ToConstructor(x => new VictoryType(0)).WhenAnyAncestorOfType<VictoryType, TestCard>();
            //Kernel.Bind<Resources>().ToConstructor(x => new Resources(0)).WhenAnyAncestorOfType<Resources, TestCard>();

            

        }

    }
}