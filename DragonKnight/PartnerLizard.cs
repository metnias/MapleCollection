using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapleCollection.DragonKnight
{
    /// <summary>
    /// If this lizard is not saved in the same shelter, move to random shelter in the same subregion
    /// and do not save the game, set the lizard to Lost Mode
    /// </summary>
    public class PartnerLizard : Lizard
    {
        public PartnerLizard(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
        {
        }
    }
}
