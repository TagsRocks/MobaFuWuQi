using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    class SkillLayoutRunner : GameObjectComponent
    {
        public SkillStateMachine stateMachine;

        public void Run()
        {
            foreach(var c in gameObject.GetChildren())
            {
                var logic = c.GetComponent<SkillLogicComponent>();
                logic.runner = this;
                logic.Run();
            }
        }
        /// <summary>
        /// 对目标NPC造成伤害
        /// </summary>
        public void DoDamage(ActorInRoom actor)
        {
            stateMachine.DoDamage(actor);
        }

        public float GetDuration()
        {
            var maxDur = 5.0f;
            foreach(var c in gameObject.GetChildren())
            {
                var logic = c.GetComponent<SkillLogicComponent>();
                logic.runner = this;
                maxDur = Math.Max(maxDur, logic.GetDuration());
            }
            return maxDur;
        }
    }
}
