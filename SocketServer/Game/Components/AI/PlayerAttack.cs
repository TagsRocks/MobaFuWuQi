using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    class PlayerAttack : AttackState
    {
        private AINPC aiNpc;
        public override void Init()
        {
            base.Init();
            aiNpc = aiCharacter.gameObject.GetComponent<AINPC>();
        }
        public override void EnterState()
        {
            base.EnterState();
            var cmd = aiCharacter.blackboard[AIParams.Command].cmd;
            var gc = GCPlayerCmd.CreateBuilder();
            gc.SkillAction = cmd.SkillAction;
            gc.Result = cmd.Cmd;
            aiCharacter.gameObject.GetRoom().AddKCPCmd(gc);
        }

        public async override Task RunLogic()
        {
            var cmd = aiCharacter.blackboard[AIParams.Command].cmd;
            var skillAct = cmd.SkillAction;
            var sk = aiCharacter.gameObject.GetComponent<SkillComponent>();
            var stateMachine = sk.CreateSkillStateMachine(skillAct, aiNpc.npcConfig.normalAttack);
            await UpdateAction(stateMachine);
            aiCharacter.ChangeState(AIStateEnum.IDLE);
        }

        private async Task UpdateAction(SkillStateMachine stateMachine)
        {
            var actConfig = aiNpc.npcConfig.GetAction(ActionType.Attack);
            var tempRunNum = runNum;

            await Task.Delay(Util.TimeToMS(actConfig.hitTime));
            //防止状态重入 导致的错误触发问题 一般在等待一段时间后执行
            if (inState && tempRunNum == runNum)
            {
                stateMachine.OnEvent(SkillEvent.EventTrigger);
                await Task.Delay(Util.TimeToMS(actConfig.totalTime - actConfig.hitTime));
            }
        }

    }
}
