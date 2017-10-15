using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    //伤害还是客户端控制的
    public class DamageZone : GameObjectComponent
    {
        public int SpawnId;
        //剩余多长时间伤害开始生效
        public float LeftTimeToSpawn = 10;
        public int MonsterID; //怪物ID

        private EntityActor entity;
        public override void Init()
        {
            base.Init();
            var rm = GetRoom();
            rm.RunTask(GenMonster);
        }

        private async Task GenMonster()
        {
            var rm = GetRoom();
            var p = this.gameObject.pos;

            var wt = (int)(LeftTimeToSpawn*1000);
            await Task.Delay(wt);
            var entityInfo = EntityInfo.CreateBuilder();
            entityInfo.UnitId = MonsterID;
            entityInfo.X = p.x;
            entityInfo.Y = p.y;
            entityInfo.Z = p.z;
            entityInfo.SpawnId = SpawnId;
            entityInfo.EType = EntityType.CHEST;
            var info = entityInfo.Build();
            var ety = GetRoom().AddEntityInfo(info);
            if (ety != null)
            {
                entity = ety;
            }
        }
    }
}
