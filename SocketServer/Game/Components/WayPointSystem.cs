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

        public override void Init()
        {
            base.Init();
            GetRoom().AddComponent<PhysicManager>();
            gridManager = GetRoom().AddComponent<GridManager>();
            gridManager.InitMap();

            var rm = GetRoom();
            rm.RunTask(GenCreep);
        }
        //通知三条路径各自生成Creep
        private async Task GenCreep()
        {
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
