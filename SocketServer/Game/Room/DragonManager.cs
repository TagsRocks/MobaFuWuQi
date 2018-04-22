using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    public class DragonManager : Component
    {
        public override void AfterAdd()
        {

        }

        /// <summary>
        /// 小飞龙 沿着小兵路线来移动
        /// 需要限制小兵产生
        /// </summary>
        /// <param name="teamColor"></param>
        public void StartDragon(int teamColor)
        {
            var room = actor as RoomActor;
            foreach(var w  in room.ways)
            {
                w.StartDragon(teamColor);
            }

        }
    }
}
