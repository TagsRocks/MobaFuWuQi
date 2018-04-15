using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StatePrinting;
using System.Numerics;
using tainicom.Aether.Physics2D.Collision;

namespace MyLib {
    public static class MobaUtil
    {
        public static void SetSkillActionPos(SkillAction skAct, ActorInRoom myself)
        {
            var pos = myself.GetIntPos();
            skAct.X = pos.x;
            skAct.Y = pos.y;
            skAct.Z = pos.z;
        }

        public static EntityProxy FindNearestEne(List<EntityProxy> proxes, float dist, ActorInRoom mySelf)
        {
            var myPos = mySelf.GetVec2Pos();
            var minDist = dist * dist;
            var findProxy = new EntityProxy();
            findProxy.ProxyId = -1;
            foreach (var e in proxes)
            {
                if (e.actor.TeamColor != mySelf.TeamColor && !e.actor.GetComponent<NpcAttribute>().IsDead())
                {
                    var enePos = e.actor.GetVec2Pos();
                    var newDist = (myPos - enePos).LengthSquared();
                    if (newDist < minDist)
                    {
                        minDist = newDist;
                        findProxy = e;
                    }
                }
            }
            return findProxy;
        }

        public static void SyncDead(AICharacter aiCharacter)
        {
            if (aiCharacter.aiNpc.mySelf.IsPlayer)
            {
                SyncPlayerDead(aiCharacter);
            }
            else
            {
                SyncEtyDead(aiCharacter);
            }
        }

        private static void SyncEtyDead(AICharacter aiCharacter)
        {
            var gc = GCPlayerCmd.CreateBuilder();
            gc.Result = "DeadActor";
            gc.ActorId = aiCharacter.aiNpc.mySelf.IDInRoom;
            var etyInfo = EntityInfo.CreateBuilder();
            etyInfo.Id = aiCharacter.aiNpc.mySelf.IDInRoom;
            gc.EntityInfo = etyInfo.Build();
            var myself = aiCharacter.aiNpc.mySelf;
            myself.GetRoom().AddKCPCmd(gc);
        }

        private static void SyncPlayerDead(AICharacter aiCharacter)
        {
            var gc = GCPlayerCmd.CreateBuilder();
            gc.Result = "DeadActor";
            gc.ActorId = aiCharacter.aiNpc.mySelf.IDInRoom;
            var etyInfo = AvatarInfo.CreateBuilder();
            etyInfo.Id = aiCharacter.aiNpc.mySelf.IDInRoom;
            gc.AvatarInfo = etyInfo.Build();
            var myself = aiCharacter.aiNpc.mySelf;
            myself.GetRoom().AddKCPCmd(gc);
        }


        public static void SetPos(dynamic DuckInfo, dynamic DuckPos)
        {
            DuckInfo.X = DuckPos.X;
            DuckInfo.Y = DuckPos.Y;
            DuckInfo.Z = DuckPos.Z;
        }

        public static readonly Stateprinter printer = new Stateprinter();
        public static string Print(object obj)
        {
            return printer.PrintObject(obj);
        }


        public static List<EntityProxy> FindNearEnemy(RoomActor room, Vector3 pos, float radius, int TeamColor)
        {
            var physic = room.GetComponent<PhysicManager>();
            var vec2 = new Vector2(pos.X, pos.Z);
            var allEnemy =  physic.GetNearBy2(vec2, radius);
            var realEnemy = new List<EntityProxy>();
            foreach (var e in allEnemy)
            {
                if (e.actor.TeamColor != TeamColor && !e.actor.GetComponent<NpcAttribute>().IsDead())
                {
                    realEnemy.Add(e);
                }
            }
            return realEnemy;
        }

        public static EntityProxy FindEnemy(ActorInRoom npc)
        {
            var aiNpc = npc.GetComponent<AINPC>();
            var npcConfig = aiNpc.npcConfig;
            var eyeSightDistance = npcConfig.eyeSightDistance;
            var attribute = aiNpc.attribute;
            var physic = aiNpc.physic;
            var proxy = aiNpc.proxy;

            var enemy = physic.GetNearyBy(proxy, eyeSightDistance);
            var myPos = proxy.actor.GetVec2Pos();

            var minDist = 100 * 100.0f;
            EntityProxy findProxy = new EntityProxy();
            findProxy.ProxyId = -1;
            foreach (var e in enemy)
            {
                if (e.actor.TeamColor != npc.TeamColor && !e.actor.GetComponent<NpcAttribute>().IsDead())
                {
                    var enePos = e.actor.GetVec2Pos();
                    var newDist = (myPos - enePos).LengthSquared();
                    if (newDist < minDist)
                    {
                        minDist = newDist;
                        findProxy = e;
                    }
                }
            }

            if (findProxy.ProxyId != -1)
            {
                if (minDist < eyeSightDistance * eyeSightDistance)
                {
                    return findProxy;
                }
            }
            return null;
        }

    

