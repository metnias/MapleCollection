using CatSub.Cat;
using RWCustom;
using UnityEngine;
using static PlayerGraphics;

namespace MapleCollection.SporeCat
{
    public class SporeCatDecoration : CatDecoration
    {
        public SporeCatDecoration(Player player) : base(player)
        {
        }

        public SporeCatDecoration() : base()
        {
        }

        private int GetTailIdx(int idx) => dotIdx[idx]; //Mathf.FloorToInt((OwnerGrp.tail.Length - 1) * ((float)dotIdx / dots.Length));

        private Vector2[] dots; private int[] dotIdx;

        protected override Color DefaultThirdColor => dotDefaultColor;

        private void InitDots()
        {
            var state = Random.state;
            Random.InitState(12345);
            dots = new Vector2[self.tail.Length * 2];
            dotIdx = new int[dots.Length];
            for (int i = 0; i < dots.Length; i++)
            {
                dotIdx[i] = Mathf.FloorToInt(Custom.LerpMap(i, 0f, dots.Length, 0f, self.tail.Length - 0.5f, 1.8f));
                //Debug.Log(string.Concat("DotIdx: ", i, " -> ", dotIdx[i]));
                dots[i] = Custom.RNV() * self.tail[GetTailIdx(i)].rad * 0.6f;
            }
            Random.state = state;
        }

        public override void InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(orig, sLeaser, rCam);
            if (dots == null) { InitDots(); }

            sprites = new FSprite[dots.Length];
            for (int i = 0; i < dots.Length; i++)
            {
                sprites[i] = new FSprite("JetFishEyeB", true);
                container.AddChild(sprites[i]);
            }
            self.AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(orig, sLeaser, rCam, timeStacker, camPos);
            for (int i = 0; i < dots.Length; i++)
            {
                int ti = GetTailIdx(i);
                Vector2 pos = Vector2.Lerp(self.tail[ti].lastPos, self.tail[ti].pos, timeStacker);
                Vector2 lastPos = ti == 0 ? Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker) :
                    Vector2.Lerp(self.tail[ti - 1].lastPos, self.tail[ti - 1].pos, timeStacker);
                float rotation = Custom.VecToDeg(pos - lastPos);
                Vector2 dotPos = pos + Custom.RotateAroundOrigo(new Vector2(dots[i].x, dots[i].y), rotation);
                sprites[i].x = dotPos.x - camPos.x;
                sprites[i].y = dotPos.y - camPos.y;
                sprites[i].rotation = Custom.VecToDeg(Custom.RotateAroundOrigo(dots[i], rotation).normalized);
                sprites[i].scaleY = Custom.LerpMap(dots[i].magnitude, 0f, 1f, 1f, 0.5f, 4f);
                sprites[i].scaleX = Mathf.Lerp(sprites[i].scaleY, 1f, 0.5f);
            }
            if (TryGetSub(out SporeCatSupplement sporeSub) && sporeSub.Charge > 0.7f) SqueezeEyes();

            void SqueezeEyes()
            {
                sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName("FaceStunned");
                Vector2 pos0 = Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker);
                Vector2 pos1 = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker);
                Vector2 head = Vector2.Lerp(self.head.lastPos, self.head.pos, timeStacker);
                float rot = Custom.AimFromOneVectorToAnother(Vector2.Lerp(pos1, pos0, 0.5f), head);
                sLeaser.sprites[9].rotation = rot;
                sLeaser.sprites[9].x = head.x - camPos.x; sLeaser.sprites[9].y = head.y - camPos.y;
                sLeaser.sprites[3].element = Futile.atlasManager.GetElementWithName("HeadA0");
                sLeaser.sprites[3].rotation = rot;
                sLeaser.sprites[3].scaleX = ((rot >= 0f) ? 1f : -1f);
            }
        }

        //RGB 216 193 149; D8C195
        internal static readonly Color catDefaultColor = new Color(0.8471f, 0.7569f, 0.5843f);

        //RGB 204 255 180: CCFF80
        internal static readonly Color dotDefaultColor = new Color(0.8f, 1f, 0.5f);

        //MapleEnums.ColorSporeDots.GetColor(PGraphics) ?? dotDefaultColor;

        public override void ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            base.ApplyPalette(orig, sLeaser, rCam, palette);
            Color dc = GetThirdColor();
            Color dotColorW = Color.Lerp(Color.Lerp(dc, palette.texture.GetPixel(11, 4), 0.2f), Color.white, 0.5f);
            Color dotColorB = Color.Lerp(Color.Lerp(dc, palette.texture.GetPixel(11, 4), 0.2f), palette.blackColor, 0.5f);
            for (int i = 0; i < this.dots.Length; i++)
            { this.sprites[i].color = i % 2 == 1 ? dotColorB : dotColorW; } //i % 3 == 2
        }
    }
}