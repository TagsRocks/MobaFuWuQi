using System;
using System.Threading.Tasks;

namespace MyLib
{
	public class WatchDog : Actor
	{
		public void Open (uint agentId, Agent agent)
		{
			//await this._messageQueue;
			var act = new PlayerActor (agentId);
			LogHelper.Log("Agent", "CreateActor " + agentId);
            //如果连接已经关闭就不要把Actor加入到ActorManager 网络断开比较快
		    if (!agent.isClose)
		    {
		        ActorManager.Instance.AddActor(act);
		    }
		}

		public void Close(uint agentId) {
			//await this._messageQueue;
			LogHelper.Log("Agent", "CloseAgent " + agentId);
		}

		protected override async System.Threading.Tasks.Task ReceiveMsg (ActorMsg msg)
		{
            /*
			if (!string.IsNullOrEmpty (msg.msg)) {
				var cmds = msg.msg.Split (' ');
				if (cmds [0] == "open") {
                    var server = ActorManager.Instance.GetActor<MyLib.SocketServer>();
				    if (!server.AcceptConnnection)
				    {
				        return;
				    }
                    var agentId = System.Convert.ToUInt32 (cmds [1]);
					var act = new PlayerActor (agentId);
					Debug.Log ("CreateActor " + agentId);
					ActorManager.Instance.AddActor (act);
				} else if (cmds [0] == "close") {
					var agentId = System.Convert.ToInt32 (cmds [1]);

				}
			}
            */
		}

	}
}

