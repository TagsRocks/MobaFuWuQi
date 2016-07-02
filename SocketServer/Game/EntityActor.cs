using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MyLib 
{
	public class EntityActor : Actor
	{
		private EntityInfo lastEntityInfo;
		private EntityInfo entityInfo;

		public RoomActor room;

		public EntityActor ()
		{
		}

		public async Task<EntityInfo> GetEntityInfo() {
			await this._messageQueue;
			if(lastEntityInfo == null){
				var na = EntityInfo.CreateBuilder(entityInfo);
                return na.Build();
            }
			var na1 = EntityInfo.CreateBuilder(lastEntityInfo);
			return na1.Build();
		}

	    public override string GetAttr()
	    {
	        if (room != null)
	        {
	            return "room: "+room.Id;
	        }
	        return "";
	    }

	    public async Task<EntityInfo> GetEntityInfoDiff() {
			await this._messageQueue;
			if (lastEntityInfo == null) {
				var na = EntityInfo.CreateBuilder (entityInfo);
				lastEntityInfo = na.Build();
				return lastEntityInfo;
			}

			var na1 = EntityInfo.CreateBuilder();
			na1.Id = entityInfo.Id;
			if (lastEntityInfo.X != entityInfo.X || lastEntityInfo.Y != entityInfo.Y || lastEntityInfo.Z != entityInfo.Z) {
				na1.X = entityInfo.X;
				na1.Y = entityInfo.Y;
				na1.Z = entityInfo.Z;
				na1.Changed = true;
			}

			if (entityInfo.UnitId != lastEntityInfo.UnitId) {
				na1.UnitId = entityInfo.UnitId;
				na1.Changed = true;
			}

			if (entityInfo.SpawnId != lastEntityInfo.SpawnId) {
				na1.SpawnId = entityInfo.SpawnId;
				na1.Changed = true;
			}
			if (entityInfo.HP != lastEntityInfo.HP) {
				na1.HP = entityInfo.HP;
				na1.Changed = true;
			}

			lastEntityInfo = EntityInfo.CreateBuilder(entityInfo).Build();
            return na1.Build();
		}

	    public Action removeCallback = null;
		public void RemoveSelf() {
            //LogHelper.Log("Room", "RemoveEntity: "+this.Id);
		    if (room != null)
		    {
		        room.RemoveEntity(this, entityInfo);
		    }
		    ActorManager.Instance.RemoveActor(Id);
		    if (removeCallback != null)
		    {
		        removeCallback();
		    }
		}

		public async Task OnlyRemoveSelf() {
			await this._messageQueue;
			ActorManager.Instance.RemoveActor (Id);
		}


		public async Task UpdateData(EntityInfo info) {
			await this._messageQueue;
			if (info.HasX) {
				entityInfo.X = info.X;
				entityInfo.Y = info.Y;
				entityInfo.Z = info.Z;
			}
			if (info.HasHP) {
				entityInfo.HP = info.HP;
			}
		}

		//初始化实体的Id
		public async Task InitInfo(EntityInfo info) {
			await this._messageQueue;
			lastEntityInfo = EntityInfo.CreateBuilder (info).Build();
			entityInfo = info;
			entityInfo.Id = Id;
		    if (entityInfo.HasLifeLeft)
		    {
		        RunTask(UpdateLife);
		    }
		}

	    private async Task UpdateLife()
	    {
	        while (!isStop)
	        {
	            await Task.Delay(1000);
	            entityInfo.LifeLeft--;
	            if (entityInfo.LifeLeft == 0)
	            {
	                this.RemoveSelf();
                    break;
	            }
	        }
	    }
	}
}

