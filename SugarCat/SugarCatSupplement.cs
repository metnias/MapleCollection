using HUD;
using Noise;
using UnityEngine;

namespace MapleCollection.SugarCat
{
    /// <summary>
    /// "the sugar shares the same diet behaviour and food points as the white slugcat"
    /// "the orange dots in the reference image is a custom mechanic that's the "photosynthesis" points, which can be acquired while standing under sunlight, and a maximum of 3 points can be acquired"
    /// "the sugar can consume 1 food point to temporarily secrete sugary sticky fluids, which causes it to leave sticky trails wherever it slides, or it can shoot sugary balls like the red lizard, which sticks onto creatures"
    /// "as its body contains a lot of sugar which makes it smell nice, the hunters have higher hostility towards it"
    /// "when using its ability, it prioritizes consuming photosynthesis points. While asleep and food points are not enough, but there's still remaining photosynthesis points, then it will be consumed to replace the missing food point(s)"
    /// "Note: If it's too hard to implement the photosynthesis point system, then it uses food points instead"
    /// I also have a new idea. The mucus of the sugar cat does not affect its speed on flat ground, but if the mucus is on a vertical wall, it can climb like a zombie cat.
    /// </summary>
    public class SugarCatSupplement : CatSupplement
    {
        public SugarCatSupplement(Player owner) : base(owner)
        {
            this.charge = 0f;
        }

        public float charge;

        public override void Update()
        {
            base.Update();
            if (this.owner.room == null || this.owner.mainBodyChunk == null) { return; }
            if (this.owner.slatedForDeletetion)
            {
                return;
            }
        }

        private void DiminishUpdate()
        {
            /*
            if (this.sporeEnergy < (this.energyLimit * energyPerHunger))
            {
                if (this.owner.playerState.foodInStomach >= 1 && this.owner.abstractCreature.world.game.GetStorySession.saveState.totFood >= 1)
                {
                    this.owner.AddFood(-1);
                    this.sporeEnergy += energyPerHunger;
                }
                else if (this.energyLimit > 1)
                {
                    this.owner.slugcatStats.foodToHibernate++;
                    this.energyLimit--;
                    if (meter != null)
                    {
                        meter.survivalLimit++;
                        meter.RefuseFood();
                    }
                }
            }*/
        }
    }
}