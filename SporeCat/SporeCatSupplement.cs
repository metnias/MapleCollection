using HUD;
using Noise;
using UnityEngine;

namespace MapleCollection.SporeCat
{
    public class SporeCatSupplement : CatSupplement
    {
        public SporeCatSupplement(Player owner) : base(owner)
        {
            this.puffBallOnTail = new AbstractPhysicalObject[maxPuff];
            this.recovering = new int[maxPuff];
            this.farewell = false;
            for (int i = 0; i < maxPuff; i++)
            { this.recovering[i] = recoverTime; }
            this.energyLimit = this.owner.Malnourished ? 1 : 4;
            this.sporeEnergy = energyPerHunger * (energyLimit + 1);
        }

        private readonly AbstractPhysicalObject[] puffBallOnTail;
        private readonly int[] recovering;
        public const int recoverTime = 100; // frames to regrow one puffball
        private int pressedTime;
        private const int chargeTime = 30; // frames to hold Grab for charge explode
        public float Charge => Mathf.Clamp01((float)this.pressedTime / chargeTime);
        private WorldCoordinate pos;
        private const int maxPuff = 4;
        private int toBlink;
        private bool farewell;
        private const int energyPerHunger = recoverTime * 6; // Number of Puffball for each hunger pip
        private int sporeEnergy, energyLimit;

        private SporeCatPuffBall PuffBall(int idx) => (this.puffBallOnTail[idx].realizedObject as SporeCatPuffBall);

        private int GetPuffNum()
        {
            int r = 0;
            for (int i = 0; i < maxPuff; i++)
            { if (puffBallOnTail[i] != null && puffBallOnTail[i].realizedObject != null && PuffBall(i).Matured) r++; }
            return r;
        }

        public int GetPuffIndex(SporeCatPuffBall ball)
        {
            for (int i = 0; i < maxPuff; i++)
            { if (puffBallOnTail[i] != null && puffBallOnTail[i].realizedObject != null && PuffBall(i) == ball) return i; }
            return -1;
        }

        public override void Update()
        {
            base.Update();
            if (this.owner.room == null || this.owner.mainBodyChunk == null) { return; }
            if (this.owner.slatedForDeletetion)
            {
                for (int i = 0; i < maxPuff; i++)
                {
                    if (puffBallOnTail[i] != null && puffBallOnTail[i].realizedObject != null)
                    { this.PuffBall(i).UnstickFromPlayer(); this.PuffBall(i).Destroy(); }
                }
                return;
            }
            // Update PuffBalls
            this.pos = new WorldCoordinate(this.owner.abstractCreature.pos.room,
                this.owner.abstractCreature.pos.x, this.owner.abstractCreature.pos.y, this.owner.abstractCreature.pos.abstractNode);
            bool recovered = this.owner.dead;
            for (int i = 0; i < maxPuff; i++)
            {
                if (!recovered && !this.owner.dead && this.sporeEnergy > 0)
                {
                    if (this.puffBallOnTail[i] == null) { if (this.recovering[i] > 0) { this.CreateBall(i); } }
                    if (this.recovering[i] < recoverTime) { this.recovering[i]++; this.sporeEnergy--; recovered = true; }
                }
                if (this.puffBallOnTail[i] != null)
                {
                    if (this.puffBallOnTail[i].realizedObject == null || this.puffBallOnTail[i].Room != this.owner.room.abstractRoom)
                    { this.puffBallOnTail[i].Destroy(); this.puffBallOnTail[i] = null; continue; } //this.recovering[i] = 0;

                    this.PuffBall(i).SetGrowth(this.recovering[i]);
                    this.PuffBall(i).parentDeco = ModifyCat.GetDeco(this.owner.graphicsModule as PlayerGraphics) as SporeCatDecoration;
                    if (PuffBall(i).mode == Weapon.Mode.OnBack)
                    {
                        if (this.owner.graphicsModule != null) { this.PuffBall(i).firstChunk.HardSetPosition((this.owner.graphicsModule as PlayerGraphics).tail[i].pos + new Vector2(0f, 5f)); }
                        else { this.PuffBall(i).firstChunk.HardSetPosition(this.owner.bodyChunks[1].pos); }
                    }
                    else
                    {
                        this.PuffBall(i).UnstickFromPlayer();
                        this.puffBallOnTail[i] = null;
                        this.recovering[i] = 0;
                    }
                }
            }

            if (this.owner.dangerGrasp != null) { this.EmergencyReflex(); }
            else if (this.owner.Consious) { this.ChargeUpdate(); }
            if (this.owner.dead) { this.farewell = true; }
            else if (this.owner.abstractCreature.world.game.IsStorySession) { this.DiminishUpdate(); }
            if (this.pressedTime > 0 && this.GetPuffNum() > maxPuff - 2)
            {
                if (this.soundLoop != null)
                {
                    this.soundLoop.alive = true;
                    this.soundLoop.pitch = Mathf.Lerp(0.7f, 1.3f, this.Charge);
                    this.soundLoop.volume = 0.3f + this.Charge * 0.6f;
                }
                else
                { // SoundID.Cyan_Lizard_Gas_Leak_LOOP
                    this.soundLoop = this.owner.room.PlaySound(SoundID.Hazer_Squirt_Smoke_LOOP, this.owner.bodyChunks[1], true, 1f, 1f);
                }
            }
            else if (this.soundLoop != null)
            { this.soundLoop.alive = false; this.soundLoop = null; }
        }

