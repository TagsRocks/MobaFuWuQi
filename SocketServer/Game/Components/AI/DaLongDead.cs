using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    class DaLongDead : DeadState
    {
        public override void EnterState()
        {
            base.EnterState();
            MobaUtil.SyncDead(aiCharacter);
        }

        public override async Task RunLogic()
        {
            await Task.Delay(3000);
            var dragon = aiCharacter.gameObject.GetRoom().GetComponent<DragonManager>();
            dragon.StartDragon(aiCharacter.gameObject.GetComponent<NpcAttribute>().killerTeamColor);
            aiCharacter.gameObject.GetComponent<NpcAttribute>().RemoveSelf();
        }
    }
}
