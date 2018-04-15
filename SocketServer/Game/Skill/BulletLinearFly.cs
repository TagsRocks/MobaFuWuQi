using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace MyLib
{
    class BulletLinearFly : GameObjectComponent
    {
        public SkillLayoutRunner runner;
        public MissileData missileData;
        public ActorInRoom attacker;
        public ActorInRoom target;

        public override void Init()
        {
            base.Init();
            GetRoom()?.RunTask(Fly);
        }

        private async Task Fly()
        {
            var tar = target as ActorInRoom;
            var me = attacker as ActorInRoom;
            var flyTime = missileData.lifeTime;
            var moveTotal = flyTime * missileData.Velocity;
            //GoDir Unity中的朝向
            var dir = gameObject.GoDir;
            var initPos = gameObject.pos.ToFloat();
            var rot = Quaternion.CreateFromYawPitchRoll(MathUtil.UnityYDegToMathRadian(dir), 0, 0);
            var rotDir = Vector3.Transform(MathUtil.forward, rot);
            var tarPos = initPos + rotDir * moveTotal;
            var passTime = 0.0f;

            var dictHurted = new HashSet<int>();
            var physic = GetRoom().GetComponent<PhysicManager>();

            while (!gameObject.IsDestroy && passTime < flyTime)
            {
                var rate = MathUtil.Clamp(passTime/flyTime, 0, 1);
                var curPos = Vector3.Lerp(initPos, tarPos, rate);
                gameObject.pos = MyVec3.FromVec3(curPos);

                var allEnemy = MobaUtil.FindNearEnemy(GetRoom(), curPos, missileData.Radius, attacker.TeamColor);
                var allEne = new HashSet<int>();
                foreach (var e in allEnemy)
                {
                    allEne.Add(e.ProxyId);
                }
                allEne.ExceptWith(dictHurted);
                foreach(var e in allEne)
                {
                    var ene = physic.GetProxy(e);
                    runner.DoDamage(ene.actor);
                    dictHurted.Add(e);
                }

                await new WaitForNextFrame(GetRoom());
                flyTime += Util.FrameSecTime;
            }
            /*
            if (tar != null)
            {
                var dist = tar.GetFloatPos() - me.GetFloatPos();
                dist.Y = 0;
                var flyTime = dist.Length() / missileData.Velocity;
                await Task.Delay(Util.TimeToMS(flyTime));
            }
            else
            {
                var flyTime = missileData.lifeTime;
                await Task.Delay(Util.TimeToMS(flyTime));
            }
            runner.DoDamage(target);
            */
        }
    }
}
