using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Security;
using Box2DX.Common;
using Microsoft.SqlServer.Server;
using Math = System.Math;


namespace MyLib
{
	/// <summary>
	/// 交换的数据都是需要不可修改的Message 而内部状态是可修改的
	/// 线程安全数据 
	/// http://www.codeproject.com/Articles/535635/Async-Await-and-the-Generated-StateMachine
	/// </summary>
	public class PlayerActor : Actor
	{
		private uint agentId;
		Agent agent;
		//当前时刻的状态
		private AvatarInfo avatarInfo;
		//上一帧更新的Avatar数据
		private AvatarInfo lastAvatarInfo = null;

	    private DeviceInfo deviceInfo;
		//WorldActor world;
		private RoomActor room;

        /////////////////////////////////////////////////
        private string pid;
	    private string uid;

	    public int level = 1;
	    public long Exp = 0;
	    private int medal = 0;
	    private int dayBattleCount = 0;
        ////////////////////////////////////////////////

	    private Body body;
	    public Agent GetAgent()
	    {
	        return agent;
	    }

		public PlayerActor (uint id)
		{
			agentId = id;
			var server = ActorManager.Instance.GetActor<SocketServer> ();
		    this.AddComponent<PhysicComponet>();
			agent = server.GetAgent (agentId);
			agent.actor = this;
			Debug.Log ("PlayerActor " + agentId);
			//临时游戏只有一个Room
			//world = ActorManager.Instance.GetActor<WorldActor>();
			//room = ActorManager.Instance.GetActor<RoomActor> ();
			room = null;
		}


	    public override string GetAttr()
	    {
	        var rid = 0;
	        if (room != null)
	        {
	            rid = room.Id;
	        }
	        if (avatarInfo != null)
	        {
	            return "room: "+rid+" : "+ avatarInfo.ToString() + "\n" + lastAvatarInfo.ToString();
	        }
	        return "room: "+rid;
	    }

	    public async Task<AvatarInfo> GetAvatarInfo ()
		{
			await _messageQueue;
			if (lastAvatarInfo == null) {
				var na = AvatarInfo.CreateBuilder (avatarInfo);
				return na.Build ();
			}
			var na1 = AvatarInfo.CreateBuilder (lastAvatarInfo);
			return na1.Build ();
		}

        //Transform 的位置 和 Rigidbody的位置
	    public async Task SetPos(Vec2 vec)
	    {
	        await this._messageQueue;
	        avatarInfo.X = (int)(vec.X*100);
	        avatarInfo.Z = (int)(vec.Y*100);
	    }

	    public async Task<AvatarInfo.Builder> GetPosInfo()
	    {
	        await this._messageQueue;
			var na1 = AvatarInfo.CreateBuilder ();
	        na1.X = avatarInfo.X;
	        na1.Y = avatarInfo.Y;
	        na1.Z = avatarInfo.Z;
	        na1.Dir = avatarInfo.Dir;
	        na1.SpeedX = avatarInfo.SpeedX;
	        na1.SpeedY = avatarInfo.SpeedY;
	        na1.ResetPos = avatarInfo.ResetPos;
	        avatarInfo.ResetPos = false;
	        return na1;
	    }

	    public async Task UpdateLevel(int rank)
	    {
	        await this._messageQueue;

	        if (level >= 40)
	        {
	            return;
	        }

            ++dayBattleCount;

            var data = GameData.RoleUpgradeConfig[level - 1];
            var baseExp = data.baseExp;
	        var extraExp = data.extraExp;

	        rank = Math.Min(rank, GameData.LevelConfig.Count-1);
	        var rankRatio = GameData.LevelConfig[rank].rankRatio / 100f;

	        dayBattleCount = Math.Min(GameData.LevelConfig.Count-1, dayBattleCount);
            var dayRatio = GameData.LevelConfig[dayBattleCount].dayRatio / 100f;

	        var getExp = baseExp * rankRatio + extraExp * dayRatio ;
	        Exp += (int)getExp;

	        while (Exp >= data.exp && level <= 40)
	        {
	            level++;
	            Exp -= data.exp;
	            data = GameData.RoleUpgradeConfig[level - 1];
	        }

	        if (level >= 40)
	        {
	            Exp = 0;
	        }
	        Login.SaveUserInfo(pid, uid, level, Exp, medal, dayBattleCount);
	    }

