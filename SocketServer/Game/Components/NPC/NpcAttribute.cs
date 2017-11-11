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
        private UnitData unitData
        {
            get
            {
                return ai.unitData;
            }
        }
        private AINPC ai;
        public override void Init()
        {
            base.Init();
            mySelf = gameObject as ActorInRoom;
            ai = gameObject.GetComponent<AINPC>();
        }

        public void DoHurt(int damage)
        {
            mySelf.DuckInfo.HP -= damage;
            mySelf.DuckInfo.HP = MathUtil.Clamp(mySelf.DuckInfo.HP, 0, unitData.HP);
            if(mySelf.DuckInfo.HP == 0)
            {
                state = State.Dead;
                gameObject.GetComponent<AICharacter>().ChangeState(AIStateEnum.DEAD);
            }
        }
        public void DoRevive()
        {
            state = State.Normal;
        }
        public void RemoveSelf()
        {
            mySelf.RemoveSelf();
        }
    }
}
