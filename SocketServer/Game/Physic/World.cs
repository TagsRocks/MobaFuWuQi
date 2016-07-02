using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Box2DX.Common;


namespace MyLib
{
    /// <summary>
    /// 1:创建物理世界
    /// 2:添加物理形状Shape
    /// 3:修改物理形状的属性
    /// 4:测试某个物理形状的碰撞
    /// 
    /// 两种Shape支持 Circle Box
    /// 先实现Circle 支持
    /// 
    /// 需要将物理世界渲染出来
    /// </summary>
    public class World
    {
        public float l, t, r, b;
        public List<Body> bodies = new List<Body>();
        private float wid, hei;
        public World(float left,  float top,float right, float bottom)
        {
            l = left;
            t = top;
            r = right;
            b = bottom;
            wid = r - l;
            hei = b - t;
        }

        public Vec2 GetWindowSize()
        {
            return new Vec2(500, 500);
        }

        /// <summary>
        /// Unity世界坐标转化到Window坐标 浮点数
        /// </summary>
        /// <param name="ix"></param>
        /// <param name="iz"></param>
        /// <returns></returns>
        public Vec2 ConvertToWindowPos(float ix, float iz)
        {
            var ox = (ix - l)*GetWindowSize().X/wid;
            var oy = GetWindowSize().Y- (iz - t)*GetWindowSize().Y/hei;
            return new Vec2(ox, oy);
        }

        public Body CreatBody()
        {
            var b = new Body();
            bodies.Add(b);
            return b;
        }

        public void AddBody(Body bd)
        {
            bodies.Add(bd);
        }

        public void RemoveBody(Body bd)
        {
            bodies.Remove(bd);
        }

        public List<Body> OverLap(Body body)
        {
            var ret = new List<Body>();
            foreach (var body1 in bodies)
            {
                if (body1 != body)
                {
                    var contact = Contact.Create(body.shape, body1.shape);
                    if (contact.Overlap())
                    {
                        ret.Add(body1);
                    }
                }
            }
            return ret;
        } 
    }
}
