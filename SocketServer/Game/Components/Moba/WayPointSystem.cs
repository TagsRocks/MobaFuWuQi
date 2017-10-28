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
            if(teamId == 0)
            {
                return;
            }
            var children = gameObject.GetChildren();

            foreach(var c in children)
            {
                var path = c.GetComponent<iTweenPath>();

                //小兵的AI需要添加上组件
                path.AddSoldier(soldierId, teamId);
                break;
            }
          
        }
        private float passTime = 0;
    }

}
