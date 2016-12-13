using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;
using KBEngine;
using System.Threading;

namespace MyLib
{
	//所有KCPAgent 统一在一个KCPServer中进行Update
	//UDP
	public class KCPAgent
	{
		private IPEndPoint remoteEnd;
		private KCPServer kcpServer;
		private KCP kcp;
		private ServerMsgReader msgReader;

		public IPEndPoint GetPort()
		{
			return remoteEnd;
		}
		//自己和PlayerActor 相互绑定
		//Agent单线程处理上下文 Update函数 一个Agent为一个独立的连接处理线程
		//PlayerActor 类似的行为
		public KCPAgent(IPEndPoint port, KCPServer server)
		{
			msgReader = new ServerMsgReader();
			msgReader.msgHandle = HandleMsg;

			remoteEnd = port;
			kcpServer = server;
			kcp = new KCP();
			kcp.outputFunc = this.SendPacket;
			kcp.closeEventHandler = this.KCPClosed;
		}


		private void KCPClosed()
		{
			if (Interlocked.Increment(ref closeRef) > 1)
			{
				return;
			}
			LogHelper.Log("KCP", "Close: " + remoteEnd);
			if (IsClose)
			{
				return;
			}
			IsClose = true;
			kcpServer.RemoveAgent(this);
			NotifyCloseToAgent();
		}

		//一方断开了停止停止ACK 另外一方过一会也会断开的 1s的反应时间
		//客户端重新建立KCP连接和服务器
		//客户端发送重新KCP初始化的请求
		private async Task NotifyCloseToAgent()
		{
			await this.kcpServer._messageQueue;
			if (agent != null)
			{
				agent.KCPLost();
				var gc = GCPlayerCmd.CreateBuilder();
				gc.Result = "KCPLost";
				agent.SendPacket(gc, 0, 0);

				LogHelper.Log("KCP", "KCPLost From Server");
			}
		}

		//内部server主线程掉哟啊那个
		//UDP--->KCP
		public void ReceiveData(byte[] data)
		{
			kcp.Input(data, data.Length);
		}

		private void SendPacket(KCPPacket pa)
		{
			if (!IsClose)
			{
				pa.remoteEnd = remoteEnd;
				kcpServer.SendUDPPacket(pa);
			}
		}

		private Agent agent;
		private PlayerActor playerActor;

		private void HandleMsg(KBEngine.Packet packet)
		{
			if (agent == null)
			{
				var cg = packet.protoBody as CGPlayerCmd;
				var playerId = cg.AvatarInfo.Id;
				var actor = ActorManager.Instance.GetActor(playerId);
				if (actor != null)
				{
					var ap = actor as PlayerActor;
					if (ap != null)
					{
						var ag = ap.GetAgent();
						ag.SetKCPAgent(this);
						agent = ag;
						playerActor = ap;
					}
				}
			}
			if (agent != null)
			{
				agent.handleMsg(packet);
			}
			else {
				Close();
			}
		}

		private bool IsClose = false;
		public bool CheckClose()
		{
			return IsClose;
		}
		private int closeRef = 0;
		//业务逻辑层控制连接关闭
		public void Close()
		{
			if (Interlocked.Increment(ref closeRef) > 1)
			{
				return;
			}
			LogHelper.Log("KCP", "Close: " + remoteEnd);
			if (IsClose)
			{
				return;
			}
			IsClose = true;
			kcpServer.RemoveAgent(this);
		}

		public void Update(double delta)
		{
			kcp.Update(delta);
			while (true)
			{
				var pack = kcp.Recv();
				if (pack != null)
				{
					msgReader.process(pack, (uint)pack.Length);
				}
				else {
					break;
				}
			}
		}

		/// <summary>
		/// 外部调用
		/// </summary>
		/// <returns>The data.</returns>
		/// <param name="data">Data.</param>
		public async Task SendData(byte[] data)
		{
			await kcpServer._messageQueue;
			if (!IsClose)
			{
				kcp.Send(data);
			}
		}
	}
}

