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
        private AINPC creepAI;
        private MoveController moveController;
        private EntityProxy target;
        public override void EnterState()
        {
            base.EnterState();
            creepAI = aiCharacter.gameObject.GetComponent<AINPC>();
            moveController = aiCharacter.gameObject.GetComponent<MoveController>();
            target = aiCharacter.blackboard[AIParams.Target].entityProxy;
        }
        public override async Task RunLogic()
        {
            var otherAttr = target.actor.GetComponent<NpcAttribute>();
            while (inState && !otherAttr.IsDead())
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

            //敌人已经死亡
            if (inState)
            {
                aiCharacter.ChangeState(AIStateEnum.IDLE);
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
            skillAct.SkillId = creepAI.npcConfig.attackSkill;
            skillAct.SkillLevel = 0;
            skillAct.X = pos.x;
            skillAct.Y = pos.y;
            skillAct.Z = pos.z;

            var fp = target.actor.GetFloatPos();
            var myPos = creepAI.mySelf.GetFloatPos();
            var dir = fp - myPos;
            dir.Y = 0;
            //Unity 是顺时针为正向 左手坐标系
            myself.dir = ((int)MathUtil.Math2UnityRot(MathUtil.RotY(dir)));
            skillAct.Dir = myself.dir;

            skillAct.Target = target.actor.IDInRoom;

            var actConfig = creepAI.npcConfig.GetAction(ActionType.Attack);
            var tt = actConfig.totalTime;
            skillAct.RunFrame = Util.GameTimeToNet(tt);

            var gc = GCPlayerCmd.CreateBuilder();
            gc.Result = "Skill";
            gc.SkillAction = skillAct.Build();
            myself.GetRoom().AddNextFrameCmd(gc);

            var sk = aiCharacter.gameObject.GetComponent<SkillComponent>();
            var stateMachine = sk.CreateSkillStateMachine(skillAct.Build(), creepAI.npcConfig.normalAttack);

            //执行对应的动作Attack 
            //确定对应的事件
            //SyncTime 驱动的
            await UpdateAction(stateMachine);
            //await Task.Delay(1 * MainClass.syncTime);
        }

        private async Task UpdateAction(SkillStateMachine stateMachine)
        {
            var actConfig = creepAI.npcConfig.GetAction(ActionType.Attack);
            var tempRunNum = runNum;

            await Task.Delay(Util.TimeToMS(actConfig.hitTime));
            //防止状态重入 导致的错误触发问题 一般在等待一段时间后执行
            if (inState && tempRunNum == runNum)
            {
                stateMachine.OnEvent(SkillEvent.EventTrigger);
                await Task.Delay(Util.TimeToMS(actConfig.totalTime - actConfig.hitTime));
            }
        }


        /// <summary>
        /// 向目标靠近
        /// </summary>
        /// <returns></returns>
        private async Task DoMove()
        {
            var pos = target.actor.GetIntPos();
            //moveController.MoveTo(pos);
            var otherAttr = target.actor.GetComponent<NpcAttribute>();
            //检测和目标的距离
            while (inState && !otherAttr.IsDead())
            {
                var tarNewPos = target.actor.GetIntPos();
                //寻路加移动 或者直线移动？
                moveController.MoveTo(tarNewPos);

                var mePos = creepAI.mySelf.GetVec2Pos();
                var tarPos = target.actor.GetVec2Pos();
                var dist = (mePos - tarPos).LengthSquared();
                //寻路追踪目标 需要时刻调整路径
                if(dist < creepAI.GetAttackRadiusSquare() * 0.9f)
                {
                    moveController.StopMove();
                    break;
                }

                //var waitTime = Util.FrameMSTime;
                //await Task.Delay(waitTime);
                await new WaitForNextFrame(creepAI.mySelf.GetRoom());
            }
            if (inState)
            {
                moveController.StopMove();
            }
        }

        /// <summary>
        /// 清理内部状态
        /// </summary>
        public override void ExitState()
        {
            moveController.StopMove();
            base.ExitState();
        }
    }
}
