using CatSub.Cat;
using Noise;
using RWCustom;
using System;
using System.Text;
using UnityEngine;

namespace MapleCollection.SporeCat
{
    public class SporeCatSupplement : CatSupplement
    {
        public SporeCatSupplement(Player player) : base(player)
        {
            puffBallOnTail = new AbstractPhysicalObject[maxPuff];
            recovering = new int[maxPuff];
            farewell = false;
            for (int i = 0; i < maxPuff; i++)
                recovering[i] = recoverTime;
            energyLimit = self.Malnourished ? 1 : 4;
            sporeEnergy = energyPerHunger * (energyLimit + 1);
        }

        public SporeCatSupplement() : base()
        {
        }

        private readonly AbstractPhysicalObject[] puffBallOnTail;
        private readonly int[] recovering;
        public const int recoverTime = 100; // frames to regrow one puffball
        private int pressedTime;
        private const int chargeTime = 30; // frames to hold Grab for charge explode
        public float Charge => Mathf.Clamp01((float)this.pressedTime / chargeTime);

        public override string TargetSubVersion => "1.2";

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

        public override void Update(On.Player.orig_Update orig, bool eu)
        {
            base.Update(orig, eu);
            if (self.room == null || self.mainBodyChunk == null) return;
            if (self.slatedForDeletetion)
            {
                for (int i = 0; i < maxPuff; i++)
                {
                    if (puffBallOnTail[i] != null && puffBallOnTail[i].realizedObject != null)
                    { PuffBall(i).UnstickFromPlayer(); PuffBall(i).Destroy(); }
                }
                return;
            }
            // Update PuffBalls
            pos = new WorldCoordinate(self.abstractCreature.pos.room,
                self.abstractCreature.pos.x, self.abstractCreature.pos.y, self.abstractCreature.pos.abstractNode);
            bool recovered = self.dead;
            for (int i = 0; i < maxPuff; i++)
            {
                if (!recovered && !self.dead && sporeEnergy > 0)
                {
                    if (puffBallOnTail[i] == null) { if (recovering[i] > 0) CreateBall(i); }
                    if (recovering[i] < recoverTime) { recovering[i]++; sporeEnergy--; recovered = true; }
                }
                if (puffBallOnTail[i] != null)
                {
                    if (puffBallOnTail[i].realizedObject == null || puffBallOnTail[i].Room != self.room.abstractRoom)
                    { puffBallOnTail[i].Destroy(); puffBallOnTail[i] = null; continue; } //recovering[i] = 0;

                    PuffBall(i).SetGrowth(recovering[i]);
                    if (TryGetDeco(out SporeCatDecoration sporeDeco))
                        PuffBall(i).parentDeco = sporeDeco;
                    if (PuffBall(i).mode == Weapon.Mode.OnBack)
                    {
                        if (self.graphicsModule != null) { PuffBall(i).firstChunk.HardSetPosition((self.graphicsModule as PlayerGraphics).tail[i].pos + new Vector2(0f, 5f)); }
                        else { PuffBall(i).firstChunk.HardSetPosition(self.bodyChunks[1].pos); }
                    }
                    else
                    {
                        PuffBall(i).UnstickFromPlayer();
                        puffBallOnTail[i] = null;
                        recovering[i] = 0;
                    }
                }
            }

            if (self.dangerGrasp != null) { EmergencyReflex(); }
            else if (self.Consious) { ChargeUpdate(); }
            if (self.dead) { farewell = true; }
            else if (self.abstractCreature.world.game.IsStorySession) { DiminishUpdate(); }
            if (pressedTime > 0 && GetPuffNum() > maxPuff - 2)
            {
                if (soundLoop != null)
                {
                    soundLoop.alive = true;
                    soundLoop.pitch = Mathf.Lerp(0.7f, 1.3f, Charge);
                    soundLoop.volume = 0.3f + Charge * 0.6f;
                }
                else
                { // SoundID.Cyan_Lizard_Gas_Leak_LOOP
                    soundLoop = self.room.PlaySound(SoundID.Hazer_Squirt_Smoke_LOOP, self.bodyChunks[1], true, 1f, 1f);
                }
            }
            else if (soundLoop != null)
            { soundLoop.alive = false; soundLoop = null; }
        }

        public override void Destroy(On.Player.orig_Destroy orig)
        {
            base.Destroy(orig);
            for (int i = 0; i < maxPuff; i++)
            {
                if (puffBallOnTail[i] != null)
                {
                    if (puffBallOnTail[i].realizedObject != null)
                    { PuffBall(i).UnstickFromPlayer(); PuffBall(i).Destroy(); }
                    else puffBallOnTail[i].Destroy();
                }
            }
            return;
        }

