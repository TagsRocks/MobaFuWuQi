using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    public class SpringAI : AINPC
    {
        private float healRange = 5;
        public override void Init()
        {
            base.Init();
            base.AfterSelectHeroInit();
            aiCharacter = gameObject.AddComponent<AICharacter>();
            aiCharacter.AddState(new TowerIdle());
            aiCharacter.AddState(new TowerAttack());
            aiCharacter.AddState(new TowerDead());
        }
        public override void RunAI()
        {
            base.RunAI();
            gameObject.RunTask(HealBuff);
        }
        private async Task HealBuff()
        {
            while(!gameObject.IsOver && !attribute.IsDead())
            {
                var nearbys = physic.GetNearyBy(proxy, healRange);
                var myPos = proxy.actor.GetVec2Pos();
                foreach (var e in nearbys)
                {
                    if (e.actor.TeamColor == mySelf.TeamColor && !e.actor.GetComponent<NpcAttribute>().IsDead())
                    {
                        var enePos = e.actor.GetVec2Pos();
                        var newDist = (myPos - enePos).LengthSquared();
                        if (newDist < healRange*healRange)
                        {
                            e.actor.GetComponent<NpcAttribute>().DoHeal(20);
                        }
                    }
                }
                await Task.Delay(1000);
            }
        }
    }
}
