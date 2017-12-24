using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using SimpleJSON;

namespace MyLib
{
	public class RoomActor : Actor
	{
		private PlayerManagerCom playerCom;
		private EntityManagerCom entityCom;
		TeamManageCom teamCom;
	    private bool IsNewUserRoom = false;
        public GridManager gridManager;

        private ulong frameId = 0;
        private double roomStartTime = 0;
        private double newTimeNow = 0;

        /// <summary>
        /// 获取当前服务器上的时间
        /// </summary>
        /// <returns></returns>
        public ulong GetFrameId()
        {
            return frameId;
        }

        /// <summary>
        /// 带小数点的服务器上FrameTime
        /// </summary>
        /// <returns></returns>
        public float GetRealFrameTime()
        {
            var passTime = Util.GetTimeSinceServerStart();
            var deltaTime = passTime - roomStartTime;
            var realFrameId =  deltaTime / MainClass.syncFreq;
            return (float)realFrameId;
        }

        /// <summary>
        /// 获得房间已经开始的时间长度 秒
        /// </summary>
        /// <returns></returns>
        public float GetRoomTimeNow()
        {
            if (roomStartTime > 0)
            {
                var passTime = Util.GetTimeSinceServerStart();
                var deltaTime = passTime - roomStartTime;
                return (float)deltaTime;
            }else
            {
                return 0;
            }
        }

	    public bool IsNewUser()
	    {
	        return IsNewUserRoom;
	    }
		public enum RoomState
		{
            Ready, //创建房间
			InGame, //正式游戏
			GameOver, //游戏结束
            SelectHero, //选择英雄
            WaitClientInit, //等待所有客户端初始化地图结束
		}

		private RoomState state = RoomState.Ready;

	    public RoomState GetState()
	    {
	        return state;
	    }
		public int maxPlayerNum = 10;
	    private ScoreComponent score;
	    private RoomInfo roomInfo;

	    public int GetLevelId()
	    {
	        return roomInfo.LevelId;
	    } 
		public RoomActor (int mp, bool newUser, RoomInfo roomInfo)
		{
            curFrameAwaiter = nextFrameAwaiter1;
		    IsNewUserRoom = newUser;
			playerCom = this.AddComponent<PlayerManagerCom> ();
			entityCom = this.AddComponent<EntityManagerCom> ();
			teamCom = this.AddComponent<TeamManageCom> ();
		    score = AddComponent<ScoreComponent>();
		    this.roomInfo = RoomInfo.CreateBuilder(roomInfo).Build();

		}

	    public async Task<int> GetLeftTime()
	    {
	        await _messageQueue;
	        return score.GetLeftTime();
	    }
	
		public async Task RemoveRoom ()
		{
		}

		public override void Init ()
		{
            InitMessageQueue();
            InitPhysic();
            InitEntity();
			RunTask (UpdateWorld);
		}

        private void InitPhysic()
        {
            AddComponent<PhysicManager>();
            gridManager = AddComponent<GridManager>();
            gridManager.InitMap();
        }

        /// <summary>
        /// 玩家客户端进入游戏准备完成
        /// </summary>
        /// <param name="pl"></param>
		public void SetReady (PlayerInRoom pl)
		{
            playerCom.SendAllPlayerTo(pl);
		    entityCom.SendAllEtyTo(pl);
		    var gc = GCPlayerCmd.CreateBuilder();
		    gc.Result = "AllReady";
            pl.SendCmd(gc);
		}

	    private JSONClass jobj;
	    private GameObjectActor entityConfig;
	    public int maxSpawnId = 0;

        /// <summary>
        /// 避免异步调用
        /// </summary>
        /// <returns></returns>
	    public int GetSpawnId()
	    {
	        return maxSpawnId++;
	    }
	    private void InitEntity()
	    {
            Debug.Log("InitEntityNow");
	        maxSpawnId = 0;
            using (var f = new StreamReader(string.Format("ConfigData/level_1_4_{0}.json", roomInfo.LevelId)))
            {
                Debug.Log("InitEntityInfo");
                var con = f.ReadToEnd();
                jobj = JSON.Parse(con).AsObject;
                entityConfig = EntityImport.InitGameObject(jobj);
                entityConfig.SetRoom(this);
                entityConfig.Start();
            }
	    }

        //public Task<int> GetTeamColor ()
        public int GetTeamColor()
		{
			return teamCom.GetTeamColor ();
		}


	    public async void PickItem(CGPlayerCmd cmd)
	    {
	        await this._messageQueue;
	        entityCom.Pick(cmd);
	    }


