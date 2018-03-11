using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    class PlayerIdle : IdleState
    {
        private PlayerInRoom me;
        public override void EnterState()
        {
            base.EnterState();
            me = aiCharacter.gameObject as PlayerInRoom;
        }

        public override async Task RunLogic()
        {
            var tempNum = runNum;
            while (CheckInState(tempNum))
            {
                var clientPos = me.GetClientVelocity();
                if(Util.IsClientMove(clientPos))
                {
                    aiCharacter.ChangeState(AIStateEnum.MOVE);
                    break;
                }
                await Task.Delay(MainClass.syncTime);
            }
        }
    }
}