        public static bool CheckFaraway(ActorInRoom me, Vector2 initCenter)
        {
            var mePos = me.GetVec2Pos();
            var aiNpc = me.GetComponent<AINPC>();

            var faraway = (mePos - initCenter).LengthSquared();
            var cfg = aiNpc.npcConfig;
            var backDist = Math.Max(cfg.eyeSightDistance + 0.2f, cfg.maxMoveRange2);
            if (faraway > backDist * backDist)
            {
                return true;
            }
            return false;

        }
        public static bool CheckInAttackRange(ActorInRoom me, ActorInRoom other)
        {
            var mePos = me.GetVec2Pos();
            var tarPos = other.GetVec2Pos();
            var dist = (mePos - tarPos).LengthSquared();
            var aiNpc = me.GetComponent<AINPC>();

            if (dist < aiNpc.GetAttackRadiusSquare())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 使用普通攻击技能 攻击目标 朝向
        /// </summary>
        /// <returns></returns>
        public static async Task DoAttack(ActorInRoom mySelf, AIState curState, ActorInRoom target)
        {
            var aiNpc = mySelf.GetComponent<AINPC>();

            var pos = mySelf.GetIntPos();
            var skillAct = SkillAction.CreateBuilder();
            skillAct.Who = mySelf.Id;
            skillAct.SkillId = mySelf.GetComponent<AINPC>().npcConfig.attackSkill;
            skillAct.SkillLevel = 0;
            skillAct.X = pos.x;
            skillAct.Y = pos.y;
            skillAct.Z = pos.z;

            var fp = target.GetFloatPos();
            var myPos = mySelf.GetFloatPos();
            var dir = fp - myPos;
            dir.Y = 0;
            //Unity 是顺时针为正向 左手坐标系
            mySelf.dir = ((int)MathUtil.Math2UnityRot(MathUtil.RotY(dir)));
            skillAct.Dir = mySelf.dir;

            skillAct.Target = target.IDInRoom;

            var actConfig = aiNpc.npcConfig.GetAction(ActionType.Attack);
            var tt = actConfig.totalTime;
            skillAct.RunFrame = Util.GameTimeToNet(tt);

            var gc = GCPlayerCmd.CreateBuilder();
            gc.Result = "Skill";
            gc.SkillAction = skillAct.Build();
            mySelf.GetRoom().AddNextFrameCmd(gc);

            var sk = mySelf.GetComponent<SkillComponent>();
            var stateMachine = sk.CreateSkillStateMachine(skillAct.Build(), aiNpc.npcConfig.normalAttack);
            await UpdateAction(mySelf, curState, stateMachine);
        }

        private static async Task UpdateAction(ActorInRoom mySelf, AIState curState, SkillStateMachine stateMachine)
        {
            var aiNpc = mySelf.GetComponent<AINPC>();
            var actConfig = aiNpc.npcConfig.GetAction(ActionType.Attack);
            var tempRunNum = curState.runNum;

            await Task.Delay(Util.TimeToMS(actConfig.hitTime));
            //防止状态重入 导致的错误触发问题 一般在等待一段时间后执行
            if (curState.CheckInState(tempRunNum))
            {
                stateMachine.OnEvent(SkillEvent.EventTrigger);
                await Task.Delay(Util.TimeToMS(actConfig.totalTime - actConfig.hitTime));
            }
        }


        /// <summary>
        /// 向目标靠近
        /// </summary>
        /// <returns></returns>
        public static async Task DoMoveWithCondition(ActorInRoom mySelf, AIState curState, ActorInRoom target, Func<bool> func)
        {
            var pos = target.GetIntPos();
            var otherAttr = target.GetComponent<NpcAttribute>();
            var tempNum = curState.runNum;
            var moveController = mySelf.GetComponent<MoveController>();
            var aiNpc = mySelf.GetComponent<AINPC>();

            //检测和目标的距离
            while (curState.CheckInState(tempNum) && !otherAttr.IsDead() && func())
            {
                var tarNewPos = target.GetIntPos();
                //寻路加移动 或者直线移动？
                moveController.MoveTo(tarNewPos);

                var mePos = mySelf.GetVec2Pos();
                var tarPos = target.GetVec2Pos();
                var dist = (mePos - tarPos).LengthSquared();
                //寻路追踪目标 需要时刻调整路径
                if (dist < aiNpc.GetAttackRadiusSquare() * 0.9f)
                {
                    moveController.StopMove();
                    break;
                }

                await new WaitForNextFrame(mySelf.GetRoom());
            }
            if (curState.CheckInState(tempNum))
            {
                moveController.StopMove();
            }
        }
        /// <summary>
        /// 向目标靠近
        /// </summary>
        /// <returns></returns>
        public static async Task DoMove(ActorInRoom mySelf, AIState curState, ActorInRoom target)
        {
            var pos = target.GetIntPos();
            var otherAttr = target.GetComponent<NpcAttribute>();
            var tempNum = curState.runNum;
            var moveController = mySelf.GetComponent<MoveController>();
            var aiNpc = mySelf.GetComponent<AINPC>();

            //检测和目标的距离
            while (curState.CheckInState(tempNum) && !otherAttr.IsDead())
            {
                var tarNewPos = target.GetIntPos();
                //寻路加移动 或者直线移动？
                moveController.MoveTo(tarNewPos);

                var mePos = mySelf.GetVec2Pos();
                var tarPos = target.GetVec2Pos();
                var dist = (mePos - tarPos).LengthSquared();
                //寻路追踪目标 需要时刻调整路径
                if (dist < aiNpc.GetAttackRadiusSquare() * 0.9f)
                {
                    moveController.StopMove();
                    break;
                }

                await new WaitForNextFrame(mySelf.GetRoom());
            }
            if (curState.CheckInState(tempNum))
            {
                moveController.StopMove();
            }
        }
    }
}