	    public async Task<AvatarInfo.Builder> GetPosInfoDiff()
	    {
	        await this._messageQueue;

			var na1 = AvatarInfo.CreateBuilder ();
			na1.Id = avatarInfo.Id;

		    if (avatarInfo.X != lastAvatarInfo.X
		        || avatarInfo.Y != lastAvatarInfo.Y
		        || avatarInfo.Z != lastAvatarInfo.Z
		        || avatarInfo.Dir != lastAvatarInfo.Dir
                || avatarInfo.SpeedX != lastAvatarInfo.SpeedX
                || avatarInfo.SpeedY != lastAvatarInfo.SpeedY)
		    {
		        na1.X = avatarInfo.X;
		        na1.Y = avatarInfo.Y;
		        na1.Z = avatarInfo.Z;
		        na1.Dir = avatarInfo.Dir;
		        na1.SpeedX = avatarInfo.SpeedX;
		        na1.SpeedY = avatarInfo.SpeedY;
		        na1.Changed = true;

		        lastAvatarInfo.X = avatarInfo.X;
		        lastAvatarInfo.Y = avatarInfo.Y;
		        lastAvatarInfo.Z = avatarInfo.Z;
		        lastAvatarInfo.Dir = avatarInfo.Dir;
		        lastAvatarInfo.SpeedX = avatarInfo.SpeedX;
		        lastAvatarInfo.SpeedY = avatarInfo.SpeedY;
		    }


	        if (InitDataYet)
	        {
	            InitDataYet = false;
	            na1.ResetPos = true;
	            na1.Changed = false;
	        }

		    if (avatarInfo.TowerDir != lastAvatarInfo.TowerDir)
		    {
		        na1.TowerDir = avatarInfo.TowerDir;
		        na1.Changed = true;

		        lastAvatarInfo.TowerDir = avatarInfo.TowerDir;
		    }

	        if (avatarInfo.FrameID != lastAvatarInfo.FrameID)
	        {
	            na1.FrameID = avatarInfo.FrameID;
	            na1.Changed = true;
	            lastAvatarInfo.FrameID = avatarInfo.FrameID;
	        }
	        if (lowChange)
	        {
	            na1.LowChange = true;
	            na1.Changed = true;
	            lowChange = false;
	        }

            //服务器端关闭UDP
	        if (agent.useUDP)
	        {
	            if (agent.udpAgent != null)
	            {
	                var now = Util.GetTimeNow();
	                var lr = agent.udpAgent.lastReceiveTime;
	                if (now - lr > 5)
	                {
	                    agent.UDPLost();
	                    var gc = GCPlayerCmd.CreateBuilder();
	                    gc.Result = "UDPLost";
                        agent.SendPacket(gc, 0, 0);

                        LogHelper.Log("UDP", "UDPLost From Server");

	                }
	            }
	        }

			return na1;
	    } 

