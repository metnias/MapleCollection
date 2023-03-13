using Menu;
using SlugBase.DataTypes;

namespace MapleCollection
{
    public static class MapleEnums
    {
        public enum MapleSlug
        {
            Not = -1,
            SlugSpore = 0,
            SlugSugar = 1,
            SlugKnight = 2
        }

        public static SlugcatStats.Name SlugSpore;
        public static SlugcatStats.Name SlugSugar;
        public static SlugcatStats.Name SlugKnight;

        public static AbstractPhysicalObject.AbstractObjectType SporePuffBall;
        //public static AbstractPhysicalObject.AbstractObjectType SugarSyrup;

        public static PlayerColor ColorSporeDots;

        internal static void RegisterExtEnum()
        {
            SporePuffBall = new AbstractPhysicalObject.AbstractObjectType(nameof(SporePuffBall), true);
            SlugSpore = new SlugcatStats.Name(nameof(SlugSpore), false);
            SlugSugar = new SlugcatStats.Name(nameof(SlugSugar), false);
            SlugKnight = new SlugcatStats.Name(nameof(SlugKnight), false);

            ColorSporeDots = new PlayerColor(nameof(ColorSporeDots));
        }

        internal static void UnregisterExtEnum()
        {
            SporePuffBall?.Unregister();
        }
    }
}