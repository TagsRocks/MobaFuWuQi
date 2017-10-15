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
        private GridManager gridManager;

        public override void Init()
        {
            base.Init();
            gridManager = GetRoom().AddComponent<GridManager>();
            using (var f = new StreamReader("ConfigData/MapSourceConfig.json"))
            {
                var con = f.ReadToEnd();
                gridManager.LoadMap(con);
            }

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
                path.AddSoldier(soldierId);
                break;
            }

            /*
            while (!this.GetRoom().IsStop())
            {
                await Task.Delay((int)(intervalTime * 1000));
            }
            */
        }
        private float passTime = 0;
    }

}
