using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    class TowerIdle : IdleState
    {
        private AINPC towerAI;
        public override void Init()
        {
            base.Init();
            towerAI = aiCharacter.gameObject.GetComponent<AINPC>();
        }

        public override async Task RunLogic()
        {
            while (inState)
            {
                var enemy = towerAI.physic.GetNearyBy(towerAI.proxy, towerAI.npcConfig.eyeSightDistance);
                var myPos = towerAI.proxy.actor.GetVec2Pos();

                var minDist = 100 * 100.0f;
                EntityProxy findProxy = new EntityProxy();
                findProxy.ProxyId = -1;

                foreach (var e in enemy)
                {
                    if (e.actor.TeamColor != towerAI.mySelf.TeamColor)
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
                    if (minDist < towerAI.npcConfig.eyeSightDistance * towerAI.npcConfig.eyeSightDistance)
                    {
                        towerAI.aiCharacter.blackboard[AIParams.Target] = new AIEvent()
                        {
                            entityProxy = findProxy,
                        };
                        aiCharacter.ChangeState(AIStateEnum.ATTACK);
                    }
                }
                await Task.Delay(Util.TimeToMS(1));
            }
        }
    }
}
