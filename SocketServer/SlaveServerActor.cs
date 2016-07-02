using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleJSON;

namespace MyLib
{
    public class SlaveServerActor : Actor
    {
        private int maxRoomNum = 50;
        public async Task<string> GetStatus()
        {
            var lob = ActorManager.Instance.GetActor<Lobby>();
            var rn = await lob.GetRoomNum();
            var isFull = rn > maxRoomNum;
            var jobj = new SimpleJSON.JSONClass();
            jobj.Add("IsFull", new JSONData(isFull)); 
            jobj.Add("RoomNum", new JSONData(rn));
            return jobj.ToString();
        }
    }
}
