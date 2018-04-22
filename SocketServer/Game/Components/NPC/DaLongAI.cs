using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    class DaLongAI : AINPC
    {
        public override void Init()
        {
            base.Init();
            base.AfterSelectHeroInit();
            aiCharacter = gameObject.AddComponent<AICharacter>();
            aiCharacter.AddState(new XiaoGuaiIdle());
            aiCharacter.AddState(new XiaoGuaiAttack());
            aiCharacter.AddState(new DaLongDead());
            aiCharacter.AddState(new CreepGoBack());

            aiCharacter.blackboard[AIParams.InitPoint] = new AIEvent()
            {
                vec2 = this.mySelf.GetVec2Pos(),
            };
            aiCharacter.blackboard[AIParams.CenterPoint] = aiCharacter.blackboard[AIParams.InitPoint];

        }
        public override void RunAI()
        {
            base.RunAI();
        }
    }
}
