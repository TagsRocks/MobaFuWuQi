using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MyLib
{
	/// <summary>
	/// 一个KCP服务器
	/// UDPClient
	/// 多个KCPAgent构成
	/// </summary>
	public class KCPServer : Actor
	{
		private UdpClient udpClient;

		private Queue<KCPPacket> packets = new Queue<KCPPacket>();

		private Dictionary<IPEndPoint, KCPAgent> agents = new Dictionary<IPEndPoint, KCPAgent>();

		private Dictionary<IPEndPoint, KCPAgent> copyAgents = new Dictionary<IPEndPoint, KCPAgent>();

		public KCPServer()
		{
		}
		public void Start(int port)
		{
			LogHelper.Log("KCP", "StartKCP:"+port);
			udpClient = new UdpClient(port);
			udpClient.BeginReceive(OnReceiveUDP, null);
			RunTask(Update);
		}
		private async Task AddAgent(IPEndPoint port, KCPAgent agent)
		{
			await this._messageQueue;
			copyAgents.Add(port, agent);
		}

		//超时 或者主动断开 
		//建立连接需要确认key正确 0 normal 1 ack 2 sync
		public async Task RemoveAgent(KCPAgent agent)
		{
			await this._messageQueue;
			copyAgents.Remove(agent.GetPort());
		}

		//网络本身是多线程的
		//如何隔离多线程和内部状态async 的调用PlayerActor
		private async Task Update()
		{
			var lastTime = Util.GetTimeNow();
			var period = 0.05;//50ms

			while (!isStop)
			{
				var now = Util.GetTimeNow();
				var passTime = now - lastTime;
				lastTime = now;

				foreach (var a in copyAgents)
				{
					a.Value.Update(passTime);
				}

				var cur = Util.GetTimeNow();
				var diff = cur - lastTime;

				if (diff < period)
				{
					await Task.Delay((int)((period-diff)*1000));
				}

			}
		}

		/// <summary>
		/// 只在主线程执行
		/// </summary>
		/// <param name="packet">Packet.</param>
		public void SendUDPPacket(KCPPacket packet)
		{
			var send = false;
			lock (packets)
			{
				packets.Enqueue(packet);
				if (packets.Count == 1)
				{
					send = true;
				}
			}
			if (send)
			{
				try
				{
					udpClient.BeginSend(packet.fullData, packet.fullData.Length, packet.remoteEnd, OnSend, null);
				}
				catch (Exception exp)
				{
					LogHelper.Log("UDP", exp.ToString());
					DequeueMsg();
				}
			}
		}

		private void DequeueMsg()
		{
			lock (packets)
			{
				if (packets.Count > 0)
				{
					packets.Dequeue();
				}
			}
		}

		private void OnSend(IAsyncResult result)
		{
			bool error = false;
			try
			{
				udpClient.EndSend(result);
			}
			catch (Exception exp)
			{
				LogHelper.Log("Error", exp.ToString());
				DequeueMsg();
				error = true;
			}

			if (udpClient != null)
			{
				KCPPacket nextBuffer = null;
				lock (packets)
				{
					if (!error)
					{
						packets.Dequeue();
					}
					if (packets.Count > 0)
					{
						nextBuffer = packets.Peek();
					}
				}

				if (nextBuffer != null)
				{
					try
					{
						udpClient.BeginSend(nextBuffer.fullData, nextBuffer.fullData.Length, nextBuffer.remoteEnd, OnSend, null);
					}
					catch (Exception exp)
					{
						LogHelper.Log("UDP", exp.ToString());
						DequeueMsg();
					}
				}
			}
		}

		private async Task ReceiveData(KCPAgent agent, byte[] data)
		{
			await this._messageQueue;
			agent.ReceiveData(data);
		}

		//所有调用移动到主线程进行
		private void OnReceiveUDP(IAsyncResult result)
		{
			try
			{
				LogHelper.Log("KCP", "KCPReceive:");
				var udpPort = new IPEndPoint(IPAddress.Any, 0);
				var bytes = udpClient.EndReceive(result, ref udpPort);
				if (bytes.Length > 0)
				{
					KCPAgent kcp = null;
					lock (agents)
					{
						//远程客户端不支持UDP连接 网络无法连接上 UDP穿透失败
						if (!agents.ContainsKey(udpPort))
						{
							var ag = new KCPAgent(udpPort, this);
							agents.Add(udpPort, ag);
							AddAgent(udpPort, ag);
						}
						kcp = agents[udpPort];
					}
					if (kcp != null)
					{
						ReceiveData(kcp, bytes);
						//kcp.ReceiveData(bytes);
					}
				}
				else
				{
					LogHelper.Log("UDP", "Error Receive 0");
				}

				udpClient.BeginReceive(OnReceiveUDP, null);
			}
			catch (Exception exp)
			{
				LogHelper.Log("Error", exp.ToString());
			}
		}
	}
}

