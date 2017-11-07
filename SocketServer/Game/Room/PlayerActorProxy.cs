using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    //避免Room和PlayerActor互相直接操作
    //线程安全的调用
    public class PlayerActorProxy
    {
        public int roomId
        {
            get;
            private set;
        }

        PlayerActor player;
        PlayerInRoom inRoom;

        public PlayerActorProxy(PlayerActor actor, PlayerInRoom inr)
        {
            player = actor;
            inRoom = inr;
        }
        public async Task InitProxy()
        {
            roomId = inRoom.GetRoom().Id;
            await player.SetProxy(this);
        }

        public async Task HandleCmd(ActorMsg msg)
        {
            await inRoom.GetRoom()._messageQueue;
            inRoom.HandleCmd(msg);
        }

        public void ConnectClose()
        {
            player.ConnectCloseAsync();
        }

        public void SendPacket(GCPlayerCmd.Builder cmd, byte flowId, byte errorCode)
        {
            player.SendPacketAsync(cmd, flowId, errorCode);
        }
        /*
        public void ForceUDP(GCPlayerCmd.Builder cmd, byte flowId, byte errorCode)
        {
            player.ForceUDP(cmd, flowId, errorCode);
        }
        public void UseUDP()
        {
            player.UseUDP();
        }
        public void UDPLost()
        {
            player.UDPLost();
        }
        */
    }
}
