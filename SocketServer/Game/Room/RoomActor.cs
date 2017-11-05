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
		PlayerManagerCom playerCom;
		public EntityManagerCom entityCom;
		TeamManageCom teamCom;
		private bool hasMaster = false;
	    private bool IsNewUserRoom = false;
        private int entityId = 0;


	    public bool IsNewUser()
	    {
	        return IsNewUserRoom;
	    }
		public enum RoomState
		{
            Ready,
			InGame,
			GameOver,
            SelectHero,
		}

		private RoomState state = RoomState.Ready;

	    public RoomState GetState()
	    {
	        return state;
	    }
		public int maxPlayerNum = 10;
	    private ScoreComponent score;
	    private PhysicWorldComponent physicWorld;
	    private RoomInfo roomInfo;

	    public int GetLevelId()
	    {
	        return roomInfo.LevelId;
	    } 
		public RoomActor (int mp, bool newUser, RoomInfo roomInfo)
		{
		    IsNewUserRoom = newUser;
			playerCom = this.AddComponent<PlayerManagerCom> ();
			entityCom = this.AddComponent<EntityManagerCom> ();
			teamCom = this.AddComponent<TeamManageCom> ();
		    score = AddComponent<ScoreComponent>();
		    this.roomInfo = RoomInfo.CreateBuilder(roomInfo).Build();


		    //physicWorld = AddComponent<PhysicWorldComponent>();
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
			RunTask (UpdateWorld);
		}

		public async Task SetReady (PlayerActor pl)
		{
			await this._messageQueue;
			playerCom.SetReady (pl);
            await playerCom.SendAllPlayerTo(pl);
		    entityCom.SendAllEtyTo(pl);
		    var gc = GCPlayerCmd.CreateBuilder();
		    gc.Result = "AllReady";
            await pl.SendCmd(gc);
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
            using (var f = new StreamReader(string.Format("level_1_4_{0}.json", roomInfo.LevelId)))
            {
                Debug.Log("InitEntityInfo");
                var con = f.ReadToEnd();
                jobj = JSON.Parse(con).AsObject;
                entityConfig = EntityImport.InitGameObject(jobj);
                entityConfig.room = this;
                entityConfig.Start();
            }
	    }

		public Task<int> GetTeamColor ()
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
	    private int MaxCount = 0;
	    public double avg = 0;
        /// <summary>
        /// 房间物理帧率实时刷新
        /// </summary>
        /// <returns></returns>
	    private async Task UpdatePhysic()
	    {
	        var syncTime = 5; //ms 物理模拟
	        var lastTime = Util.GetTimeNow();

	        while (!isStop)
	        {
	            await Task.Delay(syncTime);
	            var endTime = Util.GetTimeNow();
	            var diffTime = endTime - lastTime;
	            sampleNum[MaxCount++] = diffTime;
	            if (MaxCount >= sampleNum.Length)
	            {
	                MaxCount = 0;
	            }
	            avg = sampleNum.Sum()/sampleNum.Length;

                //50ms 帧率
	            var num = (int)( diffTime/0.05f);
	            await playerCom.UpdatePhysic(num);
	            lastTime += num*0.05f;
	        }
	    }

	    public double SyncPeriod = 0;

        //将HP状态同步和命令同步放到同一帧来广播
		private async Task UpdateWorld ()
		{
		    while (state == RoomState.Ready)
		    {
                await Task.Delay(1000);
            }

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
                await Task.Delay(1000);
            }

		    if (state == RoomState.InGame)
		    {
                score.Init();
                var gc2 = GCPlayerCmd.CreateBuilder();
                gc2.Result = "StartGame";
                playerCom.BroadcastToAll(gc2);
		        playerCom.RefreshLiveTime();
		    }
            //RunTask(UpdatePhysic);

            InitEntity();
            //var syncTime = (int)(MainClass.syncFreq * 1000*0.8f);
            var syncTime = MainClass.syncTime;
		    double lastTime = 0.0;
			while (!isStop) {
				Debug.Log ("UpdateTime: " + syncTime);
				await Task.Delay (syncTime);
			    var nextTime = Util.GetTimeNow();
			    SyncPeriod = nextTime - lastTime;
			    lastTime = nextTime;
                Debug.Log ("UpdatePlayer");
			    foreach (var cmd in beforeCmdList)
			    {
			        playerCom.BroadcastToAll(cmd);
			    }
                beforeCmdList.Clear();

				await playerCom.UpdatePlayer ();
				Debug.Log ("UpdateEntity");
				await entityCom.UpdateEntity ();
				Debug.Log ("UpdateFinish");
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
			}
		}

	    public async Task<bool> ReAddPlayer(PlayerActor pl, int mp)
	    {
	        await this._messageQueue;

			var num = playerCom.GetPlayerNum ();
			if (state == RoomState.InGame && num < maxPlayerNum) {
				var ainfo = await pl.GetAvatarInfo ();
				AddPlayer (pl, ainfo);
				return true;
			}
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
		    /*
			if (IsNewUserRoom != newUser)
		    {
		        return false;
		    }
			*/

			var num = playerCom.GetPlayerNum ();
			if ((state == RoomState.InGame || state == RoomState.Ready) && num < maxPlayerNum) {
				var ainfo = await pl.GetAvatarInfo ();
				AddPlayer (pl, ainfo);
			    //var num1 = playerCom.GetPlayerNum();
                //多人准备进入选择人物界面
                if (state == RoomState.Ready)
     		    {
                    state = RoomState.SelectHero;
                }
				return true;
			}
			return false;
		}

        /// <summary>
        /// 确定选择英雄 就进入游戏
        /// </summary>
        /// <returns></returns>
        public async Task ChooseHero()
        {
            await _messageQueue;
            if(state == RoomState.SelectHero)
            {
                state = RoomState.InGame;
            }
        }
		public async Task GameOver ()
		{
			await this._messageQueue;

		    if (state != RoomState.GameOver)
		    {
		        await playerCom.UpdateAllPlayersLevel();
		    }

            state = RoomState.GameOver;
		}

		public async Task StartGame ()
		{
			await this._messageQueue;
		}


		private void AddPlayer (PlayerActor player, AvatarInfo ainfo)
		{
			playerCom.AddPlayer (player, ainfo);
		    //physicWorld.AddPlayer(player);
			if (!hasMaster) {
				hasMaster = true;
				player.SetMaster ();
			}
		}

        /// <summary>
        /// 当所有玩家离开房间
        /// 或者游戏主动及诶数的时候
        /// </summary>
        /// <param name="player"></param>
        /// <param name="ainfo"></param>
        /// <returns></returns>
		public async Task RemovePlayer (PlayerActor player, AvatarInfo ainfo)
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
	        entity.room = this;
            //TODO: Entity Actor 不需要添加进入全局的ActorManager中
            //本地局部管理
            entity.Id = ++entityId;
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

        /// <summary>
        /// 在UpdatePlayer和 UpdateEntity之前发送命令
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
	    public async Task AddBeforeCmd(GCPlayerCmd.Builder cmd)
	    {
	        await this._messageQueue;
            beforeCmdList.Add(cmd);
	    }
		public async Task AddKCPCmd(GCPlayerCmd.Builder cmd)
		{
			await _messageQueue;
			kcpList.Add(cmd);
		}
		public async Task AddCmd (GCPlayerCmd.Builder cmd)
		{
			await _messageQueue;
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
	                var ainfo = await p.GetAvatarInfo();
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
	            //var ainfo = await att.GetAvatarInfo();
	            var einfo = await ene.GetAvatarInfo();
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

