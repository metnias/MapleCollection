using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static PlayerGraphics;

namespace MapleCollection
{
    public abstract class CatDecoration
    {
        public CatDecoration(AbstractCreature owner)
        {
            this.owner = owner;
            this.zRot = new float[5, 2];
        }

        public readonly AbstractCreature owner;
        public Player player => owner.realizedCreature as Player;

        protected internal PlayerGraphics OwnerGraphic => player.graphicsModule as PlayerGraphics;
        // private SporeCatSupplement OwnerSub => ModifyCat.GetSub(owner.player);

        internal FSprite[] sprites;
        internal FContainer container;

        private readonly float[,] zRot;

        protected bool updateZRot = false;

        public virtual void Update()
        {
            if (this.player == null || this.player.room == null || this.OwnerGraphic == null) { return; }
            if (!updateZRot) return;
            BackupZRotation();
            Vector2 upDir = Custom.DirVec(this.OwnerGraphic.drawPositions[1, 0], this.OwnerGraphic.drawPositions[0, 0]);
            float upRot = Custom.VecToDeg(upDir);
            CalculateHeadRotation();
            CalculateTailRotation();

            void BackupZRotation()
            {
                for (int q = 0; q < zRot.GetLength(0); q++) { zRot[q, 1] = zRot[q, 0]; }
            }

            void CalculateHeadRotation()
            {
                Vector2 lookDir = this.OwnerGraphic.lookDirection * 3f * (1f - this.player.sleepCurlUp);
                if (this.player.sleepCurlUp > 0f)
                {
                    lookDir.y -= 2f * this.player.sleepCurlUp;
                    lookDir.x -= 4f * Mathf.Sign(this.OwnerGraphic.drawPositions[0, 0].x - this.OwnerGraphic.drawPositions[1, 0].x) * this.player.sleepCurlUp;
                }
                else if (this.player.room.gravity == 0f) { }
                else if (this.player.Consious)
                {
                    if (this.player.bodyMode == Player.BodyModeIndex.Stand && this.player.input[0].x != 0)
                    { lookDir.x += 4f * Mathf.Sign(this.player.input[0].x); lookDir.y++; }
                    else if (this.player.bodyMode == Player.BodyModeIndex.Crawl)
                    { lookDir.x += 4f * Mathf.Sign(this.OwnerGraphic.drawPositions[0, 0].x - this.OwnerGraphic.drawPositions[1, 0].x); lookDir.y++; }
                }
                else { lookDir *= 0f; }
                float lookRot = lookDir.magnitude > float.Epsilon ? (Custom.VecToDeg(lookDir) -
                    (this.player.Consious && this.player.bodyMode == Player.BodyModeIndex.Crawl ? 0f : upRot)) : 0f;
                if (Mathf.Abs(lookRot) < 90f)
                { zRot[0, 0] = Custom.LerpMap(lookRot, 0f, Mathf.Sign(lookRot) * 90f, 0f, Mathf.Sign(lookRot) * 60f, 0.5f); }
                else
                { zRot[0, 0] = Custom.LerpMap(lookRot, Mathf.Sign(lookRot) * 180f, Mathf.Sign(lookRot) * 90f, Mathf.Sign(lookRot) * 60f, 0f, 0.5f); }
            }

            void CalculateTailRotation()
            {
                float totTailRot = zRot[0, 0], lastTailRot = -upRot;
                for (int t = 0; t < 4; t++)
                {
                    float tailRot = -Custom.AimFromOneVectorToAnother(t == 0 ? this.OwnerGraphic.drawPositions[1, 0]
                        : this.OwnerGraphic.tail[t - 1].pos, this.OwnerGraphic.tail[t].pos);
                    tailRot -= lastTailRot; lastTailRot += tailRot;
                    totTailRot += tailRot;
                    //dbg[t] = tailRot;
                    zRot[1 + t, 0] = totTailRot < 0f ? Mathf.Clamp(totTailRot, -90f, 0f) : Mathf.Clamp(totTailRot, 0f, 90f);
                }
            }
        }

#pragma warning disable IDE0060
        //private float[] dbg = new float[4];

        public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            if (this.container != null) { this.container.RemoveAllChildren(); this.container.RemoveFromContainer(); }
            this.container = new FContainer();
            //this.AddToContainer(sLeaser, rCam, null);
        }

        public virtual void SuckedIntoShortCut()
        {
            this.container.RemoveFromContainer();
        }

