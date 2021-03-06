﻿using System;
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
        public ActorInRoom target;
        public ActorInRoom attacker;
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
            attacker = GetRoom().GetActorInRoom(action.Who);
            target = GetRoom().GetActorInRoom(action.Target);
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
            try
            {
                foreach (var item in config.eventList)
                {
                    var runner = item.layout.go.AddComponent<SkillLayoutRunner>();
                    runner.stateMachine = this;
                    deadTime = Math.Max(runner.GetDuration(), deadTime);
                }
                deadTime += 1;
            }catch(Exception exp)
            {
                Log.Error(exp.ToString());
            }

            await Task.Delay(Util.TimeToMS(deadTime));
            gameObject.Destroy();    
        }

        public void OnEvent(SkillEvent evt)
        {
            try
            {
                foreach (var item in config.eventList)
                {
                    if (item.evt == evt)
                    {
                        InitLayout(item);
                    }
                }
            }catch(Exception exp)
            {
                Log.Error(exp.ToString());
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
        public void DoDamage(ActorInRoom target)
        {
            if(attacker == null || target == null)
            {
                return;
            }
            var dmg = attacker.GetComponent<AINPC>().unitData.Damage;
            //装备伤害加成
            var player = attacker as PlayerInRoom;
            if(player != null)
            {
                var avatarInfo = player.GetAvatarInfo();
                var itemList = avatarInfo.ItemInfoList;
                foreach(var i in itemList)
                {
                    var info = GMDataBaseSystem.database.SearchId<EquipConfigData>(GameData.EquipConfig, i);
                    dmg += info.attack;
                }
            }


            var cfg = attacker.GetComponent<AINPC>().npcConfig;
            var tai = target.GetComponent<AINPC>() as TowerAI;
            //小炮对塔伤害加成
            if (tai != null)
            {
                dmg = (int)(dmg * cfg.damageToTower);
            }

            target.GetComponent<NpcAttribute>().DoHurt(attacker, dmg);

            var gcPlayerCmd = GCPlayerCmd.CreateBuilder();
            var dmgInfo = DamageInfo.CreateBuilder();
            dmgInfo.Attacker = attacker.GetComponent<AINPC>().mySelf.IDInRoom;
            dmgInfo.Enemy = target.GetComponent<AINPC>().mySelf.IDInRoom;
            dmgInfo.IsCritical = false;
            dmgInfo.IsStaticShoot = false;
            dmgInfo.Damage = dmg;
            gcPlayerCmd.DamageInfo = dmgInfo.Build();
            gcPlayerCmd.Result = "Damage";
            GetRoom().AddCmd(gcPlayerCmd);
        }
    }
}
