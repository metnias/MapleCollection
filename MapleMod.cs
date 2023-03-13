using MapleCollection.SugarCat;
using Partiality.Modloader;
using System.IO;
using UnityEngine;

namespace MapleCollection
{
    public class MapleMod : PartialityMod
    {
        public MapleMod()
        {
            this.ModID = "MapleCollection";
            this.Version = "0200";
            this.author = "MapleCat & topicular";
        }

        // xcopy "$(TargetDir)SporeCatResources\*.*" "E:\Game\Rain World\mods\SporeCatResources" /Y /I /E
        // xcopy "$(TargetDir)SporeCatResources\*.*" "E:\SteamLibrary\steamapps\common\Rain World\Mods\SporeCatResources" /Y /I /E

        public override void OnEnable()
        {
            base.OnEnable();
            enabled = true;
            customResources = false;

            resourcePath = string.Concat(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), Path.DirectorySeparatorChar, "MapleResources", Path.DirectorySeparatorChar);
            /* string chk = AddScene.CheckIntegrity();
            if (string.IsNullOrEmpty(chk)) { customResources = true; }
            else
            {
                string msg = string.Concat("SugarCatResources is installed incorrectly: Missing \'", chk, "\'");
                Debug.Log(msg);
                Debug.LogException(new MissingReferenceException(msg));
                customResources = false;
            } */
            AddScene.SubPatch();
            AddPlayer.SubPatch();
            ModifyCat.SubPatch();
            ModifyWorld.SubPatch();
            TutorialPatch.SubPatch();
            On.RainWorld.LoadResources += new On.RainWorld.hook_LoadResources(LoadMapleSprites);
            //DataManager.LogAllResources();
            //DataManager.EncodeExternalPNG();

            //RXColorHSL c = RXColor.HSLFromColor(SugarCatDecoration.frillStart);
            //Debug.Log(c.h.ToString());
            //Debug.Log(c.s.ToString());
            //Debug.Log(c.l.ToString());
        }

        public static string resourcePath;

        private static bool enabled, customResources;
        public static bool Enabled => EnumExt && enabled;
        public static bool CustomResources => EnumExt && enabled && customResources;
        public static bool EnumExt => (int)EnumExt_Maple.SlugSpore > 2;

        public static string Translate(string text) => text;

        public static void LoadMapleSprites(On.RainWorld.orig_LoadResources orig, RainWorld self)
        {
            orig.Invoke(self);
            string[] pngName = new string[] { "SugarHeadStripes" };
            string[] dataName = new string[] { "SugarHeadStripesData" };

            for (int i = 0; i < pngName.Length; i++)
            {
                Texture2D tex = DataManager.ReadPNG("Sprites." + pngName[i]);
                if (Futile.atlasManager.DoesContainElementWithName(pngName[i]))
                { Futile.atlasManager.ActuallyUnloadAtlasOrImage(pngName[i]); }
                FAtlas atlas = Futile.atlasManager.LoadAtlasFromTexture(pngName[i], tex);
                if (string.IsNullOrEmpty(dataName[i])) { continue; }
                string data = DataManager.ReadTXT("Sprites." + dataName[i]);
                DataManager.LoadAtlasDataFromString(ref atlas, data);
            }
        }
    }
}