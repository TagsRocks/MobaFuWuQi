using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    /// <summary>
    /// 按照路点行走
    /// </summary>
    public class CreepMove : MoveState
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
            var cp = aiCharacter.blackboard[AIParams.CurrentPoint].intVal;
            var nextPoint = cp + 1;
            var path = creepAI.path;
            var tempNum = runNum;
            while(path.nodes.Count > nextPoint && CheckInState(tempNum))
            {
                var pos = path.nodes[nextPoint];
                //Log.AI("MoveToPos:"+pos+":point:"+nextPoint);
                await moveController.MoveTo(pos);
                var tarPos = pos.ToFloat();
                var nowPos = aiCharacter.aiNpc.mySelf.GetFloatPos();
                var dist = Util.XZDistSqrt(tarPos, nowPos);

                //需要检查Grid网格距离，而不是实际位置距离
                if (CheckInState(tempNum) && dist <= 4)
                {
                    aiCharacter.blackboard[AIParams.CurrentPoint].intVal = nextPoint;
                    nextPoint++;
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
