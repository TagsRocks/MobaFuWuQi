using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib { 
    public static class MobaUtil
    {
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
