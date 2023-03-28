using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;

namespace MapleCollection
{
    public static class ModifyWorld
    {
        public static void Patch()
        {
            On.OverWorld.LoadWorld += GetWorldLoaded;
            On.WorldLoader.GeneratePopulation += GenPopSwap;
            VanillaException = new List<CreatureTemplate.Type>()
            {
                // Obviously
                CreatureTemplate.Type.Slugcat,
                // Insects
                CreatureTemplate.Type.Centipede,
                CreatureTemplate.Type.Centiwing,
                CreatureTemplate.Type.RedCentipede,
                CreatureTemplate.Type.SmallCentipede,
                CreatureTemplate.Type.DropBug,
                CreatureTemplate.Type.EggBug,
                CreatureTemplate.Type.CicadaA,
                CreatureTemplate.Type.CicadaB,
                CreatureTemplate.Type.Spider,
                CreatureTemplate.Type.BigSpider,
                CreatureTemplate.Type.SpitterSpider,
                CreatureTemplate.Type.BigNeedleWorm,
                CreatureTemplate.Type.SmallNeedleWorm,
                // Plants
                CreatureTemplate.Type.PoleMimic,
                CreatureTemplate.Type.TentaclePlant,
                // Misc/Itemlike
                CreatureTemplate.Type.Hazer,
                CreatureTemplate.Type.VultureGrub,
                CreatureTemplate.Type.TubeWorm,
                CreatureTemplate.Type.GarbageWorm,
                // Water based
                CreatureTemplate.Type.BigEel,
                CreatureTemplate.Type.Salamander,
                CreatureTemplate.Type.JetFish,
                CreatureTemplate.Type.SeaLeech,
                CreatureTemplate.Type.Leech,
                // Special
                CreatureTemplate.Type.Scavenger,
                CreatureTemplate.Type.BrotherLongLegs,
                CreatureTemplate.Type.DaddyLongLegs,
                CreatureTemplate.Type.Overseer,
                //CreatureTemplate.Type.RedLizard,
                CreatureTemplate.Type.YellowLizard,
                CreatureTemplate.Type.MirosBird,
                CreatureTemplate.Type.TempleGuard,
                CreatureTemplate.Type.Deer
            };
            WaterCreatures = new List<CreatureTemplate.Type>();
            if (ModManager.MSC)
            {
                VanillaException.AddRange(
                    new CreatureTemplate.Type[]
                    {
                        // Special
                        MoreSlugcatsEnums.CreatureTemplateType.SlugNPC,
                        MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy,
                        MoreSlugcatsEnums.CreatureTemplateType.Inspector,
                        MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs,
                        MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing,
                        MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite,
                        // Insects
                        MoreSlugcatsEnums.CreatureTemplateType.AquaCenti,
                        MoreSlugcatsEnums.CreatureTemplateType.FireBug,
                        MoreSlugcatsEnums.CreatureTemplateType.MotherSpider,
                        MoreSlugcatsEnums.CreatureTemplateType.Yeek,
                        // Water based
                        MoreSlugcatsEnums.CreatureTemplateType.BigJelly,
                        MoreSlugcatsEnums.CreatureTemplateType.JungleLeech
                    }
                    );
                WaterCreatures.AddRange(
                    new CreatureTemplate.Type[]
                    {
                        CreatureTemplate.Type.JetFish,
                        CreatureTemplate.Type.SeaLeech,
                        MoreSlugcatsEnums.CreatureTemplateType.JungleLeech
                    }
                    );
            }
        }

        private static void GetWorldLoaded(On.OverWorld.orig_LoadWorld orig, OverWorld self, string worldName, SlugcatStats.Name playerCharacterNumber, bool singleRoomWorld)
        {
            mapleworld = ModifyCat.SwitchName(playerCharacterNumber);
            orig(self, worldName, playerCharacterNumber, singleRoomWorld);
        }

        internal static MapleEnums.MapleSlug mapleworld = MapleEnums.MapleSlug.Not;

