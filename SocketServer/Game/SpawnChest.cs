using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    public class GameObjectComponent : Component
    {
        public GameObjectActor gameObject
        {
            get { return this.actor as GameObjectActor; }
        }
        private RoomActor roomAct = null;
        protected RoomActor GetRoom()
        {
            if (roomAct == null)
            {
                var act = this.actor as GameObjectActor;
                while (act != null && act.room == null)
                {
                    act = act.parent;
                }
                roomAct = act.room;
            }
            return roomAct;
        }
    }
    public class SpawnChest : GameObjectComponent
    {
        public int ChestId;
        public int SpawnId;

        public override void Init()
        {
            var rm = this.GetRoom();
            SpawnId = rm.maxSpawnId++;

            var go = this.actor as GameObjectActor;
            var p = go.pos;

            var entityInfo = EntityInfo.CreateBuilder();
            entityInfo.UnitId = ChestId;
            entityInfo.X = p.x;
            entityInfo.Y = p.y;
            entityInfo.Z = p.z;
            entityInfo.SpawnId = SpawnId;
            entityInfo.HP = Util.GetUnitData(false, ChestId, 0).HP;
            var info = entityInfo.Build();
            rm.AddEntityInfo(info);
        }
    }
}
