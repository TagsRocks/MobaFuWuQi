using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib { 
    public static class MobaUtil
    {
        public static void SetSkillActionPos(SkillAction skAct, ActorInRoom myself)
        {
            var pos = myself.GetIntPos();
            skAct.X = pos.x;
            skAct.Y = pos.y;
            skAct.Z = pos.z;
        }

        public static EntityProxy FindNearestEne(List<EntityProxy> proxes, float dist, ActorInRoom mySelf)
        {
            var myPos = mySelf.GetVec2Pos();
            var minDist = dist * dist;
            var findProxy = new EntityProxy();
            findProxy.ProxyId = -1;
            foreach (var e in proxes)
            {
                if (e.actor.TeamColor != mySelf.TeamColor && !e.actor.GetComponent<NpcAttribute>().IsDead())
                {
                    var enePos = e.actor.GetVec2Pos();
                    var newDist = (myPos - enePos).LengthSquared();
                    if (newDist < minDist)
                    {
                        minDist = newDist;
                        findProxy = e;
                    }
                }
            }
            return findProxy;
        }

        public static void SyncDead(AICharacter aiCharacter)
        {
            if (aiCharacter.aiNpc.mySelf.IsPlayer)
            {
                SyncPlayerDead(aiCharacter);
            }else
            {
                SyncEtyDead(aiCharacter);
            }
        }

        private static void SyncEtyDead(AICharacter aiCharacter)
        {
            var gc = GCPlayerCmd.CreateBuilder();
            gc.Result = "DeadActor";
            gc.ActorId = aiCharacter.aiNpc.mySelf.IDInRoom;
            var etyInfo = EntityInfo.CreateBuilder();
            etyInfo.Id = aiCharacter.aiNpc.mySelf.IDInRoom;
            gc.EntityInfo = etyInfo.Build();
            var myself = aiCharacter.aiNpc.mySelf;
            myself.GetRoom().AddKCPCmd(gc);
        }

        private static void SyncPlayerDead(AICharacter aiCharacter)
        {
            var gc = GCPlayerCmd.CreateBuilder();
            gc.Result = "DeadActor";
            gc.ActorId = aiCharacter.aiNpc.mySelf.IDInRoom;
            var etyInfo = AvatarInfo.CreateBuilder();
            etyInfo.Id = aiCharacter.aiNpc.mySelf.IDInRoom;
            gc.AvatarInfo = etyInfo.Build();
            var myself = aiCharacter.aiNpc.mySelf;
            myself.GetRoom().AddKCPCmd(gc);
        }


        public static void SetPos(dynamic DuckInfo, dynamic DuckPos)
        {
            DuckInfo.X = DuckPos.X;
            DuckInfo.Y = DuckPos.Y;
            DuckInfo.Z = DuckPos.Z;
        }

    }
}
