using MapleCollection.SporeCat;
using MapleCollection;
using RWCustom;
using Smoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using MoreSlugcats;

namespace MapleCollection.SporeCat
{
    public class SporeCatPuffBall : Weapon
    {
        public SporeCatPuffBall(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
        {
            base.bodyChunks = new BodyChunk[1];
            base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 7f, 0.11f);
            this.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[0];
            base.airFriction = 0.98f;
            base.gravity = 0.86f;
            this.bounce = 0.2f;
            this.surfaceFriction = 0.3f;
            this.collisionLayer = 2;
            base.waterFriction = 0.98f;
            base.buoyancy = 1.8f;
            this.tailPos = base.firstChunk.pos;
#pragma warning disable CS0618
            int seed = UnityEngine.Random.seed;
            UnityEngine.Random.seed = abstractPhysicalObject.ID.RandomSeed;
            this.exitThrownModeSpeed = 15f;
            this.superBoom = false; this.deerCall = true;
            this.dots = new Vector2[UnityEngine.Random.Range(6, 11)];
            for (int i = 0; i < this.dots.Length; i++)
            {
                this.dots[i] = Custom.DegToVec((float)i / (float)this.dots.Length * 360f) * UnityEngine.Random.value + Custom.RNV() * 0.2f;
            }
            UnityEngine.Random.seed = seed;
#pragma warning restore CS0618
            for (int j = 0; j < 3; j++)
            {
                for (int k = 0; k < this.dots.Length; k++)
                {
                    for (int l = 0; l < this.dots.Length; l++)
                    {
                        if (Custom.DistLess(this.dots[k], this.dots[l], 1.4f))
                        {
                            Vector2 a = Custom.DirVec(this.dots[k], this.dots[l]) * (Vector2.Distance(this.dots[k], this.dots[l]) - 1.4f);
                            float num = (float)k / ((float)k + (float)l);
                            this.dots[k] += a * num;
                            this.dots[l] -= a * (1f - num);
                        }
                    }
                }
            }
            float num2 = 1f;
            float num3 = -1f;
            float num4 = 1f;
            float num5 = -1f;
            for (int m = 0; m < this.dots.Length; m++)
            {
                num2 = Mathf.Min(num2, this.dots[m].x);
                num3 = Mathf.Max(num3, this.dots[m].x);
                num4 = Mathf.Min(num4, this.dots[m].y);
                num5 = Mathf.Max(num5, this.dots[m].y);
            }
            for (int n = 0; n < this.dots.Length; n++)
            {
                this.dots[n].x = -1f + 2f * Mathf.InverseLerp(num2, num3, this.dots[n].x);
                this.dots[n].y = -1f + 2f * Mathf.InverseLerp(num4, num5, this.dots[n].y);
            }
            float num6 = 0f;
            for (int num7 = 0; num7 < this.dots.Length; num7++)
            { num6 = Mathf.Max(num6, this.dots[num7].magnitude); }
            for (int num8 = 0; num8 < this.dots.Length; num8++)
            { this.dots[num8] /= num6; }
            this.segments = new Vector2[(int)Mathf.Lerp(3f, 8f, UnityEngine.Random.value), 3]; //15f
            this.growth = SporeCatSupplement.recoverTime;
        }

        #region Patching

        public static void SubPatch()
        {
            On.SporeCloud.ctor += SporeCloudPatch;
            On.AbstractPhysicalObject.Realize += AbsObjPatch;
            On.RegionState.AdaptRegionStateToWorld += RemovePuffFromSave;
            On.ItemSymbol.SpriteNameForItem += SpriteNameForPuff;
            On.HUD.Map.ShelterMarker.ItemInShelterMarker.ItemInShelterData.DataFromAbstractPhysical += NoPuffFromAbstractPhysical;
            On.DeerAI.NewRoom += DeerNewRoomPatch;
            On.DeerAI.PuffBallLegal += DeerLegalPatch;
            On.DeerAI.TrackItem += DeerTrackPatch;
            On.Deer.Act += DeerActPatch;
            On.Scavenger.PickUpAndPlaceInInventory += ScavPickUpPatch;

            if (ModManager.MSC) OnMSCEnablePatch();
        }

