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

        public Dictionary<int, EntityActor> allEntities = new Dictionary<int, EntityActor>();

		public EntityManagerCom ()
		{
		}

		public void InitEntityDataToPlayer (PlayerActor player)
		{
			var earr = entities;
			foreach (var p in earr ) {
				var info = p.GetEntityInfo ();
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

        public EntityActor GetEntity(int id)
        {
            if (allEntities.ContainsKey(id))
            {
                return allEntities[id];
            }
            return null;
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

			var earr = entities;
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
            allEntities.Add(actor.Id, actor);
		}

		public void  Remove (EntityActor actor, EntityInfo info)
		{
			entities.Remove (actor);
			removeEntities.Add (info);
            allEntities.Remove(actor.Id);
		}

        public override void Destroy()
        {
            RemoveAll();
        }

        /// <summary>
        /// Master 离开游戏删除所有Entity 
        /// </summary>
        /// <returns>The all.</returns>
        private void RemoveAll ()
		{
			var earr = entities;
			foreach(var e in earr){
				//var e = entities [i];
				var info = e.entityInfo;
				e.OnlyRemoveSelf ();
				Remove (e, info);
			}
		}
	}
}

