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
	public class MainClass
	{
        /// <summary>
        /// 每个逻辑帧时间
        /// </summary>
		public static float syncFreq
        {
            get
            {
                return Util.FrameSecTime;
            }
        }
        /// <summary>
        /// ms 毫秒 时间
        /// </summary>
        public static int syncTime
        {
            get
            {
                return Util.FrameMSTime;
            }
        }

		public static void Main (string[] args)
		{
            Util.startTime = Util.GetTimeNow();

            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

			//Console.WriteLine ("StartServer");
            LogHelper.Log("Server", "StartServer");
            NpcDataManager.Instance.Init();
            SkillDataManager.Instance.Init();

			Console.CancelKeyPress += new ConsoleCancelEventHandler (myHandler);
			var sg = new SaveGame ();
            var config = new ServerConfig();
			var am = new ActorManager ();
			var dog = new WatchDog ();
			am.AddActor (dog, true);

			var lobby = new Lobby ();
			am.AddActor (lobby, true);

            Debug.Log("Args: "+args.Length);
            /*
            if (args.Length > 0) {
				var sync = System.Convert.ToSingle (args [0]);
				syncFreq = sync;
                Debug.Log("SyncTime: "+syncFreq);
			}
            */
            
			//syncTime = (int)(MainClass.syncFreq * 1000);
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