		/// <summary>
		/// 得到玩家属性的diff
		/// public 的Async方法需要 和Actor自身同步 
		/// delta 数据压缩
		/// 	repeat的Delta数据压缩
		/// </summary>
		public async Task<AvatarInfo.Builder> GetAvatarInfoDiff ()
		{
			await _messageQueue;
			if (lastAvatarInfo == null) {
				var na = AvatarInfo.CreateBuilder (avatarInfo);
				lastAvatarInfo = na.Build ();
				return AvatarInfo.CreateBuilder(lastAvatarInfo);
			}

			var na1 = AvatarInfo.CreateBuilder ();
			na1.Id = avatarInfo.Id;
            
		    if (avatarInfo.HP != lastAvatarInfo.HP) {
				na1.HP = avatarInfo.HP;
				na1.Changed = true;

		        lastAvatarInfo.HP = avatarInfo.HP;
		    }
			if (avatarInfo.TeamColor != lastAvatarInfo.TeamColor) {
				na1.TeamColor = avatarInfo.TeamColor;
				na1.Changed = true;

			    lastAvatarInfo.TeamColor = avatarInfo.TeamColor;
			}
			if (avatarInfo.IsMaster != lastAvatarInfo.IsMaster) {
				na1.IsMaster = avatarInfo.IsMaster;
				na1.Changed = true;

			    lastAvatarInfo.IsMaster = avatarInfo.IsMaster;
			}
			if (avatarInfo.NetSpeed != lastAvatarInfo.NetSpeed) {
				na1.NetSpeed = avatarInfo.NetSpeed;
				na1.Changed = true;

			    lastAvatarInfo.NetSpeed = avatarInfo.NetSpeed;
			}

			if (avatarInfo.ThrowSpeed != lastAvatarInfo.ThrowSpeed) {
				na1.ThrowSpeed = avatarInfo.ThrowSpeed;
				na1.Changed = true;

			    lastAvatarInfo.ThrowSpeed = avatarInfo.ThrowSpeed;
			}

			if (avatarInfo.JumpForwardSpeed != lastAvatarInfo.JumpForwardSpeed) {
				na1.JumpForwardSpeed = avatarInfo.JumpForwardSpeed;
				na1.Changed = true;

			    lastAvatarInfo.JumpForwardSpeed = avatarInfo.JumpForwardSpeed;
			}
			if (avatarInfo.Name != lastAvatarInfo.Name) {
				na1.Name = avatarInfo.Name;
				na1.Changed = true;

			    lastAvatarInfo.Name = avatarInfo.Name;
			}

			if (avatarInfo.Job != lastAvatarInfo.Job) {
				na1.Job = avatarInfo.Job;
				na1.Changed = true;

			    lastAvatarInfo.Job = avatarInfo.Job;
			}

		    if (avatarInfo.KillCount != lastAvatarInfo.KillCount)
		    {
		        na1.KillCount = avatarInfo.KillCount;
		        na1.Changed = true;

		        lastAvatarInfo.KillCount = avatarInfo.KillCount;
		    }

            if (avatarInfo.DeadCount != lastAvatarInfo.DeadCount)
            {
                na1.DeadCount = avatarInfo.DeadCount;
                na1.Changed = true;

                lastAvatarInfo.DeadCount = avatarInfo.DeadCount;
            }

            if (avatarInfo.SecondaryAttackCount != lastAvatarInfo.SecondaryAttackCount)
            {
                na1.SecondaryAttackCount = avatarInfo.SecondaryAttackCount;
                na1.Changed = true;

                lastAvatarInfo.SecondaryAttackCount = avatarInfo.SecondaryAttackCount;
            }

            if (avatarInfo.Score != lastAvatarInfo.Score)
		    {
		        na1.Score = avatarInfo.Score;
		        na1.Changed = true;

		        lastAvatarInfo.Score = avatarInfo.Score;
		    }

		    if (avatarInfo.ContinueKilled != lastAvatarInfo.ContinueKilled)
		    {
		        na1.ContinueKilled = avatarInfo.ContinueKilled;
		        na1.Changed = true;

		        lastAvatarInfo.ContinueKilled = avatarInfo.ContinueKilled;
		    }
			return na1;
		}

		//所有Async方法都应该自动补全这个await 函数 函数修饰器
		public async Task SendCmd (GCPlayerCmd.Builder cmd)
		{
			await _messageQueue;

			agent.SendPacket (cmd, 0, 0); 
		}

