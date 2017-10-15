using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    public class iTweenPath : GameObjectComponent
    {
        public List<MyVec3> nodes;
        private int spawnId;
        public override void Init()
        {
            base.Init();
            spawnId = System.Convert.ToInt32(gameObject.name.Replace("Path", ""));
        }

        public async Task AddSoldier(int soldierId)
        {
            var startPos = nodes[0];

            LogHelper.Log("iTweenPath", "AddSoldier:"+startPos);
            var entityInfo = EntityInfo.CreateBuilder();
            entityInfo.UnitId = soldierId;
            entityInfo.ItemNum = 1;
            entityInfo.X = startPos.x;
            //高度没有意义应该使用实际GridManager中的地面高度
            entityInfo.Y = startPos.y;
            entityInfo.Z = startPos.z;
            entityInfo.SpawnId = spawnId;
            entityInfo.EType = EntityType.CHEST;
            var info = entityInfo.Build();
            var ety = GetRoom().AddEntityInfo(info);
            if (ety != null)
            {
                var creepAI = ety.AddComponent<CreepAI>();
                creepAI.path = this;
                creepAI.RunAI();
            }
        }
    }
}
