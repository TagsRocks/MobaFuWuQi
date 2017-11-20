using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    class MainTowerDead : DeadState
    {
        public override void EnterState()
        {
            base.EnterState();
            MobaUtil.SyncDead(aiCharacter);
            aiCharacter.gameObject.GetRoom().MainTowerBroken(aiCharacter.aiNpc.mySelf);
        }

        public override async Task RunLogic()
        {
            await Task.Delay(3000);
            aiCharacter.gameObject.GetComponent<NpcAttribute>().RemoveSelf();
        }
    }
}
