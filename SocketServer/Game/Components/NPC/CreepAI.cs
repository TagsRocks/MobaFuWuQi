using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace MyLib 
{
    
    /// <summary>
    /// 由两部分构成
    /// AI状态机 底层
    /// 上层AI决策层
    /// 
    /// 只会攻击5m范围内的敌人
    /// </summary>
    class CreepAI : AINPC 
    {
        public float eyeSightDistance = 10;
        public float attackRangeDist = 2;
        public int attackSkill = 19;

        public iTweenPath path;
        public override void Init()
        {
            base.Init();
            aiCharacter = gameObject.AddComponent<AICharacter>();
            aiCharacter.AddState(new CreepIdle());
            aiCharacter.AddState(new CreepMove());
            aiCharacter.AddState(new CreepAttack());
            aiCharacter.AddState(new CreepDead());

            //当前所在点 路径点
            aiCharacter.blackboard[AIParams.CurrentPoint] = new AIEvent{ intVal = 0 };
        }
        public void RunAI()
        {
            aiCharacter.ChangeState(AIStateEnum.IDLE);
            gameObject.RunTask(FindEnemy);
        }

        private async Task FindEnemy()
        {
            while (!gameObject.IsOver && !attribute.IsDead())
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
                            if (e.actor.entityInfo.TeamColor != mySelf.entityInfo.TeamColor && !e.actor.GetComponent<NpcAttribute>().IsDead())
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
                                //target = findProxy;
                                aiCharacter.blackboard[AIParams.Target] = new AIEvent()
                                {
                                    entityProxy = findProxy,
                                };
                                aiCharacter.ChangeState(AIStateEnum.ATTACK);
                            }
                        }
                    }
                }
                await Task.Delay(MainClass.syncTime);
            }
        }
     
        public float GetAttackRadiusSquare()
        {
            return attackRangeDist * attackRangeDist;
        }
       
    }
}
