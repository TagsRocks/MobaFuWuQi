using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{ 
    /// <summary>
    /// 什么时候删除SkillStateMachine
    /// 添加一个DeadTime即可
    /// </summary>
    class SkillStateMachine : GameObjectComponent
    {
        public SkillAction action;
        private SkillDataConfig config;
        public GameObjectActor target;
        public GameObjectActor attacker;
        /// <summary>
        /// 如果需要定制技能状态机移除时间，可以通过外部设置999时间，接着外部主动发送事件删除
        /// 技能本身存活时间超出玩家的动作状态时间的
        /// </summary>
        public float deadTime = 5;

        /// <summary>
        /// 设置Action在添加StateMachine 到GameObject之前
        /// 
        /// TODO:玩家Actor如何和Entity Actor合并
        /// </summary>
        public override void Init()
        {
            base.Init();
            attacker = GetRoom().entityCom.GetEntity(action.Who);
            target = GetRoom().entityCom.GetEntity(action.Target);
            config = gameObject.GetComponent<SkillDataConfig>();
            OnEvent(SkillEvent.EventStart);

            GetRoom().RunTask(WaitDelete);
        }

        /// <summary>
        /// 移除掉状态机
        /// </summary>
        /// <returns></returns>
        private async Task WaitDelete()
        {
            await Task.Delay(Util.TimeToMS(deadTime));
            gameObject.Destroy();    
        }

        public void OnEvent(SkillEvent evt)
        {
            foreach (var item in config.eventList)
            {
                if (item.evt == evt)
                {
                    InitLayout(item);
                }
            }
        }

        /// <summary>
        /// 激活对应的Layout
        /// </summary>
        /// <param name="item"></param>
        private void InitLayout(EventItem item)
        {
            if(item.layout != null)
            {
                var runner = item.layout.go.AddComponent<SkillLayoutRunner>();
                runner.stateMachine = this;
                runner.Run();
            }
        }

        /// <summary>
        /// 获取攻击力
        /// 获取防御力
        /// 计算伤害
        /// 添加伤害到NPC的Life上
        /// 广播伤害命令
        /// </summary>
        /// <param name="target"></param>
        public void DoDamage(GameObjectActor target)
        {
            var dmg = attacker.GetComponent<AINPC>().unitData.Damage;
            target.GetComponent<NpcAttribute>().DoHurt(dmg);

            var gcPlayerCmd = GCPlayerCmd.CreateBuilder();
            var dmgInfo = DamageInfo.CreateBuilder();
            dmgInfo.Attacker = attacker.GetComponent<AINPC>().mySelf.entityInfo.Id;
            dmgInfo.Enemy = target.GetComponent<AINPC>().mySelf.entityInfo.Id;
            dmgInfo.IsCritical = false;
            dmgInfo.IsStaticShoot = false;
            gcPlayerCmd.DamageInfo = dmgInfo.Build();
            gcPlayerCmd.Result = "Damage";
            GetRoom().AddCmd(gcPlayerCmd);
        }
    }
}
