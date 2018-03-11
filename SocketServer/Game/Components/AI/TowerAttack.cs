using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    class TowerAttack : AttackState
    {
        private AINPC towerAI;
        private EntityProxy target;
        public override void Init()
        {
            base.Init();
            towerAI = aiCharacter.gameObject.GetComponent<AINPC>();
        }

        public override void EnterState()
        {
            base.EnterState();
            target = aiCharacter.blackboard[AIParams.Target].entityProxy;
        }

        public override async Task RunLogic()
        {
            var otherAttr = target.actor.GetComponent<NpcAttribute>();
            var attackRange = towerAI.npcConfig.attackRangeDist;
            var tempNum = runNum;
            while (CheckInState(tempNum) && !otherAttr.IsDead())
            {
                var mePos = towerAI.mySelf.GetVec2Pos();
                var tarPos = target.actor.GetVec2Pos();
                var dist = (mePos - tarPos).LengthSquared();
                if (dist < attackRange * attackRange)
                {
                    await DoAttack();
                }else
                {
                    break;
                }
            }
            if(CheckInState(tempNum))
            {
                aiCharacter.ChangeState(AIStateEnum.IDLE);
            }
        }

        private async Task DoAttack()
        {
            var myself = towerAI.mySelf;
            var pos = towerAI.mySelf.GetIntPos();
            var skillAct = SkillAction.CreateBuilder();
            skillAct.Who = towerAI.mySelf.Id;
            skillAct.SkillId = towerAI.npcConfig.attackSkill;
            skillAct.SkillLevel = 0;
            skillAct.X = pos.x;
            skillAct.Y = pos.y;
            skillAct.Z = pos.z;

            var fp = target.actor.GetFloatPos();
            var myPos = towerAI.mySelf.GetFloatPos();
            var dir = fp - myPos;
            dir.Y = 0;
            //Unity 是顺时针为正向 左手坐标系
            myself.dir = (int)MathUtil.Math2UnityRot(MathUtil.RotY(dir));
            skillAct.Dir = myself.dir;
            skillAct.Target = target.actor.IDInRoom;

            var actConfig = towerAI.npcConfig.GetAction(ActionType.Attack);
            var tt = actConfig.totalTime;
            skillAct.RunFrame = Util.GameTimeToNet(tt);

            var gc = GCPlayerCmd.CreateBuilder();
            gc.Result = "Skill";
            gc.SkillAction = skillAct.Build();
            myself.GetRoom().AddKCPCmd(gc);

            var sk = aiCharacter.gameObject.GetComponent<SkillComponent>();
            var stateMachine = sk.CreateSkillStateMachine(skillAct.Build(), towerAI.npcConfig.normalAttack);
            await UpdateAction(stateMachine);

            //await Task.Delay(1);
        }

        private async Task UpdateAction(SkillStateMachine stateMachine)
        {
            var actConfig = towerAI.npcConfig.GetAction(ActionType.Attack);
            var tempRunNum = runNum;

            await Task.Delay(Util.TimeToMS(actConfig.hitTime));
            //防止状态重入 导致的错误触发问题 一般在等待一段时间后执行
            if (CheckInState(tempRunNum))
            {
                stateMachine.OnEvent(SkillEvent.EventTrigger);
                await Task.Delay(Util.TimeToMS(actConfig.totalTime - actConfig.hitTime));
            }
        }
    }
}
