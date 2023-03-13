using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MapleCollection.SugarCat
{
    public class SugarCatDecoration : CatDecoration
    {
        public SugarCatDecoration(Player owner) : base(owner)
        {
            this.frills = new SugarFrill[5];
            this.frillHeight = Futile.atlasManager.GetElementWithName("LizardScaleA2").sourcePixelSize.y; //2
            this.excitement = 0f;
            this.syrups = new List<SlugSyrup>()
            {
                //new SlugSyrup(this, -1.5f, 6),
                //new SlugSyrup(this, -3.5f, 9),
                //new SlugSyrup(this, -5.5f, 6),
                //new SlugSyrup(this, 2.2f, 9),
                //new SlugSyrup(this, 3.4f, 12)
            };
        }

        private SugarCatSupplement OwnerSub => ModifyCat.GetSub(this.owner) as SugarCatSupplement;
        public static readonly bool debug = false;
        private readonly SugarFrill[] frills;
        private float excitement;
        private int FirstStripeIdx => this.frills.Length * 2;

        private TriangleMesh Stripe(int idx) => this.sprites[FirstStripeIdx + idx] as TriangleMesh;

        public override void Update()
        {
            if (this.owner == null || this.owner.room == null || this.OwnerGrp == null) { return; }
            base.Update();
            this.excitement = Mathf.Lerp(this.excitement, Mathf.Max(this.owner.aerobicLevel * 0.3f, this.OwnerSub.charge), 0.5f);

            for (int g = 0; g < 5; g++)
            {
                Vector2 outerPos = this.GetOuterPos(this.frills[g].idx, 1f, this.frills[g].rotation);
                Vector2 dir = this.GetDir(this.frills[g].idx, 1f);
                Vector2 v = Vector2.Lerp(dir, Custom.DirVec(this.GetPos(this.frills[g].idx, 1f), outerPos), Mathf.Abs(GetZRot(this.frills[g].idx, 1f) + this.frills[g].rotation));
                v = Vector2.Lerp(v, dir, Mathf.Pow(0.6f, Mathf.Lerp(1f, 15f, this.excitement))).normalized;
                Vector2 v2 = outerPos + v * this.frills[g].length;
                if (!Custom.DistLess(this.frills[g].pos, v2, this.frills[g].length / 2f))
                {
                    Vector2 a = Custom.DirVec(this.frills[g].pos, v2);
                    float m = Vector2.Distance(this.frills[g].pos, v2) - (this.frills[g].length / 2f);
                    this.frills[g].pos += a * m; this.frills[g].vel += a * m;
                }
                this.frills[g].vel += Vector2.ClampMagnitude(v2 - this.frills[g].pos, Mathf.Lerp(10f, 20f, this.excitement)) / 3f;
                this.frills[g].vel *= 0.9f;
                if (this.excitement > 0.1f) { this.frills[g].vel += Custom.RNV() * Mathf.Lerp(0f, 6f, this.excitement); }

                this.frills[g].ConnectToPoint(outerPos, this.frills[g].length, true, 0f, Vector2.zero, 0f, 0f);
                this.frills[g].Update();
            }
            for (int s = 0; s < syrups.Count; s++) { syrups[s].Update(); }
        }

#pragma warning disable IDE0060

        private readonly float frillHeight;
        private FContainer backContainer;
        private List<SlugSyrup> syrups;

        public override void SuckedIntoShortCut()
        {
            base.SuckedIntoShortCut();
            this.backContainer.RemoveFromContainer();
        }

        public override void Reset()
        {
            base.Reset();
            for (int s = 0; s < syrups.Count; s++)
            { syrups[s].ResetSlime(); }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
            if (this.backContainer != null) { this.backContainer.RemoveAllChildren(); this.backContainer.RemoveFromContainer(); }
            this.OwnerGrp.tail[1].rad = 5f;
            this.OwnerGrp.tail[2].rad = 3.5f;
            this.OwnerGrp.tail[3].rad = 1f;

            this.backContainer = new FContainer();
            for (int g = 0; g < this.frills.Length; g++)
            { this.frills[g] = new SugarFrill(this) { length = 20f, width = 1.0f, rotation = -40f + g * 20f, idx = 0.2f }; }
            this.frills[0].length = 15f; this.frills[0].width = 0.8f; this.frills[0].idx = 0.15f;
            this.frills[this.frills.Length - 1].length = 15f; this.frills[this.frills.Length - 1].width = 0.8f; this.frills[this.frills.Length - 1].idx = 0.15f;
            this.sprites = new FSprite[16];
            for (int g = 0; g < this.frills.Length; g++)
            {
                this.sprites[g] = new FSprite("LizardScaleA2", true)
                { scaleY = this.frills[g].length / this.frillHeight, anchorY = 0.1f };
                this.sprites[g + this.frills.Length] = new FSprite("LizardScaleB2", true)
                { scaleY = this.frills[g].length / this.frillHeight, anchorY = 0.1f };
                this.backContainer.AddChild(this.sprites[g]); this.backContainer.AddChild(this.sprites[g + this.frills.Length]);
            }
            TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[]
            { new TriangleMesh.Triangle(0, 1, 2), new TriangleMesh.Triangle(1, 2, 4), new TriangleMesh.Triangle(1, 3, 4) };
            this.sprites[FirstStripeIdx] = new TriangleMesh("Futile_White", tris, false, false);
            this.sprites[FirstStripeIdx + 1] = new TriangleMesh("Futile_White", tris, false, false);
            tris = new TriangleMesh.Triangle[] {
                new TriangleMesh.Triangle(0, 1, 2), new TriangleMesh.Triangle(1, 2, 3),
                new TriangleMesh.Triangle(2, 3, 5), new TriangleMesh.Triangle(2, 4, 5),
                new TriangleMesh.Triangle(4, 5, 6), new TriangleMesh.Triangle(5, 6, 7),
                new TriangleMesh.Triangle(6, 7, 9), new TriangleMesh.Triangle(6, 8, 9),
                new TriangleMesh.Triangle(8, 9, 10), new TriangleMesh.Triangle(9, 10, 11)
            };
            this.sprites[FirstStripeIdx + 2] = new TriangleMesh("Futile_White", tris, false, false);
            this.sprites[FirstStripeIdx + 3] = new TriangleMesh("Futile_White", tris, false, false);
            tris = new TriangleMesh.Triangle[]
            { new TriangleMesh.Triangle(0, 1, 2), new TriangleMesh.Triangle(0, 2, 3), new TriangleMesh.Triangle(0, 3, 4) };
            this.sprites[FirstStripeIdx + 4] = new TriangleMesh("Futile_White", tris, false, false);
            this.sprites[FirstStripeIdx + 5] = new FSprite("HeadA0", true);

            for (int t = 0; t < 6; t++)
            { this.container.AddChild(this.sprites[FirstStripeIdx + t]); }
            this.AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            if (debug)
            {
                this.OwnerGrp.DEBUGLABELS[1].label.text = string.Concat("zRot0: ", GetZRot(0, timeStacker).ToString("00.0"),
                    " 1: ", GetZRot(1, timeStacker).ToString("00.0"), " 2: ", GetZRot(2, timeStacker).ToString("00.0"),
                    " 3: ", GetZRot(3, timeStacker).ToString("00.0"), " 4: ", GetZRot(4, timeStacker).ToString("00.0"));
                this.OwnerGrp.DEBUGLABELS[2].label.text = string.Empty;
            }
            for (int g = 0; g < this.frills.Length; g++)
            {
                Vector2 outerPos = this.GetOuterPos(this.frills[g].idx, timeStacker);
                this.sprites[g].x = outerPos.x - camPos.x; this.sprites[g].y = outerPos.y - camPos.y;
                this.sprites[g].rotation = Custom.AimFromOneVectorToAnother(outerPos, Vector2.Lerp(this.frills[g].lastPos, this.frills[g].pos, timeStacker));
                this.sprites[g].scaleX = this.frills[g].width * Mathf.Sign(this.GetZRot(this.frills[g].idx, timeStacker) + this.frills[g].rotation);
                SwitchContainer(g, Mathf.Abs(GetZRot(this.frills[g].idx, timeStacker) + this.frills[g].rotation) > 80f);
                this.sprites[g + this.frills.Length].x = outerPos.x - camPos.x; this.sprites[g + this.frills.Length].y = outerPos.y - camPos.y;
                this.sprites[g + this.frills.Length].rotation = Custom.AimFromOneVectorToAnother(outerPos, Vector2.Lerp(this.frills[g].lastPos, this.frills[g].pos, timeStacker));
                this.sprites[g + this.frills.Length].scaleX = this.frills[g].width * Mathf.Sign(this.GetZRot(this.frills[g].idx, timeStacker) + this.frills[g].rotation);
                SwitchContainer(g + this.frills.Length, Mathf.Abs(GetZRot(this.frills[g].idx, timeStacker) + this.frills[g].rotation) > 80f);
            }

            // Head Stripes : just make stripe sprites
            this.sprites[FirstStripeIdx + 5].x = sLeaser.sprites[3].x; this.sprites[FirstStripeIdx + 5].y = sLeaser.sprites[3].y;
            this.sprites[FirstStripeIdx + 5].rotation = sLeaser.sprites[3].rotation;
            this.sprites[FirstStripeIdx + 5].scaleX = sLeaser.sprites[3].scaleX;
            this.sprites[FirstStripeIdx + 5].element = Futile.atlasManager.GetElementWithName("SugarHeadStripes" +
                sLeaser.sprites[3].element.name.Substring(5));

            #region backStripes

            // Back Stripes
            float strThk = 1.2f; //Half of stripe thickness
            for (int b = 0; b < 2; b++)
            {
                float bIdx = 0.5f + b * 0.3f;
                float bRot = this.GetZRot(bIdx, timeStacker);
                float[] segRot = new float[3];
                if (Mathf.Abs(bRot) < 10f) { Stripe(b).isVisible = false; continue; }
                Stripe(b).isVisible = true;
                if (bRot > 0f)
                {
                    segRot[2] = -90f;
                    segRot[1] = Mathf.Max(-90f, bRot - 120f);
                    segRot[0] = bRot - 90f;
                }
                else
                {
                    segRot[2] = 90f;
                    segRot[1] = Mathf.Min(90f, bRot + 120f);
                    segRot[0] = bRot + 90f;
                }
                Vector2 bkDir = GetDir(bIdx, timeStacker);
                Vector2 bkPpd = -Custom.PerpendicularVector(bkDir);
                Stripe(b).MoveVertice(0, this.GetPos(bIdx, timeStacker) + bkPpd * Mathf.Sin(segRot[0] * Mathf.Deg2Rad) * this.GetRad(bIdx) - rCam.pos);
                for (int t = 0; t < 2; t++)
                {
                    Stripe(b).MoveVertice(1 + t * 2, this.GetPos(bIdx, timeStacker) + bkDir * strThk
                        + bkPpd * Mathf.Sin(segRot[1 + t] * Mathf.Deg2Rad) * this.GetRad(bIdx) - rCam.pos);
                    Stripe(b).MoveVertice(2 + t * 2, this.GetPos(bIdx, timeStacker) - bkDir * strThk
                        + bkPpd * Mathf.Sin(segRot[1 + t] * Mathf.Deg2Rad) * this.GetRad(bIdx) - rCam.pos);
                }
            }

            #endregion backStripes

            #region tailStripes

            // Tail Stripes
            for (int s = 0; s < 2; s++)
            {
                float tIdx = 1.6f + 0.9f * s;
                float tRot = this.GetZRot(tIdx, timeStacker);
                float[] zigzag;
                if (tRot % 90f < 45f)
                {
                    zigzag = new float[6] { 1f, -1f, 1f, -1f, 1f, -1f };
                    zigzag[0] = Mathf.Lerp(1f, -1f, (tRot % 90f) / 45f);
                    zigzag[5] = Mathf.Lerp(-1f, 1f, (tRot % 90f) / 45f);
                }
                else
                {
                    zigzag = new float[6] { -1f, 1f, -1f, 1f, -1f, 1f };
                    zigzag[0] = Mathf.Lerp(-1f, 1f, (tRot % 90f - 45f) / 45f);
                    zigzag[5] = Mathf.Lerp(1f, -1f, (tRot % 90f - 45f) / 45f);
                }
                Vector2 tDir = GetDir(tIdx, timeStacker);
                Vector2 tPpd = -Custom.PerpendicularVector(tDir);
                for (int z = 0; z < 6; z++)
                {
                    float rot = Mathf.Clamp(tRot % 45f + (z - 1) * 45f - 90f, -90f, 90f) * Mathf.Deg2Rad;
                    Stripe(s + 2).MoveVertice(z * 2, this.GetPos(tIdx, timeStacker) - tDir * strThk * (1f + zigzag[z])
                        + tPpd * Mathf.Sin(rot) * this.GetRad(tIdx) - rCam.pos);
                    Stripe(s + 2).MoveVertice(z * 2 + 1, this.GetPos(tIdx, timeStacker) + tDir * strThk * (1f - zigzag[z])
                        + tPpd * Mathf.Sin(rot) * this.GetRad(tIdx) - rCam.pos);
                    if (debug && s == 0)
                    { this.OwnerGrp.DEBUGLABELS[2].label.text += string.Concat(z, ": ", (rot * Mathf.Rad2Deg).ToString("00"), " "); }
                }
            }

            #endregion tailStripes

            #region tailTip

            // Tail Tip
            Stripe(4).MoveVertice(0, (sLeaser.sprites[2] as TriangleMesh).vertices[14]);
            float tipRot = this.GetZRot(4, timeStacker);
            float[] creep = new float[4];
            if (tipRot % 180f < 90f)
            {
                creep[0] = Mathf.Lerp(0f, 1f, (tipRot % 180f) / 90f);
                creep[1] = 1f; creep[2] = 0f;
                creep[3] = Mathf.Lerp(1f, 0f, (tipRot % 180f) / 90f);
            }
            else
            {
                creep[0] = Mathf.Lerp(1f, 0f, (tipRot % 180f - 90f) / 90f);
                creep[1] = 0f; creep[2] = 1f;
                creep[3] = Mathf.Lerp(0f, 1f, (tipRot % 180f - 90f) / 90f);
            }
            Vector2 tipPpd = Custom.PerpendicularVector(GetDir(3.5f, timeStacker));
            for (int c = 0; c < 4; c++)
            {
                float cIdx = 3.5f - creep[c] * 0.4f;
                float thk = GetRad(cIdx);
                float off = c == 0 ? -1f : 1f;
                if (c > 0 && c < 3) { off = Mathf.Sin((tipRot % 90f + (c - 2) * 90f) * Mathf.Deg2Rad); }
                Stripe(4).MoveVertice(c + 1, GetPos(cIdx, timeStacker) + tipPpd * thk * off - rCam.pos);
            }

            #endregion tailTip

            //Syrups
            for (int s = 0; s < syrups.Count; s++)
            {
                if (!syrups[s].sprInit) { syrups[s].InitiateSprites(this.backContainer, rCam); }
                syrups[s].DrawSprites(rCam, timeStacker, camPos);
            }
        }

        private void SwitchContainer(int sprIdx, bool front)
        {
            this.sprites[sprIdx].RemoveFromContainer();
            if (front) { this.container.AddChild(this.sprites[sprIdx]); }
            else { this.backContainer.AddChild(this.sprites[sprIdx]); }
        }

        public static Color catColor = new Color(0.5686f, 0.3098f, 0.3020f); // 0x914F4D
        public static Color frillStart = new Color(0.8588f, 0.3569f, 0.3059f); // 0xDB5B4E
        public static Color frillEnd = new Color(0.9333f, 0.5059f, 0.3765f); // 0xEE8160
        public static Color stripeColor = new Color(0.7451f, 0.5373f, 0.4980f); // 0xBE897F

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            base.ApplyPalette(sLeaser, rCam, palette);
            for (int g = 0; g < this.frills.Length; g++)
            {
                this.sprites[g].color = frillStart;
                this.sprites[g + this.frills.Length].color = frillEnd;
            }
            for (int t = 0; t < 6; t++)
            { this.sprites[FirstStripeIdx + t].color = stripeColor; }
            for (int s = 0; s < syrups.Count; s++) { syrups[s].ApplyPalette(palette); }
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            base.AddToContainer(sLeaser, rCam, newContatiner);
            if (this.backContainer == null) { return; }
            this.backContainer.RemoveFromContainer();
            rCam.ReturnFContainer("Background").AddChild(this.backContainer);
            for (int s = 0; s < syrups.Count; s++) { syrups[s].sprInit = false; }
        }

        private class SugarFrill : BodyPart
        {
            public SugarFrill(SugarCatDecoration deco) : base(deco.OwnerGrp)
            {
                this.deco = deco;
            }

            public override void Update()
            {
                base.Update();
                if (this.owner.owner.room.PointSubmerged(this.pos))
                { this.vel *= 0.5f; }
                else
                { this.vel *= 0.9f; }
                this.lastPos = this.pos;
                this.pos += this.vel;
            }

            public readonly SugarCatDecoration deco;

            public float length;
            public float width;
            public float rotation;
            public float idx;
        }

        private class SlugSyrup
        {
            public SlugSyrup(SugarCatDecoration owner, float stuckIdx, int amount)
            {
                this.owner = owner;
                this.stuckIdx = stuckIdx;
                this.slime = new Vector2[amount, 5];
                this.stuckPosSlime = UnityEngine.Random.Range(0, amount - 1);
                for (int i = 0; i < this.slime.GetLength(0); i++)
                {
                    float cnRad;
                    if (i == 0 || UnityEngine.Random.value < 0.5f)
                    { cnRad = -1f; }
                    //else if (UnityEngine.Random.value < 0.2f)
                    //{ cnRad = i - 1; }
                    else
                    { cnRad = UnityEngine.Random.Range(0f, i); }
                    this.slime[i, 3] = new Vector2(cnRad, Mathf.Lerp(5f, 13f, UnityEngine.Random.value));
                    this.slime[i, 4] = Custom.RNV();
                }
                this.sprInit = false;
                this.health = 1f;
            }

            public bool sprInit;
            public readonly SugarCatDecoration owner;
            private readonly Vector2[,] slime;
            private readonly int stuckPosSlime;
            private int HighLightSprite => this.slime.GetLength(0);

            private FContainer container;
            private FSprite[] sprites;
            private Color color;
            public float stuckIdx, stuckRot;
            private Vector2 stuckPos, stuckLastPos;
            public float health;

            private void UpdateStuck()
            {
                if (stuckIdx < 0f)
                {
                    int idx = Mathf.FloorToInt(-stuckIdx) - 1;
                    this.stuckPos = Vector2.Lerp(this.owner.GetPos(this.owner.frills[idx].idx, 1f), this.owner.frills[idx].pos, -stuckIdx - idx);
                    this.stuckRot = this.owner.frills[idx].rotation;
                }
                else
                {
                    this.stuckPos = this.owner.GetPos(stuckIdx, 1f);
                    this.stuckRot = Custom.VecToDeg(this.owner.GetDir(stuckIdx, 1f));
                }
            }

            public void Update()
            {
                this.stuckLastPos = this.stuckPos;
                this.UpdateStuck();
                for (int i = 0; i < this.slime.GetLength(0); i++)
                {
                    // 0: pos, 1: lastPos, 2: vel
                    this.slime[i, 1] = this.slime[i, 0];
                    this.slime[i, 0] += this.slime[i, 2];
                    this.slime[i, 2] *= 0.95f; //airFriction
                    this.slime[i, 2].y -= 0.9f; // gravity
                    if ((int)this.slime[i, 3].x < 0 || (int)this.slime[i, 3].x >= this.slime.GetLength(0))
                    {
                        Vector2 cenDir = Custom.DirVec(this.slime[i, 0], this.stuckPos);
                        float cenDist = Vector2.Distance(this.slime[i, 0], this.stuckPos);
                        float cnRad = this.slime[i, 3].y * this.health;
                        this.slime[i, 0] -= cenDir * (cnRad - cenDist) * 0.9f;
                        this.slime[i, 2] -= cenDir * (cnRad - cenDist) * 0.9f;
                    }
                    else
                    {
                        Vector2 offDir = Custom.DirVec(this.slime[i, 0], this.slime[(int)this.slime[i, 3].x, 0]);
                        float offDist = Vector2.Distance(this.slime[i, 0], this.slime[(int)this.slime[i, 3].x, 0]);
                        float cnRad = this.slime[i, 3].y * this.health;
                        this.slime[i, 0] -= offDir * (cnRad - offDist) * 0.5f;
                        this.slime[i, 2] -= offDir * (cnRad - offDist) * 0.5f;
                        this.slime[(int)this.slime[i, 3].x, 0] += offDir * (cnRad - offDist) * 0.5f;
                        this.slime[(int)this.slime[i, 3].x, 2] += offDir * (cnRad - offDist) * 0.5f;
                        Vector2 b = Custom.RotateAroundOrigo(this.slime[i, 4], this.stuckRot);
                        this.slime[i, 2] += b;
                        this.slime[(int)this.slime[i, 3].x, 2] -= b;
                    }
                    if (Custom.DistLess(this.slime[i, 0], this.stuckPos, 100f))
                    {
                        SharedPhysics.TerrainCollisionData terrainCollisionData
                            = new SharedPhysics.TerrainCollisionData(this.slime[i, 0], this.slime[i, 1], this.slime[i, 2], 3f,
                            new IntVector2(0, 0), false);
                        terrainCollisionData = SharedPhysics.VerticalCollision(this.owner.owner.room, terrainCollisionData);
                        terrainCollisionData = SharedPhysics.HorizontalCollision(this.owner.owner.room, terrainCollisionData);
                        this.slime[i, 0] = terrainCollisionData.pos;
                        this.slime[i, 2] = terrainCollisionData.vel;
                    }
                }
                this.slime[this.stuckPosSlime, 0] = this.stuckPos;
                this.slime[this.stuckPosSlime, 2] *= 0f;
                for (int j = 0; j < this.slime.GetLength(0); j++)
                {
                    if (j != this.stuckPosSlime)
                    {
                        this.slime[j, 2] += Custom.DirVec(this.stuckPos, this.slime[j, 0])
                            * Custom.LerpMap(Vector2.Distance(this.stuckPos, this.slime[j, 0]), 3f, 24f, 6f, 0f);
                    }
                }
            }

            public void ResetSlime()
            {
                this.UpdateStuck(); this.stuckLastPos = this.stuckPos;
                for (int i = 0; i < this.slime.GetLength(0); i++)
                {
                    this.slime[i, 0] = this.stuckPos + Custom.RNV() * 4f * UnityEngine.Random.value;
                    this.slime[i, 1] = this.slime[i, 0];
                    this.slime[i, 2] = Vector2.zero;
                }
            }

            public void InitiateSprites(FContainer backContainer, RoomCamera rCam)
            {
                if (this.container != null) { this.container.RemoveAllChildren(); this.container.RemoveFromContainer(); }
                this.container = new FContainer();
                backContainer.AddChild(this.container);
                this.sprites = new FSprite[this.slime.GetLength(0) + 1];
                for (int i = 0; i < this.slime.GetLength(0); i++)
                {
                    this.sprites[i] = new FSprite("Futile_White", true)
                    {
                        anchorY = 0.05f,
                        alpha = UnityEngine.Random.value,
                        shader = rCam.game.rainWorld.Shaders["JaggedCircle"]
                    };
                    this.container.AddChild(this.sprites[i]);
                }
                this.sprites[this.HighLightSprite] = new FSprite("Circle20", true);
                this.container.AddChild(this.sprites[this.HighLightSprite]);
                this.ApplyPalette(rCam.currentPalette);
            }

            public static Color ColorFromPalette(RoomPalette palette) =>
                Color.Lerp(Custom.HSL2RGB(Mathf.Lerp(0.025f, 0.005f, palette.darkness), 0.8f, 0.6f),
                    palette.fogColor, Mathf.Lerp(0.25f, 0.35f, palette.fogAmount) * Mathf.Lerp(0.1f, 1f, palette.darkness));

            public void DrawSprites(RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                Vector2 pos = Vector2.Lerp(this.stuckLastPos, this.stuckPos, timeStacker);
                this.sprites[this.HighLightSprite].x = pos.x - camPos.x;
                this.sprites[this.HighLightSprite].y = pos.y - camPos.y;
                this.sprites[this.HighLightSprite].color = Color.Lerp(this.color, Color.white, 0.3f);
                this.sprites[this.HighLightSprite].scaleY = Mathf.Lerp(0.3f, 0.05f, this.health);
                this.sprites[this.HighLightSprite].scaleX = Mathf.Lerp(0.25f, 0.15f, this.health);
                for (int i = 0; i < this.slime.GetLength(0); i++)
                {
                    Vector2 sPos = Vector2.Lerp(this.slime[i, 1], this.slime[i, 0], timeStacker);
                    Vector2 sstkPos = this.StuckPosOfSlime(i, timeStacker);
                    this.sprites[i].x = sPos.x - camPos.x;
                    this.sprites[i].y = sPos.y - camPos.y;
                    this.sprites[i].scaleY = (Vector2.Distance(sPos, sstkPos) + 3f) / 16f;
                    this.sprites[i].rotation = Custom.AimFromOneVectorToAnother(sPos, sstkPos);
                    this.sprites[i].scaleX = Custom.LerpMap(Vector2.Distance(sPos, sstkPos), 0f, this.slime[i, 3].y * 3.5f, 4f, 1.5f, 2f) / 16f;
                    this.sprites[i].color = color;
                }
            }

            private Vector2 StuckPosOfSlime(int s, float timeStacker)
            {
                if ((int)this.slime[s, 3].x < 0 || (int)this.slime[s, 3].x >= this.slime.GetLength(0))
                { return Vector2.Lerp(this.stuckLastPos, this.stuckPos, timeStacker); }
                return Vector2.Lerp(this.slime[(int)this.slime[s, 3].x, 1], this.slime[(int)this.slime[s, 3].x, 0], timeStacker);
            }

            public void ApplyPalette(RoomPalette palette) => this.color = ColorFromPalette(palette);
        }
    }
}