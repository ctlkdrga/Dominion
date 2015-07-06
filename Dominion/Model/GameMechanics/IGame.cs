using System.Collections.Generic;
using gbd.Dominion.Model.Zones;

namespace gbd.Dominion.Model.GameMechanics
{
    public interface IGame: IGameObject
    {

        IList<IPlayer> Players { get;  }
        ISupplyZone SupplyZone { get; }


        IPlayer CurrentPlayer { get; }
        ITrashZone Trash { get; set; }


        IList<IPlayer> GetPlayers(PlayerChoice who);

        void NextTurn();
        void EndActionsPhase();
    }
}