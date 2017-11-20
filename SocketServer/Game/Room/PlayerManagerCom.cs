using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Box2DX.Common;

namespace MyLib
{
	class PlayerManagerCom : Component
	{
		List<PlayerInRoom> players = new List<PlayerInRoom> ();
		List<AvatarInfo> newPlayer = new List<AvatarInfo> ();
		List<AvatarInfo> removePlayer = new List<AvatarInfo> ();
		Dictionary<int, bool> chooseHero = new Dictionary<int, bool> ();
        Dictionary<int, PlayerInRoom> playerDict = new Dictionary<int, PlayerInRoom>();
        public AllPlayerStart allPlayerStart;
        public void SetAllPlayer(AllPlayerStart ap)
        {
            allPlayerStart = ap;
        }

		public PlayerManagerCom ()
		{
		}

	    public List<PlayerInRoom> GetPlayers()
	    {
	        return players;
	    }

		//只能RoomActor调用 别人不能调用
		public int GetPlayerNum() {
			return players.Count;
		}

		public void SetChoose(PlayerInRoom  pl) {
			chooseHero [pl.Id] = true;
		}


        /// <summary>
        /// 所有客户端选择英雄结束
        /// </summary>
        /// <returns></returns>
		public bool IsAllChoose() {
			foreach(var p in players) {
				if (!chooseHero.ContainsKey (p.Id)) {
					return false;
				}
			}
			return true;
		}

        /// <summary>
        /// 所有客户端发送Ready命令给服务器
        /// </summary>
        /// <returns></returns>
        public bool IsAllClientInit()
        {
            foreach(var p in players)
            {
                if(p.GetAvatarInfo().State != PlayerState.AfterReset)
                {
                    return false;
                }
            }
            return true;
        }

	    public void SendAllPlayerTo(PlayerInRoom pl)
	    {
	        foreach (var playerActor in players)
	        {
	            var gc = GCPlayerCmd.CreateBuilder();
	            gc.Result = "Add";
	            gc.AvatarInfo = playerActor.GetAvatarInfo();
	            pl.SendCmd(gc);
	        }
	    }


		public void UpdatePlayer ()
		{
			foreach (var p in newPlayer) {
				var gc = GCPlayerCmd.CreateBuilder ();
				gc.Result = "Add";
				gc.AvatarInfo = p;
				BroadcastToAll (gc); 
				var rp = GetPlayer (p.Id);
			    if (rp != null)
			    {
			        CurrentPlayerToNew(rp);
			        CurrentEntityToNew(rp);
			    }
			}

			newPlayer.Clear ();

			foreach (var p in removePlayer) {
				var gc = GCPlayerCmd.CreateBuilder ();
				gc.Result = "Remove";
				gc.AvatarInfo = p;
				BroadcastToAll (gc);
			}
			removePlayer.Clear ();

            var rmActor = this.actor as RoomActor;
			foreach (var p in players)
			{
			    var ret = true;
			    if (rmActor.GetState() == RoomActor.RoomState.InGame)
			    {
			       // ret = await p.CheckLive();
			    }

			    if (ret)
			    {
			        var diff = p.GetAvatarInfoDiff();
			        if (diff.Changed)
			        {
			            diff.ClearChanged();
			            var gc = GCPlayerCmd.CreateBuilder();
			            gc.Result = "Update";
			            gc.AvatarInfo = diff.Build();
			            BroadcastToAll(gc);
			        }

			        var posInfo = p.GetPosInfoDiff();
			        if (posInfo.Changed)
			        {
			            var lowChange = posInfo.LowChange;
			            posInfo.ClearChanged();
			            var gc = GCPlayerCmd.CreateBuilder();
			            gc.Result = "Update";
			            gc.AvatarInfo = posInfo.Build();
                        //ID区间段变化需要TCP保证重新标定
			            if (lowChange)
			            {
			                BroadcastToAll(gc);
			            }
			            else
			            {
			                BroadcastUDPToAll(gc);
			            }
			        }

			    }
			}
		}

