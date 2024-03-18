using System.IO;
using System.Reflection;
using BepInEx;
using MapleCollection;
using UnityEngine;
using SlugBase;
using BepInEx.Logging;
using System.Linq;
using System;
using MapleCollection.SporeCat;
using System.Security.Permissions;
using System.Security;
using CatSub.Cat;
using static MapleCollection.MapleEnums;
using CatSub.Story;
using RWCustom;

#region Assembly attributes

[module: UnverifiableCode]
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618
[assembly: AssemblyVersion(MaplePlugin.PLUGIN_VERSION)]
[assembly: AssemblyFileVersion(MaplePlugin.PLUGIN_VERSION)]
[assembly: AssemblyTitle(MaplePlugin.PLUGIN_NAME + " (" + MaplePlugin.PLUGIN_ID + ")")]
[assembly: AssemblyProduct(MaplePlugin.PLUGIN_NAME)]

#endregion Assembly attributes

namespace MapleCollection
{
    [BepInDependency("com.rainworldgame.topicular.catsupplement.plugin")]
    [BepInDependency("slime-cubed.slugbase")]
    [BepInPlugin(PLUGIN_ID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInProcess("RainWorld.exe")]
    public class MaplePlugin : BaseUnityPlugin
    {
        public const string PLUGIN_ID = "com.rainworldgame.maplecollection.plugin";
        public const string PLUGIN_NAME = "MapleCollection";
        public const string PLUGIN_VERSION = "2.1.0.3";

        public void OnEnable()
        {
            LogSource = this.Logger;
            On.RainWorld.OnModsInit += WrapInit(OnInit);
            On.RainWorld.OnModsEnabled += OnModsEnabled;
            On.RainWorld.OnModsDisabled += OnModsDisabled;
        }

        private static bool init = false;
        internal static ManualLogSource LogSource;

        private static void OnInit(RainWorld rw)
        {
            lastMSCEnabled = ModManager.MSC;
            RegisterExtEnum();
            SubRegistry.Register(SlugSpore, (player) => new SporeCatSupplement(player));
            DecoRegistry.Register(SlugSpore, (player) => new SporeCatDecoration(player));
            StoryRegistry.RegisterStartPos("LF_A11", new IntVector2(11, 30));
            StoryRegistry.RegisterTimeline(new StoryRegistry.TimelinePointer(SlugSpore, StoryRegistry.TimelinePointer.Relative.Before, SlugcatStats.Name.Red));

            AddPlayer.Patch();
            ModifyCat.Patch();
            ModifyWorld.Patch();
            LogSource.LogInfo("MapleCollection Initialized!");
        }

        public static On.RainWorld.hook_OnModsInit WrapInit(Action<RainWorld> loadResources)
        {
            return (orig, self) =>
            {
                orig(self);

                if (init) return;

                try
                {
                    loadResources(self);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                init = true;
            };
        }

        private static bool lastMSCEnabled;

        private static void OnModsEnabled(On.RainWorld.orig_OnModsEnabled orig, RainWorld rw, ModManager.Mod[] newlyEnabledMods)
        {
            orig(rw, newlyEnabledMods);
            if (!lastMSCEnabled && ModManager.MSC)
            {
                LogSource.LogInfo("MapleCollection detected MSC newly enabled.");
                ModifyCat.OnMSCEnablePatch();
                SporeCatPuffBall.OnMSCEnablePatch();
                lastMSCEnabled = ModManager.MSC;
            }
        }

        private static void OnModsDisabled(On.RainWorld.orig_OnModsDisabled orig, RainWorld rw, ModManager.Mod[] newlyDisabledMods)
        {
            orig(rw, newlyDisabledMods);
            if (lastMSCEnabled && !ModManager.MSC)
            {
                LogSource.LogInfo("MapleCollection detected MSC newly disabled.");
                ModifyCat.OnMSCDisablePatch();
                SporeCatPuffBall.OnMSCDisablePatch();
                lastMSCEnabled = ModManager.MSC;
            }
            /*
            if (!init) return;
            foreach (var mod in newlyDisabledMods)
            {
                if (mod.id == "maplecollection")
                {
                    ExtEnum_Maple.UnregisterExtEnum();

                    init = false;
                    return;
                }
            }*/
        }
    }
}