using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    /// <summary>
    /// 动作类型
    /// </summary>
    public enum ActionType
    {
        None,
        Attack,
    }

    [System.Serializable]
    public struct ActionConfig
    {
        public ActionType type;
        public float totalTime;
        public float hitTime;
    }
    public class NpcConfig : GameObjectComponent
    {
        public bool IsPlayer = false;
        public int npcTemplateId;
        public List<ActionConfig> actionList;
        public string normalAttack = "monsterSingle";
        public float eyeSightDistance = 10;
        public float attackRangeDist = 10;
        public int attackSkill = 1;
        public float moveSpeed = 5;

        public ActionConfig GetAction(ActionType tp)
        {
            foreach(var a in actionList)
            {
                if(a.type == tp)
                {
                    return a;
                }
            }
            return new ActionConfig() { type = ActionType.None};
        }

    }
}
