using CatSub.Cat;
using MapleCollection.SporeCat;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using static MapleCollection.MapleEnums;
using static Player;

namespace MapleCollection
{
    public static class ModifyCat
    {
        public static void Patch()
        {
            On.Player.CanEatMeat += CanEatMeatPatch;
            On.Player.Grabability += GrababilityPatch;
            On.Player.ObjectEaten += ObjEatenPatch;
            On.Player.GrabUpdate += GrabUpdatePatch;

            SporeCatPuffBall.SubPatch();

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

        private static void GrabUpdatePatch(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            orig.Invoke(self, eu);
            switch (SwitchName(self.slugcatStats.name))
            {
                case MapleSlug.SlugSpore:
                    if (CatSupplement.TryGetSub(self.playerState, out SporeCatSupplement sporeSub))
                        if (self.swallowAndRegurgitateCounter > 0 && sporeSub.Charge > 0f)
                            self.swallowAndRegurgitateCounter = 0;
                    break;
            }
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
                    if (CatSupplement.TryGetSub(self.playerState, out SporeCatSupplement sporeSub))
                    {
                        if (sporeSub.Charge > 0f) return ObjectGrabability.CantGrab; // can't grab while charging
                    }
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
                            if ((item as SporeCatPuffBall).mode == Weapon.Mode.OnBack)
                            {
                                if (self.creature.Room.shelter) return; // don't starve sporecat in shelter!
                                rng *= 12f; // much less likely to grab
                            }
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
    }
}