using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tainicom.Aether.Physics2D.Collision;
using System.Numerics;

namespace MyLib 
{
    /// <summary>
    /// 值对象不要放置可变的成员变量
    /// </summary>
    public struct EntityProxy
    {
        //public AABB aabb;
        public EntityActor actor;
        public int ProxyId;
        //public Vector2 lastPos;
    }
    public class PhysicManager : Component
    {
        private DynamicTree<EntityProxy> tree = new DynamicTree<EntityProxy>();
        public override void AfterAdd()
        {
            base.AfterAdd();
        }

        public EntityProxy AddEntity(EntityActor ety)
        {
            var ep = new EntityProxy();
            var pos = ety.GetFloatPos();
            var pvec2 = new Vector2(pos.X, pos.Z);
            //ep.lastPos = pvec2; 
            ep.actor = ety;
            var aabb = new AABB(pvec2, pvec2);
            ep.ProxyId = tree.AddProxy(ref aabb, ep);
            return ep;
        }
        public void RemoveEntity(EntityProxy proxy)
        {
            tree.RemoveProxy(proxy.ProxyId);
        }

        public void MoveEntity(ref EntityProxy proxy, Vector3 displace)
        {
            //var pos = proxy.actor.GetFloatPos();
            //var pvec2 = new Vector2(pos.X, pos.Z);
            var disp = new Vector2(displace.X, displace.Z);
            var pos = proxy.actor.GetVec2Pos();
            var aabb = new AABB(pos, pos);
            //proxy.aabb = aabb;
            tree.MoveProxy(proxy.ProxyId, ref aabb, disp);
            //proxy.lastPos = pvec2;
        }

        private int queryProxyId;
        private List<EntityProxy> pairs;
        public List<EntityProxy> GetNearyBy(EntityProxy proxy, float dist)
        {
            queryProxyId = proxy.ProxyId;
            pairs = new List<EntityProxy>();
            var aabb = new AABB();
            var pvec2 = proxy.actor.GetVec2Pos();
            aabb.LowerBound = pvec2 - new Vector2(dist, dist);
            aabb.UpperBound = pvec2 + new Vector2(dist, dist);
            tree.Query(QueryCallback, ref aabb);
            return pairs;
        }
        private bool QueryCallback(int proxyId)
        {
            if(proxyId == queryProxyId)
            {
                return true;
            }
            pairs.Add(tree.GetUserData(proxyId));
            return true;
        }
    }
}
