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
        public void StopMove()
        {
            if (inMove)
            {
                stopMove = true;
            }
        }
        private AINPC aiNpc;
        private PhysicManager physicManager;
        public override void Init()
        {
            base.Init();
            aiNpc = gameObject.GetComponent<AINPC>();
            physicManager = GetRoom().GetComponent<PhysicManager>();
        }

        /// <summary>
        /// 移动代码中间不能打断
        /// 防止代码重入
        /// 单线程 多协程的时候
        /// </summary>
        /// <param name="v3Pos"></param>
        /// <returns></returns>
        public async Task MoveTo(MyVec3 v3Pos)
        {
            if (inMove)
            {
                curMoveId++;
            }
            var speed = aiNpc.npcConfig.moveSpeed;
            var runMoveId = ++curMoveId;
            inMove = true;
            var tarPos = v3Pos.ToFloat();
            var curObj = gameObject as ActorInRoom;
            //curObj.entityInfo.Speed = Util.GameVecToNet(speed);
            var nowPos = curObj.GetFloatPos();
            var initPos = nowPos;

            var grid = GetRoom().GetComponent<GridManager>();
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

                var dist = (wp - initPos).Length();
                var totalTime = dist / speed;
                var passTime = 0.0f;
                while (passTime < totalTime && !stopMove && runMoveId == curMoveId)
                {
                    passTime += MainClass.syncFreq;
                    var newPos = Vector3.Lerp(initPos, wp, MathUtil.Clamp(passTime / totalTime, 0, 1));
                    var myPos = MyVec3.FromFloat(newPos.X, newPos.Y, newPos.Z);
                    /*
                    entityInfo.X = myPos.x;
                    entityInfo.Y = myPos.y;
                    entityInfo.Z = myPos.z;
                    physicManager.MoveEntity(ref aiNpc.proxy, newPos - nowPos);
                    */

                    aiNpc.mySelf.SetPos(myPos);

                    nowPos = newPos;
                    await Task.Delay(MainClass.syncTime);
                }
                initPos = wp;
                curPoint++;
            }

            if (runMoveId == curMoveId)
            {
                stopMove = false;
                inMove = false;
            }
        }
    }
}
