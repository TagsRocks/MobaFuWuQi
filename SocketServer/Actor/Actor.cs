using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using System.Collections.Concurrent;
using SimpleJSON;


namespace MyLib
{

	public static class ActorUtil
	{
        /*
		public static void SendMsg (this Actor target, string msg)
		{
			var m = new ActorMsg ();
			m.msg = msg;
			target.mailbox.SendAsync (m);
		}

		public static void SendMsg (this Actor target, KBEngine.Packet packet)
		{
			var m = new ActorMsg (){ packet = packet };
			target.mailbox.SendAsync (m);
		}

		public static void SendMsg (this Actor target, ActorMsg msg)
		{
			target.mailbox.SendAsync (msg);
		}
        */
	}

	public class ActorMsg
	{
		public string msg;
		public KBEngine.Packet packet;
		public object obj;
		public object obj1;
	}
	/// <summary>
	/// Actor 的对外不应该提供同步调用的方法 Actor对外的都是
	/// Async的方法 通过同步的SynchronizationContext来同步外部调用 
	/// </summary>
	public class Actor
	{
		protected List<Component> components = new List<Component> ();

        private ActorSynchronizationContext _mq;// = new ActorSynchronizationContext();

	    public virtual ActorSynchronizationContext _messageQueue
	    {
	        get { return _mq; }
        }

		public int Id = -1;

        //public BufferBlock<ActorMsg> mailbox;// = new BufferBlock<ActorMsg> ();
		protected bool isStop = false;

	    public bool IsStop()
	    {
	        return isStop;
	    }

	    public async Task<Component[]> GetComponents()
	    {
	        await _messageQueue;
	        return components.ToArray();
	    } 

		public Actor ()
		{
		}


        public T GetComponent<T> () where T : Component
		{
			foreach (var c in components) {
				if (c is T) {
					return (T)c;
				}
			}
			return null;
		}

		public async Task<T> AddComponentAsync<T> () where T : Component
		{
			await this._messageQueue;
			var c = (T)Activator.CreateInstance (typeof(T));
			components.Add (c);
			c.actor = this;
			return c;
		}

        /// <summary>
        /// 同一个组件只能添加一次
        /// 防止多次添加组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
		public virtual T AddComponent<T> () where T : Component
		{
            var oldCom = GetComponent<T>();
            if(oldCom != null)
            {
                return oldCom;
            }
			var c = (T)Activator.CreateInstance (typeof(T));
			components.Add (c);
			c.actor = this;
            c.AfterAdd();
			return c;
		}

        /*
		/// <summary>
		/// 执行在自己的任务调度器中 
		/// </summary>
		private async Task Dispatch ()
		{
			while (!isStop) {
				var msg = await mailbox.ReceiveAsync ();
				//Console.WriteLine ("threadId receive " + this.GetType () + " id " + Thread.CurrentThread.ManagedThreadId);
				//Console.WriteLine ("receive msg " + msg);
				await ReceiveMsg (msg);
			}

		}
        */

		protected virtual async Task ReceiveMsg (ActorMsg msg)
		{
			await Task.FromResult (default(object));
		}

		/// <summary>
		/// 在当前Actor的Context 下启动一个Task
		/// 这样所有的Task启动，都需要去调用这个Context的Post方法 这样就保证了 Actor内部的Task都是在同一个线程执行的 
		/// </summary>
		/// <param name="cb">Cb.</param>
		public virtual void RunTask (System.Func<Task> cb)
		{
			var surroundContext = SynchronizationContext.Current;
			SynchronizationContext.SetSynchronizationContext (_messageQueue);
			var t =Task.Factory.StartNew (cb,
				CancellationToken.None,
				TaskCreationOptions.DenyChildAttach,
				TaskScheduler.FromCurrentSynchronizationContext ()
			);
			SynchronizationContext.SetSynchronizationContext (surroundContext);
		}

        /// <summary>
        /// 只初始化消息队列
        /// </summary>
        protected void InitMessageQueue()
        {
            _mq = new ActorSynchronizationContext();
        }
		/// <summary>
		/// 启动Dispatch接受消息队列消息
		/// 使用Actor自己的任务调度器
        /// 
        /// Actor 添加进ActorManager时候 调用
        /// GameObjectActor 加入Room时候调用
        /// 
        /// TODO:彻底废弃 mailbox
		/// </summary>
		public virtual void Init ()
		{
            InitMessageQueue();
            //mailbox = new BufferBlock<ActorMsg>();
			//RunTask (Dispatch);
		}

