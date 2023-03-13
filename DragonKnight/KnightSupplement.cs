using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapleCollection.DragonKnight
{
    public class KnightSupplement : CatSupplement
    {
        public KnightSupplement(Player owner) : base(owner)
        {
        }

        public AbstractCreature partner;
        public PartnerLizard Lizard => partner.realizedCreature as PartnerLizard;
    }
}