	    public async Task SendUDP(GCPlayerCmd.Builder cmd)
	    {
	        await _messageQueue;
            agent.SendUDPPacket(cmd, 0, 0);
	    }

		public async void SetMaster ()
		{
			await _messageQueue;
			avatarInfo.IsMaster = true;
		}

        List<int[]> scoreList = new List<int[]>()
        {
            new []{10, 10},
            new []{20, 15},
            new []{30, 20},
            new []{40, 25},
            new []{50, 30},
            new []{60, 35},
            new []{70, 40},
            new []{20, 40},
        };

        public async Task AddSecondaryAttack()
        {
            await _messageQueue;
            avatarInfo.SecondaryAttackCount++;
        }

	    public async Task AddKillCount()
	    {
            await _messageQueue;
            avatarInfo.KillCount++;
        }

        public async Task AddDeadCount()
        {
            await _messageQueue;
            avatarInfo.DeadCount++;
        }

	    public int finalScore;
	    public async Task GetScore()
	    {
            await _messageQueue;
            finalScore = avatarInfo.Score;
	    }

        public async Task AddScore(int eneContinue, int eneId)
	    {
	        await _messageQueue;
	        var myCount = this.avatarInfo.ContinueKilled;
            //我的连杀得分
	        if (myCount < scoreList.Count)
	        {
	            var s = scoreList[myCount][0];
	            this.avatarInfo.Score += s;
	        }
	        else
	        {
	            var s = scoreList[scoreList.Count - 1][0];
	            this.avatarInfo.Score += s;
	        }

            //击杀连杀玩家补充得分
	        if (eneContinue < scoreList.Count)
	        {
	            var s = scoreList[eneContinue][1];
	            this.avatarInfo.Score += s;
	        }
	        else
	        {
	            var s = scoreList[scoreList.Count - 1][1];
	            this.avatarInfo.Score += s;
	        }
	        avatarInfo.ContinueKilled++;

	        var dmg = GCPlayerCmd.CreateBuilder();
	        var dinfo = DamageInfo.CreateBuilder();
	        var ainfo = AvatarInfo.CreateBuilder();
	        ainfo.ContinueKilled = avatarInfo.ContinueKilled;
	        dinfo.Attacker = this.Id;
	        dinfo.Enemy = eneId;
	        dmg.Result = "Dead";
	        dmg.DamageInfo = dinfo.Build();
	        dmg.AvatarInfo = ainfo.Build();
	        room.AddCmd(dmg);
	    }

	    public async Task DecScore()
	    {
	        await this._messageQueue;
	        avatarInfo.ContinueKilled = 0;
	        avatarInfo.Score = (int) (avatarInfo.Score*0.8f);
	    }

