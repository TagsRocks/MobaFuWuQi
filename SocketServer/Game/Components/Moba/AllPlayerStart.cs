using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    class AllPlayerStart : GameObjectComponent
    {
        public override void Init()
        {
            base.Init();
            GetRoom().GetComponent<PlayerManagerCom>().SetAllPlayer(this);
        }

        /// <summary>
        /// 初始化玩家位置
        /// </summary>
        /// <param name="pIn"></param>
        public void InitPlayerPos(PlayerInRoom pIn)
        {
            var info = pIn.GetAvatarInfo();
            info.ResetPos = true;
            foreach(var c in gameObject.GetChildren())
            {
                var ps = c.GetComponent<PlayerStart>();
                if(ps.teamId == pIn.TeamColor)
                {
                    pIn.SetPos(ps.gameObject.pos);
                    pIn.DuckInfo.ResetPos = true; //通知客户端重置初始位置
                    break;
                }
            }
        }
    }
}
