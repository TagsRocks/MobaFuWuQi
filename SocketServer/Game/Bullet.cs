using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2DX.Common;

namespace MyLib
{
    class Bullet : Actor
    {
        public MissileData missileData;
        private float velocity;
        private float maxDistance;
        private XForm xform;
        public EntityInfo entityInfo;

        public override void Init()
        {
            this.RunTask(Update);
        }

        //服务器上使用UnityEngine 的数据
        private async Task Update()
        {
            velocity = GameConst.Instance.BulletSpeed;
            maxDistance = missileData.MaxDistance;
            while (!isStop)
            {
            }

        }
        
    }
}
