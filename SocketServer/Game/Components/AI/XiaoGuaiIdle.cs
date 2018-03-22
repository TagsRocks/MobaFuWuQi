using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    class XiaoGuaiIdle : IdleState
    {
        public override void EnterState()
        {
            base.EnterState();
        }

        public override async Task RunLogic()
        {
            var tempNum = runNum;
            while (CheckInState(tempNum))
            {
                if (aiCharacter.blackboard.ContainsKey(AIParams.Attacker))
                {
                    aiCharacter.blackboard.Remove(AIParams.Attacker);
                    var ene = MobaUtil.FindEnemy(aiCharacter.aiNpc.mySelf);
                    if(ene != null)
                    {
                        aiCharacter.ChangeState(AIStateEnum.ATTACK);
                    }
                }
                await new WaitForNextFrame(aiCharacter.aiNpc.mySelf.GetRoom());
            }
        }
    }
}
