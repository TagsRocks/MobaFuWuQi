using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    class NpcAttribute : GameObjectComponent
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

        public EntityActor mySelf;
        private UnitData unitData;
        public override void Init()
        {
            base.Init();
            mySelf = gameObject as EntityActor;
            unitData = Util.GetUnitData(false, mySelf.entityInfo.UnitId, 0);
        }

        public void DoHurt(int damage)
        {
            mySelf.entityInfo.HP -= damage;
            mySelf.entityInfo.HP = MathUtil.Clamp(mySelf.entityInfo.HP, 0, unitData.HP);
            if(mySelf.entityInfo.HP == 0)
            {
                state = State.Dead;
                gameObject.GetComponent<AICharacter>().ChangeState(AIStateEnum.DEAD);
            }
        }
        public void RemoveSelf()
        {
            mySelf.RemoveSelf();
        }
    }
}
