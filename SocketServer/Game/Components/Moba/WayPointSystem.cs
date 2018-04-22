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
        public int paoId = 66;
        public float offSetZ2 = -4;


        private GridManager gridManager;
        private enum State
        {
            Normal,
            Dragon,
        }
        private State state = State.Normal;
        private int dragonCount = 0;

        //TODO:Grid Physic 都需要放到其它中初始化
        public override void Init()
        {
            base.Init();
            gridManager = GetRoom().GetComponent<GridManager>();

            var rm = GetRoom();
            rm.RunTask(GenCreep);
            rm.AddWayPointSystem(this);
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
                    if (path.gameObject.name == "Path1"
                        || path.gameObject.name == "Path7")
                    {
                        if (state == State.Normal)
                        {
                            //小兵的AI需要添加上组件
                            path.AddSoldier(soldierId, teamId);
                            path.AddSoldier(archerId, teamId, offSetZ);
                            path.AddSoldier(paoId, teamId, offSetZ2);
                        }else if(state == State.Dragon)
                        {
                            path.AddSoldier(70, teamId);
                        }
                    }
                }
                if(state == State.Dragon)
                {
                    dragonCount++;
                    if(dragonCount >= 3)
                    {
                        state = State.Normal;
                    }
                }
                await Task.Delay(Util.TimeToMS(intervalTime));
            }
        }

        public void StartDragon(int teamColor)
        {
            if(teamColor == teamId)
            {
                state = State.Dragon;
                dragonCount = 0;
            }
        }
    }

}
