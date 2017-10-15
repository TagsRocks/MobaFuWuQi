using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    public class JobZone : GameObjectComponent
    {
        public float TimeToSpawn = 1;
        public int JobId = 107;

        private List<EntityActor> entities = new List<EntityActor>();

        private Random rd;
        private int SpawnId;

        public override void Init()
        {
            base.Init();
            rd = new Random();
            SpawnId = this.GetRoom().GetSpawnId();
            var rm = GetRoom();
            rm.RunTask(GenGoods);
        }

        private async Task GenGoods()
        {

            var p = this.gameObject.pos;
            var s = this.gameObject.scale;
            var xmin = p.x - s.x/2;
            var xmax = p.x + s.x/2;
            var zmin = p.z - s.z/2;
            var zmax = p.z + s.z/2;

            var rm = this.GetRoom();
            var waitTime = (int) (1000*TimeToSpawn);
            while (!rm.IsStop() && rm.GetState() != RoomActor.RoomState.GameOver)
            {
                await Task.Delay(waitTime);
                if (entities.Count == 0)
                {
                    var rx = (int) (rd.NextDouble()*(xmax - xmin) + xmin);
                    var ry = p.y;
                    var rz = (int) (rd.NextDouble()*(zmax - zmin) + zmin);

                    var entityInfo = EntityInfo.CreateBuilder();
                    entityInfo.ItemId = JobId;
                    entityInfo.ItemNum = 1;
                    entityInfo.X = rx;
                    entityInfo.Y = ry;
                    entityInfo.Z = rz;
                    entityInfo.SpawnId = SpawnId;
                    entityInfo.EType = EntityType.DROP;

                    var info = entityInfo.Build();
                    var ety = GetRoom().AddEntityInfo(info);
                    if (ety != null)
                    {
                        entities.Add(ety);
                        ety.removeCallback = () =>
                        {
                            RemoveFood(ety);
                        };
                    }
                }
            }
        }

        private async Task RemoveFood(EntityActor ety)
        {
            await this.GetRoom()._messageQueue;
            entities.Remove(ety);
        }
    }
}
