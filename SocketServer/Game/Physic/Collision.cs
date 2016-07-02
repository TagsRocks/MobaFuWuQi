using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2DX.Common;

namespace MyLib 
{
    public struct AABB
    {
        public Vec2 LowerBound;
        public Vec2 UpperBound;
        public override string ToString()
        {
            return "[AABB]" + LowerBound + " up " + UpperBound;
        }
    }

    public struct OBB
    {
        public Mat22 R;
        public Vec2 Center;
        public Vec2 Extents;
    }
    public partial class Collision
    {

    }


    public class ManifoldPoint
    {
        public Vec2 Normal1;//方向
    }

    public class Manifold
    {
        public ManifoldPoint[] Points = new ManifoldPoint[2];
        public int PointCount = 0;
    }


}
