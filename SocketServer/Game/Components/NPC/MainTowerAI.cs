using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    public class MainTowerAI : AINPC
    {
        public override void Init()
        {
            base.Init();
            base.AfterSelectHeroInit();
            aiCharacter = gameObject.AddComponent<AICharacter>();
            aiCharacter.AddState(new TowerIdle());
            aiCharacter.AddState(new TowerAttack());
            aiCharacter.AddState(new MainTowerDead());
        }
    }
}
