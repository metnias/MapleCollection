using MapleCollection.SporeCat;
using MoreSlugcats;
using RWCustom;
using System.Collections.Generic;
using UnityEngine;
using static MapleCollection.MapleEnums;
using static Player;

//using MapleCollection.DragonKnight;
//using MapleCollection.SugarCat;

namespace MapleCollection
{
    public static class ModifyCat
    {
        public static void SubPatch()
        {
            On.Player.CanEatMeat += CanEatMeatPatch;
            On.Player.Grabability += GrababilityPatch;
            On.Player.ObjectEaten += ObjEatenPatch;
            On.Player.ctor += CtorPatch;
            On.Player.Update += UpdatePatch;
            On.Player.GrabUpdate += GrabUpdatePatch;
            On.Player.Destroy += DestroyPatch;
            On.PlayerGraphics.InitiateSprites += InitSprPatch;
            On.PlayerGraphics.Reset += ResetPatch;
            On.PlayerGraphics.SuckedIntoShortCut += SuckedIntoShortCutPatch;
            On.PlayerGraphics.DrawSprites += DrawSprPatch;
            On.PlayerGraphics.AddToContainer += AddToCtnrPatch;
            On.PlayerGraphics.ApplyPalette += PalettePatch;
            On.PlayerGraphics.Update += GrafUpdatePatch;

            SporeCatPuffBall.SubPatch();
            subs = new CatSupplement[4]; ghostSubs = new Dictionary<AbstractCreature, CatSupplement>();
            decos = new CatDecoration[4]; ghostDecos = new Dictionary<AbstractCreature, CatDecoration>();

            if (ModManager.MSC) OnMSCEnablePatch();
        }

        internal static void OnMSCEnablePatch()
        {
            On.MoreSlugcats.SlugNPCAI.PassingGrab += PupPassingGrab;
            On.MoreSlugcats.SlugNPCAI.LethalWeaponScore += PupLethalWeaponScore;
        }

        internal static void OnMSCDisablePatch()
        {
            On.MoreSlugcats.SlugNPCAI.PassingGrab -= PupPassingGrab;
            On.MoreSlugcats.SlugNPCAI.LethalWeaponScore -= PupLethalWeaponScore;
        }

        public static MapleSlug SwitchName(SlugcatStats.Name name)
        {
            if (name == SlugSpore) { return MapleSlug.SlugSpore; }
            else if (name == SlugSugar) { return MapleSlug.SlugSugar; }
            else if (name == SlugKnight) { return MapleSlug.SlugKnight; }
            return MapleSlug.Not;
        }

        public static bool IsMapleCat(Player self) => SwitchName(self.slugcatStats.name) >= 0;

        private static void ObjEatenPatch(On.Player.orig_ObjectEaten orig, Player self, IPlayerEdible edible)
        {
            if (!IsMapleCat(self)) { orig.Invoke(self, edible); return; }
            if (self.slugcatStats.name == SlugSpore)
            {
                if (self.graphicsModule != null)
                { (self.graphicsModule as PlayerGraphics).LookAtNothing(); }
                bool nerf = true;
                if (edible is Centipede) { nerf = false; }
                else if (edible is VultureGrub) { nerf = false; }
                //else if (edible is DangleFruit) { nerf = false; }
                else if (edible is EggBugEgg) { nerf = false; }
                else if (edible is SmallNeedleWorm) { nerf = false; }
                else if (edible is JellyFish) { nerf = false; }
                if (nerf)
                { for (int i = 0; i < edible.FoodPoints; i++) { self.AddQuarterFood(); } }
                else { self.AddFood(edible.FoodPoints); }
                if (self.spearOnBack != null) { self.spearOnBack.interactionLocked = true; }
            }
            else { orig.Invoke(self, edible); }
        }

        private static CatSupplement[] subs;
        private static Dictionary<AbstractCreature, CatSupplement> ghostSubs;

        public static void ClearSubsAndDecos()
        {
            for (int i = 0; i < subs.Length; i++) subs[i] = null;
            ghostSubs.Clear();
            for (int i = 0; i < decos.Length; i++) decos[i] = null;
            ghostDecos.Clear();
        }

        public static CatSupplement GetSub(Player self) => GetSub(self.abstractCreature);

        public static CatSupplement GetSub(AbstractCreature self)
        {
            if (!(self.realizedCreature?.State is PlayerState pState)) return null;
            if (!pState.isGhost) { return subs[pState.playerNumber]; }
            if (ghostSubs.TryGetValue(self, out var sub)) return sub;
            return null;
        }

