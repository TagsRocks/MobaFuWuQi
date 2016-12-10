using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using SimpleJSON;
using Google.ProtocolBuffers;
using System.Threading;


namespace MyLib
{
	/// <summary>
	/// Gate--->Forward OpenAgent To Somebody 
	/// 单线程程序
	/// </summary>
	public class Agent
	{
		private static uint maxId = 0;
		public uint id;

		Socket mSocket;
		ServerMsgReader msgReader;
		private bool _isClose = false;

		public bool isClose
		{
			get { return _isClose; }
			set { _isClose = value; }
		}

		public Actor actor;
		public WatchDog watchDog;

		public SocketServer server;

		List<MsgBuffer> msgBuffer = new List<MsgBuffer>();
		public EndPoint ep;
		private byte[] mTemp = new byte[0x2000];

		private ulong mReceivePacketCount;
		private ulong mReceivePacketSizeCount;
		private ulong mSendPacketCount;
		private ulong mSendPacketSizeCount;

		public UDPAgent udpAgent;
		private KCPAgent kcpAgent;
		public void SetKCPAgent(KCPAgent kcp)
		{
			kcpAgent = kcp;
		}

		public void SetUDPAgent(UDPAgent ud)
		{
			udpAgent = ud;
		}
		public Agent(Socket socket)
		{
			socket.NoDelay = true;
			id = ++maxId;
			mSocket = socket;
			ep = mSocket.RemoteEndPoint;
			msgReader = new ServerMsgReader();
			msgReader.msgHandle = handleMsg;
			Debug.Log("AgentCreate " + id);

			var ip = socket.RemoteEndPoint as IPEndPoint;
			LogHelper.LogClientLogin(string.Format("ip={0}", ip.Address));
		}

		public void StartReceiving()
		{
			if (mSocket != null && mSocket.Connected && !isClose)
			{
				try
				{
					watchDog.Open(id, this);
					mSocket.BeginReceive(mTemp, 0, mTemp.Length, SocketFlags.None, OnReceive, mSocket);
				}
				catch (Exception exception)
				{
					LogHelper.Log("Agent", exception.ToString());
					Close();
				}
			}
		}

		public void handleMsg(KBEngine.Packet packet)
		{
			if (actor != null)
			{
				actor.SendMsg(packet);
			}
			var proto = packet.protoBody as CGPlayerCmd;
			var cmd = proto.Cmd;
			var size = 2 + packet.msglen;
			mReceivePacketCount += 1;
			mReceivePacketSizeCount += (ulong)size;
			LogHelper.LogReceivePacket(string.Format("cmd={0} size={1}", cmd, size));
		}

		void OnReceive(IAsyncResult result)
		{
			int bytes = 0;
			if (mSocket == null)
			{
				LogHelper.Log("Error", "SocketClosed");
				Close();
				return;
			}
			try
			{
				bytes = mSocket.EndReceive(result);

			}
			catch (Exception exception)
			{
				Debug.LogError(exception.Message);
				Close();
			}
			if (bytes <= 0)
			{
				Debug.LogError("bytes " + bytes);
				Close();
			}
			else {
				//MessageReader
				//BeginReceive
				uint num = (uint)bytes;
				msgReader.process(mTemp, num);
				if (mSocket != null)
				{
					try
					{
						mSocket.BeginReceive(mTemp, 0, mTemp.Length, SocketFlags.None, OnReceive, mSocket);
					}
					catch (Exception exception2)
					{
						Util.Log(exception2.Message);
						Close();
					}
				}
			}
		}

		private int closeReq = 0;
		public void Close()
		{
			if (Interlocked.Increment(ref closeReq) != 1)
			{
				return;
			}

			if (isClose)
			{
				return;
			}
			isClose = true;

			LogHelper.Log("Agent", "CloseAgent");
			if (mSocket != null && mSocket.Connected)
			{
				Debug.LogError("CloseSocket");
				try
				{
					mSocket.Shutdown(SocketShutdown.Both);
					mSocket.Close();
				}
				catch (Exception exception)
				{
					Debug.LogError(Util.FlattenException(exception));
					//Util.PrintStackTrace();
				}
			}
			mSocket = null;

			if (actor != null)
			{
				actor.SendMsg(string.Format("close"));
			}

			watchDog.Close(id);
			if (mSocket != null)
			{
				var ip = mSocket.RemoteEndPoint as IPEndPoint;
				LogHelper.LogClientLogout(string.Format("ip={0}", ip.Address));
			}
			if (server != null)
			{
				server.RemoveAgent(this);
			}
			if (udpAgent != null)
			{
				udpAgent.Close();
			}
			if (kcpAgent != null)
			{
				kcpAgent.Close();
			}
		}

		public void SendUDPBytes(byte[] bytes)
		{
			if (udpAgent != null && useUDP)
			{
				mSendPacketCount++;
				mSendPacketSizeCount += (ulong)bytes.Length;
				udpAgent.SendBytes(bytes);
			}
			else
			{
				SendBytes(bytes);
			}
		}

		public void SendUDPPacket(IBuilderLite retpb, byte flowId, byte errorCode)
		{
			if (udpAgent != null && useUDP)
			{
				udpAgent.SendPacket(retpb);
			}
			else
			{
				SendPacket(retpb, flowId, errorCode);
			}
		}

