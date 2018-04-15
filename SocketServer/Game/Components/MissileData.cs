using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    /// <summary>
    /// 子弹类型 追踪目标
    /// 瞄准位置
    /// </summary>
    public enum MissileType
    {
        Target,
        Pos,
        Linear,
    }

    class MissileData : GameObjectComponent 
    {
        public MissileType missileType = MissileType.Target;
        public float lifeTime = 5;
        public float Velocity = 20;
        public float Radius = 0.8f;
    }
}
