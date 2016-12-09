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

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config",Watch = true)]
namespace MyLib
{
	public class MsgBuffer
	{
		public int position = 0;
		public System.Byte[] buffer;
	    public ServerBundle bundle;
	    public IPEndPoint remoteEnd;

		public int Size {
			get {
				return buffer.Length - position;
			}
		}
	}

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
            set { _isClose = true; }
        }

		public Actor actor;
		public WatchDog watchDog;

	    public SocketServer server;

		List<MsgBuffer> msgBuffer = new List<MsgBuffer> ();
		public EndPoint ep;
		private byte[] mTemp = new byte[0x2000];

	    private ulong mReceivePacketCount;
	    private ulong mReceivePacketSizeCount;
	    private ulong mSendPacketCount;
	    private ulong mSendPacketSizeCount;

	    public UDPAgent udpAgent;

	    public void SetUDPAgent(UDPAgent ud)
	    {
	        udpAgent = ud;
	    }
		public Agent (Socket socket)
		{
		    socket.NoDelay = true;
			id = ++maxId;
			mSocket = socket;
			ep = mSocket.RemoteEndPoint;
			msgReader = new ServerMsgReader ();
			msgReader.msgHandle = handleMsg;
			Debug.Log ("AgentCreate " + id);

		    var ip = socket.RemoteEndPoint as IPEndPoint;
		    LogHelper.LogClientLogin(string.Format("ip={0}",ip.Address));
		}

		public async void StartReceiving ()
		{
			if (mSocket != null && mSocket.Connected && !isClose) {
				try {
					watchDog.Open(id, this);
					mSocket.BeginReceive (mTemp, 0, mTemp.Length, SocketFlags.None, OnReceive, mSocket);
				} catch (Exception exception) {
					LogHelper.Log("Agent", exception.ToString());
					Close ();
				}
			}
		}

		public void handleMsg (KBEngine.Packet packet)
		{
			if (actor != null) {
				actor.SendMsg (packet);
			}
            var proto = packet.protoBody as CGPlayerCmd;
		    var cmd = proto.Cmd;
		    var size = 2 + packet.msglen;
		    mReceivePacketCount += 1;
		    mReceivePacketSizeCount += (ulong)size;
            LogHelper.LogReceivePacket(string.Format("cmd={0} size={1}",cmd,size));
		}

		void OnReceive (IAsyncResult result)
		{
			int bytes = 0;
		    if (mSocket == null)
		    {
                LogHelper.Log("Error", "SocketClosed");
                Close();
		        return;
		    }
			try {
				bytes = mSocket.EndReceive (result);

			} catch (Exception exception) {
				Debug.LogError (exception.Message);
				Close ();
			}
			if (bytes <= 0) {
				Debug.LogError ("bytes " + bytes);
				Close ();
			} else {
				//MessageReader
				//BeginReceive
				uint num = (uint)bytes;
				msgReader.process (mTemp, num);
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
		public void Close ()
		{
		    if (Interlocked.Increment(ref closeReq) != 1)
		    {
		        return;
		    }

			if (isClose) {
				return;
			}
			isClose = true;

            LogHelper.Log("Agent", "CloseAgent");
			if (mSocket != null && mSocket.Connected) {
				Debug.LogError ("CloseSocket");
				try {
					mSocket.Shutdown (SocketShutdown.Both);
					mSocket.Close ();
				} catch (Exception exception) {
					Debug.LogError (Util.FlattenException (exception));
					//Util.PrintStackTrace();
				}
			}
			mSocket = null;

			if (actor != null) {
				actor.SendMsg (string.Format ("close"));
			} 
			
			watchDog.Close(id);
		    if (mSocket != null)
		    {
		        var ip = mSocket.RemoteEndPoint as IPEndPoint;
		        LogHelper.LogClientLogout(string.Format("ip={0}", ip.Address));
		    }
		    if (server != null) {
				server.RemoveAgent (this);
			}
		    if (udpAgent != null)
		    {
		        udpAgent.Close();
		        //udpAgent = null;
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

	    public bool useUDP = false;
	    public void UseUDP()
	    {
	        if (!lostYet)
	        {
	            useUDP = true;
	        }
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
	        mSendPacketSizeCount += (ulong) bytes.Length;

	        var mb = new MsgBuffer() {position = 0, buffer = bytes, bundle = null};
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
	        mSendPacketSizeCount += (ulong) bytes.Length;
	        LogHelper.LogSendPacket(string.Format("actor={0} result={1} size={2}", id, result, bytes.Length));

	        var mb = new MsgBuffer() {position = 0, buffer = bytes, bundle = bundle};
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
            jsonObj.Add("id",new JSONData(id));
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

	        sj.Add("Agent",jsonObj);
	        return sj;
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
		public Dictionary<uint, Agent> agents = new Dictionary<uint, Agent> ();
		WatchDog dog;

	    public bool AcceptConnnection = true;

	    private UdpClient udpClient;

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

		public bool  Start (int tcpPort)
		{
            LogHelper.Log("Server", "ServerPort: "+tcpPort);
			try {
				mListenerPort = tcpPort;
				mListener = new TcpListener (IPAddress.Any, tcpPort);
			    mListener.Server.NoDelay = true;
				mListener.Start (50);
			} catch (Exception exception) {
				//Util.Log (exception.Message);
                LogHelper.Log("Error", exception.Message);
				return false;
			}

		    var udpPort =ServerConfig.instance.configMap["UDPPort"].AsInt;
            LogHelper.Log("UDP", "UDPStart: "+udpPort);
            remoteUDPPort = new IPEndPoint(IPAddress.Any, udpPort);
            udpClient = new UdpClient(remoteUDPPort);
		    udpClient.BeginReceive(OnReceiveUDP, null);

			dog = ActorManager.Instance.GetActor<WatchDog> ();
			//Debug.Log ("GetWatchDog " + dog);
            LogHelper.Log("Actor", "ServerStartSuc");
			mThread = new Thread (new ThreadStart (this.ThreadFunction));
			mThread.Start ();
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



	    void AddAgent (Socket socket)
		{
			var item = new Agent (socket);
		    item.server = this;
			item.watchDog = dog;
			lock (agents) {
				agents.Add (item.id, item);
			}
			item.StartReceiving ();
		}

		public void RemoveAgent (Agent agent)
		{
			lock (agents) {
                agents.Remove (agent.id);
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

		public Agent GetAgent (uint agentId)
		{
			Agent agent = null;
			lock (agents) {
				var ok = agents.TryGetValue (agentId, out agent);
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
	            LogHelper.Log("Error", "Accept SocketError: "+exp.ToString());
	        } 
	    }
        private ManualResetEvent signal = new ManualResetEvent(false);

		void ThreadFunction ()
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
            jsonObj.Add("AgentCount",new JSONData(AgentCount));

            var jsonArray = new JSONArray();
            lock (agents)
            {         
                foreach (var agent in agents)
                {
                    jsonArray.Add("Agent",agent.Value.GetJsonStatus());
                }
            }
            jsonObj.Add("Agents", jsonArray);
            sj.Add("AgentStatus", jsonObj);
            return sj.ToString();
        }
    }

	public class MainClass
	{
		public static float syncFreq = 0.1f;

		public static void Main (string[] args)
		{
			//Console.WriteLine ("StartServer");
            LogHelper.Log("Server", "StartServer");
			Console.CancelKeyPress += new ConsoleCancelEventHandler (myHandler);
			var sg = new SaveGame ();
            var config = new ServerConfig();
			var am = new ActorManager ();
			var dog = new WatchDog ();
			am.AddActor (dog, true);

			var lobby = new Lobby ();
			am.AddActor (lobby, true);

            Debug.Log("Args: "+args.Length);
			if (args.Length > 0) {
				var sync = System.Convert.ToSingle (args [0]);
				syncFreq = sync;
                Debug.Log("SyncTime: "+syncFreq);
			}

			var ss = new SocketServer ();
			am.AddActor (ss, true);

            AppDomain.CurrentDomain.UnhandledException += UnhandleExcepition;
		    TaskScheduler.UnobservedTaskException += (sender, eventArgs) =>
		    {
                eventArgs.SetObserved();
		        var error = eventArgs.Exception;
                LogHelper.LogUnhandleException(sender.ToString() +"  " +error.ToString());
		    };

            var monitor = new MonitorActor();
		    am.AddActor(monitor, true);

            var httpServer = new HttpServerActor();
		    am.AddActor(httpServer, true);

		    var chat = new ChatActor();
		    am.AddActor(chat, true);

		    if (ServerConfig.instance.configMap["IsMaster"].AsBool)
		    {
		        var masterActor = new MasterServerActor();
		        am.AddActor(masterActor, true);
		    }

            var slaveActor = new SlaveServerActor();
            am.AddActor(slaveActor, true);

		    //var tp = new TestPhysic();
		    //am.AddActor(tp);
            //初始化物理系统的静态数据
            Contact.InitializeRegister();

		    var port = ServerConfig.instance.configMap["Port"].AsInt; 
			ss.Start (port);
			ss.mThread.Join ();
            GC.Collect();
			Console.WriteLine ("EndServer");
		}

		static void myHandler (object sender, ConsoleCancelEventArgs args)
		{
			Debug.Log ("ServerStop");
			ActorManager.Instance.Stop ();
		}
	    static void UnhandleExcepition(object sender, UnhandledExceptionEventArgs e)
	    {
	        var error = e.ExceptionObject as Exception;
            LogHelper.LogUnhandleException(sender.ToString() + "  " + error.ToString());

            MailSender.SendMail(error.ToString());
	    }
	}
}
