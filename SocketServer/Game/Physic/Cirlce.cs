using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Box2DX.Common;
using Math = Box2DX.Common.Math;

namespace MyLib 
{
    public class PolygonShape : Shape
    {
        public Vec2 localPos;
        public int vertexCount;
        public Vec2[] vertices = new Vec2[Settings.MaxPolygonVertices];
        public Vec2[] normals = new Vec2[Settings.MaxPolygonVertices];

        public void InitNormal()
        {
            for(var i = 0; i < vertexCount; i++)
            {
                var i1 = i;
                var i2 = i + 1 < vertexCount ? i + 1 : 0;
                var edge = vertices[i2] - vertices[i1];
                normals[i] = Vec2.Cross(edge, 1.0f);
                normals[i].Normalize(); 
            }
        }

        public PolygonShape()
        {
            _type = ShapeType.PolygonShape;
            vertexCount = 0;
        }

        public void AddVert(float x, float y)
        {
            vertices[vertexCount++].Set(x, y);
        }

        public override void ComputeAABB(ref AABB aabb, XForm xf)
        {
            var minX = 1000.0f;
            var maxX = -1000.0f;
            var minY = 1000.0f;
            var maxY = -1000.0f;

            foreach (var vertex in vertices)
            {
                var nv = Math.Mul(xf, vertex);
                minX = Math.Min(nv.X, minX);
                minY = Math.Min(nv.Y, minY);
                maxX = Math.Max(nv.X, maxX);
                maxY = Math.Max(nv.Y, maxY);
            }
            aabb.LowerBound.Set(minX, minY);
            aabb.UpperBound.Set(maxX, maxY);
        }
    }

    public class Circle : Shape
    {
        public Box2DX.Common.Vec2 localPos;
        public float radius;

        public Circle()
        {
            _type = ShapeType.CircleShape;
        }

        public override void ComputeAABB(ref AABB aabb, XForm xf)
        {
            throw new NotImplementedException();
        }
    }



    public partial class Collision
    {
        public static void CollideCircles(ref Manifold manifold, Circle circle1, XForm x1, Circle circle2, XForm x2)
        {
            manifold.PointCount = 0;
            var p1 = Math.Mul(x1, circle1.localPos);
            var p2 = Math.Mul(x2, circle2.localPos);
            var d = p2 - p1;
            var distSqrt = Vec2.Dot(d, d);
            var r1 = circle1.radius;
            var r2 = circle2.radius;
            var radiusSum = r1 + r2;
            if (distSqrt > radiusSum*radiusSum)
            {
                return;
            }
            manifold.PointCount = 1;
        }

        public static void CollidePolygonAndCircle(ref Manifold manifold, PolygonShape polygon, XForm x1, Circle circle,
            XForm x2)
        {
            manifold.PointCount = 0;
            //将Circle转化到Polygon的空间里面
            var c = Math.Mul(x2, circle.localPos);
            var cLocal = Math.MulT(x1, c);

            //寻找最近的边
            var normalIndex = 0;
            var separation = -Settings.FLT_MAX;
            var radius = circle.radius;
            var vertexCount = polygon.vertexCount;
            var vertices = polygon.vertices;
            var normals = polygon.normals; 
            //找到距离最远的边
            for (var i = 0; i < vertexCount; i++)
            {
                var s = Vec2.Dot(normals[i], cLocal - vertices[i]);
                if (s > radius)
                {
                    return;
                }
                if (s > separation)
                {
                    separation = s;
                    normalIndex = i;
                }
            }
            //圆心在边上
            if (separation < Settings.FLT_EPSILON)
            {
                manifold.PointCount = 1;
                return;
            }

            var vertIndex1 = normalIndex;
            var vertIndex2 = vertIndex1 + 1 < vertexCount ? vertIndex1 + 1 : 0;
            var e = vertices[vertIndex2] - vertices[vertIndex1];
            var length = e.Normalize();
            //边上的投影 在内部
            var u = Vec2.Dot(cLocal - vertices[vertIndex1], e);
            Vec2 p;
            if (u <= 0.0f)
            {
                p = vertices[vertIndex1];
            }
            else if (u >= length)
            {
                p = vertices[vertIndex2];
            }
            else
            {
                p = vertices[vertIndex1] + u*e;
            }

            var d = cLocal - p;
            var dist = d.Normalize();
            if (dist > radius)
            {
                return;
            }
            manifold.PointCount = 1;
        }

        private static float FindMaxSeparation(ref int edgeIndex, PolygonShape poly1, XForm xf1, PolygonShape poly2,
            XForm xf2)
        {
            return 0;
        }

        //速度垂直于对方Normal的分量保留即可
        //碰撞的点以及对应的Normal法线
        //只考虑AABB碰撞和法线即可
        public static void CollidePolygons(ref Manifold manifold, PolygonShape polyA, XForm xfA, PolygonShape polyB, XForm xfB)
        {
            manifold.PointCount = 0;
            var edgeA = 0;
            var separationA = Collision.FindMaxSeparation(ref edgeA, polyA, xfA, polyB, xfB);
            if (separationA > 0.0f)
            {
                return;
            }
            var edgeB = 0;
            var separationB = Collision.FindMaxSeparation(ref edgeB, polyB, xfB, polyA, xfA);
            if (separationB > 0.0f)
            {
                return;
            }

            PolygonShape poly1, poly2;
            XForm xf1, xf2;

        }
    }
}
