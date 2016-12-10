using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Web.Security;
using Google.ProtocolBuffers;
using System.Diagnostics;
using System.Text;
using log4net;
using SimpleJSON;
using SocketServer.Game;

namespace MyLib
{
	public class MsgBuffer
	{
		public int position = 0;
		public System.Byte[] buffer;
		public ServerBundle bundle;
		public IPEndPoint remoteEnd;

		public int Size
		{
			get
			{
				return buffer.Length - position;
			}
		}
	}



	/// <summary>
	/// Socket服务器
	/// EventLoop 启动
	/// 分发 确保在SocketServer所在的线程安全么？
	/// 避免线程全部使用Message投递机制 Actor的Message
	/// 
	/// Actor 要比较简单的调用另外一个Actor的方法
	/// 将方法调用转化为Message发送
	///     隐藏：PushMessage和HandlerMsg的代码
	/// 
	/// HandlerMsg可以在类初始化的时候构建Method到Msg映射
	/// 通过Attribute调用方法的时候自动调用SendMsg方法 最后再调用实际的方法
	/// </summary>
	public class SocketServer : Actor
	{
		TcpListener mListener;
		int mListenerPort;

		public Thread mThread;
		public Dictionary<uint, Agent> agents = new Dictionary<uint, Agent>();
		WatchDog dog;

		public bool AcceptConnnection = true;

		private UdpClient udpClient;

		private KCPServer kcpServer;

		public int AgentCount
		{
			get
			{
				var count = 0;
				lock (agents)
				{
					count = agents.Count;
				}
				return count;
			}
		}

		public bool Start(int tcpPort)
		{
			LogHelper.Log("Server", "ServerPort: " + tcpPort);
			try
			{
				mListenerPort = tcpPort;
				mListener = new TcpListener(IPAddress.Any, tcpPort);
				mListener.Server.NoDelay = true;
				mListener.Start(50);
			}
			catch (Exception exception)
			{
				//Util.Log (exception.Message);
				LogHelper.Log("Error", exception.Message);
				return false;
			}

			var udpPort = ServerConfig.instance.configMap["UDPPort"].AsInt;
			LogHelper.Log("UDP", "UDPStart: " + udpPort);
			remoteUDPPort = new IPEndPoint(IPAddress.Any, udpPort);
			udpClient = new UdpClient(remoteUDPPort);
			udpClient.BeginReceive(OnReceiveUDP, null);

			kcpServer = new KCPServer();
			ActorManager.Instance.AddActor(kcpServer, true);
			kcpServer.Start(6060);

			dog = ActorManager.Instance.GetActor<WatchDog>();
			//Debug.Log ("GetWatchDog " + dog);
			LogHelper.Log("Actor", "ServerStartSuc");
			mThread = new Thread(new ThreadStart(this.ThreadFunction));
			mThread.Start();
			return true;
		}

		private Dictionary<IPEndPoint, UDPAgent> udpAgents = new Dictionary<IPEndPoint, UDPAgent>();

		private IPEndPoint remoteUDPPort;
		private void OnReceiveUDP(IAsyncResult result)
		{
			if (udpClient == null)
			{
				return;
			}
			try
			{
				var udpPort = new IPEndPoint(IPAddress.Any, 0);
				var bytes = udpClient.EndReceive(result, ref udpPort);
				if (bytes.Length > 0)
				{
					UDPAgent ag1 = null;
					lock (udpAgents)
					{
						//远程客户端不支持UDP连接 网络无法连接上 UDP穿透失败
						if (!udpAgents.ContainsKey(udpPort))
						{
							var ag = new UDPAgent(udpPort, this, udpClient);
							udpAgents.Add(udpPort, ag);
						}
						ag1 = udpAgents[udpPort];
					}
					if (ag1 != null)
					{
						ag1.ReceiveData(bytes);
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

		private Queue<MsgBuffer> msgBuffers = new Queue<MsgBuffer>();
		public void SendUDPPacket(MsgBuffer mb)
		{
			var send = false;
			lock (msgBuffers)
			{
				msgBuffers.Enqueue(mb);
				if (msgBuffers.Count == 1)
				{
					send = true;
				}
			}
			if (send)
			{
				try
				{
					udpClient.BeginSend(mb.buffer, mb.buffer.Length, mb.remoteEnd, OnSend, null);
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
			lock (msgBuffers)
			{
				if (msgBuffers.Count > 0)
				{
					msgBuffers.Dequeue();
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
				MsgBuffer nextBuffer = null;
				lock (msgBuffers)
				{
					if (!error)
					{
						msgBuffers.Dequeue();
					}
					if (msgBuffers.Count > 0)
					{
						nextBuffer = msgBuffers.Peek();
					}
				}

				if (nextBuffer != null)
				{
					try
					{
						udpClient.BeginSend(nextBuffer.buffer, nextBuffer.buffer.Length, nextBuffer.remoteEnd, OnSend, null);
					}
					catch (Exception exp)
					{
						LogHelper.Log("UDP", exp.ToString());
						DequeueMsg();
					}
				}
			}
		}



		void AddAgent(Socket socket)
		{
			var item = new Agent(socket);
			item.server = this;
			item.watchDog = dog;
			lock (agents)
			{
				agents.Add(item.id, item);
			}
			item.StartReceiving();
		}

		public void RemoveAgent(Agent agent)
		{
			lock (agents)
			{
				agents.Remove(agent.id);
			}
			//RemoveUdpAgent(agent.udpAgent);
		}

		public void RemoveUdpAgent(UDPAgent agent)
		{
			lock (udpAgents)
			{
				udpAgents.Remove(agent.remoteEnd);
			}
		}

		public Agent GetAgent(uint agentId)
		{
			Agent agent = null;
			lock (agents)
			{
				var ok = agents.TryGetValue(agentId, out agent);
			}
			return agent;
		}

		private void AcceptCallback(IAsyncResult result)
		{
			try
			{
				var listener = (TcpListener)result.AsyncState;
				var socket = listener.EndAcceptSocket(result);
				AddAgent(socket);
				listener.BeginAcceptSocket(new AsyncCallback(AcceptCallback), listener);
			}
			catch (Exception exp)
			{
				LogHelper.Log("Error", "Accept SocketError: " + exp.ToString());
			}
		}
		private ManualResetEvent signal = new ManualResetEvent(false);

		void ThreadFunction()
		{
			mListener.BeginAcceptSocket(new AsyncCallback(AcceptCallback), mListener);
			signal.WaitOne();
			/*
			while (!isStop) {
				if (this.mListener != null && mListener.Pending ())
				{
					var socket = mListener.AcceptSocket ();
					AddAgent (socket);
				}
				Thread.Sleep (1);
			}
             */
		}

		public override void Stop()
		{
			base.Stop();
			signal.Set();
		}

		public override string ToString()
		{
			var sj = new SimpleJSON.JSONClass();

			var jsonObj = new JSONClass();
			jsonObj.Add("AgentCount", new JSONData(AgentCount));

			var jsonArray = new JSONArray();
			lock (agents)
			{
				foreach (var agent in agents)
				{
					jsonArray.Add("Agent", agent.Value.GetJsonStatus());
				}
			}
			jsonObj.Add("Agents", jsonArray);
			sj.Add("AgentStatus", jsonObj);
			return sj.ToString();
		}
	}
}
