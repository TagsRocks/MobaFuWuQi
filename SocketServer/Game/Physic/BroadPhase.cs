using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Box2DX.Common;
using Gtk;

namespace MyLib 
{
    public class Cell
    {
        public List<Body> bodies = new List<Body>();
        public int Id;
        public void Add(Body body)
        {
            bodies.Add(body);
        }

        public void Del(Body body)
        {
            
        }
    }

    /// <summary>
    /// 只加入静态物体用于碰撞检测
    /// 忽略TankPass 碰撞体
    /// 
    /// 小心越界的问题
    /// </summary>
    public class BroadPhase
    {
        //public Cell[] cells;
        public Dictionary<int, Cell> cells; 
        private int nrow, ncol;
        public float cellSize, offset;
        /// <summary>
        /// row Y 方向坐标
        /// col X 方向坐标
        /// </summary>
        /// <param name="r"></param>
        /// <param name="c"></param>
        /// <param name="cs"></param>
        /// <param name="off"></param>
        public BroadPhase(int r, int c, float cs, float off)
        {
            nrow = r;
            ncol = c;
            cellSize = cs;
            offset = off;
            //cells = new Cell[r*c];
            cells = new Dictionary<int, Cell>();
            /*
            for (var i = 0; i < r*c; i++)
            {
                cells[i] = new Cell();
            }
            */
        }
        

        public void AddBody(Body body)
        {
            AddCells(body);
        }

        private void CalCells(Body body)
        {
            var aabb = new AABB();
            body.shape.ComputeAABB(ref aabb, body.xform);

            var minx = aabb.LowerBound.X;
            var miny = aabb.LowerBound.Y;
            var maxX = aabb.UpperBound.X;
            var maxY = aabb.UpperBound.Y;

            var startX = GetId(minx);
            var startY = GetId(miny);
            var endX = GetId(maxX);
            var endY = GetId(maxY);

            body.cells.Clear();
            for (var i = startX; i <= endX; i++)
            {
                for(var j = startY; j <= endY; j++)
                {
                    var cell = GetorAddCell(j, i);
                    if (cell != null)
                    {
                        body.cells.Add(cell);
                    }
                    //cell.Add(body);
                }
            }
        }

        private bool t1 = false;
        private void AddCells(Body body)
        {

            var aabb = new AABB();
            body.shape.ComputeAABB(ref aabb, body.xform);

            var minx = aabb.LowerBound.X;
            var miny = aabb.LowerBound.Y;
            var maxX = aabb.UpperBound.X;
            var maxY = aabb.UpperBound.Y;
            LogHelper.Log("Physic", "AABB:"+aabb.ToString());

            var startX = GetId(minx);
            var startY = GetId(miny);
            var endX = GetId(maxX);
            var endY = GetId(maxY);

            body.cells.Clear();
            for (var i = startX; i <= endX; i++)
            {
                for(var j = startY; j <= endY; j++)
                {
                    var cell = GetorAddCell(j, i);
                    if (cell != null)
                    {
                        body.cells.Add(cell);
                        cell.Add(body);
                    }
                }
            }
        }

        /// <summary>
        /// 将坐标偏移之后确保坐标都是在正数范围
        /// 这样网格都是在正数范围
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private int GetId(float p)
        {
            return (int)((p + offset)/cellSize);
        }

        private float GetPos(int id)
        {
            return id*cellSize - offset;
        }

        private Cell GetorAddCell(int r, int c)
        {
            var cid = r*ncol + c;
            if (cells.ContainsKey(cid))
            {
                return cells[cid];
            }
            else
            {
                var cell = new Cell();
                cell.Id = cid;
                cells[cid] = cell;
                return cell;
            }
        }

        public Vec2 GetCellPos(int id)
        {
            var row = id/ncol;
            var col = id%ncol;
            return new Vec2(GetPos(col), GetPos(row));
        }


        public void MoveBody(Body body)
        {
            
        }

        public void RemoveBody(Body body)
        {
            
        }

        /// <summary>
        /// 计算可能发生碰撞的AABB对象
        /// List消耗比较大的内存但是不用分配内存了
        /// 参数传入不用分配了 
        /// 某个网格不能行走不需要精确的物理
        /// 
        /// 坦克和Cell之间的力方向
        /// 简单的AABB方向
        /// </summary>
        /// <param name="body"></param>
        /// <param name="bodies"></param>
        public bool Query(Body body, ref Manifold manifold)
        {
            CalCells(body);
            Vec2 totalNormal = new Vec2();
            bool col = false;
            foreach (var cell in body.cells)
            {
                if (cell.bodies.Count > 0)
                {
                    manifold.PointCount = 1;
                    var cp = this.GetCellPos(cell.Id);
                    cp.X += cellSize/2;
                    cp.Y += cellSize/2;
                    var dir = body.xform.Position - cp;
                    var ax = Math.Abs(dir.X);
                    var ay = Math.Abs(dir.Y);
                    if (ax > ay)
                    {
                        dir.Y = 0;
                    }
                    else
                    {
                        dir.X = 0;
                    }
                    dir.Normalize();
                    totalNormal += dir;
                    /*
                    manifold.Points[0] = new ManifoldPoint()
                    {
                        LocalPoint1 = cp,
                        Normal1 = dir,
                    };
                    manifold.PointCount = 1;
                    */
                    col = true;
                    //return true;
                }
            }
            if (col)
            {
                manifold.PointCount = 1;
                totalNormal.Normalize();
                //综合考虑所有碰撞的对象的推力
                manifold.Points[0] = new ManifoldPoint()
                {
                    Normal1 = totalNormal,
                };
                return true;
            }

            manifold.PointCount = 0;
            return false;
        }

    }
}
