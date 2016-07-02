using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MyLib
{
	public class EntityManagerCom : Component
	{
		List<EntityActor> entities = new List<EntityActor> ();

		List<EntityInfo> newEntities = new List<EntityInfo> ();
		List<EntityInfo> removeEntities = new List<EntityInfo> ();

		public EntityManagerCom ()
		{
		}

		public async Task InitEntityDataToPlayer (PlayerActor player)
		{
			var earr = entities.ToArray ();
			foreach (var p in earr ) {
				var info = await p.GetEntityInfo ();
				var gc = GCPlayerCmd.CreateBuilder ();
				gc.Result = "AddEntity";
				gc.EntityInfo = info;
				player.SendCmd (gc);
			}
		}

	    public void SendAllEtyTo(PlayerActor pl)
	    {
	        InitEntityDataToPlayer(pl);
	    }

	    public void Pick(CGPlayerCmd cmd)
	    {
	        Actor ety = null;
	        foreach (var entityActor in entities)
	        {
	            if (entityActor.Id == cmd.PickAction.Id)
	            {
	                var gc = GCPlayerCmd.CreateBuilder();
	                gc.Result = "Pick";
	                gc.PickAction = cmd.PickAction;
	                var ra = this.actor as RoomActor;
	                ra.AddBeforeCmd(gc);
	                ety = entityActor;
	                break;
	            }
	        }
	        if (ety != null)
	        {
	            ((EntityActor) ety).RemoveSelf();
	        }
	    }

		public async Task UpdateEntity ()
		{
			foreach (var p in newEntities) {
				var gc = GCPlayerCmd.CreateBuilder ();
				gc.Result = "AddEntity";
				gc.EntityInfo = p;
				BroadcastToAll (gc); 
			}
			newEntities.Clear ();

			foreach (var p in removeEntities) {
				var gc = GCPlayerCmd.CreateBuilder ();
				gc.Result = "RemoveEntity";
				gc.EntityInfo = p;
				BroadcastToAll (gc);
			}
			removeEntities.Clear ();

			var earr = entities.ToArray ();
			foreach (var p in earr ) {
				var diff = await p.GetEntityInfoDiff ();
				if (diff.Changed) {
					var gc = GCPlayerCmd.CreateBuilder ();
					gc.Result = "UpdateEntity";
					gc.EntityInfo = diff;
					BroadcastToAll (gc);
				}
			}

		}

		//Entity 广播给所有玩家 其它Actor 调用这个Actor的方法
		void BroadcastToAll (GCPlayerCmd.Builder cmd)
		{
			var playerCom = this.actor.GetComponent<PlayerManagerCom> ();
			playerCom.BroadcastToAll (cmd);
		}


		public void Add (EntityActor actor, EntityInfo info)
		{
			entities.Add (actor);
			newEntities.Add (info);
		}

		public void  Remove (EntityActor actor, EntityInfo info)
		{
			entities.Remove (actor);
			removeEntities.Add (info);
		}

		/// <summary>
		/// Master 离开游戏删除所有Entity 
		/// </summary>
		/// <returns>The all.</returns>
		public async Task RemoveAll ()
		{
			await this.actor._messageQueue;
			var earr = entities.ToArray ();
			//for (var i = 0; i < entities.Count;) {
			foreach(var e in earr){
				//var e = entities [i];
				var info = await e.GetEntityInfo ();
				e.OnlyRemoveSelf ();
				Remove (e, info);
			}
		}
	}
}

