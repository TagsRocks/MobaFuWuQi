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
        //NPC移动速度
        public float speed = 5;
        private bool stopMove = false;
        private bool inMove = false;
        public void StopMove()
        {
            if (inMove)
            {
                stopMove = true;
            }
        }
        /// <summary>
        /// 移动代码中间不能打断
        /// </summary>
        /// <param name="v3Pos"></param>
        /// <returns></returns>
        public async Task MoveTo(MyVec3 v3Pos)
        {
            inMove = true;
            var tarPos = v3Pos.ToFloat();
            var curObj = gameObject as EntityActor;
            var x = curObj.entityInfo.X;
            var y = curObj.entityInfo.Y;
            var z = curObj.entityInfo.Z;
            curObj.entityInfo.Speed = Util.GameToNet(speed);

            var curPos = new MyVec3(x, y, z).ToFloat();

            var grid = GetRoom().GetComponent<GridManager>();
            var g1 = grid.mapPosToGrid(curPos);
            var g2 = grid.mapPosToGrid(tarPos);
            var path = grid.FindPath(g1, g2);
            var nodes = new Vector3[path.Count];
            var i = 0;
            foreach(var p in path)
            {
                var wp = grid.gridToMapPos(new Vector2(p.x, p.y));
                nodes[i++] = wp;
            }

            var room = GetRoom();
            var entityInfo = curObj.entityInfo;

            var curPoint = 1;
            while(curPoint < nodes.Length && !stopMove)
            {
                var nextPos = nodes[curPoint];
                var wp = nextPos;

                var dist = (wp - curPos).Length();
                var totalTime = dist / speed;
                var passTime = 0.0f;
                while(passTime < totalTime && !stopMove)
                {
                    passTime += MainClass.syncFreq;
                    var newPos = Vector3.Lerp( curPos, wp, MathUtil.Clamp(passTime/totalTime, 0, 1));
                    var myPos = MyVec3.FromFloat(newPos.X, newPos.Y, newPos.Z);
                    entityInfo.X = myPos.x;
                    entityInfo.Y = myPos.y;
                    entityInfo.Z = myPos.z;
                    await Task.Delay(MainClass.syncTime);
                }
                curPos = wp;
                curPoint++;
            }
            stopMove = false;
            inMove = false;
        }
    }
    /// <summary>
    /// 由两部分构成
    /// AI状态机 底层
    /// 上层AI决策层
    /// </summary>
    public class CreepAI : GameObjectComponent
    {
        private AICharacter aiCharacter;
        public iTweenPath path;

        public override void Init()
        {
            base.Init();
            gameObject.AddComponent<MoveController>();

            aiCharacter = gameObject.AddComponent<AICharacter>();
            aiCharacter.AddState(new CreepIdle());
            aiCharacter.AddState(new CreepMove());
            aiCharacter.AddState(new CreepAttack());
            //当前所在点
            aiCharacter.blackboard[AIParams.CurrentPoint] = new AIEvent{ intVal = 0 };
        }
        public void RunAI()
        {
            aiCharacter.ChangeState(AIStateEnum.IDLE);
        }
    }
}
