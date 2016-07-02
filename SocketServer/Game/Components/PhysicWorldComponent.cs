using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Box2DX.Common;
using SimpleJSON;
using Math = Box2DX.Common.Math;

namespace MyLib 
{
    class PhysicWorldComponent :Component
    {
        private World physicWorld;
        private Dictionary<int,Body> bodies = new Dictionary<int, Body>();
        private Dictionary<BoxCollider, Body> staticBodies = new Dictionary<BoxCollider, Body>();
 
        private GameObjectActor entityConfig;
        private BroadPhase broadPhase;
        public PhysicWorldComponent()
        {
            physicWorld = new World(-40, -30, 40, 30);
            broadPhase = new BroadPhase(96, 200, 0.5f, 50);
        }

        public override void AfterAdd()
        {
            using (var f = new StreamReader("Colliders.json"))
            {
                var con = f.ReadToEnd();
                var jobj = JSON.Parse(con).AsObject;
                entityConfig = EntityImport.InitGameObject(jobj);
                entityConfig.room = this.actor as RoomActor;
                entityConfig.Start();
            }
        }

        public void AddStaticCollider(BoxCollider collider)
        {
            var b = new Body();
            var s1 = new PolygonShape();
            var sx = collider.Size.x/100.0f;
            var sz = collider.Size.z/100.0f;
            s1.AddVert(-0.5f*sx, 0.5f*sz);
            s1.AddVert(-0.5f*sx, -0.5f*sz);
            s1.AddVert(0.5f*sx, -0.5f*sz);
            s1.AddVert(0.5f*sx, 0.5f*sz);
            s1.InitNormal();
            b.AddShape(s1);
            staticBodies.Add(collider, b);
            b.xform.Position.Set(collider.Center.x/100.0f, collider.Center.z/100.0f);

            lock (physicWorld)
            {
                physicWorld.AddBody(b);
                if (collider.Layer != "TankPass")
                {
                    broadPhase.AddBody(b);
                }
            }
        }

        public void AddPlayer(PlayerActor player)
        {
            var b = new Body();
            var s1 = new PolygonShape();
            s1.AddVert(-0.6f, 1.3f);
            s1.AddVert(-0.6f, -0.7f);
            s1.AddVert(0.6f, -0.7f);
            s1.AddVert(0.6f, 1.3f);
            s1.InitNormal();
            b.AddShape(s1);
            bodies.Add(player.Id, b);
            lock (physicWorld)
            {
                physicWorld.AddBody(b);
            }
            LogHelper.Log("Physic", "AddPlayer: "+b+" pid "+player.Id);
        }

        public void RemovePlayer(PlayerActor player)
        {
            LogHelper.Log("Physic", "remoePlayer: "+player.Id);
            lock (physicWorld)
            {
                if (bodies.ContainsKey(player.Id))
                {
                    var bd = bodies[player.Id];
                    bodies.Remove(player.Id);
                    physicWorld.RemoveBody(bd);
                }
            }
        }
        private Manifold manifold = new Manifold();

        public Vec2 UpdatePlayerPhysic(int id, AvatarInfo.Builder pi, int num)
        {
            Vec2 newPos = new Vec2();
            lock (physicWorld)
            {
                var x = pi.X;
                var z = pi.Z;
                var sx = pi.SpeedX/100.0f;
                var sy = pi.SpeedY/100.0f;

                if (bodies.ContainsKey(id))
                {
                    var bd = bodies[id];
                    if (pi.ResetPos)
                    {
                        var curPos = new Vec2(x/100.0f, z/100.0f);
                        bd.xform.Position = curPos;
                    }
                    bd.xform.R.Set(-Math.Deg2Rad(pi.Dir));

                    /*
                    var col1 = broadPhase.Query(bd, ref manifold);
                    //施加推力将坦克推开
                    if (col1)
                    {
                        bd.xform.Position += manifold.Points[0].Normal1*6*0.05f;
                        newPos = bd.xform.Position;
                    }
                    */
                    // else
                    {
                        for (var i = 0; i < num; i++)
                        {
                            var nowPos = bd.xform.Position;
                            var mx = sx*0.05f;
                            var my = sy*0.05f;
                            //子弹穿透
                            //沿着速度移动
                            nowPos.X += mx;
                            nowPos.Y += my;
                            bd.xform.Position = nowPos;
                            var col = broadPhase.Query(bd, ref manifold);
                            //计算AABB如果碰撞了则恢复为旧的位置
                            if (col)
                            {
                                var normal = manifold.Points[0].Normal1;
                                var fix = Math.Max(0.5f, Vec2.Dot(normal, new Vec2(mx, my)))*normal;
                                bd.xform.Position += fix;
                                newPos = bd.xform.Position;
                            }
                            else
                            {
                                bd.xform.Position = nowPos;
                                newPos = nowPos;
                            }
                        }
                    }

                }
            }

            if (render != null)
            {
                lock (physicWorld)
                {
                    render.Update(physicWorld, broadPhase);
                }
            }

            //LogHelper.Log("Physic", "UpdatePlayer playerId: "+id+" pi "+pi.Build()+" newPos "+newPos);
            return newPos;
        }


        private WorldRender render;
        private void GTKThread()
        {
            render = new WorldRender();
            render.Render(physicWorld.GetWindowSize().X, physicWorld.GetWindowSize().Y);
        }
        public void Show()
        {
            var th = new Thread(GTKThread);
            th.Start();
        }
    }
}
