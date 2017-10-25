using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    class SkillComponent : GameObjectComponent
    {
        private CreepAI ai;
        public override void Init()
        {
            base.Init();
            ai = gameObject.GetComponent<CreepAI>();
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
    }
}
