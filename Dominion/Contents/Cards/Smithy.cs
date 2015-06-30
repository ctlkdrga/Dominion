using gbd.Dominion.Model.Cards;

namespace gbd.Dominion.Contents.Cards
{
    public class Smithy : Card, ICard
    {
        public Smithy(ICardMechanics mechanics, GameExtension ext, Include inc)
            : base(mechanics, ext, inc) { }

   
        public override ICardMechanics Mechanics { get; protected set; }

        
    }
}