	    private double[] sampleNum = new double[]
	    {
            0, 0, 0, 0, 0
	    };
	    public double avg = 0;
	    public double SyncPeriod = 0;

        //开始游戏一个房间需要的晚间人数
        private int roomPlayer = 1;
        //将HP状态同步和命令同步放到同一帧来广播
		private async Task UpdateWorld ()
		{
		    while (state == RoomState.Ready)
		    {
                var pNum = playerCom.GetPlayerNum();
                if(pNum >= roomPlayer)
                {
                    break;
                }
                await Task.Delay(1000);
            }

            state = RoomState.SelectHero;
            //倒计时等待玩家选择英雄
            //倒计时结束 根据玩家选择英雄的状态， 没有选择则随机一个英雄给玩家
            if(state == RoomState.SelectHero)
            {
                var gc2 = GCPlayerCmd.CreateBuilder();
                gc2.Result = "SelectHero";
                playerCom.BroadcastToAll(gc2);
                playerCom.RefreshLiveTime();
            }
            //等待玩家选择英雄
            while(state == RoomState.SelectHero)
            {
                if (playerCom.IsAllChoose())
                {
                    break;
                }
                await Task.Delay(1000);
            }
            //通知客户端开始初始化
            {
                score.Init();
                var gc2 = GCPlayerCmd.CreateBuilder();
                gc2.Result = "StartGame";
                playerCom.BroadcastToAll(gc2);
                playerCom.RefreshLiveTime();
            }
            state = RoomState.WaitClientInit;
            while (state == RoomState.WaitClientInit)
            {
                if(playerCom.IsAllClientInit())
                {
                    break;
                }
                await Task.Delay(1000);
            }
            state = RoomState.InGame;

		 

            roomStartTime = Util.GetTimeSinceServerStart();
            frameId = 0;
            var nextFrameTime = roomStartTime;
            var syncTime = MainClass.syncTime;
            var secSyncTime = Util.MSToSec(syncTime);

		    double lastTime = 0.0;
			while (!isStop) {
			    var nextTime = Util.GetTimeSinceServerStart();
			    SyncPeriod = nextTime - lastTime;
			    lastTime = nextTime;

                //先更新状态再广播
                //交换队列 防止执行过程中 添加Awaiter
                var lastAwaiter = curFrameAwaiter;
                if(curFrameAwaiter == nextFrameAwaiter1)
                {
                    curFrameAwaiter = nextFrameAwaiter2;
                }else
                {
                    curFrameAwaiter = nextFrameAwaiter1;
                }
                foreach(var a in lastAwaiter)
                {
                    a.Run();
                }
                lastAwaiter.Clear();


                var gc = GCPlayerCmd.CreateBuilder();
                gc.Result = "SyncFrame";
                gc.FrameId = GetFrameId();
                AddBeforeCmd(gc);

			    foreach (var cmd in beforeCmdList)
			    {
			        playerCom.BroadcastToAll(cmd);
			    }
                beforeCmdList.Clear();

				playerCom.UpdatePlayer ();
				entityCom.UpdateEntity ();
			    foreach (var cmd in cmdList)
			    {
			        playerCom.BroadcastToAll(cmd);
			    }
                cmdList.Clear();

				foreach (var cmd in kcpList)
				{
					playerCom.BroadcastKCPToAll(cmd);
				}
				kcpList.Clear();

                foreach(var cmd in nextFrameCmd)
                {
                    playerCom.BroadcastToAll(cmd);
                }
                nextFrameCmd.Clear();

                newTimeNow = Util.GetTimeSinceServerStart();
                nextFrameTime += secSyncTime;
                frameId++;

                //继续执行
                if(newTimeNow >= (nextFrameTime-0.005) )
                {
                }else//睡眠一段时间
                {
                    var sleepTime = nextFrameTime - newTimeNow;
                    await Task.Delay (Util.TimeToMS(sleepTime));
                }
			}
		}

	    public async Task<bool> ReAddPlayer(PlayerActor pl, int mp)
	    {
	        await this._messageQueue;

            /*
			var num = playerCom.GetPlayerNum ();
			if (state == RoomState.InGame && num < maxPlayerNum) {
				var ainfo = pl.GetAvatarInfo ();
				await AddPlayer (pl, ainfo);
				return true;
			}
            */

			return false;
	    }

	    public override string GetAttr()
	    {
	        return "ID: " + Id;
	    }