	    private bool lowChange = false;
	    private int lastFrameId = -1;
        /// <summary>
        /// 1:FrameID ==0  128 TCP稳定发过来
        /// 2：服务器lowChange 事件同步给客户端
        /// 3：客户端FrameID 更新
        /// </summary>
        /// <param name="cmd"></param>
	    private void UpdateData(CGPlayerCmd cmd)
	    {
	        if (avatarInfo == null)
	        {
	            avatarInfo = cmd.AvatarInfo;
                return;
	        }

            if (cmd.AvatarInfo.HasIsRobot)
            {
                avatarInfo.IsRobot = cmd.AvatarInfo.IsRobot;
            }

	        if (cmd.AvatarInfo.HasFrameID)
	        {
	            var newFrameId = cmd.AvatarInfo.FrameID;
                //newFrame == 0 为客户端发送的可靠报文 确保到达
	            //TCP 发送的重新标定 ID序列的可靠报文
	            if (lastFrameId == -1)
	            {
	                lowChange = true;
	                lastFrameId = newFrameId;
	                avatarInfo.FrameID = newFrameId;
	            }
                else if (newFrameId == 0)
                {
                    lowChange = true;
	                lastFrameId = newFrameId;
	                avatarInfo.FrameID = newFrameId;
	            }else if (newFrameId == 128)
	            {
	                lowChange = true;
	                lastFrameId = newFrameId;
	                avatarInfo.FrameID = newFrameId;
	            }
	            else if(newFrameId > 127 && lastFrameId <= 127)//报文不在同一个区间段 上一阶段的报文
	            {
	                return;
                }
                else if (newFrameId <= 127 && lastFrameId > 127) //报文不在同一个区间段 上一阶段的报文
	            {
                    return;
	            }
	            else if (newFrameId <= lastFrameId) //报文不是新的
	            {
	                return;
	            }
	            else //通知客户端更新FrameID
	            {
	                lastFrameId = newFrameId;
	                avatarInfo.FrameID = newFrameId;
	            }
	        }

            //同步速度
            if (cmd.AvatarInfo.HasSpeedX)
            {
                avatarInfo.SpeedX = cmd.AvatarInfo.SpeedX;
                avatarInfo.SpeedY = cmd.AvatarInfo.SpeedY;
            }

	        if (cmd.AvatarInfo.HasX)
	        {
	            avatarInfo.X = cmd.AvatarInfo.X;
	            avatarInfo.Y = cmd.AvatarInfo.Y;
	            avatarInfo.Z = cmd.AvatarInfo.Z;
	        }

	        if (cmd.AvatarInfo.HasDir)
	        {
	            avatarInfo.Dir = cmd.AvatarInfo.Dir;
	        }
	        if (cmd.AvatarInfo.HasHP)
	        {
	            avatarInfo.HP = cmd.AvatarInfo.HP;
	        }
	        if (cmd.AvatarInfo.HasNetSpeed)
	        {
	            avatarInfo.NetSpeed = cmd.AvatarInfo.NetSpeed;
	        }
	        if (cmd.AvatarInfo.HasThrowSpeed)
	        {
	            avatarInfo.ThrowSpeed = cmd.AvatarInfo.ThrowSpeed;
	        }
	        if (cmd.AvatarInfo.HasJumpForwardSpeed)
	        {
	            avatarInfo.JumpForwardSpeed = cmd.AvatarInfo.JumpForwardSpeed;
	        }
	        if (cmd.AvatarInfo.HasName)
	        {
	            avatarInfo.Name = cmd.AvatarInfo.Name;
	        }
	        if (cmd.AvatarInfo.HasJob)
	        {
	            avatarInfo.Job = cmd.AvatarInfo.Job;
	        }
	        if (cmd.AvatarInfo.HasTowerDir)
	        {
	            avatarInfo.TowerDir = cmd.AvatarInfo.TowerDir;
	        }
	    }

	    private double GetTimeNow()
	    {
	        return DateTime.UtcNow.Ticks/10000000.0;
	    }

	    public async Task RefreshLive()
	    {
	        await this._messageQueue;
	        lastReceiveTime = GetTimeNow();
	    }

	    public async Task<bool> CheckLive()
	    {
	        await this._messageQueue;
	        var now = GetTimeNow();
	        if (now - lastReceiveTime > 5)
	        {
                LogHelper.Log("Actor", "Client Pause For Too Long Time");
                this.agent.Close();
	            return false;
	        }
	        return true;
	    }

	    private void RemovePlayer()
	    {
	        ActorManager.Instance.RemoveActor(Id);
	        LogHelper.Log("PlayerActor", "CloseActor " + Id);
	        if (room != null)
	        {
	            room.RemovePlayer(this, AvatarInfo.CreateBuilder(avatarInfo).Build());
	        }
            Login.QuitRoom(deviceInfo, (agent.ep as IPEndPoint).Address.ToString() );
	    }

