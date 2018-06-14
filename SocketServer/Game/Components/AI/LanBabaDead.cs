using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    class LanBabaDead : DeadState
    {
        private int deadSkillId = 195;

        public override void EnterState()
        {
            base.EnterState();
            var killer = aiCharacter.gameObject.GetComponent<NpcAttribute>().killerId;
            var who = aiCharacter.gameObject.GetRoom().GetActorInRoom(killer);
            var mySelf = aiCharacter.gameObject as ActorInRoom;
            var aiNpc = aiCharacter.aiNpc;
            var intPos = mySelf.GetIntPos();

            var skillAct = SkillAction.CreateBuilder();
            skillAct.Who = mySelf.IDInRoom;
            skillAct.SkillId = deadSkillId;
            skillAct.SkillLevel = 0;
            skillAct.X = intPos.x;
            skillAct.Y = intPos.y;
            skillAct.Z = intPos.z;
            skillAct.Target = killer;
            skillAct.Dir = mySelf.dir;
            skillAct.RunFrame = 1;

            var skData = Util.GetSkillData(deadSkillId, 1);
            var sk = aiCharacter.gameObject.GetComponent<SkillComponent>();
            var stateMachine = sk.CreateSkillStateMachine(skillAct.Build(), skData.template);

            var gc = GCPlayerCmd.CreateBuilder();
            gc.Result = "Skill";
            gc.SkillAction = skillAct.Build();
            mySelf.GetRoom().AddKCPCmd(gc);

            MobaUtil.SyncDead(aiCharacter);
        }

        public override async Task RunLogic()
        {
            await Task.Delay(3000);
            aiCharacter.gameObject.GetComponent<NpcAttribute>().RemoveSelf();
        }
    }
}