	    //匹配阶段
		//小心Task的死锁
		//提高性能就是将异步方法改写成不要锁的同步方法
		public async Task<bool> AddPlayerNew (PlayerActor pl, int mp, bool newUser)
		{
			await this._messageQueue;

			var num = playerCom.GetPlayerNum ();
			if ((state == RoomState.InGame || state == RoomState.Ready) && num < maxPlayerNum) {
                var ainfo = AvatarInfo.CreateBuilder();
                ainfo.Id = pl.Id;
				//var ainfo = await pl.GetAvatarInfo ();
				await AddPlayer (pl, ainfo.Build());
			    //var num1 = playerCom.GetPlayerNum();

                //多人准备进入选择人物界面
                //等待多个人进入游戏
                if (state == RoomState.Ready)
     		    {
                    //state = RoomState.SelectHero;

                }
				return true;
			}
			return false;
		}

        /// <summary>
        /// 确定选择英雄 就进入游戏
        /// </summary>
        /// <returns></returns>
        public void ChooseHero(PlayerInRoom inRoom)
        {
            playerCom.SetChoose(inRoom);
        }

        public void MainTowerBroken(ActorInRoom tower)
        {
            GameOver();
        }

        public async Task GameOverAsync()
        {
            await _messageQueue;
            GameOver();
        }

		public void GameOver ()
		{
		    if (state != RoomState.GameOver)
		    {
		        playerCom.UpdateAllPlayersLevel();
		    }
            state = RoomState.GameOver;

            var gc2 = GCPlayerCmd.CreateBuilder();
            gc2.Result = "GameOver";
            AddCmd(gc2);
        }

		public async Task StartGame ()
		{
			await this._messageQueue;
		}

        public ActorInRoom GetActorInRoom(int idInRoom)
        {
            var ety = entityCom.GetEntity(idInRoom);
            var player = playerCom.GetPlayer(idInRoom);
            if(ety != null)
            {
                return ety;
            }
            return player;
        }

        /// <summary>
        /// 创建玩家在Room内的状态机 proxy代理
        /// </summary>
        /// <param name="player"></param>
        /// <param name="ainfo"></param>
		private async Task AddPlayer (PlayerActor player, AvatarInfo ainfo)
		{
			await playerCom.AddPlayer (player, ainfo);
            //physicWorld.AddPlayer(player);
            /*
            if (!hasMaster) {
				hasMaster = true;
				player.SetMaster ();
			}
            */
		}

        /// <summary>
        /// 当所有玩家离开房间
        /// 或者游戏主动及诶数的时候
        /// </summary>
        /// <param name="player"></param>
        /// <param name="ainfo"></param>
        /// <returns></returns>
		public async Task RemovePlayer (PlayerInRoom player, AvatarInfo ainfo)
		{
			await this._messageQueue;
			Debug.Log ("RemovePlayer: "+ainfo);
			playerCom.RemovePlayer (player, ainfo);
			Debug.Log ("PlayerNum: "+playerCom.GetPlayerNum());
			//游戏过程中或者InGame
			if (state == RoomState.GameOver || state == RoomState.InGame) {
				var num = playerCom.GetPlayerNum ();
				if (num == 0) {
					Debug.LogError ("RemoveRoom: "+this.Id);
					var lb = ActorManager.Instance.GetActor <Lobby> ();
					lb.DeleteRoom (this.Id);
					ActorManager.Instance.RemoveActor (Id);
				}
			}

		}


	    public override void Stop()
	    {
	        base.Stop();
	        if (entityConfig != null)
	        {
	            entityConfig.Destroy();
	        }
	    }

	    public EntityActor AddEntityInfo(EntityInfo info)
	    {
	        //游戏结束不要创建新的Entity了
            if (isStop)
	        {
	            return null;
	        }

	        var entity = new EntityActor();
	        entity.SetRoom(this);
            //TODO: Entity Actor 不需要添加进入全局的ActorManager中
            //本地局部管理
            entity.Id = ActorManager.Instance.GetFreeId();
            entity.Init();
	        entity.InitInfo(info);
	        AddEntity(entity, entity.entityInfo);
            entity.Start();
	        return entity;
        }


	    private void AddEntity (EntityActor actor, EntityInfo info)
		{
			entityCom.Add (actor, info);
		}

        //房间内调用外部不能直接调用隔离
		public void  RemoveEntity (EntityActor actor, EntityInfo info)
		{
			entityCom.Remove (actor, info);
		}

