using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2DX.Common;

namespace MyLib 
{
    public enum ShapeType
    {
        UnknownShape = -1,
        CircleShape,
        PolygonShape,
        ShapeCount,
    }

    public abstract class Shape
    {
        public ShapeType _type;
        public Body _body;

        public abstract void ComputeAABB(ref MyAABB aabb, XForm xf);
    }


}
