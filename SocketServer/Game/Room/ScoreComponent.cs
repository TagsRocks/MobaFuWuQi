using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    public class ScoreComponent : Component
    {
        private RoomActor room;
        private int leftTime = GameConst.TotalLeftTime;
        public override void Init()
        {
            base.Init();
            room = this.actor as RoomActor;
            this.actor.RunTask(CountDown);
        }

        public int GetLeftTime()
        {
            return leftTime;
        }

        public async Task CountDown()
        {
			var ct = 1000;
            while (!this.actor.IsStop())
            {
                var gc = GCPlayerCmd.CreateBuilder();
                gc.Result = "SyncTime";
                gc.LeftTime = leftTime;
                room.AddCmd(gc);
				await Task.Delay (ct);
                leftTime--;
                leftTime = Math.Max(0, leftTime);
                if (leftTime == 0)
                {
                    //var gc2 = GCPlayerCmd.CreateBuilder();
                    //gc2.Result = "GameOver";
                    room.GameOver();
                    //room.AddCmd(gc2);
                }
                if (room.GetState() == RoomActor.RoomState.GameOver)
                {
                    leftTime = 0;
                }
            }
        }

        public override void Destroy()
        {
            base.Destroy();
        }
    }
}
