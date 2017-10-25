using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    class CreepDead : DeadState
    {
        public override async Task RunLogic()
        {
            await Task.Delay(3000);
            aiCharacter.gameObject.GetComponent<NpcAttribute>().RemoveSelf();
        }
    }
}
