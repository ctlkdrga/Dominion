using gbd.Dominion.Model.Cards;

namespace gbd.Dominion.Contents.Cards
{
    public class Chapel: SelectableCard
    {
        public override GameExtension Extension
        {
            get { return GameExtension.BaseGame; }
        }
    }
}