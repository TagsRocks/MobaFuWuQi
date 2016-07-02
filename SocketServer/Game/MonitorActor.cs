using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    public class MonitorActor : Actor
    {
        public override void Init()
        {
            base.Init();
            RunTask(MonitorServer);
        }

        public int maxPlayer = 0;
        public int lowPlayer = 1000000;
        private async Task MonitorServer()
        {
            var syncTime = 300000;//5min Test 5s
            var day = DateTime.UtcNow.DayOfWeek;

            while (!isStop)
            {
                await Task.Delay(syncTime);
                var server = ActorManager.Instance.GetActor<SocketServer>();
                LogHelper.LogAgentsCount(server.AgentCount.ToString());
                var today = DateTime.UtcNow.DayOfWeek;
                //更新每天
                if (today != day)
                {
                    day = today;
                    maxPlayer = server.AgentCount;
                    lowPlayer = server.AgentCount;
                }
                else
                {
                    maxPlayer = Math.Max(maxPlayer, server.AgentCount);
                    lowPlayer = Math.Min(lowPlayer, server.AgentCount);
                }
                Login.OnlineUser(server.AgentCount);
            }
            GC.Collect();
        }
    }
}
