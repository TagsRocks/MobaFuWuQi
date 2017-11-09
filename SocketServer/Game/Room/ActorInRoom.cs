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
    }
}
