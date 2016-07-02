using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace MyLib 
{
    public class BoxCollider : GameObjectComponent
    {
        public string Layer;
        public Vector3 Center;
        public Vector3 Size;

        private PhysicWorldComponent physicWorld;
        public override void Init()
        {
            base.Init();
            var rm = GetRoom();
            physicWorld = rm.GetComponent<PhysicWorldComponent>();
            if (physicWorld != null)
            {
                physicWorld.AddStaticCollider(this);
            }
        }

    }
}
