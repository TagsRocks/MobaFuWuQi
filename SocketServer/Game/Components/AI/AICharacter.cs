using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib 
{
    public class AIEvent
    {
        public enum AIEventType
        {

        }
        public AIEventType type;
        public int intVal;
        public EntityProxy entityProxy;
        public CGPlayerCmd cmd;
    }

    public enum AIParams
    {
        CurrentPoint,
        Target,
        Command,
    }
    /// <summary>
    /// AI状态机 Lomotion状态
    /// 挂在GameObjectActor上的组件
    /// 支持事件
    /// 进入
    /// 退出
    /// 切换
    /// 添加
    /// </summary>
    public class AICharacter : GameObjectComponent
    {
        public AIState state;
        private Dictionary<AIStateEnum, AIState> stateMap = new Dictionary<AIStateEnum, AIState>();
        public AIState current;
        public Dictionary<AIParams, AIEvent> blackboard = new Dictionary<AIParams, AIEvent>();
        private AINPC _npc;
        public AINPC aiNpc
        {
            get
            {
                return _npc;
            }
        }
        public override void AfterAdd()
        {
            base.AfterAdd();
            _npc = gameObject.GetComponent<AINPC>();
        }

        public void AddState(AIState state)
        {
            stateMap[state.type] = state;
            state.aiCharacter = this;
            state.Init();
        }
        public void ChangeState(AIStateEnum s)
        {
            var next = stateMap[s];
            //避免重复进入状态 除非状态自己允许
            if (next == current)
            {
                return;
            }

            if(current != null)
            {
                if(!current.CheckNextState(s))
                {
                    return;
                }
                current.ExitState();
            }
            current = stateMap[s];
            var cur = current;
            //这里有可能切换状态
            current.EnterState();
            if (cur == current)
            {
                gameObject.RunTask(current.RunLogic);
            }else
            {
                LogHelper.LogError("AIState", "Switch State Too Early:"+gameObject.name+":"+cur.type);
            }
        }
  
        public void OnEvent(AIEvent evt)
        {
            if(current != null)
            {
                current.OnEvent(evt);
            }
        }
    }
}
