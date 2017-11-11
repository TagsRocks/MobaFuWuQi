using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    //AOE Damage
    class HurtNearby : SkillLogicComponent
    {
        public override void Run()
        {
            var atk = runner.stateMachine.attacker as ActorInRoom;
            var phy = GetRoom().GetComponent<PhysicManager>();
            var ai = atk.GetComponent<AINPC>();
            var ene = phy.GetNearyBy(ai.proxy, ai.npcConfig.attackRangeDist);
            var myPos = atk.GetVec2Pos();
            var canAttackEne = new List<EntityProxy>();
            foreach (var e in ene)
            {
                if(e.actor.TeamColor != atk.TeamColor && ! e.actor.GetComponent<NpcAttribute>().IsDead())
                {
                    var enePos = e.actor.GetVec2Pos();
                    var newDist = (myPos - enePos).LengthSquared();
                    if(newDist < ai.GetAttackRadiusSquare())
                    {
                        runner.DoDamage(e.actor);
                    }
                }
            }

        }
    }
}
