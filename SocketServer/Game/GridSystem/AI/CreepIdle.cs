using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib 
{
    /// <summary>
    /// 
    /// </summary>
    public class CreepIdle : IdleState
    {
        public override void EnterState()
        {
            base.EnterState();
        }
        public override async Task RunLogic()
        {
            var creepAI = aiCharacter.gameObject.GetComponent<CreepAI>();
            var cp = aiCharacter.blackboard[AIParams.CurrentPoint].intVal;
            var nextPoint = cp + 1;
            var path = creepAI.path;
            if(path.nodes.Count > nextPoint)
            {
                aiCharacter.ChangeState(AIStateEnum.MOVE);
            }
        }
    }
}
