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
        private AINPC creepAI = null;

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
                        var unitData = Util.GetUnitData(false, npcId, 0);
                        var ai = unitData.AITemplate;
                        var type = Type.GetType("MyLib." + unitData.AITemplate);
                        var m = ety.GetType().GetMethod("AddComponent");
                        var geMethod = m.MakeGenericMethod(type);
                        var ret = geMethod.Invoke(ety, null) as AINPC;
                        creepAI = ret;
                        //creepAI = ety.AddComponent<XiaoGuaiAI>();
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
