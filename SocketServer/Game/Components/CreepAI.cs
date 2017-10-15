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
        public float speed = 5;//
        public async Task MoveTo(MyVec3 v3Pos)
        {
            var tarPos = v3Pos.ToFloat();
            var curObj = gameObject as EntityActor;
            var x = curObj.entityInfo.X;
            var y = curObj.entityInfo.Y;
            var z = curObj.entityInfo.Z;

            var curPos = new MyVec3(x, y, z).ToFloat();

            var grid = GetRoom().GetComponent<GridManager>();
            var g1 = grid.mapPosToGrid(curPos);
            var g2 = grid.mapPosToGrid(tarPos);
            var path = grid.FindPath(g1, g2);

            var room = GetRoom();
            var entityInfo = curObj.entityInfo;

            if (path.Count > 0)
            {
                var nextGrid = 0;
                var nextPos = path[0];
                var wp = grid.gridToMapPos(new System.Numerics.Vector2(nextPos.x, nextPos.y));
                wp.Y += 0.1f;

                var moveDir = wp - curPos;
                moveDir.Y = 0;
                moveDir = moveDir / moveDir.Length();
                var syncFreq = MainClass.syncFreq;
                var moveDelta = moveDir * speed * syncFreq;

                var dist = (wp - curPos).Length();
                var totalTime = dist / speed;
                var passTime = 0.0f;
                while (!room.IsStop() )
                {
                    /*
                    var deltaDist = wp - curPos;
                    deltaDist.Y = 0;
                    var len = deltaDist.LengthSquared();
                    if(len < moveDelta*moveDelta)
                    {

                    }
                    */
                    passTime += syncFreq;
                    //var rate = MathUtil.Clamp(passTime / totalTime, 0, 1);
                    //var newPos = Vector3.Lerp(curPos, wp, rate);

                    var newPos = curPos + moveDelta;
                    curPos = newPos;
                    
                    var myPos = MyVec3.FromFloat(newPos.X, newPos.Y, newPos.Z);
                    entityInfo.X = myPos.x;
                    entityInfo.Y = myPos.y;
                    entityInfo.Z = myPos.z;
                    await Task.Delay(MainClass.syncTime);
                }
            }
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
