using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    class PlayerAI : AINPC
    {
        public override void Init()
        {
            base.Init();
            aiCharacter = gameObject.AddComponent<AICharacter>();
            aiCharacter.AddState(new PlayerIdle());
            aiCharacter.AddState(new PlayerMove());
            aiCharacter.AddState(new PlayerAttack());
            aiCharacter.AddState(new PlayerDead());
            aiCharacter.AddState(new PlayerRevive());
        }

        public override void AfterInGame()
        {
            base.AfterInGame();
            gameObject.RunTask(HPRecover);
            gameObject.RunTask(GoldAdd);
        }

        private async Task GoldAdd()
        {
            while (!gameObject.IsDestroy)
            {
                await Task.Delay(5000);
                var pinRoom = mySelf as PlayerInRoom;
                var avatarInfo = pinRoom.GetAvatarInfo();
                avatarInfo.Gold += 1;
            }
        } 

        private async Task HPRecover()
        {
            var rec = npcConfig.hpRecover;
            var addHP = 0.0f;
            while (!gameObject.IsDestroy)
            {
                await Task.Delay(1000);
                addHP += (unitData.HP * rec);
                if (addHP > 1)
                {
                    var add = (int)addHP;
                    addHP -= add;
                    attribute.DoHeal(add);
                } 
            }
        }
    }
}
