using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    /// <summary>
    /// 移动攻击逻辑
    /// 靠近敌人 技能攻击范围内
    /// 对敌人发起攻击
    /// </summary>
    class XiaoGuaiAttack : AttackState
    {

        public override void EnterState()
        {
            base.EnterState();
        }

        public override async Task RunLogic()
        {
            var tempNum = runNum;
            var enemy = MobaUtil.FindEnemy(aiCharacter.aiNpc.mySelf);
            var myself = aiCharacter.aiNpc.mySelf;
            myself.GetRoom().RunTask(CheckFaraway);
            while (enemy != null && CheckInState(tempNum))
            {
                var otherAttr = enemy.actor.GetComponent<NpcAttribute>();
                if (!otherAttr.IsDead())
                {
                    if (MobaUtil.CheckFaraway(myself, aiCharacter.blackboard[AIParams.InitPoint].vec2))
                    {
                        //enemy = MobaUtil.FindEnemy(myself);
                        break;
                    }else
                    {
                        var inAtt = MobaUtil.CheckInAttackRange(myself, enemy.actor);
                        if(inAtt)
                        {
                            await MobaUtil.DoAttack(myself, this, enemy.actor);
                        }else
                        {
                            await MobaUtil.DoMove(myself, this, enemy.actor);
                        }
                    }
                }else
                {
                    break;
                }
                await new WaitForNextFrame(myself.GetRoom());
            }

            if (CheckInState(tempNum))
            {
                aiCharacter.ChangeState(AIStateEnum.GO_BACK);
            }
        }
        private async Task CheckFaraway()
        {
            var tempNum = runNum;
            var myself = aiCharacter.aiNpc.mySelf;
            while (CheckInState(tempNum))
            {
                if (MobaUtil.CheckFaraway(myself, aiCharacter.blackboard[AIParams.InitPoint].vec2))
                {
                    break;
                }
                await new WaitForNextFrame(myself.GetRoom());
            }
            if (CheckInState(tempNum))
            {
                aiCharacter.ChangeState(AIStateEnum.GO_BACK);
            }
        }

        /// <summary>
        /// 清理内部状态
        /// </summary>
        public override void ExitState()
        {
            var moveController = aiCharacter.gameObject.GetComponent<MoveController>();
            moveController.StopMove();
            base.ExitState();
        }
    }
}