        private void DiminishUpdate()
        {
            if (sporeEnergy < (energyLimit * energyPerHunger))
            {
                if (self.playerState.foodInStomach >= 1 && self.abstractCreature.world.game.GetStorySession.saveState.totFood >= 1)
                {
                    self.SubtractFood(1);
                    sporeEnergy += energyPerHunger;
                }
                else if (energyLimit > 1)
                {
                    ++self.slugcatStats.foodToHibernate;
                    --energyLimit;
                    if (self.room.game.IsStorySession)
                    {
                        var meter = self.room.game.cameras[0].hud.foodMeter;
                        if (meter != null)
                        {
                            ++meter.survivalLimit;
                            meter.RefuseFood();
                        }
                    }
                }
            }
        }

        private void ChargeUpdate()
        {
            if (!self.input[0].pckp || self.input[0].y >= 0 || self.swallowAndRegurgitateCounter > 90
                || GetPuffNum() < maxPuff)
            { pressedTime = 0; return; }
            if (self.bodyMode == Player.BodyModeIndex.Crawl || self.bodyMode == Player.BodyModeIndex.CorridorClimb)
            {
                self.Blink(4);
                pressedTime++;
                if (puffBallOnTail[toBlink] != null)
                {
                    PuffBall(toBlink).Blink();
                    toBlink++;
                    if (toBlink >= maxPuff) { toBlink = 0; }
                }
                if (pressedTime > chargeTime)
                {
                    pressedTime = 0;
                    ExplodeAll(true);
                }
            }
            else { pressedTime = 0; }
        }

        private void EmergencyReflex()
        {
            if (self.dead)
            {
                if (!farewell) { farewell = true; ExplodeAll(false); }
                return;
            }
            if (self.dangerGraspTime > 60) { pressedTime = 0; return; }
            pressedTime += chargeTime / 15;
            if (puffBallOnTail[toBlink] != null)
            {
                PuffBall(toBlink).Blink();
                toBlink++;
                if (toBlink >= maxPuff) { toBlink = 0; }
            }
            if (pressedTime > chargeTime && GetPuffNum() > maxPuff - 2)
            {
                pressedTime = 0;
                ExplodeAll(false);
                self.dangerGrasp.grabber.Stun(20);
                self.dangerGrasp.Release();
            }
        }

        public void ExplodeAll(bool super)
        {
            bool deer = true;
            for (int j = 0; j < maxPuff; j++)
            {
                if (puffBallOnTail[j] == null || !PuffBall(j).Matured) { continue; }
                PuffBall(j).UnstickFromPlayer();
                PuffBall(j).superBoom = super;
                PuffBall(j).deerCall = deer; if (deer) { deer = false; }
                PuffBall(j).thrownBy = self;
                PuffBall(j).Explode();
                puffBallOnTail[j] = null;
                recovering[j] = super ? recoverTime / -2 : 0;
            }
            self.AerobicIncrease(super ? 0.9f : 0.6f);
            self.room.InGameNoise(new InGameNoise(self.bodyChunks[1].pos, super ? 300f : 200f, self, 1f));
        }

        private void CreateBall(int i)
        {
            puffBallOnTail[i] = new AbstractConsumable(self.room.world, MapleEnums.SporePuffBall,
                null, pos, self.room.game.GetNewID(), -1, -1, null);
            self.room.abstractRoom.AddEntity(puffBallOnTail[i]);
            puffBallOnTail[i].RealizeInRoom();
            if (TryGetDeco(out SporeCatDecoration sporeDeco))
                PuffBall(i).parentDeco = sporeDeco;
            PuffBall(i).OnBackMode();
            PuffBall(i).StickToPlayer(self, this);
            if (i % 2 == 0) PuffBall(i).hideBehindPlayer = new bool?(true);
        }

        public override string ControlTutorial()
        {
            var text = new StringBuilder();
            text.Append($"{Translate("Sporecat interactions:")}{Environment.NewLine}{Environment.NewLine}");
            text.Append($"- {Translate("Sporecat's diet is exclusively insectivore, regardless of the prey's size")}{Environment.NewLine}");
            text.Append($"- {Translate("Hold UP and press PICK UP to grab a Puffball from the tail")}{Environment.NewLine}");
            text.Append($"- {Translate("Hold DOWN and PICK UP for charged explosion")}{Environment.NewLine}");
            text.Append($"- {Translate("However, using too many Puffballs costs hunger")}");
            return text.ToString();

            string Translate(string t) => Custom.rainWorld.inGameTranslator.Translate(t);
        }
    }
}