		public void ForceUDP(IBuilderLite retpb, byte flowId, byte errorCode)
		{
			if (udpAgent != null)
			{
				udpAgent.SendPacket(retpb);
			}
		}
		public void ForceKCP(IBuilderLite retpb, byte flowId, byte errorCode)
		{
			if (kcpAgent != null)
			{
				ServerBundle bundle;
				var bytes = ServerBundle.sendImmediateError(retpb, 0, 0, out bundle);
				ServerBundle.ReturnBundle(bundle);
				kcpAgent.SendData(bytes);
			}
		}
		public bool useUDP = false;
		public void UseUDP()
		{
			if (!lostYet)
			{
				useUDP = true;
			}
		}

		public bool useKCP = false;
		public void UseKCP()
		{
			useKCP = true;
		}

		/// <summary>
		/// KCP连接一旦断开 需要重发一些数据给客户端
		/// 通过TCP发送数据 reInit 协议重新初始化
		/// 跟随初始化数据
		/// </summary>
		public void KCPLost()
		{
			useKCP = false;
		}

		private bool lostYet = false;
		public void UDPLost()
		{
			lostYet = true;
			useUDP = false;
		}

		public void SendBytes(byte[] bytes)
		{
			mSendPacketCount += 1;
			mSendPacketSizeCount += (ulong)bytes.Length;

			var mb = new MsgBuffer() { position = 0, buffer = bytes, bundle = null };
			var send = false;
			lock (msgBuffer)
			{
				msgBuffer.Add(mb);
				if (msgBuffer.Count == 1)
				{
					send = true;
				}
			}
			if (send)
			{
				try
				{
					mSocket.BeginSend(mb.buffer, mb.position, mb.Size, SocketFlags.None, OnSend, null);
				}
				catch (Exception exception)
				{
					Debug.LogError(exception.Message);
					Close();
				}
			}
		}

		/// <summary>
		/// 内部Actor将Agent要发送的消息推送给客户端 
		/// SendPacket 应该以SendBuff行驶发送
		/// 同一个Socket的Write Read只能加入一次 epoll 
		/// Read在初始化的时候加入
		/// Write在每次要写入的时候加入
		/// </summary>
		public void SendPacket(IBuilderLite retpb, byte flowId, byte errorCode)
		{
			if (isClose)
			{
				return;
			}

			var proto = retpb as GCPlayerCmd.Builder;
			var result = proto.Result;
			ServerBundle bundle;
			var bytes = ServerBundle.sendImmediateError(retpb, flowId, errorCode, out bundle);
			//Debug.Log ("SendBytes: " + bytes.Length);
			mSendPacketCount += 1;
			mSendPacketSizeCount += (ulong)bytes.Length;
			LogHelper.LogSendPacket(string.Format("actor={0} result={1} size={2}", id, result, bytes.Length));

			var mb = new MsgBuffer() { position = 0, buffer = bytes, bundle = bundle };
			var send = false;
			lock (msgBuffer)
			{
				msgBuffer.Add(mb);
				if (msgBuffer.Count == 1)
				{
					send = true;
				}
			}
			if (send)
			{
				try
				{
					mSocket.BeginSend(mb.buffer, mb.position, mb.Size, SocketFlags.None, OnSend, null);
				}
				catch (Exception exception)
				{
					Debug.LogError(exception.Message);
					Close();
				}
			}
		}

		private void OnSend(IAsyncResult result)
		{
			int num = 0;
			try
			{
				num = mSocket.EndSend(result);
			}
			catch (Exception exception)
			{
				num = 0;
				Close();
				Debug.LogError(exception.Message);
				return;
			}

			if (mSocket != null && mSocket.Connected)
			{
				MsgBuffer mb = null;
				lock (msgReader)
				{
					mb = msgBuffer[0];
				}
				MsgBuffer nextBuffer = null;
				if (mb.Size == num)
				{
					lock (msgBuffer)
					{
						msgBuffer.RemoveAt(0);
						if (msgBuffer.Count > 0)
						{
							nextBuffer = msgBuffer[0];
						}
					}
					ServerBundle.ReturnBundle(mb.bundle);
				}
				else if (mb.Size > num)
				{
					mb.position += num;
					nextBuffer = msgBuffer[0];
				}
				else
				{
					ServerBundle.ReturnBundle(mb.bundle);
					lock (msgBuffer)
					{
						msgBuffer.RemoveAt(0);
						if (msgBuffer.Count > 0)
						{
							nextBuffer = msgBuffer[0];
						}
					}
				}

				if (nextBuffer != null)
				{
					try
					{
						mSocket.BeginSend(nextBuffer.buffer, nextBuffer.position, nextBuffer.Size, SocketFlags.None,
							new AsyncCallback(OnSend), null);
					}
					catch (Exception exception)
					{
						Debug.LogError(exception.Message);
						Close();
					}
				}

			}
			else
			{
				Close();
			}
		}

		public JSONClass GetJsonStatus()
		{
			var sj = new SimpleJSON.JSONClass();

			var jsonObj = new JSONClass();
			jsonObj.Add("id", new JSONData(id));
			if (mSocket != null)
			{
				var ip = mSocket.RemoteEndPoint as IPEndPoint;
				jsonObj.Add("ip", new JSONData(ip.ToString()));
				jsonObj.Add("Active", new JSONData("true"));
				jsonObj.Add("ReceivePackets", new JSONData(mReceivePacketCount));
				jsonObj.Add("ReceivePacketsSize", new JSONData(mReceivePacketSizeCount));
				jsonObj.Add("SendPackets", new JSONData(mSendPacketCount));
				jsonObj.Add("SendPacketsSize", new JSONData(mSendPacketSizeCount));
				jsonObj.Add("MsgQueueLength", new JSONData(msgBuffer.Count));
			}
			else
			{
				jsonObj.Add("Active", new JSONData("false"));
			}

			sj.Add("Agent", jsonObj);
			return sj;
		}
	}

}
