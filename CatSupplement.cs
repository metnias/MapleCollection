using HUD;
using Noise;
using UnityEngine;

namespace MapleCollection
{
    public abstract class CatSupplement
    {
        public CatSupplement(AbstractCreature owner)
        {
            this.owner = owner;
        }

        public readonly AbstractCreature owner;
        public Player player => owner.realizedCreature as Player;
        public static FoodMeter meter;
        internal ChunkSoundEmitter soundLoop;

        public virtual void Update()
        {
            if (this.player.room == null || this.player.mainBodyChunk == null) { return; }
        }

        public virtual void Destroy()
        {
        }
    }
}