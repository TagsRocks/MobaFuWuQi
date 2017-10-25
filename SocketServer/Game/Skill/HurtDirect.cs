using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    class HurtDirect : SkillLogicComponent
    {
        public override void Run()
        {
            var runner = gameObject.parent.GetComponent<SkillLayoutRunner>();
            runner.DoDamage(runner.stateMachine.target);
        }
    }
}
