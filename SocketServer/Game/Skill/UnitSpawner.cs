using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    /// <summary>
    /// 生成子弹
    /// 子弹飞行
    /// 子弹碰撞伤害
    /// 
    /// 正常的单个子弹生成器
    /// 逻辑子弹只是生成XZ平面
    /// 表现子弹自己追击目标
    /// </summary>
    class UnitSpawner : SkillLogicComponent
    {
        public FakeGameObject Missile;
        public override void Run()
        {
            var runner = gameObject.parent.GetComponent<SkillLayoutRunner>();
            var attacker = runner.stateMachine.attacker;
            if (attacker != null)
            {
                //子弹需要同步Entity信息给客户端么
                //如果不需要的话 则不同步只把技能同步 子弹表现客户端自己表现
                //伤害服务器计算
                var go = new GameObjectActor();
                go.name = "bullet_" + runner.stateMachine.gameObject.name;

                var bullet = go.AddComponent<Bullet>();
                bullet.missileData = Missile.go.GetComponent<MissileData>();
                bullet.runner = runner;
                bullet.attacker = runner.stateMachine.attacker;
                bullet.target = runner.stateMachine.target;
                var entity = bullet.attacker as ActorInRoom;
                go.pos = entity.GetIntPos();
                go.GoDir = entity.dir;
                gameObject.AddChild(go);
            }
        }
    }
}
