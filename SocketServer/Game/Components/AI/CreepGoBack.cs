using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib 
{
    /// <summary>
    /// 禁止掉小兵 自动寻找敌人的功能
    /// </summary>
    public class CreepGoBack : GoBackState
    {
        private MoveController moveController;
        private CreepAI creepAI;
        public override void EnterState()
        {
            base.EnterState();
            creepAI = aiCharacter.gameObject.GetComponent<CreepAI>();
            moveController = aiCharacter.gameObject.GetComponent<MoveController>();
        }

        public override async Task RunLogic()
        {
            //Log.AI("MoveRunTask:");
            //var cp = aiCharacter.blackboard[AIParams.CurrentPoint].intVal;
            var tarPos1 = aiCharacter.blackboard[AIParams.CenterPoint].vec2;
            var vec3Pos = MyVec3.FromFloat(tarPos1.X, 0, tarPos1.Y);
            var tempNum = runNum;
            while (CheckInState(tempNum))
            {
                //Log.AI("MoveToPos:"+pos+":point:"+nextPoint);
                await moveController.MoveTo(vec3Pos);
                var tarPos = vec3Pos.ToFloat();
                var nowPos = aiCharacter.aiNpc.mySelf.GetFloatPos();
                //需要检查Grid网格距离，而不是实际位置距离
                if (CheckInState(tempNum) && Util.CloseToTargetPos(nowPos, tarPos))
                {
                    break;
                }
                await new WaitForNextFrame(aiCharacter.aiNpc.mySelf.GetRoom());
            }
            if (CheckInState(tempNum))
            {
                aiCharacter.ChangeState(AIStateEnum.IDLE);
            }
        }
        public override void ExitState()
        {
            moveController.StopMove();
            base.ExitState();
        }
    }
}
