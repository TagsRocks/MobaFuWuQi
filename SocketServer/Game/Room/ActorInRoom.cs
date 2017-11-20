using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace MyLib
{
    public abstract class ActorInRoom : GameObjectActor
    {
        public abstract void InitAfterSetRoom();
        public abstract void HandleCmd(ActorMsg msg);
        public abstract void RunAI();
        public Vector3 GetFloatPos()
        {
            var myVec = GetIntPos();
            return myVec.ToFloat();
        }
        public abstract int GetUnitId();
        public Vector2 GetVec2Pos()
        {
            var v3 = GetFloatPos();
            return new Vector2(v3.X, v3.Z);
        }
        public abstract MyVec3 GetIntPos();

        public abstract int dir
        {
            get;
            set;
        }
        public abstract int IDInRoom
        {
            get;
        }
        public abstract int TeamColor
        {
            get;
        }
        //统一访问AvatarInfo和 EntityInfo成员
        public abstract dynamic DuckInfo
        {
            get;
        }
        public abstract bool IsPlayer
        {
            get;
        }
        public abstract void RemoveSelf();
        public abstract int Level
        {
            get;
        }

      
        //设置位置同时需要同步到物理世界中
        //需要外插值预测客户端位置 服务器上实际位置和 客户端发送的位置不同
        public void SetPos(MyVec3 p)
        {
            var curPos = GetFloatPos();
            DuckInfo.X = p.x;
            DuckInfo.Y = p.y;
            DuckInfo.Z = p.z;
            var newPos = GetFloatPos();
            GetComponent<AINPC>().Move(newPos-curPos);
        }

        public virtual void SetPosWithPhysic(Vector3 cp, Vector3 np)
        {
            throw new NotImplementedException();
        }

        public UnitData GetUnitData()
        {
            var uid = GetUnitId();
            return Util.GetUnitData(IsPlayer, uid, Level);
        }
    }
}
