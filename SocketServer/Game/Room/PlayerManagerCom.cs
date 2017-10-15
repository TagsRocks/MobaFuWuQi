using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Box2DX.Common;

namespace MyLib
{
	public class PlayerManagerCom : Component
	{
		List<PlayerActor> players = new List<PlayerActor> ();
		List<AvatarInfo> newPlayer = new List<AvatarInfo> ();
		List<AvatarInfo> removePlayer = new List<AvatarInfo> ();
		Dictionary<int, bool> ready = new Dictionary<int, bool> ();

		public PlayerManagerCom ()
		{
		}

	    public List<PlayerActor> GetPlayers()
	    {
	        return players;
	    }

		//只能RoomActor调用 别人不能调用
		public int GetPlayerNum() {
			return players.Count;
		}

		public void SetReady(PlayerActor  pl) {
			ready [pl.Id] = true;
		}


		public bool IsAllReady() {
			foreach(var p in players) {
				if (!ready.ContainsKey (p.Id)) {
					return false;
				}
			}
			return true;
		}

	    public async Task SendAllPlayerTo(PlayerActor pl)
	    {
	        var ps = players.ToArray();
	        foreach (var playerActor in ps)
	        {
	            var gc = GCPlayerCmd.CreateBuilder();
	            gc.Result = "Add";
	            gc.AvatarInfo = await playerActor.GetAvatarInfo();
	            await pl.SendCmd(gc);
	        }
	    }

		public async Task SendAllPlayerToAllClient() {
			var ps = players.ToArray ();
			foreach (var p in ps) {
				var gc = GCPlayerCmd.CreateBuilder ();
				gc.Result = "Add";
				gc.AvatarInfo = await p.GetAvatarInfo(); 
				BroadcastToAll (gc);
			}
		}

		public async Task  UpdatePlayer ()
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

			var parr = players.ToArray ();
			foreach (var p in parr )
			{
			    var rmActor = this.actor as RoomActor;
			    var ret = true;
			    if (rmActor.GetState() == RoomActor.RoomState.InGame)
			    {
			        ret = await p.CheckLive();
			    }

			    if (ret)
			    {
			        var diff = await p.GetAvatarInfoDiff();
			        if (diff.Changed)
			        {
			            diff.ClearChanged();
			            var gc = GCPlayerCmd.CreateBuilder();
			            gc.Result = "Update";
			            gc.AvatarInfo = diff.Build();
			            BroadcastToAll(gc);
			        }

			        var posInfo = await p.GetPosInfoDiff();
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
			ServerBundle bundle;
			var bytes = ServerBundle.sendImmediateError(cmd, 0, 0, out bundle);
			ServerBundle.ReturnBundle(bundle);

			var parr = players.ToArray();
			foreach (var pa in parr)
			{
				pa.GetAgent().SendKCPBytes(bytes);
			}
		}

		private void BroadcastUDPToAll(GCPlayerCmd.Builder cmd)
	    {
		    ServerBundle bundle;
            var bytes = ServerBundle.sendImmediateError(cmd, 0, 0, out bundle);
            ServerBundle.ReturnBundle(bundle);

	        var parr = players.ToArray();
	        foreach (var playerActor in parr)
	        {
                playerActor.GetAgent().SendUDPBytes(bytes);
	        }
	    }

	    public async Task UpdateAllPlayersLevel()
	    {
            var ps = players.ToArray();
            foreach (var playerActor in ps)
            {
                await playerActor.GetScore();
            }
            
            players.Sort((obj1, obj2) =>
            {
                return obj2.finalScore.CompareTo(obj1.finalScore);
            });

	        for (int i = 0; i < players.Count; i++)
	        {
	            await players[i].UpdateLevel(i);
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
			var parr = players.ToArray ();
		    ServerBundle bundle;
            var bytes = ServerBundle.sendImmediateError(cmd, 0, 0, out bundle);
            ServerBundle.ReturnBundle(bundle);

			foreach (var p in parr ) {
				//p.SendCmd (cmd);
                p.GetAgent().SendBytes(bytes);
			}
		}

	    public PlayerActor GetPlayerIndex(int ind)
	    {
	        return players[ind];
	    }

		public PlayerActor GetPlayer (int id)
		{
			var parr = players.ToArray ();
			foreach (var p in parr ) {
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
		private async void CurrentEntityToNew (PlayerActor player)
		{
			var etyCom = this.actor.GetComponent<EntityManagerCom> ();
			await etyCom.InitEntityDataToPlayer (player);
		}

		/// <summary>
		/// 新加入场景的玩家 获得所有场景当前的玩家的信息 
		/// </summary>
		/// <param name="player">Player.</param>
		private async void CurrentPlayerToNew (PlayerActor player)
		{
			var parr = players.ToArray ();
			foreach (var p in parr ) {
				if (p.Id != player.Id) {
					var info = await p.GetAvatarInfo ();
					var gc = GCPlayerCmd.CreateBuilder ();
					gc.Result = "Add";
					gc.AvatarInfo = info;
					player.SendCmd (gc);
				}
			}
		}

		public void AddPlayer (PlayerActor player, AvatarInfo ainfo)
		{
			players.Add (player);
			newPlayer.Add (ainfo);
		}

		public async Task AddPlayerAsync (PlayerActor player, AvatarInfo ainfo)
		{
			await this.actor._messageQueue;
			players.Add (player);
			newPlayer.Add (ainfo);
		}

		public void  RemovePlayer (PlayerActor player, AvatarInfo ainfo)
		{
			players.Remove (player);
			removePlayer.Add (ainfo);
		}

		public async Task RemovePlayerAsync (PlayerActor player, AvatarInfo ainfo)
		{
			await this.actor._messageQueue;
			players.Remove (player);
			removePlayer.Add (ainfo);
		}

	    public async Task UpdatePhysic(int num)
	    {
	        if (num > 0)
	        {
	            var pw = (this.actor as RoomActor).GetComponent<PhysicWorldComponent>();
	            var arr = players.ToArray();
	            foreach (var p in arr)
	            {
	                var pi = await p.GetPosInfo();
                    Vec2 np = new Vec2();
	                np = pw.UpdatePlayerPhysic(p.Id, pi, num);
	                p.SetPos(np);
	            }
	        }
	    }
	}
}