        private static float ProbabilityMultiplier(string worldName)
        {
            if (worldName == "UW" || worldName == "DS") { return 2f; }
            if (worldName == "SL" || worldName == "SH") { return 1.5f; }
            return 1f;
        }

        private static bool IsAirFocusedRegion(World world)
        {
            AbstractRoom[] rooms = world.abstractRooms;
            int fallCount = 0;
            for (int i = 0; i < rooms.Length; i++)
            { if (CheckFallRoom(rooms[i])) { fallCount++; } }
            MaplePlugin.LogSource.LogInfo(string.Concat("Sporecat SkyRegion Check ", world.name, " fallCount: ", fallCount, "/", rooms.Length, " Verdict: ", fallCount > rooms.Length * 0.35f));
            return fallCount > rooms.Length * 0.35f;
        }

        private static bool CheckFallRoom(AbstractRoom room)
        {
            if (room.gate || room.name.ToLower().Contains("offscreen")) { return false; }
            try
            {
                string[] lines = File.ReadAllLines(WorldLoader.FindRoomFile(room.name, false, ".txt"));
                if (lines[1].Split(new char[] { '|' })[1] != "-1") { return false; } // water
                string[] array = lines[1].Split(new char[] { '|' })[0].Split(new char[] { '*' });
                int width = Convert.ToInt32(array[0]); int height = Convert.ToInt32(array[1]);
                string[] array5 = lines[11].Split(new char[] { '|' });
                IntVector2 pos = new IntVector2(0, height - 1);
                for (int m = 0; m < array5.Length - 1; m++)
                {
                    if (pos.y == 0)
                    {
                        string[] array6 = array5[m].Split(new char[] { ',' });
                        //Debug.Log(string.Concat("Tile ", pos.x, ",", pos.y, ": ", ((Room.Tile.TerrainType)int.Parse(array6[0])).ToString()));
                        if (int.Parse(array6[0]) != (int)Room.Tile.TerrainType.Solid) { return true; }
                    }
                    pos.y--;
                    if (pos.y < 0) { pos.x++; pos.y = height - 1; }
                }
                return false;
            }
            catch (Exception e) { MaplePlugin.LogSource.LogError(e); return false; }
        }

        private static List<CreatureTemplate.Type> VanillaException;
        private static List<CreatureTemplate.Type> WaterCreatures;

