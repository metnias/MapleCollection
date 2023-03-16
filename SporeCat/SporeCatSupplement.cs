using HUD;
using Noise;
using UnityEngine;

namespace MapleCollection.SporeCat
{
    public class SporeCatSupplement : CatSupplement
    {
        public SporeCatSupplement(AbstractCreature owner) : base(owner)
        {
            this.puffBallOnTail = new AbstractPhysicalObject[maxPuff];
            this.recovering = new int[maxPuff];
            this.farewell = false;
            for (int i = 0; i < maxPuff; i++)
            { this.recovering[i] = recoverTime; }
            this.energyLimit = this.player.Malnourished ? 1 : 4;
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
            if (this.player.room == null || this.player.mainBodyChunk == null) { return; }
            if (this.player.slatedForDeletetion)
            {
                for (int i = 0; i < maxPuff; i++)
                {
                    if (puffBallOnTail[i] != null && puffBallOnTail[i].realizedObject != null)
                    { this.PuffBall(i).UnstickFromPlayer(); this.PuffBall(i).Destroy(); }
                }
                return;
            }
            // Update PuffBalls
            this.pos = new WorldCoordinate(this.player.abstractCreature.pos.room,
                this.player.abstractCreature.pos.x, this.player.abstractCreature.pos.y, this.player.abstractCreature.pos.abstractNode);
            bool recovered = this.player.dead;
            for (int i = 0; i < maxPuff; i++)
            {
                if (!recovered && !this.player.dead && this.sporeEnergy > 0)
                {
                    if (this.puffBallOnTail[i] == null) { if (this.recovering[i] > 0) { this.CreateBall(i); } }
                    if (this.recovering[i] < recoverTime) { this.recovering[i]++; this.sporeEnergy--; recovered = true; }
                }
                if (this.puffBallOnTail[i] != null)
                {
                    if (this.puffBallOnTail[i].realizedObject == null || this.puffBallOnTail[i].Room != this.player.room.abstractRoom)
                    { this.puffBallOnTail[i].Destroy(); this.puffBallOnTail[i] = null; continue; } //this.recovering[i] = 0;

                    this.PuffBall(i).SetGrowth(this.recovering[i]);
                    this.PuffBall(i).parentDeco = ModifyCat.GetDeco(this.player.graphicsModule as PlayerGraphics) as SporeCatDecoration;
                    if (PuffBall(i).mode == Weapon.Mode.OnBack)
                    {
                        if (this.player.graphicsModule != null) { this.PuffBall(i).firstChunk.HardSetPosition((this.player.graphicsModule as PlayerGraphics).tail[i].pos + new Vector2(0f, 5f)); }
                        else { this.PuffBall(i).firstChunk.HardSetPosition(this.player.bodyChunks[1].pos); }
                    }
                    else
                    {
                        this.PuffBall(i).UnstickFromPlayer();
                        this.puffBallOnTail[i] = null;
                        this.recovering[i] = 0;
                    }
                }
            }

            if (this.player.dangerGrasp != null) { this.EmergencyReflex(); }
            else if (this.player.Consious) { this.ChargeUpdate(); }
            if (this.player.dead) { this.farewell = true; }
            else if (this.player.abstractCreature.world.game.IsStorySession) { this.DiminishUpdate(); }
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
                    this.soundLoop = this.player.room.PlaySound(SoundID.Hazer_Squirt_Smoke_LOOP, this.player.bodyChunks[1], true, 1f, 1f);
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
                if (this.player.playerState.foodInStomach >= 1 && this.player.abstractCreature.world.game.GetStorySession.saveState.totFood >= 1)
                {
                    this.player.AddFood(-1);
                    this.sporeEnergy += energyPerHunger;
                }
                else if (this.energyLimit > 1)
                {
                    this.player.slugcatStats.foodToHibernate++;
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
            if (!this.player.input[0].pckp || this.player.input[0].y >= 0 || this.player.swallowAndRegurgitateCounter > 90
                || this.GetPuffNum() < maxPuff)
            { this.pressedTime = 0; return; }
            if (this.player.bodyMode == Player.BodyModeIndex.Crawl || this.player.bodyMode == Player.BodyModeIndex.CorridorClimb)
            {
                this.player.Blink(4);
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
            if (this.player.dead)
            {
                if (!this.farewell) { this.farewell = true; this.ExplodeAll(false); }
                return;
            }
            if (this.player.dangerGraspTime > 60) { this.pressedTime = 0; return; }
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
                this.player.dangerGrasp.grabber.Stun(20);
                this.player.dangerGrasp.Release();
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
                this.PuffBall(j).thrownBy = this.player;
                this.PuffBall(j).Explode();
                this.puffBallOnTail[j] = null;
                this.recovering[j] = super ? recoverTime / -2 : 0;
            }
            this.player.AerobicIncrease(super ? 0.9f : 0.6f);
            this.player.room.InGameNoise(new InGameNoise(this.player.bodyChunks[1].pos, super ? 300f : 200f, this.player, 1f));
        }

        private void CreateBall(int i)
        {
            this.puffBallOnTail[i] = new AbstractConsumable(this.player.room.world, MapleEnums.SporePuffBall,
                null, this.pos, this.player.room.game.GetNewID(), -1, -1, null);
            this.player.room.abstractRoom.AddEntity(this.puffBallOnTail[i]);
            this.puffBallOnTail[i].RealizeInRoom();
            this.PuffBall(i).parentDeco = ModifyCat.GetDeco(this.player.graphicsModule as PlayerGraphics) as SporeCatDecoration;
            this.PuffBall(i).OnBackMode();
            this.PuffBall(i).StickToPlayer(this.player);
            if (i % 2 == 0)
            { this.PuffBall(i).hideBehindPlayer = new bool?(true); }
        }
    }
}