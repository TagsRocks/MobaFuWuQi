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
	public class EntityActor :  ActorInRoom 
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
			var na1 = lastEntityInfo;
			return na1;
		}


        public override string GetAttr()
	    {
	        if (GetRoom() != null)
	        {
	            return "room: "+GetRoom().Id;
	        }
	        return "";
	    }

	    public EntityInfo GetEntityInfoDiff() {
			var na1 = EntityInfo.CreateBuilder();
			na1.Id = entityInfo.Id;
			if (lastEntityInfo.X != entityInfo.X 
                || lastEntityInfo.Y != entityInfo.Y || lastEntityInfo.Z != entityInfo.Z
                || lastEntityInfo.SpeedX != entityInfo.SpeedX
                || lastEntityInfo.SpeedY != entityInfo.SpeedY
                ) {
				na1.X = entityInfo.X;
				na1.Y = entityInfo.Y;
				na1.Z = entityInfo.Z;
                na1.SpeedX = entityInfo.SpeedX;
                na1.SpeedY = entityInfo.SpeedY;

				na1.Changed = true;

                lastEntityInfo.X = entityInfo.X;
                lastEntityInfo.Y = entityInfo.Y;
                lastEntityInfo.Z = entityInfo.Z;
                lastEntityInfo.SpeedX = entityInfo.SpeedX;
                lastEntityInfo.SpeedY = entityInfo.SpeedY;
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
            /*
            if(entityInfo.SpeedX != lastEntityInfo.SpeedX || entityInfo.SpeedY != lastEntityInfo.SpeedY)
            {
                na1.SpeedX = entityInfo.SpeedX;
                na1.SpeedY = entityInfo.SpeedY;

                na1.Changed = true;
                lastEntityInfo.SpeedX = entityInfo.SpeedX;
                lastEntityInfo.SpeedY = entityInfo.SpeedY;
            }
            */

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

            //直接field 拷贝 不用 复制整个了
            return na1.Build();
		}


		//初始化实体的Id
        //需要Info的iD 被修改 用于NewEntity中的同步
		public void  InitInfo(EntityInfo info) {
            entityInfo = info; 
			entityInfo.Id = Id;
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
     
    

        #region ActorInROOM
        public override void InitAfterSetRoom()
        {
            throw new NotImplementedException();
        }
        public override void HandleCmd(ActorMsg msg)
        {
            throw new NotImplementedException();
        }
        public override void RunAI()
        {
            throw new NotImplementedException();
        }
        public override int GetUnitId()
        {
            return entityInfo.UnitId;
        }

        public override MyVec3 GetIntPos()
        {
            var myVec = new MyVec3(entityInfo.X, entityInfo.Y, entityInfo.Z);
            return myVec;
        }
        public override int dir
        {
            get
            {
                return entityInfo.Dir;
            }

            set
            {
                entityInfo.Dir = value;
            }
        }
        public override int IDInRoom
        {
            get
            {
                return Id;
            }
        }
        public override int TeamColor
        {
            get
            {
                return entityInfo.TeamColor;
            }
        }
        public override dynamic DuckInfo
        {
            get
            {
                return entityInfo;
            }
        }
        public override bool IsPlayer
        {
            get
            {
                return false;
            }
        }

        public Action removeCallback = null;
        public override void RemoveSelf()
        {
            if (GetRoom() != null)
            {
                GetRoom().RemoveEntity(this, entityInfo);
            }
            //不属于全局管理
            this.Stop();
            if (removeCallback != null)
            {
                removeCallback();
            }
        }
        public override int Level
        {
            get
            {
                return 0;
            }
        }
        #endregion
    }
}

