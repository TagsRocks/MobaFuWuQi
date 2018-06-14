using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    class LanBabaAffix : AffixSpawn
    {
        public FakeGameObject effect;

        public override void Run()
        {
            var runner = gameObject.parent.GetComponent<SkillLayoutRunner>();
            var attacker = runner.stateMachine.attacker;
            var target = runner.stateMachine.target;
            var modify = target.GetComponent<ModifyComponent>();
            modify.AddBuff(this);
            gameObject.RunTask(Buff);
        }

        private async Task Buff()
        {
            // 添加
            //移除
            //处理事件
        }
        public void OnTakeDamage()
        {

        }
    }
}
