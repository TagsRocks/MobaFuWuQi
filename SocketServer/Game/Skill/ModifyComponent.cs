using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    /// <summary>
    /// 类似Dota中的修改器
    /// </summary>
    class ModifyComponent : GameObjectComponent
    {
        public int totalDefenseAdd = 0;

        private List<AffixSpawn> allAffixes = new List<AffixSpawn>();

        public override void Init()
        {
            base.Init();
            gameObject.RunTask(UpdateBuff);
        }

        private async Task UpdateBuff()
        {
            while(!gameObject.IsDestroy)
            {
                var removeList = new List<AffixSpawn>();
                foreach(var b in allAffixes)
                {
                    var now = Util.GetServerTime();
                    if(now - b.startTime > b.duration)
                    {
                        removeList.Add(b);
                    }
                }

                foreach(var r in removeList)
                {
                    //allAffixes.Remove(r);
                    //r.OnExit();
                    RemoveBuff(r.AffixName);
                }
                await new WaitForNextFrame(gameObject.GetRoom());
            }
        }

        public void AddBuff(AffixSpawn affix)
        {
            affix.modify = this;
            allAffixes.Add(affix);
            affix.OnEnter();

            var gcCmd = GCPlayerCmd.CreateBuilder();
            gcCmd.Result = "AddBuff";
            var bin = BuffInfo.CreateBuilder();
            var actorInRoom = gameObject as ActorInRoom;
            bin.Target = actorInRoom.IDInRoom;
            bin.Attacker = affix.runner.stateMachine.attacker.IDInRoom;
            bin.SkillId = affix.runner.stateMachine.action.SkillId;
            bin.BuffName = affix.AffixName;
            gcCmd.BuffInfo = bin.Build();

            gameObject.GetRoom().AddKCPCmd(gcCmd);
        }

        public void RemoveBuff(string buffName)
        {
            AffixSpawn buff = null;
            foreach(var a in allAffixes)
            {
                if(a.AffixName == buffName)
                {
                    allAffixes.Remove(a);
                    buff = a;
                    break;
                }
            }
            if(buff != null)
            {
                var affix = buff;

                var gcCmd = GCPlayerCmd.CreateBuilder();
                gcCmd.Result = "RemoveBuff";
                var bin = BuffInfo.CreateBuilder();
                var actorInRoom = gameObject as ActorInRoom;
                bin.Target = actorInRoom.IDInRoom;
                bin.Attacker = affix.runner.stateMachine.attacker.IDInRoom;
                bin.SkillId = affix.runner.stateMachine.action.SkillId;
                bin.BuffName = affix.AffixName;
                gcCmd.BuffInfo = bin.Build();

                gameObject.GetRoom().AddKCPCmd(gcCmd);

                buff.OnExit();
            }
        }
    }
}