	    private bool InitDataYet = false;
	    private double lastReceiveTime = 0;
	    private int buffId = 1;
	    //在锁区域不要调用有福作用的函数
		/// <summary>
		/// 该函数已经在当前的Context下面执行了
		/// 如果一个Task await到 Time.Delay 那么Task就会从执行流里面被扔出去 
		/// </summary>
		/// <returns>The message.</returns>
		/// <param name="msg">Message.</param>
		protected override async Task ReceiveMsg (ActorMsg msg)
		{
            //tick 1毫秒 = 10000 tick
		    lastReceiveTime = GetTimeNow();

			if (!string.IsNullOrEmpty (msg.msg)) {
				var cmds = msg.msg.Split (' ');
				if (cmds [0] == "close") {
                    RemovePlayer();
				}
			} else {
				//LogHelper.Log("PlayerActor", "ReceivePacket " + Id + " p " + msg.packet.protoBody.ToString ());
				if (msg.packet.protoBody.GetType () == typeof(CGPlayerCmd)) {
					var cmd = msg.packet.protoBody as CGPlayerCmd;
					var cmds = cmd.Cmd.Split (' ');
					if (cmds[0] == "Login")
					{
						var ret = GCPlayerCmd.CreateBuilder();
						ret.Result = string.Format("Login {0}", Id);
						deviceInfo = cmd.DeviceInfo;
						pid = deviceInfo.Pid;
						uid = deviceInfo.Uid;
						agent.SendPacket(ret, (byte)msg.packet.flowId, 0);
						var data = Login.LoginQueryInfo(pid, uid);

						try
						{
							level = (int)data[0][0];
							Exp = (int)data[0][1];
							medal = (int)data[0][2];
							dayBattleCount = (int)data[0][3];
						}
						catch (Exception e)
						{
							Console.WriteLine(e);
						}
					}
					else if (cmds[0] == "Login2")
					{
						var proto = msg.packet.protoBody as CGPlayerCmd;
						var result = Login.LoginGame(proto);
						var ret = GCPlayerCmd.CreateBuilder();
						ret.Result = string.Format("Login {0}", result);
						agent.SendPacket(ret, (byte)msg.packet.flowId, 0);
					}
					else if (cmds[0] == "InitData")
					{
						UpdateData(cmd);
						avatarInfo.Id = Id;
						avatarInfo.ResetPos = true;
						InitDataYet = true;

						if (lastAvatarInfo == null)
						{
							lastAvatarInfo = AvatarInfo.CreateBuilder(avatarInfo).Build();
						}
						//avatarInfo = cmd.AvatarInfo;
						var gc = GCPlayerCmd.CreateBuilder();
						gc.Result = "InitData";
						agent.SendPacket(gc, msg.packet.flowId, 0);
					}
					else if (cmds[0] == "UpdateData")
					{
						UpdateData(cmd);
					}
					else if (cmds[0] == "Damage")
					{
						var gc = GCPlayerCmd.CreateBuilder();
						gc.DamageInfo = cmd.DamageInfo;
						gc.Result = cmd.Cmd;
						//world.AddCmd (gc);
						room.AddCmd(gc);
					}
					else if (cmds[0] == "Skill")
					{

						var gc = GCPlayerCmd.CreateBuilder();
						gc.SkillAction = cmd.SkillAction;
						gc.Result = cmd.Cmd;
						room.AddCmd(gc);

					}
					else if (cmds[0] == "Move")
					{//快速移动
						avatarInfo.X = cmd.AvatarInfo.X;
						avatarInfo.Y = cmd.AvatarInfo.Y;
						avatarInfo.Z = cmd.AvatarInfo.Z;
						avatarInfo.Dir = cmd.AvatarInfo.Dir;

						var gc = GCPlayerCmd.CreateBuilder();
						gc.AvatarInfo = cmd.AvatarInfo;
						gc.AvatarInfo.Id = Id;
						gc.Result = "Update";
						room.AddCmd(gc);
					}
					else if (cmds[0] == "Buff")
					{
						//直接事件RPC通知 而不是状态通知
						if (room != null)
						{
							var gc = GCPlayerCmd.CreateBuilder();
							cmd.BuffInfo.BuffId = ++buffId;
							gc.BuffInfo = cmd.BuffInfo;
							gc.Result = cmd.Cmd;
							room.AddCmd(gc);
							this.lastAvatarInfo.BuffInfoList.Add(cmd.BuffInfo);
						}
					}
					else if (cmds[0] == "RemoveBuff")
					{
						if (room != null)
						{
							var gc = GCPlayerCmd.CreateBuilder();
							gc.BuffInfo = cmd.BuffInfo;
							gc.Result = cmd.Cmd;
							room.AddCmd(gc);
							var i = 0;
							foreach (var buff in this.lastAvatarInfo.BuffInfoList)
							{
								if (buff.BuffType == cmd.BuffInfo.BuffType)
								{
									this.lastAvatarInfo.BuffInfoList.RemoveAt(i);
									break;
								}
								i++;
							}
						}
					}
					else if (cmds[0] == "AddEntity")
					{
						var entity = new EntityActor();
						ActorManager.Instance.AddActor(entity);
						await entity.InitInfo(cmd.EntityInfo);
						room.AddEntity(entity, cmd.EntityInfo);
					}
					else if (cmds[0] == "RemoveEntity")
					{
						/*
						var ety = cmd.EntityInfo;
						var act = ActorManager.Instance.GetActor (ety.Id);
						var eact = (EntityActor)act;
						eact.RemoveSelf ();
                        */
					}
					else if (cmds[0] == "UpdateEntityData")
					{
						var ety = cmd.EntityInfo;
						var act = ActorManager.Instance.GetActor(ety.Id);
						var eact = (EntityActor)act;
						eact.UpdateData(ety);
					}
					else if (cmds[0] == "Pick")
					{
						/*
						var gc = GCPlayerCmd.CreateBuilder ();
						gc.PickAction = cmd.PickAction;
						gc.Result = cmd.Cmd;
						room.AddCmd (gc);
                        */
						room.PickItem(cmd);
					}
					else if (cmds[0] == "SyncTime")
					{
						var gc = GCPlayerCmd.CreateBuilder();
						gc.LeftTime = cmd.LeftTime;
						gc.Result = cmd.Cmd;
						room.AddCmd(gc);
					}
					else if (cmds[0] == "Dead")
					{
						/*
						var gc = GCPlayerCmd.CreateBuilder ();
						gc.DamageInfo = cmd.DamageInfo;
						gc.Result = cmd.Cmd;
						room.AddCmd (gc);
                        */
						room.Dead(cmd);
					}
					else if (cmds[0] == "GameOver")
					{
						var gc = GCPlayerCmd.CreateBuilder();
						gc.Result = cmd.Cmd;
						await room.GameOver();
						room.AddCmd(gc);

					}
					else if (cmds[0] == "Revive")
					{
						UpdateData(cmd);
						avatarInfo.ResetPos = true;
						avatarInfo.HP = 100;
						var gc = GCPlayerCmd.CreateBuilder();
						gc.Result = cmd.Cmd;
						gc.AvatarInfo = cmd.AvatarInfo;
						room.AddCmd(gc);
					}
					else if (cmds[0] == "Match")
					{
						MatchRoom(msg, cmd, false);
						LogHelper.Log("PlayerActor", "MatchingGame");
					}
					else if (cmds[0] == "MatchNew")
					{
						MatchRoom(msg, cmd, true);
						LogHelper.Log("PlayerActor", "MatchNew");
					}
					else if (cmds[0] == "StartGame")
					{
						await room.StartGame();
						var gc = GCPlayerCmd.CreateBuilder();
						gc.Result = cmd.Cmd;
						room.AddCmd(gc);
					}
					else if (cmds[0] == "Ready")
					{
						room.SetReady(this);
					}
					else if (cmds[0] == "HeartBeat")
					{
						var gc = GCPlayerCmd.CreateBuilder();
						gc.Result = "HeartBeat";
						agent.SendPacket(gc, msg.packet.flowId, 0);
					}
					else if (cmds[0] == "Reconnect")
					{
						//重新连接类似于 Match
						if (room != null)
						{
							room.RemovePlayer(this, AvatarInfo.CreateBuilder(avatarInfo).Build());
							room = null;
						}
						var oldRoom = cmd.RoomInfo.Id;
						var maxP = cmd.RoomInfo.MaxPlayerNum;
						var lobby = ActorManager.Instance.GetActor<Lobby>();
						var r = await lobby.GetRoom(oldRoom, maxP, this);

						var gc = GCPlayerCmd.CreateBuilder();
						gc.Result = "Reconnect";
						var rinfo = RoomInfo.CreateBuilder();

						if (r == null)
						{
							rinfo.Id = -1;
						}
						else
						{
							rinfo.Id = r.Id;
						}
						room = r;

						gc.RoomInfo = rinfo.Build();
						agent.SendPacket(gc, msg.packet.flowId, 0);
						if (room != null)
						{
							var tc = room.GetTeamColor();
							await tc;
							avatarInfo.TeamColor = tc.Result;
						}
					}
					else if (cmds[0] == "LoginServer") //登陆合法性验证
					{
					}
					else if (cmds[0] == "TestUDP")
					{
						LogHelper.Log("UDP", "TestUDP");
						var gc = GCPlayerCmd.CreateBuilder();
						gc.Result = "TestUDP";
						agent.ForceUDP(gc, 0, 0);
					}
					else if (cmds[0] == "UDPConnect")
					{
						LogHelper.Log("UDP", "UseUDP");
						agent.UseUDP();
					}
					else if (cmds[0] == "UDPLost")
					{
						agent.UDPLost();
						LogHelper.Log("UDP", "UDPLost");
					}
					else if (cmds[0] == "TestKCP")
					{
						LogHelper.Log("KCP", "TestKCP");
						var gc = GCPlayerCmd.CreateBuilder();
						gc.Result = "TestKCP";
						agent.ForceKCP(gc, 0, 0);
					}
					else if (cmds[0] == "KCPConnect")
					{
						LogHelper.Log("KCP", "UseKCP");
						agent.UseKCP();
					}
					else if (cmds[0] == "KCPLost")
					{
						LogHelper.Log("KCP", "KCPLost");
						agent.KCPLost();
					}
				}
			}
		}

