using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Security;
using Box2DX.Common;
using Microsoft.SqlServer.Server;
using Math = System.Math;


namespace MyLib
{
	/// <summary>
	/// 交换的数据都是需要不可修改的Message 而内部状态是可修改的
	/// 线程安全数据 
	/// http://www.codeproject.com/Articles/535635/Async-Await-and-the-Generated-StateMachine
	/// </summary>
	public class PlayerActor : Actor
	{
		private uint agentId;
        //和房间内的Player通信的机制，通过这个Proxy来通信
        private PlayerActorProxy inRoomProxy = null;

		Agent agent;

		//上一帧已经广播的Avatar数据
		//private AvatarInfo lastAvatarInfo = null;

		//当前时刻的状态
		//private AvatarInfo avatarInfo;

	    private DeviceInfo deviceInfo;
		//WorldActor world;
		//private RoomActor room;

        /////////////////////////////////////////////////
        private string pid;
	    private string uid;

        ////////////////////////////////////////////////

        public enum State
        {
            OutRoom,
            InRoom,
        }

        private State state = State.OutRoom;

	    private Body body;
	    public Agent GetAgent()
	    {
	        return agent;
	    }

		public PlayerActor (uint id)
		{
			agentId = id;
			var server = ActorManager.Instance.GetActor<SocketServer> ();
		    this.AddComponent<PhysicComponet>();
			agent = server.GetAgent (agentId);
			agent.actor = this;
			Debug.Log ("PlayerActor " + agentId);
			//临时游戏只有一个Room
			//world = ActorManager.Instance.GetActor<WorldActor>();
			//room = ActorManager.Instance.GetActor<RoomActor> ();
			//room = null;
		}

        public async Task SetProxy(PlayerActorProxy proxy)
        {
            await this._messageQueue;
            if(inRoomProxy != null)
            {
                Log.Error("ResetProxy:"+inRoomProxy);
                inRoomProxy = null;
                return;
            }

            inRoomProxy = proxy;
        }

	    public override string GetAttr()
	    {
	        var rid = 0;
            if(inRoomProxy != null)
            {
                rid = inRoomProxy.roomId;
            }
            /*
	        if (avatarInfo != null)
	        {
                return "room: " + rid;// +" : "+ avatarInfo.ToString() + "\n" + lastAvatarInfo.ToString();
	        }
            */
	        return "room: "+rid;
	    }

	   
        public async Task SendPacketAsync(GCPlayerCmd.Builder cmd, byte flowId, byte errorCode)
        {
            await _messageQueue;
            agent.SendPacket(cmd, flowId, errorCode);
        }

        public async Task ForceUDP(GCPlayerCmd.Builder cmd, byte flowId, byte errorCode)
        {
            await _messageQueue;
            agent.ForceUDP(cmd, flowId, errorCode);
        }
        public async Task UseUDP()
        {
            await _messageQueue;
            agent.UseUDP();
        }
        public async Task UDPLost()
        {
            await _messageQueue;
            agent.UDPLost();
        }

		//所有Async方法都应该自动补全这个await 函数 函数修饰器
		public async Task SendCmd (GCPlayerCmd.Builder cmd)
		{
			await _messageQueue;

			agent.SendPacket (cmd, 0, 0); 
		}

	    public async Task SendUDP(GCPlayerCmd.Builder cmd)
	    {
	        await _messageQueue;
            agent.SendUDPPacket(cmd, 0, 0);
	    }

		
      
	    private double GetTimeNow()
	    {
	        return DateTime.UtcNow.Ticks/10000000.0;
	    }

	
        public async Task ConnectCloseAsync()
        {
            await _messageQueue;
            ConnectClose();
        }
	    private void ConnectClose()
	    {
	        ActorManager.Instance.RemoveActor(Id);
	        LogHelper.Log("PlayerActor", "CloseActor " + Id);
            Login.QuitRoom(deviceInfo, (agent.ep as IPEndPoint).Address.ToString() );
	    }

	    private double lastReceiveTime = 0;

        /// <summary>
        /// 登陆准备匹配
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="cmds"></param>
        /// <param name="msg"></param>
        private void HandleLogin(CGPlayerCmd cmd, string[] cmds, ActorMsg msg)
        {
            var ret = GCPlayerCmd.CreateBuilder();
            ret.Result = string.Format("Login {0}", Id);
            deviceInfo = cmd.DeviceInfo;
            pid = deviceInfo.Pid;
            uid = deviceInfo.Uid;
            agent.SendPacket(ret, (byte)msg.packet.flowId, 0);
        }


        private async Task HandleMsg(ActorMsg msg)
        {
            if (!string.IsNullOrEmpty(msg.msg))
            {
                var cmds = msg.msg.Split(' ');
                if (cmds[0] == "close")
                {
                    ConnectClose();
                }
            }
            else
            {
                //LogHelper.Log("PlayerActor", "ReceivePacket " + Id + " p " + msg.packet.protoBody.ToString ());
                if (msg.packet.protoBody.GetType() == typeof(CGPlayerCmd))
                {
                    var cmd = msg.packet.protoBody as CGPlayerCmd;
                    var cmds = cmd.Cmd.Split(' ');
                    var cmd0 = cmds[0];
                    switch (cmd0)
                    {
                        case "Login":
                            HandleLogin(cmd, cmds, msg);
                            break;
                        case "Match":
                            await MatchRoom(msg, cmd, false);
                            break;
                    }
                }
            }
        }


        /// <summary>
        /// 为了保证内部状态的一致性 
        /// 需要对网络Msg进行阻塞处理
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task NetMsg(ActorMsg msg)
        {
            await this._messageQueue;
            lastReceiveTime = GetTimeNow();
            if (state == State.InRoom)
            {
                if (inRoomProxy != null)
                {
                    await inRoomProxy.HandleCmd(msg);
                }
                else
                {
                    Log.Error("NotInRoom State Not Right");
                }
            }
            else if (state == State.OutRoom)
            {
                await HandleMsg(msg);
            }
        }

 

	    private async Task MatchRoom(ActorMsg msg, CGPlayerCmd cmd, bool isNew)
	    {
	        var mp = cmd.RoomInfo.MaxPlayerNum;
	        var lobby = ActorManager.Instance.GetActor<Lobby>();
	        var r = await lobby.FindRoom(this, mp, cmd.RoomInfo, isNew);
            state = State.InRoom;

	        var gc = GCPlayerCmd.CreateBuilder();
	        gc.Result = "Match";

	        var rinfo = RoomInfo.CreateBuilder();
	        rinfo.Id = r.Id;
	        rinfo.MaxPlayerNum = r.maxPlayerNum;
	        rinfo.LevelId = r.GetLevelId();

	        gc.RoomInfo = rinfo.Build();
	        agent.SendPacket(gc, msg.packet.flowId, 0);

	        Login.StartMatch(deviceInfo, (agent.ep as IPEndPoint).Address.ToString());
	    }
	}
}

