using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib 
{
    public class SpawnFood : GameObjectComponent
    {
        public float TimeToSpawn = 1;

        public override void Init()
        {
            LogHelper.Log("Room", "SpawnFood");
            base.Init();
            var rm = this.GetRoom();
            rm.RunTask(GenFood);
        }

        private async Task GenFood()
        {
            var children = this.gameObject.GetChildren();
            var foodZones = new List<FoodZone>();
            foreach (var gameObjectActor in children)
            {
                foodZones.Add(gameObjectActor.GetComponent<FoodZone>());
            }

            var waitTime = (int)(1000*TimeToSpawn);
            LogHelper.Log("Room", "TimeToSpawn: "+waitTime);
            while (!this.GetRoom().IsStop())
            {
                await Task.Delay(waitTime);
                foreach (var f in foodZones)
                {
                    await f.GenFood();
                }
            }
        }
    }
}
