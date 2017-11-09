using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            GenTower();
        }
        private void GenTower()
        {
            return;
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
            var tai = ety.AddComponent<TowerAI>();
            tai.RunAI();
        }
    }
}
