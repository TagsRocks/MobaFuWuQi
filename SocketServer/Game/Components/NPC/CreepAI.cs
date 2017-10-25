using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace MyLib 
{
    /// <summary>
    /// 执行移动逻辑
    /// A 移动到 B
    /// 移动结束通知上层
    /// 可以被打断
    /// </summary>
    public class MoveController : GameObjectComponent
    {
        private enum State
        {
            Idle,
            Move,
            TryToStop,
        }
        private State state = State.Idle;

        //NPC移动速度
        public float speed = 10;
        private bool stopMove = false;
        private bool inMove = false;
        private int curMoveId = 0;
        public void StopMove()
        {
            if (inMove)
            {
                stopMove = true;
            }
        }
        private CreepAI creepAi;
        private PhysicManager physicManager;
        public override void Init()
        {
            base.Init();
            creepAi = gameObject.GetComponent<CreepAI>();
            physicManager = GetRoom().GetComponent<PhysicManager>();
        }

        /// <summary>
        /// 移动代码中间不能打断
        /// 防止代码重入
        /// 单线程 多协程的时候
        /// </summary>
        /// <param name="v3Pos"></param>
        /// <returns></returns>
        public async Task MoveTo(MyVec3 v3Pos)
        {
            if(inMove)
            {
                curMoveId++;
            }

            var runMoveId = ++curMoveId;
            inMove = true;
            var tarPos = v3Pos.ToFloat();
            var curObj = gameObject as EntityActor;
            var x = curObj.entityInfo.X;
            var y = curObj.entityInfo.Y;
            var z = curObj.entityInfo.Z;
            curObj.entityInfo.Speed = Util.GameVecToNet(speed);

            var curPos = new MyVec3(x, y, z).ToFloat();
            var nowPos = curPos;

            var grid = GetRoom().GetComponent<GridManager>();
            var g1 = grid.mapPosToGrid(curPos);
            var g2 = grid.mapPosToGrid(tarPos);
            var path = grid.FindPath(g1, g2);
            var nodes = new Vector3[path.Count];
            var i = 0;
            foreach(var p in path)
            {
                var wp = grid.gridToMapPos(new Vector2(p.x, p.y));
                nodes[i++] = wp;
            }

            var room = GetRoom();
            var entityInfo = curObj.entityInfo;

            var curPoint = 1;
            while(curPoint < nodes.Length && !stopMove && runMoveId == curMoveId)
            {
                var nextPos = nodes[curPoint];
                var wp = nextPos;

                var dist = (wp - curPos).Length();
                var totalTime = dist / speed;
                var passTime = 0.0f;
                while(passTime < totalTime && !stopMove && runMoveId == curMoveId)
                {
                    passTime += MainClass.syncFreq;
                    var newPos = Vector3.Lerp( curPos, wp, MathUtil.Clamp(passTime/totalTime, 0, 1));
                    var myPos = MyVec3.FromFloat(newPos.X, newPos.Y, newPos.Z);
                    entityInfo.X = myPos.x;
                    entityInfo.Y = myPos.y;
                    entityInfo.Z = myPos.z;
                    physicManager.MoveEntity(ref creepAi.proxy, newPos-nowPos);

                    nowPos = newPos;
                    await Task.Delay(MainClass.syncTime);
                }
                curPos = wp;
                curPoint++;
            }

            if (runMoveId == curMoveId)
            {
                stopMove = false;
                inMove = false;
            }
        }
    }
    /// <summary>
    /// 由两部分构成
    /// AI状态机 底层
    /// 上层AI决策层
    /// 
    /// 只会攻击5m范围内的敌人
    /// </summary>
    public class CreepAI : GameObjectComponent
    {
        public float eyeSightDistance = 10;
        public float attackRangeDist = 2;
        public int attackSkill = 19;

        private AICharacter aiCharacter;
        public iTweenPath path;
        public EntityProxy proxy;
        private PhysicManager physic;
        public EntityProxy target;
        public EntityActor mySelf;
        public NpcConfig npcConfig;
        public UnitData unitData;
        public override void Init()
        {
            base.Init();
            mySelf = gameObject as EntityActor;
            physic = GetRoom().GetComponent<PhysicManager>();
            proxy = physic.AddEntity(mySelf);

            gameObject.AddComponent<MoveController>();
            gameObject.AddComponent<SkillComponent>();
            gameObject.AddComponent<NpcAttribute>();

            npcConfig = NpcDataManager.Instance.GetConfig(mySelf.entityInfo.UnitId);
            unitData = Util.GetUnitData(false, mySelf.entityInfo.UnitId, 0);


            aiCharacter = gameObject.AddComponent<AICharacter>();
            aiCharacter.AddState(new CreepIdle());
            aiCharacter.AddState(new CreepMove());
            aiCharacter.AddState(new CreepAttack());
            aiCharacter.AddState(new CreepDead());

            //当前所在点 路径点
            aiCharacter.blackboard[AIParams.CurrentPoint] = new AIEvent{ intVal = 0 };
            gameObject.RunTask(FindEnemy);
        }
        public void RunAI()
        {
            aiCharacter.ChangeState(AIStateEnum.IDLE);
        }

        private async Task FindEnemy()
        {
            while (!gameObject.IsOver)
            {
                if(aiCharacter.current != null)
                {
                    var state = aiCharacter.current.type;
                    if(state == AIStateEnum.IDLE || state == AIStateEnum.MOVE)
                    {
                        var enemy = physic.GetNearyBy(proxy, eyeSightDistance);
                        //var myPos = proxy.lastPos;
                        var myPos = proxy.actor.GetVec2Pos();

                        var minDist = 100 * 100.0f;
                        EntityProxy findProxy = new EntityProxy();
                        findProxy.ProxyId = -1;

                        foreach (var e in enemy)
                        {
                            if (e.actor.entityInfo.TeamColor != mySelf.entityInfo.TeamColor)
                            {
                                //var p = e.lastPos;
                                var enePos = e.actor.GetVec2Pos();
                                var newDist = (myPos - enePos).LengthSquared();
                                if (newDist < minDist)
                                {
                                    minDist = newDist;
                                    findProxy = e;
                                }
                            }
                        } 
                        if(findProxy.ProxyId != -1)
                        {
                            if (minDist < eyeSightDistance * eyeSightDistance)
                            {
                                target = findProxy;
                                aiCharacter.ChangeState(AIStateEnum.ATTACK);
                            }
                        }
                    }
                }
                await Task.Delay(MainClass.syncTime);
            }
        }
        public override void Destroy()
        {
            base.Destroy();
            physic.RemoveEntity(proxy);
        }
        public float GetAttackRadiusSquare()
        {
            return attackRangeDist * attackRangeDist;
        }

       
    }
}
