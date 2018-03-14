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
    public class ActionConfig
    {
        public ActionType type = ActionType.Attack;
        public float totalTime;
        public float hitTime;
        public string aniName = "AbilityR";
        public float skillAttackRange = 8; //伤害的范围大小
        public float skillAttackTargetDist = 8; //锁定目标的距离
        public int skillId;//技能伤害计算的ID
        public bool needEnemy = false; //锁定目标技能必须有目标才可以释放 
    }

    public class NpcConfig : GameObjectComponent
    {
        public bool IsPlayer = false;
        public int npcTemplateId;
        public List<ActionConfig> actionList;
        /// <summary>
        /// 塔和NPC的技能模板
        /// 只有一个技能
        /// </summary>
        public string normalAttack = "monsterSingle";
        public float eyeSightDistance = 10;  //搜索敌人的范围
        public float attackRangeDist = 10; //攻击范围
        public int attackSkill = 1;
        public float moveSpeed = 5;
        public float damageToTower = 1.0f;
        public float maxMoveRange2 = 11;
        public float hpRecover = 0;

        public ActionConfig GetAction(ActionType tp)
        {
            foreach(var a in actionList)
            {
                if(a.type == tp)
                {
                    return a;
                }
            }
            return null;
        }
        public ActionConfig GetActionBySkillId(int skillId)
        {
            foreach(var a in actionList)
            {
                if(a.skillId == skillId)
                {
                    return a;
                }
            }
            return null;
        }

    }
}
