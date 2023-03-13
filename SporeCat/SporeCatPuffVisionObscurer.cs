using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MapleCollection.SporeCat
{
    public class SporeCatPuffVisionObscurer : VisionObscurer
    {
        public SporeCatPuffVisionObscurer(Vector2 pos, bool super, bool deer)
            : base(pos, super ? 120f : 70f, super ? 200f : 140f, super ? 3f : 1f)
        {
            this.super = super; this.deer = deer; this.prog = 0f;
        }

        private readonly bool super, deer;
        private float DeerTime => super ? 0.15f : 0.3f;

        public override void Update(bool eu)
        {
            base.Update(eu);
            float lastProg = this.prog;
            this.prog += 0.0076923077f * (super ? 0.5f : 1f);
            if (lastProg <= DeerTime && this.prog > DeerTime && this.deer) { this.AttractADeer(); }
            this.obscureFac = Mathf.InverseLerp(super ? 3f : 1f, super ? 0.6f : 0.3f, this.prog);
            this.rad = Mathf.Lerp(70f, 140f, Mathf.Pow(this.prog, 0.5f)) + (super ? 50f : 0f);
            if (this.prog > 1f) { this.Destroy(); }
        }

        private void AttractADeer()
        {
            WorldCoordinate worldCoordinate = this.room.GetWorldCoordinate(this.pos);
            if (!this.room.aimap.TileAccessibleToCreature(worldCoordinate.Tile, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Deer)))
            {
                for (int i = 0; i < 7; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        if (this.room.aimap.TileAccessibleToCreature(worldCoordinate.Tile + Custom.eightDirections[j] * i, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Deer)))
                        {
                            worldCoordinate.Tile += Custom.eightDirections[j] * i;
                            i = 1000;
                            break;
                        }
                    }
                }
            }
            CreatureSpecificAImap creatureSpecificAImap = this.room.aimap.CreatureSpecificAImap(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Deer));
            int num = int.MaxValue;
            int num2 = -1;
            for (int k = 0; k < creatureSpecificAImap.numberOfNodes; k++)
            {
                if (this.room.abstractRoom.nodes[this.room.abstractRoom.CreatureSpecificToCommonNodeIndex(k, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Deer))].entranceWidth > 4 && creatureSpecificAImap.GetDistanceToExit(worldCoordinate.x, worldCoordinate.y, k) > 0 && creatureSpecificAImap.GetDistanceToExit(worldCoordinate.x, worldCoordinate.y, k) < num)
                {
                    num = creatureSpecificAImap.GetDistanceToExit(worldCoordinate.x, worldCoordinate.y, k);
                    num2 = k;
                }
            }
            if (num2 > -1)
            {
                worldCoordinate.abstractNode = this.room.abstractRoom.CreatureSpecificToCommonNodeIndex(num2, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Deer));
            }
            List<AbstractCreature> list = new List<AbstractCreature>();
            for (int l = 0; l < this.room.abstractRoom.creatures.Count; l++)
            {
                if (this.room.abstractRoom.creatures[l].creatureTemplate.type == CreatureTemplate.Type.Deer && this.room.abstractRoom.creatures[l].realizedCreature != null && this.room.abstractRoom.creatures[l].realizedCreature.Consious && (this.room.abstractRoom.creatures[l].realizedCreature as Deer).AI.goToPuffBall == null && (this.room.abstractRoom.creatures[l].realizedCreature as Deer).AI.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate))
                {
                    list.Add(this.room.abstractRoom.creatures[l]);
                }
            }
            if (list.Count > 0)
            {
                (list[UnityEngine.Random.Range(0, list.Count)].abstractAI as DeerAbstractAI).AttractToSporeCloud(worldCoordinate);
                Debug.Log("A DEER IN THE ROOM WAS ATTRACTED!");
                this.room.PlaySound(SoundID.In_Room_Deer_Summoned, 0f, 1f, 1f);
                if (UnityEngine.Random.value < 0.5f)
                { return; }
            }
            if (this.room.world.rainCycle.TimeUntilRain < 800)
            { return; }
            bool flag = false;
            int num3 = 0;
            while (num3 < DeerAbstractAI.UGLYHARDCODEDALLOWEDROOMS.Length && !flag)
            {
                if (DeerAbstractAI.UGLYHARDCODEDALLOWEDROOMS[num3] == this.room.abstractRoom.name) { flag = true; }
                num3++;
            }
            if (!flag) { return; }
            if (worldCoordinate.NodeDefined)
            {
                list.Clear();
                for (int m = 0; m < this.room.world.NumberOfRooms; m++)
                {
                    if (this.room.world.firstRoomIndex + m != this.room.abstractRoom.index)
                    {
                        AbstractRoom abstractRoom = this.room.world.GetAbstractRoom(this.room.world.firstRoomIndex + m);
                        if (abstractRoom.realizedRoom == null)
                        {
                            for (int n = 0; n < abstractRoom.creatures.Count; n++)
                            {
                                if (abstractRoom.creatures[n].creatureTemplate.type == CreatureTemplate.Type.Deer)
                                {
                                    list.Add(abstractRoom.creatures[n]);
                                }
                            }
                        }
                    }
                }
                if (list.Count > 0)
                {
                    (list[UnityEngine.Random.Range(0, list.Count)].abstractAI as DeerAbstractAI).AttractToSporeCloud(worldCoordinate);
                    float pan = 0f;
                    if (this.room.abstractRoom.nodes[worldCoordinate.abstractNode].type == AbstractRoomNode.Type.SideExit)
                    {
                        RoomBorderExit roomBorderExit = this.room.borderExits[worldCoordinate.abstractNode - this.room.exitAndDenIndex.Length];
                        if (roomBorderExit.borderTiles[0].x == 0)
                        {
                            pan = -1f;
                        }
                        else if (roomBorderExit.borderTiles[0].x == this.room.TileWidth - 1)
                        {
                            pan = 1f;
                        }
                    }
                    this.room.PlaySound(SoundID.Distant_Deer_Summoned, pan, 1f, 1f);
                    Debug.Log("A DEER WAS ATTRACTED! " + worldCoordinate);
                }
            }
        }

        private float prog;
    }
}