        private static void CtorPatch(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig.Invoke(self, abstractCreature, world);
            switch (SwitchName(self.slugcatStats.name))
            {
                case MapleSlug.SlugSpore:
                    {
                        if (GetSub(self.abstractCreature) != null) break;
                        if (!self.playerState.isGhost)
                        {
                            subs[self.playerState.playerNumber] = new SporeCatSupplement(self.abstractCreature);
                            decos[self.playerState.playerNumber] = new SporeCatDecoration(self.abstractCreature);
                        }
                        else
                        {
                            ghostSubs.Add(self.abstractCreature, new SporeCatSupplement(self.abstractCreature));
                            ghostDecos.Add(self.abstractCreature, new SporeCatDecoration(self.abstractCreature));
                        }
                    }
                    break;

                    /*
                case MapleSlug.SlugSugar:
                    if (!self.playerState.isGhost)
                    {
                        subs[self.playerState.playerNumber] = new SugarCatSupplement(self);
                        decos[self.playerState.playerNumber] = new SugarCatDecoration(self);
                    }
                    else
                    {
                        ghostSubs.Add(new SugarCatSupplement(self));
                        ghostDecos.Add(new SugarCatDecoration(self));
                    }
                    break;

                case MapleSlug.SlugKnight:
                    if (!self.playerState.isGhost)
                    {
                        subs[self.playerState.playerNumber] = new KnightSupplement(self);
                        decos[self.playerState.playerNumber] = new KnightDecoration(self);
                    }
                    else
                    {
                        ghostSubs.Add(new KnightSupplement(self));
                        ghostDecos.Add(new KnightDecoration(self));
                    }
                    break;
                    */
            }
        }

        private static void UpdatePatch(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig.Invoke(self, eu);
            if (IsMapleCat(self)) GetSub(self).Update();
        }

        private static void GrabUpdatePatch(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            orig.Invoke(self, eu);
            switch (SwitchName(self.slugcatStats.name))
            {
                case MapleSlug.SlugSpore:
                    {
                        if (self.slugcatStats.name == SlugSpore)
                        {
                            if (self.swallowAndRegurgitateCounter > 0 && (GetSub(self) as SporeCatSupplement).Charge > 0f)
                            { self.swallowAndRegurgitateCounter = 0; }
                        }
                    }
                    break;
            }
        }

        private static void DestroyPatch(On.Player.orig_Destroy orig, Player self)
        {
            orig(self);
            if (IsMapleCat(self)) GetSub(self).Destroy();
        }

        private static bool CanEatMeatPatch(On.Player.orig_CanEatMeat orig, Player self, Creature crit)
        {
            switch (SwitchName(self.slugcatStats.name))
            {
                case MapleSlug.SlugSpore: return crit.dead && crit is InsectoidCreature;
            }
            return orig.Invoke(self, crit);
        }

