using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;
using KBEngine;

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
		}

		//内部server主线程掉哟啊那个
		public void ReceiveData(byte[] data)
		{
			kcp.Input(data, data.Length);
		}

		private void SendPacket(KCPPacket pa)
		{
			pa.remoteEnd = remoteEnd;
			kcpServer.SendUDPPacket(pa);
		}

		private void HandleMsg(KBEngine.Packet packet)
		{
			
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
			kcp.Send(data);
		}
	}
}