        /// <summary>
        /// Actor 摧毁之后Componet清理
        /// </summary>
		public virtual void Stop ()
		{
			isStop = true;
		    foreach (var component in components)
		    {
		        component.Destroy();
		    }
		}

	    public virtual string GetAttr()
	    {
	        return "id: "+Id;
	    }
	}

	public class ActorManager
	{
		public static ActorManager Instance;
		Dictionary<int, Actor> actorDict;
		Dictionary<Type, Actor> actorType;

		private int actId = 0;
		bool isStop = false;

		public ActorManager ()
		{
			actorDict = new Dictionary<int, Actor> ();
			actorType = new Dictionary<Type, Actor> ();
			Instance = this;
		}

        public int GetFreeId()
        {
            var id = Interlocked.Increment(ref actId);
            return id;
        }

		/// <summary>
		/// 增加Actor将会引起副作用的代码放在锁外面 
		/// </summary>
		/// <returns>The actor.</returns>
		/// <param name="act">Act.</param>
		/// <param name="addType">If set to <c>true</c> add type.</param>
		public int AddActor (Actor act, bool addType = false)
		{
			LogHelper.Log("Actor", "AddActor " + act + " addType " + addType);
			if (isStop) {
				return -1;
			}
			var id = Interlocked.Increment (ref actId);
			lock (actorDict) {
				actorDict.Add (id, act);
				if (addType) {
					actorType.Add (act.GetType (), act);
				}
			}
			act.Id = id;
			act.Init ();
			return id;
		}

	    /// <summary>
	    /// 移除Actor不会调用其它函数避免异常 
	    /// </summary>
	    /// <param name="id">Identifier.</param>
	    public void RemoveActor(int id)
	    {
	        Actor act = null;
	        lock (actorDict)
	        {
	            if (actorDict.ContainsKey(id))
	            {
	                act = actorDict[id];
	                if (actorType.ContainsKey(act.GetType()))
	                {
	                    var act2 = actorType[act.GetType()];
	                    if (act2 == act)
	                    {
	                        actorType.Remove(act.GetType());
	                    }
	                }
	            }
	            actorDict.Remove(id);
	        }
	        if (act != null)
	        {
	            act.Stop();
	        }
	    }

	    public Actor GetActor (int key)
		{
			Actor ret = null;
			lock (actorDict) {
				actorDict.TryGetValue (key, out ret);
			}
			return ret;
		}

		public T GetActor<T> () where T : Actor
		{
			T ret = null;
			lock (actorDict) {
				Actor a = null;
				actorType.TryGetValue (typeof(T), out a);
				ret = (T)a;
			}
			return ret;
		}


		public void Stop ()
		{
			isStop = true;
			lock (actorDict) {
				foreach (var act in actorDict) {
					act.Value.Stop ();
				}
			}
		}

	    public override string ToString()
	    {
	        var sj = new SimpleJSON.JSONClass();

	        var jsonObj = new JSONClass();
	        KeyValuePair<int, Actor>[] ad;
	        lock (actorDict)
	        {
	            ad = actorDict.ToArray();
	        }

	        jsonObj.Add("ActorCount", new JSONData(ad.Length));
	        var jsonArray = new JSONArray();
	        foreach (var actor in ad)
	        {
	            var actorJson = new JSONClass();
	            actorJson.Add("type", new JSONData(actor.Value.GetType().ToString()));
	            var actorComponents = new JSONClass();
                /*
	            foreach (var compoent in actor.Value.GetComponents().Result)
	            {
	                actorComponents.Add("Component", new JSONData(compoent.GetType().ToString()));
	            }
                */
	            actorJson.Add("Components", actorComponents);
	            actorJson.Add("Attribute", actor.Value.GetAttr());
	            jsonArray.Add("Actor", actorJson);
	        }
	        jsonObj.Add("Actors", jsonArray);

	        sj.Add("AtorStatus", jsonObj);
	        return sj.ToString();
	    }
	}
}
