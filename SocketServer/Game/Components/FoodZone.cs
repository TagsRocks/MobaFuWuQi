using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    public class FoodZone : GameObjectComponent
    {
        private List<EntityActor> foods = new List<EntityActor>();
        private Random rd;
        private int SpawnId;
        public int FoodMaxNum = 2;
        public override void Init()
        {
            base.Init();
            rd = new Random();
            SpawnId = this.GetRoom().GetSpawnId();
        }

        

        public async Task GenFood()
        {
            var p = this.gameObject.pos;
            var s = this.gameObject.scale;
            var xmin = p.x - s.x/2;
            var xmax = p.x + s.x/2;
            var zmin = p.z - s.z/2;
            var zmax = p.z + s.z/2;

            if (foods.Count < FoodMaxNum)
            {
                //LogHelper.Log("Room", "AddFood");
                var rx = (int) (rd.NextDouble()*(xmax - xmin) + xmin);
                var ry = p.y;
                var rz = (int) (rd.NextDouble()*(zmax - zmin) + zmin);

                var entityInfo = EntityInfo.CreateBuilder();
                entityInfo.ItemId = 101;
                entityInfo.ItemNum = 1;
                entityInfo.X = rx;
                entityInfo.Y = ry;
                entityInfo.Z = rz;
                entityInfo.SpawnId = SpawnId;
                entityInfo.EType = EntityType.DROP;

                var info = entityInfo.Build();
                var ety = await this.GetRoom().AddEntityInfo(info);
                if (ety != null)
                {
                    foods.Add(ety);
                    ety.removeCallback = () =>
                    {
                        //foods.Remove(ety);
                        RemoveFood(ety);
                    };
                }
            }
        }

        //线程安全这个方法是被EntityActor 调用的 EntityActor 需要使用 Room的_messageContext 才可以
        private async Task RemoveFood(EntityActor ety)
        {
            await this.GetRoom()._messageQueue;
            foods.Remove(ety);
        }
    }
}
