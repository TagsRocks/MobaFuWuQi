using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Box2DX.Common;
using Gdk;
using Gtk;
using Cairo;

namespace MyLib
{
    public class CairoDrawing : DrawingArea
    {
        public World world;
        public BroadPhase broadPhase;

        private static void DrawCurvedRect(Cairo.Context gr, double x, double y, double width, double height)
        {
            gr.Save();
            gr.MoveTo(x, y + height/2);
            gr.CurveTo(x, y, x, y, x + width/2, y);
            gr.CurveTo(x + width, y, x + width, y, x + width, y + height/2);
            gr.CurveTo(x + width, y + height, x + width, y + height, x + width/2, y + height);
            gr.CurveTo(x, y + height, x, y + height, x, y + height/2);
            gr.Restore();
        }

        private static void DrawCircle(Context gr, double x, double y, double radius)
        {
            gr.Save();
            gr.MoveTo(x+radius, y);
            gr.Arc(x, y, radius, 0, 360);
            gr.Restore();
        }

        private static void DrawPolygon(Context gr, Vec2[] vertices, int num)
        {
            gr.Save();
            var v0 = vertices[0];
            gr.MoveTo(v0.X, v0.Y);
            for (var i = 0; i < num; i++)
            {
                var v = vertices[i];
                gr.LineTo(v.X, v.Y);
                //LogHelper.Log("DrawPolygon", "n "+num+" line "+v.X+" vy "+v.Y);
            }
            gr.LineTo(v0.X, v0.Y);
            gr.Restore();
        }

        private void DrawBody(Body b, Context gr)
        {
            var pos = b.xform.Position;
            if (b.shape._type == ShapeType.CircleShape)
            {
                var radius = ((Circle) b.shape).radius;
                DrawCircle(gr, pos.X, pos.Y, radius);
            }else if (b.shape._type == ShapeType.PolygonShape)
            {
                var pl = b.shape as PolygonShape;
                var vert = new Vec2[pl.vertexCount];
                for (var i = 0; i < pl.vertexCount; i++)
                {
                    //LogHelper.Log("Vertices: ", "v "+pl.vertices[i]);
                    var nv = Box2DX.Common.Math.Mul(b.xform, pl.vertices[i]);
                    nv = this.world.ConvertToWindowPos(nv.X, nv.Y);
                    vert[i] = nv;
                }
                DrawPolygon(gr, vert, vert.Length);
            }
        }

        private void DrawCell(Vec2 pos, float sz, Context gr)
        {
            var vert = new Vec2[4];
            var v1 = this.world.ConvertToWindowPos(pos.X, pos.Y);
            vert[0] = v1;
            vert[1] = this.world.ConvertToWindowPos(pos.X + sz, pos.Y);
            vert[2] = this.world.ConvertToWindowPos(pos.X + sz, pos.Y+sz);
            vert[3] = this.world.ConvertToWindowPos(pos.X, pos.Y+sz);
            DrawPolygon(gr, vert, vert.Length);
        }

        public int drawRef = 0;
        protected override bool OnExposeEvent(EventExpose evnt)
        {
            if (world != null)
            {
                lock (world)
                {
                    using (var g = Gdk.CairoHelper.Create(evnt.Window))
                    {
                        foreach (var body in world.bodies)
                        {
                            DrawBody(body, g);
                        }
                        g.Color = new Cairo.Color(0.2, 0.8, 1, 1);
                        g.LineWidth = 2;
                        g.Stroke();
                        if (broadPhase != null)
                        {
                            foreach (var i in broadPhase.cells.Keys)
                            {
                                var cp = broadPhase.GetCellPos(i);
                                var cell = broadPhase.cells[i];
                                if (cell.bodies.Count > 0)
                                {
                                    DrawCell(cp, broadPhase.cellSize, g);
                                }

                            }

                            g.Color = new Cairo.Color(0.8, 0.2, 0.2, 1);
                            g.LineWidth = 2;
                            g.Stroke();
                        }
                        //LogHelper.Log("Physic", "World Render");
                    }
                }
            }
            return true;
        }
    }

    public class WorldRender
    {
        public Gtk.Window window;
        private CairoDrawing area;
        public void Render(float w, float h)
        {
            var width = (int)w;
            var height = (int)h;
            Application.Init();
            Gtk.Window window = new Gtk.Window("Test");
            this.window = window;
            var a = new CairoDrawing();
            area = a;
            var box = new HBox();
            box.Add(a);

            window.Add(box);
            window.SetDefaultSize(width, height);

            window.ShowAll();
            GLib.Timeout.Add(20, new GLib.TimeoutHandler(Forever));
            Application.Run();
        }

        public CairoDrawing GetArea()
        {
            return area;
        }
        public void Update(World w, BroadPhase bp)
        {
            if (area != null)
            {
                area.world = w;
                area.broadPhase = bp;
            }
        }

        bool Forever()
        {
            window.QueueDraw();
            return true;
        }
    }
}