	    private async Task MatchRoom(ActorMsg msg, CGPlayerCmd cmd, bool isNew)
	    {
	        if (room != null)
	        {
	            room.RemovePlayer(this, AvatarInfo.CreateBuilder(avatarInfo).Build());
	            room = null;
	        }
	        var mp = cmd.RoomInfo.MaxPlayerNum;
	        var lobby = ActorManager.Instance.GetActor<Lobby>();
	        var r = await lobby.FindRoom(this, mp, cmd.RoomInfo, isNew);
	        room = r;

	        var gc = GCPlayerCmd.CreateBuilder();
	        gc.Result = "Match";

	        var rinfo = RoomInfo.CreateBuilder();
	        rinfo.Id = r.Id;
	        rinfo.MaxPlayerNum = r.maxPlayerNum;
	        rinfo.LevelId = r.GetLevelId();

	        gc.RoomInfo = rinfo.Build();
	        agent.SendPacket(gc, msg.packet.flowId, 0);

	        var tc = room.GetTeamColor();
	        await tc;
	        avatarInfo.TeamColor = tc.Result;

	        if (room.GetState() == RoomActor.RoomState.InGame)
	        {
	            var gc2 = GCPlayerCmd.CreateBuilder();
	            gc2.Result = "StartGame";
	            agent.SendPacket(gc2, 0, 0);
	        }
	        Login.StartMatch(deviceInfo, (agent.ep as IPEndPoint).Address.ToString());
	    }
	}
}

