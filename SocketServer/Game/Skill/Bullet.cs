using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2DX.Common;

namespace MyLib
{
    class Bullet : GameObjectComponent 
    {
        public SkillLayoutRunner runner;
        public MissileData missileData;
        public GameObjectActor attacker;
        public GameObjectActor target;

        public override void Init()
        {
            base.Init();
            GetRoom().RunTask(Fly);
        }
        private async Task Fly()
        {
            if(missileData.missileType == MissileType.Target)
            {
                var tar = target as EntityActor;
                var me = attacker as EntityActor;
                var dist = tar.GetFloatPos() - me.GetFloatPos();
                dist.Y = 0;
                var flyTime = dist.Length() / missileData.Velocity;
                await Task.Delay(Util.TimeToMS(flyTime));
                runner.DoDamage(target);
            }
        }
    }
}
