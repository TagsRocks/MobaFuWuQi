using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    class PlayerAI : AINPC
    {
        public override void Init()
        {
            base.Init();
            aiCharacter = gameObject.AddComponent<AICharacter>();
            aiCharacter.AddState(new PlayerIdle());
            aiCharacter.AddState(new PlayerMove());
            aiCharacter.AddState(new PlayerAttack());
            aiCharacter.AddState(new PlayerDead());
        }
    }
}
