using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    public class NpcAttribute : GameObjectComponent
    {
        public enum State
        {
            Normal,
            Dead,
        }
        public State state = State.Normal;
        public bool IsDead()
        {
            return state == State.Dead;
        }

        public ActorInRoom mySelf;
        public UnitData unitData
        {
            get
            {
                return ai.unitData;
            }
        }
        public NpcConfig npcConfig
        {
            get
            {
                return ai.npcConfig;
            }
        }
        private AINPC ai;
        public override void Init()
        {
            base.Init();
            mySelf = gameObject as ActorInRoom;
            ai = gameObject.GetComponent<AINPC>();
        }

        public int killerTeamColor;

        public void DoHurt(ActorInRoom attacker, int damage)
        {
            var modifies = gameObject.GetComponent<ModifyComponent>();
            damage -= modifies.totalDefenseAdd;
            damage = Math.Max(0, damage);

            mySelf.DuckInfo.HP -= damage;
            mySelf.DuckInfo.HP = MathUtil.Clamp(mySelf.DuckInfo.HP, 0, unitData.HP);
            if(mySelf.DuckInfo.HP <= 0)
            {
                killerTeamColor = attacker.TeamColor;
                state = State.Dead;
                gameObject.GetComponent<AICharacter>().ChangeState(AIStateEnum.DEAD);

                if (!mySelf.IsPlayer)
                {
                    var gold = ai.npcConfig.dropGold;
                    if(attacker != null && attacker.IsPlayer)
                    {
                        var pinRoom = attacker as PlayerInRoom;
                        pinRoom.GetAvatarInfo().Gold += gold;
                        pinRoom.AddExp(ai.npcConfig.XPGain);
                    }
                }
            }

            ai.aiCharacter.blackboard[AIParams.Attacker] = new AIEvent()
            {
                actor = attacker,
            };
        }

        public void DoHeal(int healNum)
        {
            mySelf.DuckInfo.HP += healNum;
            mySelf.DuckInfo.HP = MathUtil.Clamp(mySelf.DuckInfo.HP, 0, unitData.HP);
        }

        public void DoRevive()
        {
            state = State.Normal;
        }
        public void RemoveSelf()
        {
            mySelf.RemoveSelf();
        }
        public void UpdateLevel()
        {
            ai.UpdateLevel();
        }
    }
}
