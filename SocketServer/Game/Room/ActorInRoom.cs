using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    public abstract class ActorInRoom : GameObjectActor
    {
        public abstract void InitAfterSetRoom();
        public abstract void HandleCmd(ActorMsg msg);
        public abstract void RunAI();
    }
}
