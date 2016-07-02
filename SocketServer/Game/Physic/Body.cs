using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2DX.Common;
using Gtk;

namespace MyLib
{
    public class Body
    {
        public XForm xform;
        public Shape shape;
        //Body所占用的网格
        public List<Cell> cells = new List<Cell>();
        public bool IsCollide = false;

        public Vec2 LinearVelocity = Vec2.Zero;
        public Body()
        {
            xform.Position.SetZero();
            xform.R.SetIdentity();
        }
        public void AddShape(Shape sd)
        {
            shape = sd;
            shape._body = this;
        }

        public override string ToString()
        {
            return xform.ToString();
        }

        public void SetPos(float x, float y)
        {
            xform.Position.Set(x, y);
        }
    }
}