        internal static void OnMSCEnablePatch()
        {
            On.MoreSlugcats.GourmandCombos.GetLibraryData_AbstractObjectType_AbstractObjectType += GourmandRecipeObjObj;
            On.MoreSlugcats.GourmandCombos.GetLibraryData_Type_AbstractObjectType += GourmandRecipeCritObj;
        }

        internal static void OnMSCDisablePatch()
        {
            On.MoreSlugcats.GourmandCombos.GetLibraryData_AbstractObjectType_AbstractObjectType -= GourmandRecipeObjObj;
            On.MoreSlugcats.GourmandCombos.GetLibraryData_Type_AbstractObjectType -= GourmandRecipeCritObj;
        }

        private static GourmandCombos.CraftDat GourmandRecipeObjObj(On.MoreSlugcats.GourmandCombos.orig_GetLibraryData_AbstractObjectType_AbstractObjectType orig,
            AbstractPhysicalObject.AbstractObjectType objectA, AbstractPhysicalObject.AbstractObjectType objectB)
        {
            if (objectA == MapleEnums.SporePuffBall) objectA = AbstractPhysicalObject.AbstractObjectType.PuffBall;
            if (objectB == MapleEnums.SporePuffBall) objectB = AbstractPhysicalObject.AbstractObjectType.PuffBall;
            return orig(objectA, objectB);
        }

        private static GourmandCombos.CraftDat GourmandRecipeCritObj(On.MoreSlugcats.GourmandCombos.orig_GetLibraryData_Type_AbstractObjectType orig,
            CreatureTemplate.Type critterA, AbstractPhysicalObject.AbstractObjectType objectB)
        {
            if (objectB == MapleEnums.SporePuffBall) objectB = AbstractPhysicalObject.AbstractObjectType.PuffBall;
            return orig(critterA, objectB);
        }

        private static void RemovePuffFromSave(On.RegionState.orig_AdaptRegionStateToWorld orig, RegionState self, int playerShelter, int activeGate)
        {
            orig.Invoke(self, playerShelter, activeGate);
            List<string> temp = new List<string>();
            for (int i = self.savedObjects.Count - 1; i >= 0; i--)
            {
                if (self.savedObjects[i].Contains("SporePuffBall")) { continue; } //Debug.Log("Removed SporeCatPuff from Save");
                temp.Add(self.savedObjects[i]);
            }
            self.savedObjects = temp;
        }