        private List<GCPlayerCmd.Builder> cmdList = new List<GCPlayerCmd.Builder>(); 
        private List<GCPlayerCmd.Builder> beforeCmdList = new List<GCPlayerCmd.Builder>();
		private List<GCPlayerCmd.Builder> kcpList = new List<GCPlayerCmd.Builder>();
        private List<GCPlayerCmd.Builder> nextFrameCmd = new List<GCPlayerCmd.Builder>();
        private List<WaitForNextFrameAwaiter> nextFrameAwaiter1 = new List<WaitForNextFrameAwaiter>();
        private List<WaitForNextFrameAwaiter> nextFrameAwaiter2 = new List<WaitForNextFrameAwaiter>();
        private List<WaitForNextFrameAwaiter> curFrameAwaiter; 
        

        public void AddAwaiter(WaitForNextFrameAwaiter a)
        {
            curFrameAwaiter.Add(a);
        }

        /// <summary>
        /// 在UpdatePlayer和 UpdateEntity之前发送命令
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
	    public void AddBeforeCmd(GCPlayerCmd.Builder cmd)
	    {
            beforeCmdList.Add(cmd);
	    }
		public void AddKCPCmd(GCPlayerCmd.Builder cmd)
		{
			kcpList.Add(cmd);
		}

        /// <summary>
        /// 下一帧率开始执行该命令而不是当前帧
        /// </summary>
        /// <param name="cmd"></param>
        public void AddNextFrameCmd(GCPlayerCmd.Builder cmd, int nextF=1)
        {
            cmd.RunInFrame = (int)GetFrameId() + nextF;
            nextFrameCmd.Add(cmd);
        }

		public void AddCmd (GCPlayerCmd.Builder cmd)
		{
            cmdList.Add(cmd);
			//playerCom.BroadcastToAll (cmd);
		}

        public async Task<JSONClass> GetJsonStatus()
        {
            await this._messageQueue;
            var sj = new SimpleJSON.JSONClass();

            var jsonObj = new JSONClass();
            jsonObj.Add("id", new JSONData(Id));
            jsonObj.Add("PlayerNum", new JSONData(playerCom.GetPlayerNum()));
            jsonObj.Add("State", new JSONData(state.ToString()));
            jsonObj.Add("MaxPlayer", new JSONData(maxPlayerNum));
            jsonObj.Add("MaxSpawnId", new JSONData(maxSpawnId));
            jsonObj.Add("Physic", new JSONData(avg));
            jsonObj.Add("SyncPeriod", new JSONData(SyncPeriod));
            sj.Add("Room", jsonObj);
            return sj;
        }

	    public async Task<bool> IsNeedRobot()
	    {
	        await this._messageQueue;
	        var pn = playerCom.GetPlayerNum();
	        var st = state;
	        var lfTime = score.GetLeftTime();
	        //房间至少4人
	        if (st == RoomState.InGame && lfTime > GameConst.LeftNotEnterTime && pn < 4)
	        {
	            var pl = playerCom.GetPlayers();
	            foreach (var p in pl)
	            {
	                var ainfo = p.GetAvatarInfo();
	                if (!ainfo.IsRobot)
	                {
	                    return true;
	                }
	            }
	        }
	        return false;
	    }

	    public async Task<int> GetPlayerNum()
	    {
	        await this._messageQueue;
	        return playerCom.GetPlayerNum();
	    }

	    public async Task Dead(CGPlayerCmd cmd)
	    {
	        await this._messageQueue;

	        var attackerList = cmd.DamageInfo.AttackerListList;

	        if (attackerList != null)
	        {
                foreach (var attackerId in attackerList)
                {
                    var attacker = playerCom.GetPlayer(attackerId);
                    if (attacker != null)
                    {
                        attacker.AddSecondaryAttack();
                    }
                }
            }

	        var att = playerCom.GetPlayer(cmd.DamageInfo.Attacker);
	        var ene = playerCom.GetPlayer(cmd.DamageInfo.Enemy);
	        if (att != null && ene != null)
	        {
	            var einfo = ene.GetAvatarInfo();
	            att.AddScore(einfo.ContinueKilled, cmd.DamageInfo.Enemy);
	            att.AddKillCount();
                ene.DecScore();
	            ene.AddDeadCount();
	        }
	    }

	    public async Task BroadcastNews(string con)
	    {
	        await this._messageQueue;
	        var gc = GCPlayerCmd.CreateBuilder();
	        gc.Result = "News";
	        gc.News = con;
            playerCom.BroadcastToAll(gc);
	    }


	    public async Task ShowPhysic()
	    {
	        await this._messageQueue;
	        //var tp = new TestPhysic();
	        //physicWorld.Show();

	    }
    }
}

