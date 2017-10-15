using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    public class CreepMove : MoveState
    {
        public override async Task RunLogic()
        {
            var creepAI = aiCharacter.gameObject.GetComponent<CreepAI>();
            var moveController = aiCharacter.gameObject.GetComponent<MoveController>();
            var cp = aiCharacter.blackboard[AIParams.CurrentPoint].intVal;
            var nextPoint = cp + 1;
            var path = creepAI.path;
            if (path.nodes.Count > nextPoint)
            {
                var pos = path.nodes[nextPoint];
                await moveController.MoveTo(pos);
            }
        }


    }
}
