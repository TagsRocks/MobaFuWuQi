using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MyLib
{
    class TowerPoint : GameObjectComponent
    {
        public int spawnId = 0;
        public int towerId = 61;
        public int teamId = 0;

        public override void Init()
        {
            base.Init();
            gameObject.RunTask(GenTower);
        }
        private async Task GenTower()
        {
            return;
            while (GetRoom().GetState() != RoomActor.RoomState.InGame)
            {
                await Task.Delay(1000);
            }

            //return;
            var startPos = gameObject.pos;
            var entityInfo = EntityInfo.CreateBuilder();
            entityInfo.UnitId = towerId;
            entityInfo.ItemNum = 1;
            entityInfo.X = startPos.x;
            //高度没有意义应该使用实际GridManager中的地面高度
            entityInfo.Y = startPos.y;
            entityInfo.Z = startPos.z;
            entityInfo.TeamColor = teamId;
            entityInfo.SpawnId = spawnId;
            entityInfo.EType = EntityType.CHEST;
            var info = entityInfo.Build();
            var ety = GetRoom().AddEntityInfo(info);
            var unitData = ety.GetUnitData();

            var type = Type.GetType("MyLib." + unitData.AITemplate);
            var m = ety.GetType().GetMethod("AddComponent");
            var geMethod = m.MakeGenericMethod(type);
            var tai = geMethod.Invoke(ety, new object[] { }) as AINPC;// as AIBase;
            tai.RunAI();
        }
    }
}
