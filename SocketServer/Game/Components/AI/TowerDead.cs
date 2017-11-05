using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    class TowerDead : DeadState
    {
        public override async Task RunLogic()
        {
            await Task.Delay(3000);
            aiCharacter.gameObject.GetComponent<NpcAttribute>().RemoveSelf();
        }
    }
}
