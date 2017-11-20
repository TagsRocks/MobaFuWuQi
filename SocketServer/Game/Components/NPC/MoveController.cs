using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace MyLib
{
    /// <summary>
    /// 执行移动逻辑
    /// A 移动到 B
    /// 移动结束通知上层
    /// 可以被打断
    /// </summary>
    public class MoveController : GameObjectComponent
    {
        private bool stopMove = false;
        private bool inMove = false;
        private int curMoveId = 0;
        private MyVec3 moveTarget;
        private bool hasNewTarget = false;


        public void StopMove()
        {
            if (inMove)
            {
                stopMove = true;
                hasNewTarget = false;
            }
        }
        private AINPC aiNpc;
        private PhysicManager physicManager;
        private GridManager grid;
        public override void Init()
        {
            base.Init();
            aiNpc = gameObject.GetComponent<AINPC>();
            physicManager = GetRoom().GetComponent<PhysicManager>();
            grid = GetRoom().GetComponent<GridManager>();
        }

        /// <summary>
        /// 移动代码中间不能打断
        /// 防止代码重入
        /// 单线程 多协程的时候
        /// 
        /// 新的移动位置重新计算寻路路径轨迹
        /// </summary>
        /// <param name="v3Pos"></param>
        /// <returns></returns>
        public async Task MoveTo(MyVec3 v3Pos)
        {
            if (inMove)
            {
                curMoveId++;
                moveTarget = v3Pos;
                hasNewTarget = true;
                return;
            }
            var curTarget = v3Pos;

StartMove:

            var speed = aiNpc.npcConfig.moveSpeed;
            var runMoveId = ++curMoveId;
            inMove = true;
            var tarPos = curTarget.ToFloat();
            var curObj = gameObject as ActorInRoom;
            //curObj.entityInfo.Speed = Util.GameVecToNet(speed);

            var nowPos = curObj.GetFloatPos();
            var initPos = nowPos;
           
            var g1 = grid.mapPosToGrid(nowPos);
            var g2 = grid.mapPosToGrid(tarPos);
            var path = grid.FindPath(g1, g2);
            var nodes = new Vector3[path.Count];
            var i = 0;
            foreach (var p in path)
            {
                var wp = grid.gridToMapPos(new Vector2(p.x, p.y));
                nodes[i++] = wp;
            }

            var room = GetRoom();
            var entityInfo = curObj.DuckInfo;

            var curPoint = 1;
            while (curPoint < nodes.Length && !stopMove && runMoveId == curMoveId)
            {
                var nextPos = nodes[curPoint];
                var wp = nextPos;
                var deltaPos = wp - initPos;
                deltaPos.Y = 0;
                var dist = (deltaPos).Length();
                var totalTime = dist / speed;
                var passTime = 0.0f;

                curObj.DuckInfo.SpeedX = Util.RealToNetPos((wp.X-initPos.X) /totalTime);
                curObj.DuckInfo.SpeedY = Util.RealToNetPos((wp.Z-initPos.Z) /totalTime);

                //Log.AI("EntityMove:" + initPos + ":" +wp+":"+speed+":sxsy:"+curObj.DuckInfo.SpeedX+":"+ curObj.DuckInfo.SpeedY+":"+passTime+":"+totalTime);
                //这个点位置太近了
                if (totalTime > 0.01f)
                {
                    while (passTime < totalTime && !stopMove && runMoveId == curMoveId)
                    {
                        passTime += Util.FrameSecTime;
                        var newPos = Vector3.Lerp(initPos, wp, MathUtil.Clamp(passTime / totalTime, 0, 1));
                        //var newPos = nowPos + 
                        var myPos = MyVec3.FromFloat(newPos.X, newPos.Y, newPos.Z);
                        aiNpc.mySelf.SetPos(myPos);
                        nowPos = newPos;
                        //await Task.Delay(MainClass.syncTime);
                        await new WaitForNextFrame(aiNpc.mySelf.GetRoom());
                    }
                }else
                {
                    var newPos = wp;
                    var myPos = MyVec3.FromVec3(newPos);
                    aiNpc.mySelf.SetPos(myPos);
                    nowPos = newPos;
                }
                initPos = wp;
                curPoint++;
            }

            if (runMoveId == curMoveId)
            {
                stopMove = false;
                inMove = false;
                curObj.DuckInfo.SpeedX = 0;
                curObj.DuckInfo.SpeedY = 0;
            }else
            {
                if(hasNewTarget)
                {
                    curTarget = moveTarget;
                    hasNewTarget = false;
                    goto StartMove;
                }else
                {
                    stopMove = false;
                    inMove = false;
                    hasNewTarget = false;

                    curObj.DuckInfo.SpeedX = 0;
                    curObj.DuckInfo.SpeedY = 0;
                }
            }

        }
    }
}
