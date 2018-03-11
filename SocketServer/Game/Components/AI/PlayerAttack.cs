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
        private RoomActor roomActor;
        private PhysicManager physics;
        private CGPlayerCmd cmd;
        private ActionConfig actConfig;
        private SkillComponent sk;
        public override void Init()
        {
            base.Init();
            aiNpc = aiCharacter.gameObject.GetComponent<AINPC>();
            roomActor = aiCharacter.gameObject.GetRoom();
            physics = roomActor.GetComponent<PhysicManager>();
            sk = aiCharacter.gameObject.GetComponent<SkillComponent>();
        }

        //攻击目标不确定
        //需要服务器寻找到正确的攻击目标
        public override void EnterState()
        {
            base.EnterState();
            cmd = aiCharacter.blackboard[AIParams.Command].cmd;

            var gc = GCPlayerCmd.CreateBuilder();
            gc.SkillAction = cmd.SkillAction;
            actConfig = aiNpc.npcConfig.GetActionBySkillId(cmd.SkillAction.SkillId);
            if(actConfig == null)
            {
                LogHelper.LogError("PlayerAttack", "actConfigIsNull:"+cmd.ToString()+":"+ MobaUtil.printer.PrintObject(aiNpc.npcConfig));
                return;
            }
            var tt = actConfig.totalTime;
            gc.SkillAction.RunFrame = Util.GameTimeToNet(tt);
            gc.SkillAction.Target = 0;

            MobaUtil.SetSkillActionPos(gc.SkillAction, aiNpc.mySelf);

            var enes = physics.GetNearyBy(aiNpc.proxy, sk.GetAttackTargetDist(gc.SkillAction.SkillId));
            var findNear = MobaUtil.FindNearestEne(enes, sk.GetAttackTargetDist(gc.SkillAction.SkillId), aiNpc.mySelf);
            //如果服务器没有设置朝向 则使用客户端玩家自己的朝向
            //攻击锁定最近的目标
            if(findNear.ProxyId != -1){
                var tarPos = findNear.actor.GetFloatPos();
                var mePos = aiNpc.mySelf.GetFloatPos();
                var deltaPos = tarPos - mePos;
                deltaPos.Y = 0;
                var dir = ((int)MathUtil.Math2UnityRot(MathUtil.RotY(deltaPos)));
                gc.SkillAction.Dir = dir;
                //cmd.SkillAction.Target = findNear.actor.IDInRoom;
                gc.SkillAction.Target = findNear.actor.IDInRoom; //基于位置的目标如何表示 创建一个虚拟物体作为目标
            }

            gc.Result = cmd.Cmd;
            gc.RunInFrame = (int)roomActor.GetFrameId();
            var tempNum = runNum;
            var canUse = sk.CheckCanUseSkill(gc.SkillAction, aiNpc.npcConfig);
            if(!canUse)
            {
                if (CheckInState(tempNum))
                {
                    aiCharacter.ChangeState(AIStateEnum.IDLE);
                }
            }
            else
            {
                aiCharacter.gameObject.GetRoom().AddKCPCmd(gc);
            }
        }

        public async override Task RunLogic()
        {
            var cmd = aiCharacter.blackboard[AIParams.Command].cmd;
            var skillAct = cmd.SkillAction;
            var skData = Util.GetSkillData(actConfig.skillId, 1);
            var tempNum = runNum;
             
            var stateMachine = sk.CreateSkillStateMachine(skillAct, skData.template);
            await UpdateAction(stateMachine);
            if (CheckInState(tempNum))
            {
                aiCharacter.ChangeState(AIStateEnum.IDLE);
            }
        }

        private async Task UpdateAction(SkillStateMachine stateMachine)
        {
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
