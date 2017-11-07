using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    class TowerAI : AINPC 
    {
        public float thinkTime = 1;
        public override void Init()
        {
            base.Init();
            aiCharacter = gameObject.AddComponent<AICharacter>();
            aiCharacter.AddState(new TowerIdle());
            aiCharacter.AddState(new TowerAttack());
            aiCharacter.AddState(new TowerDead());
        }
        public override void RunAI()
        {
            base.RunAI();
        }

    }
}
