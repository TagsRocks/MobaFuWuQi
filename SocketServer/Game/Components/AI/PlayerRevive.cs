using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    class PlayerRevive : ReviveState
    {
        public override void EnterState()
        {
            base.EnterState();
            var pmc = aiCharacter.aiNpc.mySelf.GetRoom().GetComponent<PlayerManagerCom>();
            var mySelf = aiCharacter.aiNpc.mySelf as PlayerInRoom;
            var unitData = aiCharacter.aiNpc.unitData;

            pmc.allPlayerStart.InitPlayerPos(mySelf);
            mySelf.DuckInfo.HP = unitData.HP;
            aiCharacter.aiNpc.attribute.DoRevive();

            aiCharacter.ChangeState(AIStateEnum.IDLE);


            var gc = GCPlayerCmd.CreateBuilder();
            gc.Result = "Revive";
            gc.ActorId = aiCharacter.aiNpc.mySelf.IDInRoom;
            var etyInfo = AvatarInfo.CreateBuilder();
            etyInfo.Id = aiCharacter.aiNpc.mySelf.IDInRoom;
            gc.AvatarInfo = etyInfo.Build();
            var myself = aiCharacter.aiNpc.mySelf;
            myself.GetRoom().AddKCPCmd(gc);
        }
    }
}
