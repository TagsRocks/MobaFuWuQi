using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
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
            Log.AI("MoveRunTask:");
            var cp = aiCharacter.blackboard[AIParams.CurrentPoint].intVal;
            var nextPoint = cp + 1;
            var path = creepAI.path;
            while(path.nodes.Count > nextPoint)
            {
                var pos = path.nodes[nextPoint];
                Log.AI("MoveToPos:"+pos+":point:"+nextPoint);
                await moveController.MoveTo(pos);
                aiCharacter.blackboard[AIParams.CurrentPoint].intVal = nextPoint;
                nextPoint++;
            }
            aiCharacter.ChangeState(AIStateEnum.IDLE);
        }
        public override void ExitState()
        {
            moveController.StopMove();
            base.ExitState();
        }
    }
}