        private static string SpriteNameForPuff(On.ItemSymbol.orig_SpriteNameForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
        {
            if (itemType == MapleEnums.SporePuffBall)
            { return orig.Invoke(AbstractPhysicalObject.AbstractObjectType.PuffBall, intData); }
            return orig.Invoke(itemType, intData);
        }

        private static HUD.Map.ShelterMarker.ItemInShelterMarker.ItemInShelterData? NoPuffFromAbstractPhysical(On.HUD.Map.ShelterMarker.ItemInShelterMarker.ItemInShelterData.orig_DataFromAbstractPhysical orig, AbstractPhysicalObject obj)
        {
            if (obj.type == MapleEnums.SporePuffBall) { return null; }
            return orig.Invoke(obj);
        }

        private static void SporeCloudPatch(On.SporeCloud.orig_ctor orig, SporeCloud self, Vector2 pos, Vector2 vel, Color color, float size, AbstractCreature killTag, int checkInsectsDelay, InsectCoordinator smallInsects)
        {
            orig.Invoke(self, pos, vel, color, size, killTag, checkInsectsDelay, smallInsects);
            self.lifeTime = Mathf.Lerp(170f, 400f, UnityEngine.Random.value) / size;
        }

        private static void AbsObjPatch(On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self)
        {
            if (self.type != MapleEnums.SporePuffBall)
            { orig.Invoke(self); return; }
            if (self.realizedObject != null) { return; }
            self.realizedObject = new SporeCatPuffBall(self, self.world);
            for (int i = 0; i < self.stuckObjects.Count; i++)
            {
                if (self.stuckObjects[i].A.realizedObject == null && self.stuckObjects[i].A != self)
                { self.stuckObjects[i].A.Realize(); }
                if (self.stuckObjects[i].B.realizedObject == null && self.stuckObjects[i].B != self)
                { self.stuckObjects[i].B.Realize(); }
            }
        }

        private static void DeerNewRoomPatch(On.DeerAI.orig_NewRoom orig, DeerAI self, Room newRoom)
        {
            orig.Invoke(self, newRoom);
            for (int i = 0; i < newRoom.abstractRoom.entities.Count; i++)
            {
                if (newRoom.abstractRoom.entities[i] is AbstractPhysicalObject
                    && (newRoom.abstractRoom.entities[i] as AbstractPhysicalObject).type == MapleEnums.SporePuffBall
                    && (newRoom.abstractRoom.entities[i] as AbstractPhysicalObject).realizedObject != null)
                {
                    self.itemTracker.SeeItem(newRoom.abstractRoom.entities[i] as AbstractPhysicalObject);
                }
            }
        }

        private static bool DeerLegalPatch(On.DeerAI.orig_PuffBallLegal orig, DeerAI self, ItemTracker.ItemRepresentation itemRep)
        {
            bool res = orig.Invoke(self, itemRep);
            if (res && itemRep.representedItem.realizedObject is SporeCatPuffBall pb)
            { if (pb.mode == Mode.OnBack || !pb.Matured) { return false; } }
            return res;
        }

        private static bool DeerTrackPatch(On.DeerAI.orig_TrackItem orig, DeerAI self, AbstractPhysicalObject obj)
        {
            return orig.Invoke(self, obj) || (obj.type == MapleEnums.SporePuffBall);
        }

        private static void DeerActPatch(On.Deer.orig_Act orig, Deer self, bool eu, float support, float forwardPower)
        {
            if (self.eatCounter == 50 && self.eatObject != null && self.eatObject is SporeCatPuffBall pb)
            { pb.beingEaten = Mathf.Max(pb.beingEaten, 0.01f); self.eatCounter--; }
            orig.Invoke(self, eu, support, forwardPower);
        }

        private static void ScavPickUpPatch(On.Scavenger.orig_PickUpAndPlaceInInventory orig, Scavenger scav, PhysicalObject obj)
        {
            if (obj is SporeCatPuffBall pb)
            { if (pb.mode == Mode.OnBack || !pb.Matured) { return; } }

            orig.Invoke(scav, obj);
        }

        #endregion Patching

        public SporeCatDecoration parentDeco;

        public void SetGrowth(int growth) => this.growth = Custom.IntClamp(growth, 1, SporeCatSupplement.recoverTime);

        private int growth;
        private float Maturity => (float)this.growth / SporeCatSupplement.recoverTime;
        public bool Matured => growth >= SporeCatSupplement.recoverTime;

        public Player owner;
        private SporeCatSupplement OwnerSub => ModifyCat.GetSub(owner) as SporeCatSupplement;

        public override void NewRoom(Room newRoom)
        {
            base.NewRoom(newRoom);
            for (int i = 0; i < this.segments.GetLength(0); i++)
            {
                this.segments[i, 0] = base.firstChunk.pos + new Vector2(0f, 5f * (float)i);
                this.segments[i, 1] = this.segments[i, 0];
                this.segments[i, 2] *= 0f;
            }
        }

        public override void ChangeMode(Mode newMode)
        {
            if (mode == Mode.OnBack && newMode != Mode.OnBack)
            {
                this.OnHandMode();
                this.hideBehindPlayer = new bool?(false);
            }
            base.ChangeMode(newMode);
            base.ChangeCollisionLayer(newMode == Weapon.Mode.Thrown ? 0 : this.DefaultCollLayer);
            base.firstChunk.collideWithObjects = (newMode != Mode.Carried && newMode != Mode.OnBack);
            base.firstChunk.collideWithTerrain = (newMode == Mode.Free || newMode == Mode.Thrown);
        }

        public override void Update(bool eu)
        {
            if (this.room == null) { return; }
            if (this.owner == null) { this.Destroy(); return; } //Spawned by Saving
            if (this.beingEaten > 0f)
            {
                this.beingEaten += 0.1f;
                for (int i = 0; i < this.segments.GetLength(0); i++)
                { this.segments[i, 0] = Vector2.Lerp(this.segments[i, 0], base.firstChunk.pos, this.beingEaten); }
                if (this.beingEaten > 1f) { this.Destroy(); }
            }
            if (this.lastModeThrown && (base.firstChunk.ContactPoint.x != 0 || base.firstChunk.ContactPoint.y != 0))
            { this.Explode(); }
            this.lastModeThrown = (base.mode == Weapon.Mode.Thrown);
            if (base.firstChunk.ContactPoint.y != 0)
            {
                this.rotationSpeed = (this.rotationSpeed * 2f + base.firstChunk.vel.x * 5f) / 3f;
            }
            for (int j = 0; j < this.segments.GetLength(0); j++)
            {
                float seg = (float)j / (float)(this.segments.GetLength(0) - 1);
                this.segments[j, 1] = this.segments[j, 0];
                this.segments[j, 0] += this.segments[j, 2];
                this.segments[j, 2] *= Mathf.Lerp(1f, 0.85f, seg);
                this.segments[j, 2] += Vector2.Lerp(this.rotation * 5f, new Vector2(Mathf.Clamp(base.firstChunk.pos.x - this.segments[j, 0].x, -2f, 2f) * 0.0025f, 0.25f) * (1f - seg), Mathf.Pow(seg, 0.01f));
                this.segments[j, 2].y += 0.01f;
                this.ConnectSegment(j);
            }
            for (int k = this.segments.GetLength(0) - 1; k >= 0; k--)
            { this.ConnectSegment(k); }
            if (this.smoke != null)
            {
                if (this.room.ViewedByAnyCamera(base.firstChunk.pos, 300f))
                {
                    float spread = this.OwnerSub.Charge > 0f ? (3f + 6f * this.OwnerSub.Charge) : 1f;
                    this.smoke.EmitSmoke(this.segments[this.segments.GetLength(0) - 1, 0],
                        Custom.DirVec(this.segments[this.segments.GetLength(0) - 2, 0], this.segments[this.segments.GetLength(0) - 1, 0])
                        + Custom.RNV() * spread + this.segments[this.segments.GetLength(0) - 1, 2], this.sporeColor);
                }
                if (this.smoke.slatedForDeletetion || this.smoke.room != this.room)
                { this.smoke = null; }
            }
            else if (this.Matured)
            {
                this.smoke = new SporesSmoke(this.room);
                this.room.AddObject(this.smoke);
            }
            bool flag = false;
            if (base.mode == Weapon.Mode.Carried && this.grabbedBy.Count > 0 && this.grabbedBy[0].grabber is Player && (this.grabbedBy[0].grabber as Player).swallowAndRegurgitateCounter > 50 && (this.grabbedBy[0].grabber as Player).objectInStomach == null && (this.grabbedBy[0].grabber as Player).input[0].pckp)
            {
                int num2 = -1;
                for (int l = 0; l < 2; l++)
                {
                    if ((this.grabbedBy[0].grabber as Player).grasps[l] != null && (this.grabbedBy[0].grabber as Player).CanBeSwallowed((this.grabbedBy[0].grabber as Player).grasps[l].grabbed))
                    {
                        num2 = l;
                        break;
                    }
                }
                if (num2 > -1 && (this.grabbedBy[0].grabber as Player).grasps[num2] != null && (this.grabbedBy[0].grabber as Player).grasps[num2].grabbed == this)
                {
                    flag = true;
                }
            }
            this.swallowed = Custom.LerpAndTick(this.swallowed, (!flag) ? 0f : 1f, 0.05f, 0.05f);
            base.Update(eu);
        }

        private void ConnectSegment(int i)
        {
            if (i == 0)
            {
                Vector2 pos = base.firstChunk.pos;
                Vector2 a = Custom.DirVec(this.segments[i, 0], pos);
                float num = Vector2.Distance(this.segments[i, 0], pos);
                this.segments[i, 0] -= a * (5f - num);
                this.segments[i, 2] -= a * (5f - num);
            }
            else
            {
                Vector2 a2 = Custom.DirVec(this.segments[i, 0], this.segments[i - 1, 0]);
                float num2 = Vector2.Distance(this.segments[i, 0], this.segments[i - 1, 0]);
                float num3 = 0.52f;
                this.segments[i, 0] -= a2 * (5f - num2) * num3;
                this.segments[i, 2] -= a2 * (5f - num2) * num3;
                this.segments[i - 1, 0] += a2 * (5f - num2) * (1f - num3);
                this.segments[i - 1, 2] += a2 * (5f - num2) * (1f - num3);
            }
        }

        public override void PlaceInRoom(Room placeRoom)
        {
            while (this.abstractPhysicalObject.pos.y >= 0 && !placeRoom.GetTile(this.abstractPhysicalObject.pos.Tile + new IntVector2(0, -1)).Solid)
            {
                AbstractPhysicalObject abstractPhysicalObject = this.abstractPhysicalObject;
                abstractPhysicalObject.pos.y -= 1;
            }
            base.PlaceInRoom(placeRoom);
            this.rotation = Custom.DegToVec(Mathf.Lerp(-45f, 45f, this.abstractPhysicalObject.world.game.SeededRandom(this.abstractPhysicalObject.ID.RandomSeed)));
        }

        public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
        {
            if (result.chunk == null)
            {
                return false;
            }
            result.chunk.vel += base.firstChunk.vel * 0.1f / result.chunk.mass;
            base.HitSomething(result, eu);
            this.Explode();
            return true;
        }

        public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
        {
            base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
            this.room.AddObject(new SporeCloud(base.firstChunk.pos, Custom.RNV() * UnityEngine.Random.value + throwDir.ToVector2() * 10f, this.sporeColor, 0.5f, null, -1, null));
            this.room.PlaySound(SoundID.Slugcat_Throw_Puffball, base.firstChunk);
        }

        public override void PickedUp(Creature upPicker)
        {
            this.room.PlaySound(SoundID.Slugcat_Pick_Up_Puffball, base.firstChunk);
            this.OnHandMode();
            this.hideBehindPlayer = new bool?(false);
        }

        public override void HitWall()
        {
            this.Explode();
            this.SetRandomSpin();
            this.ChangeMode(Weapon.Mode.Free);
            base.forbiddenToPlayer = 10;
        }

        public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk)
        {
            base.HitByExplosion(hitFac, explosion, hitChunk);
            this.Explode();
        }

