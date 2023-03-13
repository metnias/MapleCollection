using HUD;
using Noise;
using UnityEngine;

namespace MapleCollection
{
    public abstract class CatSupplement
    {
        public CatSupplement(Player owner)
        {
            this.owner = owner;
        }

        public readonly Player owner;
        public static FoodMeter meter;
        internal ChunkSoundEmitter soundLoop;

        public virtual void Update()
        {
            if (this.owner.room == null || this.owner.mainBodyChunk == null) { return; }
        }

        public virtual void Destroy()
        {
        }
    }
}