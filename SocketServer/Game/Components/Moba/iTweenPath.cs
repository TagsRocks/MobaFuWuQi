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
        public int spawnId;

        public override void Init()
        {
            base.Init();
            spawnId = System.Convert.ToInt32(gameObject.name.Replace("Path", ""));
        }

        public void AddSoldier(int soldierId, int teamId, float offset=0)
        {
            var startPos = nodes[0];
            startPos.x += (int)(offset*100);

            //LogHelper.Log("iTweenPath", "AddSoldier:" + startPos);
            var entityInfo = EntityInfo.CreateBuilder();
            entityInfo.UnitId = soldierId;
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
            if (ety != null)
            {
                var creepAI = ety.AddComponent<CreepAI>();
                creepAI.path = this;
                creepAI.RunAI();
            }
        }
    }
}
