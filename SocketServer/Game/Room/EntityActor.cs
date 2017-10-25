using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace MyLib 
{
    /// <summary>
    /// 虽然继承GameObjectActor
    /// 但是没有调用Start方法
    /// 组件的初始化需要在Add的时候进行
    /// </summary>
	public class EntityActor : GameObjectActor 
	{
        /// <summary>
        /// 上一帧已经同步 广播出去的数据状态
        /// </summary>
		private EntityInfo lastEntityInfo;
        /// <summary>
        /// 当前帧操作的数据
        /// </summary>
		public EntityInfo entityInfo;

		public EntityActor ()
		{
		}

		public EntityInfo GetEntityInfo() {
			if(lastEntityInfo == null){
                lastEntityInfo = EntityInfo.CreateBuilder(entityInfo).Build();
            }
			var na1 = lastEntityInfo;
			return na1;
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

                lastEntityInfo.X = entityInfo.X;
                lastEntityInfo.Y = entityInfo.Y;
                lastEntityInfo.Z = entityInfo.Z;
			}

			if (entityInfo.UnitId != lastEntityInfo.UnitId) {
				na1.UnitId = entityInfo.UnitId;
				na1.Changed = true;

                lastEntityInfo.UnitId = entityInfo.UnitId;
			}

			if (entityInfo.SpawnId != lastEntityInfo.SpawnId) {
				na1.SpawnId = entityInfo.SpawnId;
				na1.Changed = true;

                lastEntityInfo.SpawnId = entityInfo.SpawnId;
			}
			if (entityInfo.HP != lastEntityInfo.HP) {
				na1.HP = entityInfo.HP;
				na1.Changed = true;

                lastEntityInfo.HP = entityInfo.HP;
			}
            if(entityInfo.Speed != lastEntityInfo.Speed)
            {
                na1.Speed = entityInfo.Speed;
                na1.Changed = true;

                lastEntityInfo.Speed = entityInfo.Speed;
            }

            if(entityInfo.TeamColor != lastEntityInfo.TeamColor)
            {
                na1.TeamColor = entityInfo.TeamColor;
                na1.Changed = true;
                lastEntityInfo.TeamColor = entityInfo.TeamColor;
            }

            if(entityInfo.Dir != lastEntityInfo.Dir)
            {
                na1.Dir = entityInfo.Dir;
                na1.Changed = true;
                lastEntityInfo.Dir = entityInfo.Dir;
            }

            //复制一遍当前的状态存储下来
            //直接field 拷贝 不用 复制整个了
			//lastEntityInfo = EntityInfo.CreateBuilder(entityInfo).Build();
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

		public void OnlyRemoveSelf() {
			ActorManager.Instance.RemoveActor (Id);
		}


        /*
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
        */

		//初始化实体的Id
		public void  InitInfo(EntityInfo info) {
			entityInfo = info;
			entityInfo.Id = Id;
            var unitData = Util.GetUnitData(false, entityInfo.UnitId, 0);
            entityInfo.HP = unitData.HP;

			lastEntityInfo = EntityInfo.CreateBuilder (info).Build();
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

        public Vector3 GetFloatPos()
        {
            var myVec = new MyVec3(entityInfo.X, entityInfo.Y, entityInfo.Z);
            return myVec.ToFloat();
        }

        public Vector2 GetVec2Pos()
        {
            var myVec = new MyVec3(entityInfo.X, entityInfo.Y, entityInfo.Z);
            var v3 =  myVec.ToFloat();
            return new Vector2(v3.X, v3.Z);
        }
        public MyVec3 GetIntPos()
        {
            var myVec = new MyVec3(entityInfo.X, entityInfo.Y, entityInfo.Z);
            return myVec;
        }
	}
}

