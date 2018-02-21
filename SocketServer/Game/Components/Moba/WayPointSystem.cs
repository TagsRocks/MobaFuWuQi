using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    public class WayPointSystem : GameObjectComponent
    {
        public float intervalTime = 10;
        public int soldierId = 59;
        public int teamId = 0;
        public int archerId = 64;
        public float offSetZ = 4;

        private GridManager gridManager;

        //TODO:Grid Physic 都需要放到其它中初始化
        public override void Init()
        {
            base.Init();
            gridManager = GetRoom().GetComponent<GridManager>();

            var rm = GetRoom();
            rm.RunTask(GenCreep);
        }
        //通知三条路径各自生成Creep
        private async Task GenCreep()
        {
            //等待开始刷新小兵
            while(GetRoom().GetState() != RoomActor.RoomState.InGame)
            {
                await Task.Delay(1000);
            }

            //5s 后开始刷新
            await Task.Delay(5000);

            var children = gameObject.GetChildren();
            //passTime = 0;
            while (true && GetRoom().GetState() == RoomActor.RoomState.InGame)
            {
                foreach (var c in children)
                {
                    var path = c.GetComponent<iTweenPath>();

                    //小兵的AI需要添加上组件
                    path.AddSoldier(soldierId, teamId);
                    path.AddSoldier(archerId, teamId, offSetZ);
                }
                //await new WaitForNextFrame(GetRoom());
                //passTime += Util.FrameSecTime;

                await Task.Delay(Util.TimeToMS(intervalTime));
            }
        }

        //private float passTime = 0;
    }

}