        public override void Destroy()
        {
            base.Destroy();
            for (int i = 0; i < maxPuff; i++)
            {
                if (puffBallOnTail[i] != null)
                {
                    if (puffBallOnTail[i].realizedObject != null)
                    { this.PuffBall(i).UnstickFromPlayer(); this.PuffBall(i).Destroy(); }
                    else puffBallOnTail[i].Destroy();
                }
            }
            return;
        }

        private void DiminishUpdate()
        {
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
            }
        }

        private void ChargeUpdate()
        {
            if (!this.owner.input[0].pckp || this.owner.input[0].y >= 0 || this.owner.swallowAndRegurgitateCounter > 90
                || this.GetPuffNum() < maxPuff)
            { this.pressedTime = 0; return; }
            if (this.owner.bodyMode == Player.BodyModeIndex.Crawl || this.owner.bodyMode == Player.BodyModeIndex.CorridorClimb)
            {
                this.owner.Blink(4);
                this.pressedTime++;
                if (this.puffBallOnTail[this.toBlink] != null)
                {
                    this.PuffBall(this.toBlink).Blink();
                    this.toBlink++;
                    if (this.toBlink >= maxPuff) { this.toBlink = 0; }
                }
                if (this.pressedTime > chargeTime)
                {
                    this.pressedTime = 0;
                    this.ExplodeAll(true);
                }
            }
            else { this.pressedTime = 0; }
        }

        private void EmergencyReflex()
        {
            if (this.owner.dead)
            {
                if (!this.farewell) { this.farewell = true; this.ExplodeAll(false); }
                return;
            }
            if (this.owner.dangerGraspTime > 60) { this.pressedTime = 0; return; }
            this.pressedTime += chargeTime / 15;
            if (this.puffBallOnTail[this.toBlink] != null)
            {
                this.PuffBall(this.toBlink).Blink();
                this.toBlink++;
                if (this.toBlink >= maxPuff) { this.toBlink = 0; }
            }
            if (this.pressedTime > chargeTime && this.GetPuffNum() > maxPuff - 2)
            {
                this.pressedTime = 0;
                this.ExplodeAll(false);
                this.owner.dangerGrasp.grabber.Stun(20);
                this.owner.dangerGrasp.Release();
            }
        }

        public void ExplodeAll(bool super)
        {
            bool deer = true;
            for (int j = 0; j < maxPuff; j++)
            {
                if (this.puffBallOnTail[j] == null || !this.PuffBall(j).Matured) { continue; }
                this.PuffBall(j).UnstickFromPlayer();
                this.PuffBall(j).superBoom = super;
                this.PuffBall(j).deerCall = deer; if (deer) { deer = false; }
                this.PuffBall(j).thrownBy = this.owner;
                this.PuffBall(j).Explode();
                this.puffBallOnTail[j] = null;
                this.recovering[j] = super ? recoverTime / -2 : 0;
            }
            this.owner.AerobicIncrease(super ? 0.9f : 0.6f);
            this.owner.room.InGameNoise(new InGameNoise(this.owner.bodyChunks[1].pos, super ? 300f : 200f, this.owner, 1f));
        }

        private void CreateBall(int i)
        {
            this.puffBallOnTail[i] = new AbstractConsumable(this.owner.room.world, MapleEnums.SporePuffBall,
                null, this.pos, this.owner.room.game.GetNewID(), -1, -1, null);
            this.owner.room.abstractRoom.AddEntity(this.puffBallOnTail[i]);
            this.puffBallOnTail[i].RealizeInRoom();
            this.PuffBall(i).parentDeco = ModifyCat.GetDeco(this.owner.graphicsModule as PlayerGraphics) as SporeCatDecoration;
            this.PuffBall(i).OnBackMode();
            this.PuffBall(i).StickToPlayer(this.owner);
            if (i % 2 == 0)
            { this.PuffBall(i).hideBehindPlayer = new bool?(true); }
        }
    }
}