        private static void GenPopSwap(On.WorldLoader.orig_GeneratePopulation orig, WorldLoader self, bool fresh)
        {
            if (mapleworld == MapleEnums.MapleSlug.SlugSpore && fresh && !self.singleRoomWorld)
            {
                MaplePlugin.LogSource.LogInfo("Sporecat converts creatures to insects!");
                float mul = ProbabilityMultiplier(self.worldName);
                bool air = IsAirFocusedRegion(self.world);
                int count = 0;
                for (int k = 0; k < self.spawners.Count; k++)
                {
                    if (self.spawners[k] is World.SimpleSpawner spawner)
                    {
                        if (VanillaException.Contains(spawner.creatureType))
                        {
                            if (!ModManager.MSC) continue;
                            if (WaterCreatures.Contains(spawner.creatureType))
                            {
                                if (UnityEngine.Random.value < 0.05f * mul)
                                {
                                    count++;
                                    spawner.creatureType = MoreSlugcatsEnums.CreatureTemplateType.AquaCenti;
                                    spawner.amount = 1;
                                }
                            }
                            continue;
                        }
                        CreatureTemplate template = StaticWorld.GetCreatureTemplate(spawner.creatureType);
                        if (spawner.creatureType == CreatureTemplate.Type.RedLizard)
                        { spawner.creatureType = CreatureTemplate.Type.RedCentipede; }
                        else if (template.IsLizard)
                        {
                            if (UnityEngine.Random.value < 0.3f * mul)
                            {
                                count++;
                                spawner.creatureType = (air && UnityEngine.Random.value < 0.5f) || (spawner.creatureType == CreatureTemplate.Type.CyanLizard ||
                                    (ModManager.MSC && spawner.creatureType == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard)) ?
                                    CreatureTemplate.Type.Centiwing : CreatureTemplate.Type.Centipede;
                            }
                        }
                        else if (spawner.creatureType == CreatureTemplate.Type.KingVulture) // Nerf King Vulture
                        { spawner.creatureType = CreatureTemplate.Type.Vulture; }
                        else
                        {
                            if (UnityEngine.Random.value < 0.2f * mul)
                            {
                                count++;
                                spawner.creatureType = template.dangerousToPlayer > 0.2f ?
                                    CreatureTemplate.Type.BigNeedleWorm :
                                    air ? (UnityEngine.Random.value < 0.5f ? CreatureTemplate.Type.CicadaA : CreatureTemplate.Type.CicadaB)
                                    : CreatureTemplate.Type.EggBug;
                            }
                        }
                    }
                    else if (self.spawners[k] is World.Lineage lineage)
                    {
                        CreatureTemplate.Type type = lineage.CurrentType((self.game.session as StoryGameSession).saveState);
                        if (type == null || type.Index < 0) continue;
                        CreatureTemplate temp = StaticWorld.GetCreatureTemplate(type);
                        bool liz; int i = 0;
                        if (temp.IsLizard)
                        {
                            if (type == CreatureTemplate.Type.RedLizard)
                            { lineage.creatureTypes[0] = (int)(ModManager.MSC ? MoreSlugcatsEnums.CreatureTemplateType.SpitLizard : CreatureTemplate.Type.GreenLizard); i = 1; }
                            if (UnityEngine.Random.value > 0.3f * mul) { continue; }
                            liz = true;
                        }
                        else if (type == CreatureTemplate.Type.KingVulture)
                        { lineage.creatureTypes[0] = (int)CreatureTemplate.Type.Vulture; continue; }
                        else { if (UnityEngine.Random.value > 0.2f * mul) { continue; } liz = false; }
                        count++;
                        MaplePlugin.LogSource.LogInfo(lineage.denString + " begin swap");
                        for (; i < lineage.creatureTypes.Length; i++)
                        {
                            CreatureTemplate.Type critType = IntToType(lineage.creatureTypes[i]);
                            if (VanillaException.Contains(critType))
                            {
                                if (!ModManager.MSC) continue;
                                if (WaterCreatures.Contains(critType) && UnityEngine.Random.value < 0.1f * mul)
                                    lineage.creatureTypes[i] = (int)MoreSlugcatsEnums.CreatureTemplateType.AquaCenti;
                                continue;
                            }
                            if (liz)
                            {
                                lineage.creatureTypes[i] = (int)(i > 2 ||
                                    critType == CreatureTemplate.Type.RedLizard ?
                                    (ModManager.MSC ? MoreSlugcatsEnums.CreatureTemplateType.FireBug : CreatureTemplate.Type.RedCentipede) :
                                    (critType == CreatureTemplate.Type.CyanLizard ||
                                    (ModManager.MSC && critType == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard) ?
                                    CreatureTemplate.Type.Centiwing : CreatureTemplate.Type.Centipede));
                            }
                            else
                            {
                                lineage.creatureTypes[i] = (int)(air ?
                                    i < 1 ? CreatureTemplate.Type.SmallNeedleWorm : CreatureTemplate.Type.BigNeedleWorm :
                                    i < 2 ? CreatureTemplate.Type.BigSpider :
                                    (ModManager.MSC ? MoreSlugcatsEnums.CreatureTemplateType.MotherSpider : CreatureTemplate.Type.SpitterSpider));
                            }
                        }
                    }
                }
                MaplePlugin.LogSource.LogInfo(string.Concat("Sporecat Insectified Spawners Count: ", count));
            }
            orig.Invoke(self, fresh);

            CreatureTemplate.Type IntToType(int type)
                => new CreatureTemplate.Type(ExtEnum<CreatureTemplate.Type>.values.GetEntry(type), false);
        }
    }
}