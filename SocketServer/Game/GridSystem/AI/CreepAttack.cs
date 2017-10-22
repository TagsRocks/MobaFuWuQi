using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace MyLib
{
    /// <summary>
    /// MoveAttackGoal
    /// 移动攻击
    /// </summary>
    public class CreepAttack : AttackState
    {
        private CreepAI creepAI;
        private MoveController moveController;
        public override void EnterState()
        {
            base.EnterState();
            creepAI = aiCharacter.gameObject.GetComponent<CreepAI>();
            moveController = aiCharacter.gameObject.GetComponent<MoveController>();
        }
        public override async Task RunLogic()
        {
            var target = creepAI.target;
            while (inState)
            {
                var mePos = creepAI.mySelf.GetVec2Pos();
                var tarPos = target.actor.GetVec2Pos();
                var dist = (mePos - tarPos).LengthSquared();
                if (dist < creepAI.GetAttackRadiusSquare())
                {
                    await DoAttack();
                }
                else
                {
                    await DoMove();
                }
            }
        }

        /// <summary>
        /// 使用普通攻击技能 攻击目标 朝向
        /// </summary>
        /// <returns></returns>
        private async Task DoAttack()
        {
            var myself = creepAI.mySelf;
            var pos = creepAI.mySelf.GetIntPos();
            var skillAct = SkillAction.CreateBuilder();
            skillAct.Who = creepAI.mySelf.Id;
            skillAct.SkillId = creepAI.attackSkill;
            skillAct.SkillLevel = 0;
            skillAct.X = pos.x;
            skillAct.Y = pos.y;
            skillAct.Z = pos.z;

            var fp = creepAI.target.actor.GetFloatPos();
            var myPos = creepAI.mySelf.GetFloatPos();
            var dir = fp - myPos;
            dir.Y = 0;
            //Unity 是顺时针为正向 左手坐标系
            myself.entityInfo.Dir = (int)MathUtil.Math2UnityRot(MathUtil.RotY(dir));

            skillAct.Dir = myself.entityInfo.Dir;

            skillAct.Target = creepAI.target.actor.entityInfo.Id;
            skillAct.RunFrame = Util.GameTimeToNet(1);

            var gc = GCPlayerCmd.CreateBuilder();
            gc.Result = "Skill";
            gc.SkillAction = skillAct.Build();
            myself.GetRoom().AddKCPCmd(gc);

            await Task.Delay(1 * MainClass.syncTime);
        }

        /// <summary>
        /// 向目标靠近
        /// </summary>
        /// <returns></returns>
        private async Task DoMove()
        {
            var pos = creepAI.target.actor.GetIntPos();
            moveController.MoveTo(pos);
            //检测和目标的距离
            while (inState)
            {
                var tarNewPos = creepAI.target.actor.GetIntPos();
                moveController.MoveTo(tarNewPos);

                var mePos = creepAI.mySelf.GetVec2Pos();
                var tarPos = creepAI.target.actor.GetVec2Pos();
                var dist = (mePos - tarPos).LengthSquared();
                //寻路追踪目标 需要时刻调整路径
                if(dist < creepAI.GetAttackRadiusSquare() * 0.9f)
                {
                    moveController.StopMove();
                    break;
                }
                var waitTime = 100;
                if(dist < 2)
                {
                    waitTime = 100;
                }
                await Task.Delay(waitTime);
            }
        }
        public override void ExitState()
        {
            moveController.StopMove();
            base.ExitState();
        }
    }
}
