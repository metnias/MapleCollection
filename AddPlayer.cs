using MapleCollection.SporeCat;

namespace MapleCollection
{
    public static class AddPlayer
    {
        public static void Patch()
        {
            SporeCatPuffBall.SubPatch();
        }
    }
}