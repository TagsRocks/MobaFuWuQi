using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace MyLib
{
    //Room FixedUpdate 帧率 Time.deltaTime
    class PlayerMove : MoveState
    {
        private PlayerInRoom me;
        private AINPC aiNpc;
        public override void Init()
        {
            base.Init();
            me = aiCharacter.gameObject as PlayerInRoom;
            aiNpc = aiCharacter.aiNpc;
        }

        public override void EnterState()
        {
            base.EnterState();
        }
        public override async Task RunLogic()
        {
            while(inState)
            {
                var clientPos = me.GetClientVelocity();
                if (Util.IsClientMove(clientPos))
                {
                    //await MoveByNet();
                    MoveSmooth();
                    //await Task.Delay(Util.FrameMSTime);
                    await new WaitForNextFrame(me.GetRoom());
                }else
                {
                    break;
                }
            }
            if (inState)
            {
                aiCharacter.ChangeState(AIStateEnum.IDLE);
            }
        }

        public override void ExitState()
        {
            var avatarInfo = me.GetAvatarInfo();
            avatarInfo.SpeedX = 0;
            avatarInfo.SpeedY = 0;
            base.ExitState();
        }

        //记录两个Update帧的时间间隔
        //缺少统一的时钟管理 乱序执行的Await
        //Logic Update  GameObjectActor Component 调用执行
        private void MoveSmooth()
        {
            var speed = aiNpc.npcConfig.moveSpeed;
            var clientDir = me.GetClientVelocity().ToFloat();
            clientDir = clientDir / clientDir.Length();
            var speedDir = speed* clientDir;
            var curPos = me.GetFloatPos();

            var avatarInfo = me.GetAvatarInfo();
            avatarInfo.SpeedX = Util.RealToNetPos(speedDir.X);
            avatarInfo.SpeedY = Util.RealToNetPos(speedDir.Z);

            //服务器固定Step 更新玩家位置
            var deltaTime = Util.FrameSecTime;
            var newPos = curPos + deltaTime * speedDir;
            me.SetPosWithPhysic(curPos, newPos);
        }

        /*
        private async Task MoveByNet()
        {
            var speed = aiNpc.npcConfig.moveSpeed;

            var tarPos = me.GetPredictClientPos();
            var curPos = me.GetFloatPos();
            var deltaPos = tarPos - curPos;
            deltaPos.Y = 0;
            var dist = deltaPos.Length();
            var totalTime = dist / speed;
            
            var passTime = 0.0f;
            var avatarInfo = me.GetAvatarInfo();
            avatarInfo.SpeedX = Util.RealToNetPos((tarPos.X - curPos.X) / totalTime);
            avatarInfo.SpeedY = Util.RealToNetPos((tarPos.Z - curPos.Z) / totalTime);

            Log.AI("MoveToPos:"+curPos+":"+tarPos);
            while (passTime < totalTime && inState)
            {
                passTime += MainClass.syncFreq;
                var newPos = Vector3.Lerp(curPos, tarPos, MathUtil.Clamp(passTime / totalTime, 0, 1));
                var myPos = MyVec3.FromFloat(newPos.X, newPos.Y, newPos.Z);
                aiNpc.mySelf.SetPos(myPos);
                await Task.Delay(MainClass.syncTime);
            }
            avatarInfo.SpeedX = 0;
            avatarInfo.SpeedY = 0;
        }
        */
    }
}
