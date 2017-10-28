using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Box2DX.Common;
using Gtk;

namespace MyLib
{
    public class TestPhysic : Actor
    {
        public override void Init()
        {
            InitMessageQueue();
            this.RunTask(DrawWorld);
            this.RunTask(UpdateWorld);
        }

        private void GTKThread()
        {
            LogHelper.Log("Physic", "BeginDraw");
            Contact.InitializeRegister();
            var w = new World(0, 0, 500, 500);
            world = w;
            var b1 = w.CreatBody();
            body1 = b1;
            b1.SetPos(40, 40);
            var c = new Circle();
            c.radius = 20;
            c.localPos.Set(0, 0);
            b1.AddShape(c);

            var b2 = w.CreatBody();
            b2.SetPos(100, 100);
            var c2 = new Circle();
            c2.radius = 20;
            c2.localPos.Set(0, 0);
            b2.AddShape(c2);

            var result = w.OverLap(b2);
            foreach (var body in result)
            {
                Console.WriteLine(body.ToString());
            }

            var b3 = w.CreatBody();
            b3.SetPos(200, 200);
            var p = new PolygonShape();
            p.AddVert(-10, 10);
            p.AddVert(-20, -10);
            p.AddVert(20, -10);
            p.AddVert(10, 10);
            p.InitNormal();
            b3.AddShape(p);

            var r = new WorldRender();
            worldRender = r;
            r.Render(w.r, w.b);
            LogHelper.Log("Physic", "DrawWorld");

        }
        private WorldRender worldRender;
        private World world;
        private Body body1;
        private async Task DrawWorld()
        {
            var th = new Thread(GTKThread);
            th.Start();
        }

        private async Task UpdateWorld()
        {
            while (!isStop)
            {
                await Task.Delay(1000);
                body1.xform.Position += new Vec2(2, 0);
            }
        }

    }
}
