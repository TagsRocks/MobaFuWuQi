using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    class SkillComponent : GameObjectComponent
    {
        private AINPC aiNpc;
        public override void Init()
        {
            base.Init();
            aiNpc = gameObject.GetComponent<AINPC>();
        }

        public SkillStateMachine CreateSkillStateMachine(SkillAction skillAct, string machineConfig)
        {
            var config = SkillDataManager.Instance.GetConfig(machineConfig);
            var stateMachine = EntityImport.InitGameObject(config);
            //生命周期如何管理 和技能绑定的一段状态执行逻辑
            var sk = stateMachine.AddComponent<SkillStateMachine>();
            sk.action = skillAct;
            gameObject.AddChild(stateMachine);
            return sk;
        }


        /// <summary>
        /// 根据玩家状态检查是否可以使用技能
        /// </summary>
        /// <param name="skillAct"></param>
        /// <param name="npcConfig"></param>
        /// <returns></returns>
        public bool CheckCanUseSkill(SkillAction skillAct, NpcConfig npcConfig)
        {
            var skId = skillAct.SkillId;
            var target = skillAct.Target;
            var actConfig = npcConfig.GetActionBySkillId(skId);
            if (actConfig.needEnemy && target == 0)
            {
                return false;
            }

            return true;
        }

        public float GetAttackTargetDist(int skillId)
        {
            var actConfig = aiNpc.npcConfig.GetActionBySkillId(skillId);
            return actConfig.skillAttackTargetDist;
        }
    }
}