        public virtual void Reset()
        {
            for (int i = 0; i < zRot.GetLength(0); i++)
            { zRot[i, 0] = 0f; zRot[i, 1] = 0f; }
        }

        internal float GetZRot(int idx, float timeStacker) => -Mathf.Lerp(zRot[idx, 1], zRot[idx, 0], timeStacker);

        internal float GetZRot(float idx, float timeStacker) =>
            Mathf.Lerp(GetZRot(Mathf.FloorToInt(idx), timeStacker), GetZRot(Mathf.FloorToInt(idx) + 1, timeStacker), idx - Mathf.FloorToInt(idx));

        internal Vector2 GetPos(int idx, float timeStacker) => idx < 1 ? Vector2.Lerp(this.OwnerGraphic.drawPositions[idx, 1], this.OwnerGraphic.drawPositions[idx, 0], timeStacker) :
                Vector2.Lerp(this.OwnerGraphic.tail[idx - 1].lastPos, this.OwnerGraphic.tail[idx - 1].pos, timeStacker);

        internal Vector2 GetPos(float idx, float timeStacker) => Vector2.Lerp(GetPos(Mathf.FloorToInt(idx), timeStacker), GetPos(Mathf.FloorToInt(idx) + 1, timeStacker), idx - Mathf.FloorToInt(idx));

        internal float GetRad(int idx) => idx < 1 ? this.player.bodyChunks[0].rad : this.OwnerGraphic.tail[idx - 1].StretchedRad;

        internal float GetRad(float idx) => Mathf.Lerp(GetRad(Mathf.FloorToInt(idx)), GetRad(Mathf.FloorToInt(idx) + 1), idx - Mathf.FloorToInt(idx));

        internal Vector2 GetOuterPos(float idx, float timeStacker, float offsetDeg = 0f) => GetPos(idx, timeStacker)
            + Custom.PerpendicularVector(GetDir(idx, timeStacker)) * Mathf.Sin((GetZRot(idx, timeStacker) + offsetDeg) * Mathf.Deg2Rad);

        internal Vector2 GetDir(float idx, float timeStacker) =>
            Custom.DirVec(GetPos(Mathf.FloorToInt(idx), timeStacker), GetPos(Mathf.FloorToInt(idx) + 1, timeStacker));

        public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (this.player == null || this.player.room == null || this.OwnerGraphic == null)
            { this.container.isVisible = false; return; }
            this.container.isVisible = true;
        }

        public Color GetBodyColor() => bodyColor;

        public Color GetFaceColor() => faceColor;

        public Color GetThirdColor() =>
            ModManager.CoopAvailable && OwnerGraphic.useJollyColor
            ? JollyColor(player.playerState.playerNumber, 2) :
            CustomColorsEnabled() ? CustomColorSafety(2) : thirdColor;

        private Color bodyColor = Color.white;
        private Color faceColor = new Color(0.01f, 0.01f, 0.01f);
        protected Color thirdColor = new Color(0.01f, 0.01f, 0.01f);

        public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            bodyColor = sLeaser.sprites[0].color;
            faceColor = sLeaser.sprites[9].color;
            /*
            Color body = PlayerGraphics.SlugcatColor(this.owner.playerState.slugcatCharacter);
            Color face = palette.blackColor;
            if (this.OwnerGraphic.malnourished > 0f)
            {
                float num = (!this.owner.Malnourished) ? Mathf.Max(0f, this.OwnerGraphic.malnourished - 0.005f) : this.OwnerGraphic.malnourished;
                body = Color.Lerp(body, Color.gray, 0.4f * num);
                face = Color.Lerp(face, Color.Lerp(Color.white, palette.fogColor, 0.5f), 0.2f * num * num);
            }
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            { sLeaser.sprites[i].color = body; }
            sLeaser.sprites[11].color = Color.Lerp(PlayerGraphics.SlugcatColor(this.owner.playerState.slugcatCharacter), Color.white, 0.3f);
            sLeaser.sprites[10].color = PlayerGraphics.SlugcatColor(this.owner.playerState.slugcatCharacter);
            sLeaser.sprites[9].color = face;
            */
        }

        public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (this.container == null) { return; }
            if (newContatiner == null) { newContatiner = rCam.ReturnFContainer("Midground"); }
            this.container.RemoveFromContainer();
            newContatiner.AddChild(this.container);
        }
    }
}