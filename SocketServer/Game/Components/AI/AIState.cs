using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    public enum AIStateEnum
    {
        IDLE,
        MOVE,
        ATTACK,
        DEAD,
        REVIVE,
        GO_BACK,
    }

    public class AIState
    {
        public AIStateEnum type;
        public AICharacter aiCharacter;
        protected bool inState = false;
        public int runNum = 0;
        /// <summary>
        /// 添加完状态之后首次初始化
        /// </summary>
        public virtual void Init()
        {

        }
        public virtual void EnterState()
        {
            inState = true;
            runNum++;
        }
        public virtual void ExitState()
        {
            inState = false;
        }

        public bool CheckInState(int tempNum)
        {
            return inState && tempNum == runNum;
        }

        public virtual bool CheckNextState(AIStateEnum next)
        {
            return true;
        }

        public virtual void OnEvent(AIEvent evt)
        {

        }

        public virtual async Task RunLogic()
        {

        }

    }

    public class IdleState : AIState
    {
        public IdleState()
        {
            type = AIStateEnum.IDLE;
        }
    }
    public class MoveState : AIState
    {
        public MoveState()
        {
            type = AIStateEnum.MOVE;
        }
    }
    public class AttackState : AIState
    {
        public AttackState()
        {
            type = AIStateEnum.ATTACK;
        }
    }
    public class DeadState : AIState
    {
        public DeadState()
        {
            type = AIStateEnum.DEAD;
        }
    }
    public class ReviveState : AIState
    {
        public ReviveState()
        {
            type = AIStateEnum.REVIVE;
        }
    }
    public class GoBackState : AIState
    {
        public GoBackState()
        {
            type = AIStateEnum.GO_BACK;
        }
    }
}
