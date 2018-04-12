using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    class AffixSpawn : SkillLogicComponent
    {
        public string AffixName = "None";
        public int defenseAdd = 0;
        public float duration = 10;

        
        public override void Run()
        {
            throw new NotImplementedException();
        }

        public virtual void OnEnter()
        {
            inBuff = true;
            startTime = Util.GetServerTime();
            modify.totalDefenseAdd += defenseAdd;
        }
        public virtual void OnExit()
        {
            inBuff = false;
            modify.totalDefenseAdd -= defenseAdd;
        }

        public override float GetDuration()
        {
            return duration;
        }

        public float startTime;
        public ModifyComponent modify;
        public bool inBuff = false;
    }
}