        public override void HitByWeapon(Weapon weapon)
        {
            base.HitByWeapon(weapon);
            this.Explode();
        }

        public void Explode()
        {
            if (base.slatedForDeletetion) { return; }
            InsectCoordinator smallInsects = null;
            for (int i = 0; i < this.room.updateList.Count; i++)
            {
                if (this.room.updateList[i] is InsectCoordinator)
                {
                    smallInsects = (this.room.updateList[i] as InsectCoordinator);
                    break;
                }
            }
            for (int j = 0; j < 70; j++)
            {
                this.room.AddObject(new SporeCloud(base.firstChunk.pos, Custom.RNV() * UnityEngine.Random.value * 10f, this.sporeColor, this.superBoom ? 5f : 1f, this.thrownBy?.abstractCreature, j % 20, smallInsects));
            }
            this.room.AddObject(new SporeCatPuffVisionObscurer(base.firstChunk.pos, this.superBoom, this.deerCall));
            for (int k = 0; k < (this.superBoom ? 10 : 7); k++)
            {
                this.room.AddObject(new PuffBallSkin(base.firstChunk.pos, Custom.RNV() * UnityEngine.Random.value * (this.superBoom ? 24f : 16f), this.color, Color.Lerp(this.color, this.sporeColor, 0.5f)));
            }
            this.room.PlaySound(SoundID.Puffball_Eplode, base.firstChunk.pos);
            this.Destroy();
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[3 + this.dots.Length * 2];
            sLeaser.sprites[0] = new FSprite("BodyA", true); // Chunk
            sLeaser.sprites[1] = new FSprite("BodyA", true)
            {
                alpha = 0.5f
            }; // Highlight
            TriangleMesh triangleMesh = TriangleMesh.MakeLongMesh(this.segments.GetLength(0), false, false);
            sLeaser.sprites[2] = triangleMesh; // String
            for (int i = 0; i < this.dots.Length; i++)
            {
                sLeaser.sprites[3 + i] = new FSprite("JetFishEyeB", true);
                sLeaser.sprites[3 + this.dots.Length + i] = new FSprite("pixel", true);
            }
            this.AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 pos = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
            float rotation = Custom.VecToDeg(Vector3.Slerp(this.lastRotation, this.rotation, timeStacker));
            if (this.vibrate > 0)
            { pos += Custom.DegToVec(UnityEngine.Random.value * 360f) * 2f * UnityEngine.Random.value; }
            sLeaser.sprites[0].x = pos.x - camPos.x;
            sLeaser.sprites[0].y = pos.y - camPos.y;
            sLeaser.sprites[1].x = pos.x - camPos.x - 2.5f;
            sLeaser.sprites[1].y = pos.y - camPos.y + 2.5f;
            sLeaser.sprites[0].rotation = rotation;
            sLeaser.sprites[1].rotation = rotation;
            float scale = Maturity;
            if (this.beingEaten > 0f || this.swallowed > 0f)
            { scale = Maturity * (1f - Mathf.Max(this.beingEaten, this.swallowed * 0.5f)); }
            sLeaser.sprites[0].scaleY = 0.9f * scale;
            sLeaser.sprites[0].scaleX = scale;
            sLeaser.sprites[1].scaleY = 0.45f * scale;
            sLeaser.sprites[1].scaleX = 0.5f * scale;
            if (this.blink > 0)
            {
                float b = this.blink > 1 ? 1f : 0f;
                if (OwnerSub.Charge > 0f)
                { b = this.blink > 1 && UnityEngine.Random.value < 0.3f ? Mathf.Clamp(OwnerSub.Charge + 0.2f, 0.2f, 0.8f) : 0f; }
                Color c = Color.Lerp(this.color, this.blinkColor, b);
                sLeaser.sprites[0].color = c; sLeaser.sprites[2].color = c;
            }
            // dots
            for (int i = 0; i < this.dots.Length; i++)
            {
                Vector2 dotPos = pos + Custom.RotateAroundOrigo(new Vector2(this.dots[i].x * 7f, this.dots[i].y * 8.5f) * scale, rotation);
                sLeaser.sprites[3 + i].x = dotPos.x - camPos.x;
                sLeaser.sprites[3 + i].y = dotPos.y - camPos.y;
                sLeaser.sprites[3 + i].rotation = Custom.VecToDeg(Custom.RotateAroundOrigo(this.dots[i], rotation).normalized);
                sLeaser.sprites[3 + i].scaleX = scale;
                sLeaser.sprites[3 + i].scaleY = Custom.LerpMap(this.dots[i].magnitude, 0f, 1f, 1f, 0.25f, 4f);
                sLeaser.sprites[3 + this.dots.Length + i].x = dotPos.x - camPos.x;
                sLeaser.sprites[3 + this.dots.Length + i].y = dotPos.y - camPos.y;
            }
            // segments
            Vector2 lastSegPos = pos;
            for (int j = 0; j < this.segments.GetLength(0); j++)
            {
                Vector2 segPos = Vector2.Lerp(this.segments[j, 1], this.segments[j, 0], timeStacker);
                segPos = Vector2.Lerp(pos, segPos, scale);
                Vector2 segDir = (segPos - lastSegPos).normalized;
                Vector2 segThk = Custom.PerpendicularVector(segDir);
                float d = Vector2.Distance(segPos, lastSegPos) / 5f;
                if (j == 0)
                {
                    (sLeaser.sprites[2] as TriangleMesh).MoveVertice(j * 4, lastSegPos - segThk * 0.5f - camPos);
                    (sLeaser.sprites[2] as TriangleMesh).MoveVertice(j * 4 + 1, lastSegPos + segThk * 0.5f - camPos);
                }
                else
                {
                    (sLeaser.sprites[2] as TriangleMesh).MoveVertice(j * 4, lastSegPos - segThk * 0.5f + segDir * d - camPos);
                    (sLeaser.sprites[2] as TriangleMesh).MoveVertice(j * 4 + 1, lastSegPos + segThk * 0.5f + segDir * d - camPos);
                }
                (sLeaser.sprites[2] as TriangleMesh).MoveVertice(j * 4 + 2, segPos - segThk * 0.5f - segDir * d - camPos);
                (sLeaser.sprites[2] as TriangleMesh).MoveVertice(j * 4 + 3, segPos + segThk * 0.5f - segDir * d - camPos);
                lastSegPos = segPos;
            }

            if (this.hideBehindPlayer != null)
            {
                if (this.hideBehindPlayer.GetValueOrDefault())
                { this.SwitchBallLayer(sLeaser, rCam, true); }
                else { this.SwitchBallLayer(sLeaser, rCam, false); }
            }
            if (base.slatedForDeletetion || this.room != rCam.room)
            { sLeaser.CleanSpritesAndRemove(); }

            //foreach (FSprite spr in sLeaser.sprites) { spr.isVisible = false; } //Invisible for Test
            ApplyPalette(sLeaser, rCam, rCam.currentPalette);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            Color catColor = parentDeco != null ? parentDeco.GetBodyColor() : SporeCatDecoration.catDefaultColor;
            this.color = Color.Lerp(catColor, palette.texture.GetPixel(11, 4), 0.2f);
            sLeaser.sprites[0].color = this.color;
            sLeaser.sprites[1].color = Color.white;
            sLeaser.sprites[2].color = this.color;
            this.sporeColor = Color.Lerp(this.color, new Color(0.06f, 0.3f, 0.08f), 0.85f);
            Color dotColor = parentDeco != null ? parentDeco.GetThirdColor() : SporeCatDecoration.dotDefaultColor;
            dotColor = Color.Lerp(Color.Lerp(dotColor, palette.texture.GetPixel(11, 4), 0.2f), palette.blackColor, 0.5f);
            for (int j = 0; j < this.dots.Length; j++)
            {
                sLeaser.sprites[3 + j].color = dotColor;
                sLeaser.sprites[3 + this.dots.Length + j].color = this.sporeColor;
            }
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer("Items");
            }
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].RemoveFromContainer();
                if (i == 3 + this.dots.Length)
                { newContatiner.AddChild(sLeaser.sprites[2]); }
                else if (i != 2)
                { newContatiner.AddChild(sLeaser.sprites[i]); }
            }
        }

        public void OnBackMode()
        {
            base.ChangeMode(Mode.OnBack);
            base.gravity = 0f;
            this.collisionRange = 0f;
            this.rotationSpeed = 0f;
        }

        public void OnHandMode()
        {
            base.gravity = 0.86f;
            this.collisionRange = 50f;
        }

        public void SwitchBallLayer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, bool newOverlap)
        {
            if (newOverlap)
            {
                this.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Background"));
                return;
            }
            this.AddToContainer(sLeaser, rCam, null);
        }

        public void StickToPlayer(Player owner)
        {
            this.owner = owner;
            if (this.abstractStick != null)
            { this.abstractStick.Deactivate(); }
            this.abstractStick = new AbstractSporeStick(owner.abstractPhysicalObject, this.abstractPhysicalObject);
        }

        public void UnstickFromPlayer()
        {
            if (this.abstractStick != null)
            { this.abstractStick.Deactivate(); }
        }

        private readonly Vector2[,] segments;
        public Color sporeColor;
        private SporesSmoke smoke;
        public float beingEaten;
        private bool lastModeThrown;
        public float swallowed;
        private readonly Vector2[] dots;
        public bool? hideBehindPlayer;
        public bool superBoom, deerCall;
        private AbstractSporeStick abstractStick;

        private class AbstractSporeStick : AbstractPhysicalObject.AbstractObjectStick
        {
            public AbstractSporeStick(AbstractPhysicalObject A, AbstractPhysicalObject B) : base(A, B)
            {
            }

            public AbstractPhysicalObject Player
            {
                get { return this.A; }
                set { this.A = value; }
            }

            public AbstractPhysicalObject PuffBall
            {
                get { return this.B; }
                set { this.B = value; }
            }

            public override string SaveToString(int roomIndex)
            {
                return string.Concat(roomIndex.ToString(),
                    "<stkA>gripStk<stkA>",
                    this.A.ID.ToString(),
                    "<stkA>",
                    this.B.ID.ToString(),
                    "<stkA>",
                    "2",
                    "<stkA>",
                    "1"
                );
            }
        }
    }
}