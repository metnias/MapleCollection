using RWCustom;
using UnityEngine;
using static PlayerGraphics;

namespace MapleCollection.SporeCat
{
    public class SporeCatDecoration : CatDecoration
    {
        public SporeCatDecoration(AbstractCreature owner) : base(owner)
        {
            thirdColor = dotDefaultColor;
        }

        private int GetTailIdx(int idx) => this.dotIdx[idx]; //Mathf.FloorToInt((OwnerGrp.tail.Length - 1) * ((float)dotIdx / dots.Length));

        private SporeCatSupplement OwnerSub => ModifyCat.GetSub(this.player) as SporeCatSupplement;

        private Vector2[] dots; private int[] dotIdx;

#pragma warning disable IDE0060

        private void InitDots()
        {
#pragma warning disable CS0618
            int seed = UnityEngine.Random.seed;
            UnityEngine.Random.seed = 12345;
            this.dots = new Vector2[this.OwnerGraphic.tail.Length * 2];
            this.dotIdx = new int[this.dots.Length];
            for (int i = 0; i < this.dots.Length; i++)
            {
                this.dotIdx[i] = Mathf.FloorToInt(Custom.LerpMap(i, 0f, dots.Length, 0f, OwnerGraphic.tail.Length - 0.5f, 1.8f));
                //Debug.Log(string.Concat("DotIdx: ", i, " -> ", dotIdx[i]));
                this.dots[i] = Custom.RNV() * this.OwnerGraphic.tail[GetTailIdx(i)].rad * 0.6f;
            }
            UnityEngine.Random.seed = seed;
#pragma warning restore CS0618
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
            if (this.dots == null) { this.InitDots(); }

            this.sprites = new FSprite[this.dots.Length];
            for (int i = 0; i < this.dots.Length; i++)
            {
                this.sprites[i] = new FSprite("JetFishEyeB", true);
                this.container.AddChild(this.sprites[i]);
            }
            this.AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            for (int i = 0; i < this.dots.Length; i++)
            {
                int ti = GetTailIdx(i);
                Vector2 pos = Vector2.Lerp(this.OwnerGraphic.tail[ti].lastPos, this.OwnerGraphic.tail[ti].pos, timeStacker);
                Vector2 lastPos = ti == 0 ? Vector2.Lerp(this.OwnerGraphic.drawPositions[1, 1], this.OwnerGraphic.drawPositions[1, 0], timeStacker) :
                    Vector2.Lerp(this.OwnerGraphic.tail[ti - 1].lastPos, this.OwnerGraphic.tail[ti - 1].pos, timeStacker);
                float rotation = Custom.VecToDeg(pos - lastPos);
                Vector2 dotPos = pos + Custom.RotateAroundOrigo(new Vector2(this.dots[i].x, this.dots[i].y), rotation);
                this.sprites[i].x = dotPos.x - camPos.x;
                this.sprites[i].y = dotPos.y - camPos.y;
                this.sprites[i].rotation = Custom.VecToDeg(Custom.RotateAroundOrigo(this.dots[i], rotation).normalized);
                this.sprites[i].scaleY = Custom.LerpMap(this.dots[i].magnitude, 0f, 1f, 1f, 0.5f, 4f);
                this.sprites[i].scaleX = Mathf.Lerp(this.sprites[i].scaleY, 1f, 0.5f);
            }
            if (this.OwnerSub.Charge > 0.7f) SqueezeEyes();

            void SqueezeEyes()
            {
                sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName("FaceStunned");
                Vector2 pos0 = Vector2.Lerp(this.OwnerGraphic.drawPositions[0, 1], this.OwnerGraphic.drawPositions[0, 0], timeStacker);
                Vector2 pos1 = Vector2.Lerp(this.OwnerGraphic.drawPositions[1, 1], this.OwnerGraphic.drawPositions[1, 0], timeStacker);
                Vector2 head = Vector2.Lerp(this.OwnerGraphic.head.lastPos, this.OwnerGraphic.head.pos, timeStacker);
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

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            base.ApplyPalette(sLeaser, rCam, palette);
            Color dc = GetThirdColor();
            Color dotColorW = Color.Lerp(Color.Lerp(dc, palette.texture.GetPixel(11, 4), 0.2f), Color.white, 0.5f);
            Color dotColorB = Color.Lerp(Color.Lerp(dc, palette.texture.GetPixel(11, 4), 0.2f), palette.blackColor, 0.5f);
            for (int i = 0; i < this.dots.Length; i++)
            { this.sprites[i].color = i % 2 == 1 ? dotColorB : dotColorW; } //i % 3 == 2
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            base.AddToContainer(sLeaser, rCam, newContatiner);
        }
    }
}