		public void BroadcastKCPToAll(GCPlayerCmd.Builder cmd)
		{
			//ServerBundle bundle;
			//var bytes = ServerBundle.sendImmediateError(cmd, 0, 0, out bundle);
			//ServerBundle.ReturnBundle(bundle);

			foreach (var pa in players)
			{
                //pa.GetAgent().SendKCPBytes(bytes);
                pa.SendCmd(cmd);
			}
		}

		private void BroadcastUDPToAll(GCPlayerCmd.Builder cmd)
	    {
		    //ServerBundle bundle;
            //var bytes = ServerBundle.sendImmediateError(cmd, 0, 0, out bundle);
            //ServerBundle.ReturnBundle(bundle);
	        //var parr = players.ToArray();
	        foreach (var playerActor in players)
	        {
                //playerActor.GetAgent().SendUDPBytes(bytes);
                playerActor.SendCmd(cmd);
	        }
	    }

	    public void UpdateAllPlayersLevel()
	    {
            foreach (var playerActor in players)
            {
                playerActor.GetScore();
            }
            
            players.Sort((obj1, obj2) =>
            {
                return obj2.finalScore.CompareTo(obj1.finalScore);
            });

	        for (int i = 0; i < players.Count; i++)
	        {
	            players[i].UpdateLevel(i);
	        }
        }

	    public void RefreshLiveTime()
	    {
	        var parr = players.ToArray();
	        foreach (var playerActor in parr)
	        {
	            playerActor.RefreshLive();
	        }
	    }

		public void BroadcastToAll (GCPlayerCmd.Builder cmd)
		{
			foreach (var p in players ) {
                //p.SendCmd (cmd);
                //p.GetAgent().SendBytes(bytes);
                p.SendCmd(cmd);
			}
		}

	    public PlayerInRoom GetPlayerIndex(int ind)
	    {
	        return players[ind];
	    }

		public PlayerInRoom GetPlayer (int id)
		{
			foreach (var p in players ) {
				if (p.Id == id) {
					return p;
				}
			}
			return null;
		}

		/// <summary>
		/// 当前场景中所有的实体传递给玩家 
		/// </summary>
		/// <param name="player">Player.</param>
		private void CurrentEntityToNew (PlayerInRoom player)
		{
			var etyCom = this.actor.GetComponent<EntityManagerCom> ();
			etyCom.InitEntityDataToPlayer (player);
		}

		/// <summary>
		/// 新加入场景的玩家 获得所有场景当前的玩家的信息 
		/// </summary>
		/// <param name="player">Player.</param>
		private void CurrentPlayerToNew (PlayerInRoom player)
		{
			foreach (var p in players ) {
				if (p.Id != player.Id) {
					var info = p.GetAvatarInfo ();
					var gc = GCPlayerCmd.CreateBuilder ();
					gc.Result = "Add";
					gc.AvatarInfo = info;
					player.SendCmd (gc);
				}
			}
		}

		public async Task AddPlayer (PlayerActor player, AvatarInfo ainfo)
		{
            try
            {
                var pInRoom = new PlayerInRoom(player, ainfo);
                pInRoom.SetRoom(this.actor as RoomActor);
                pInRoom.InitAfterSetRoom();
                await pInRoom.InitProxy();
                pInRoom.Start();
                allPlayerStart.InitPlayerPos(pInRoom);
                pInRoom.AfterInitPos();
                pInRoom.RunAI();

                playerDict[pInRoom.IDInRoom] = pInRoom;
                players.Add(pInRoom);
                newPlayer.Add(ainfo);
            }catch(Exception exp)
            {
                Log.Error(exp.ToString());
            }
		}

		public void  RemovePlayer (PlayerInRoom player, AvatarInfo ainfo)
		{
			players.Remove (player);
			removePlayer.Add (ainfo);
            playerDict.Remove(player.IDInRoom);
		}

	  
	}
}