        private static ObjectGrabability GrababilityPatch(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        {
            if (obj is SporeCatPuffBall ball)
            {
                if (!ball.Matured) return ObjectGrabability.CantGrab;
                if (ball.mode == Weapon.Mode.OnBack)
                {
                    if (ModManager.MSC && (self.isSlugpup || self.isNPC)) return ObjectGrabability.OneHand; // allow pup to snag this

                    if (self.slugcatStats.name == SlugSpore)
                    {
                        if (ball.parentDeco.player != self) return ObjectGrabability.CantGrab; // use yours, another sporecat!
                    }
                    else if (ModManager.CoopAvailable && !Custom.rainWorld.options.friendlySteal)
                        return ObjectGrabability.CantGrab; // don't snag when friendlySteal is off

                    if (self.input[0].y <= 0) return ObjectGrabability.CantGrab; // need to hold up
                }
                if (self.slugcatStats.name == SlugSpore)
                {
                    var sporeSub = GetSub(self) as SporeCatSupplement;
                    if (sporeSub.Charge > 0f) return ObjectGrabability.CantGrab; // can't grab while charging
                }
                return ObjectGrabability.OneHand;
            }
            return orig(self, obj);
        }

        private static void PupPassingGrab(On.MoreSlugcats.SlugNPCAI.orig_PassingGrab orig, SlugNPCAI self)
        {
            orig(self);
            if (self.behaviorType == SlugNPCAI.BehaviorType.Idle || self.behaviorType == SlugNPCAI.BehaviorType.Following)
            {
                var rng = UnityEngine.Random.value;
                if (self.itemTracker.ItemCount > 0)
                {
                    for (int i = 0; i < self.itemTracker.ItemCount; i++)
                    {
                        PhysicalObject item = self.itemTracker.GetRep(i).representedItem.realizedObject;
                        if (item is SporeCatPuffBall)
                        {
                            if (!self.CanGrabItem(item) || item.grabbedBy.Count != 0) continue;
                            //brave pups are less inclined to pick up objects in general
                            if (rng < Mathf.Lerp(0f, 0.9f, Mathf.InverseLerp(0.4f, 1f, self.cat.abstractCreature.personality.bravery)))
                            {
                                continue;
                            }
                            if (rng < Mathf.Lerp(0f, 0.05f, Mathf.InverseLerp(0.3f, 1f, self.cat.abstractCreature.personality.dominance)))
                            {
                                //dominant pups will grab misc useful stuff
                                self.cat.NPCForceGrab(item);
                                break;
                            }
                            else if (rng < Mathf.Lerp(0f, 0.05f, Mathf.InverseLerp(0.3f, 1f, self.cat.abstractCreature.personality.nervous)))
                            {
                                //nervous pups will grab non-lethal weapons
                                self.cat.NPCForceGrab(item);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private static float PupLethalWeaponScore(On.MoreSlugcats.SlugNPCAI.orig_LethalWeaponScore orig, SlugNPCAI self, PhysicalObject obj, Creature target)
        {
            if (obj is SporeCatPuffBall && target is InsectoidCreature)
            {
                return 5f;
            }
            return orig(self, obj, target);
        }

        private static CatDecoration[] decos;
        private static Dictionary<AbstractCreature, CatDecoration> ghostDecos;

        public static CatDecoration GetDeco(PlayerGraphics self) => GetDeco(self.player.abstractCreature);

        public static CatDecoration GetDeco(AbstractCreature self)
        {
            if (!(self.realizedCreature?.State is PlayerState pState)) return null;
            if (!pState.isGhost) { return decos[pState.playerNumber]; }
            if (ghostDecos.TryGetValue(self, out var deco)) return deco;
            return null;
        }

        private static void GrafUpdatePatch(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig.Invoke(self);
            if (IsMapleCat(self.player)) GetDeco(self).Update();
        }

        private static void InitSprPatch(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self,
            RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            if (!IsMapleCat(self.player)) { orig.Invoke(self, sLeaser, rCam); return; }
            /*
            if (self.player.slugcatStats.name == SlugSugar && SugarCatDecoration.debug)
            {
                if (self.DEBUGLABELS != null && self.DEBUGLABELS.Length > 0)
                { foreach (DebugLabel l in self.DEBUGLABELS) { l.label.RemoveFromContainer(); } }
                self.DEBUGLABELS = new DebugLabel[3]
                    {
                        new DebugLabel(self.owner, new Vector2(36f, 12f)) { relativePos = true },
                        new DebugLabel(self.owner, new Vector2(36f, 0f)) { relativePos = true },
                        new DebugLabel(self.owner, new Vector2(36f, -12f)) { relativePos = true }
                    };
                Debug.Log("SlugSugar DebugLabel active");
            } */
            orig.Invoke(self, sLeaser, rCam);
            GetDeco(self).InitiateSprites(sLeaser, rCam);
        }

        private static void SuckedIntoShortCutPatch(On.PlayerGraphics.orig_SuckedIntoShortCut orig,
            PlayerGraphics self, Vector2 shortCutPosition)
        {
            orig.Invoke(self, shortCutPosition);
            if (IsMapleCat(self.player)) GetDeco(self).SuckedIntoShortCut();
        }

        private static void ResetPatch(On.PlayerGraphics.orig_Reset orig, PlayerGraphics self)
        {
            orig.Invoke(self);
            if (IsMapleCat(self.player)) GetDeco(self).Reset();
        }

        private static void DrawSprPatch(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self,
            RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig.Invoke(self, sLeaser, rCam, timeStacker, camPos);
            if (IsMapleCat(self.player)) GetDeco(self).DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        private static void AddToCtnrPatch(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self,
            RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig.Invoke(self, sLeaser, rCam, newContatiner);
            if (IsMapleCat(self.player)) GetDeco(self).AddToContainer(sLeaser, rCam, newContatiner);
        }

        private static void PalettePatch(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self,
            RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig.Invoke(self, sLeaser, rCam, palette);
            if (IsMapleCat(self.player)) GetDeco(self).ApplyPalette(sLeaser, rCam, palette);
        }
    }
}