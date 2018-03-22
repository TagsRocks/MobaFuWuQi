using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace MyLib
{
    public class XiaoGuaiMgr : GameObjectComponent
    {
        public float intervalTime = 30;
        public int npcId = 68;

        private GridManager gridManager;
        public override void Init()
        {
            base.Init();
            gridManager = GetRoom().GetComponent<GridManager>();

            var rm = GetRoom();
            rm.RunTask(GenMonster);
        }
        private XiaoGuaiAI creepAI = null;

        private async Task GenMonster()
        {
            //等待开始刷新小兵
            while (GetRoom().GetState() != RoomActor.RoomState.InGame)
            {
                await Task.Delay(1000);
            }
            await Task.Delay(5000);
            while (true && GetRoom().GetState() == RoomActor.RoomState.InGame)
            {
                var startPos = gameObject.pos;

                LogHelper.Log("iTweenPath", "AddSoldier:" + startPos);
                if (creepAI == null || creepAI.gameObject.IsDestroy)
                {
                    var entityInfo = EntityInfo.CreateBuilder();
                    entityInfo.UnitId = npcId;
                    entityInfo.ItemNum = 1;
                    entityInfo.X = startPos.x;
                    //高度没有意义应该使用实际GridManager中的地面高度
                    entityInfo.Y = startPos.y;
                    entityInfo.Z = startPos.z;
                    entityInfo.TeamColor = 2;
                    entityInfo.SpawnId = 0;
                    entityInfo.EType = EntityType.CHEST;
                    var info = entityInfo.Build();
                    var ety = GetRoom().AddEntityInfo(info);
                    if (ety != null)
                    {
                        creepAI = ety.AddComponent<XiaoGuaiAI>();
                        creepAI.RunAI();
                    }
                }
                else
                {
                    if (creepAI.gameObject.IsDestroy)
                    {
                        creepAI = null;
                    }
                }
                await Task.Delay(Util.TimeToMS(intervalTime));
            }
        }
    }
}
