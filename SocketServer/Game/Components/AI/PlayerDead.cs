using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    class PlayerDead : DeadState
    {
        public override void EnterState()
        {
            base.EnterState();
            MobaUtil.SyncDead(aiCharacter);
        }
        public override bool CheckNextState(AIStateEnum next)
        {
            if(next == AIStateEnum.REVIVE)
            {
                return true;
            }
            return false;
        }

        //死亡进入等待复活状态
        //复活后生命值回复
        //玩家位置设置到固定位置
        //通知客户端死亡
        public override async Task RunLogic()
        {
            await Task.Delay(3000);
            aiCharacter.ChangeState(AIStateEnum.REVIVE);

        }
    }
}
