using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    class AINPC : GameObjectComponent
    {
        public AICharacter aiCharacter;
        public PhysicManager physic;
        public EntityProxy proxy;
        public NpcConfig npcConfig;
        public UnitData unitData;
        public EntityActor mySelf;
        public NpcAttribute attribute;
        public override void Init()
        {
            base.Init();
            mySelf = gameObject as EntityActor;
            physic = GetRoom().GetComponent<PhysicManager>();
            proxy = physic.AddEntity(mySelf);

            gameObject.AddComponent<MoveController>();
            gameObject.AddComponent<SkillComponent>();
            attribute = gameObject.AddComponent<NpcAttribute>();

            npcConfig = NpcDataManager.Instance.GetConfig(mySelf.entityInfo.UnitId);
            unitData = Util.GetUnitData(false, mySelf.entityInfo.UnitId, 0);

        }

        public virtual void RunAI()
        {
            aiCharacter.ChangeState(AIStateEnum.IDLE);
        }

        public override void Destroy()
        {
            base.Destroy();
            physic.RemoveEntity(proxy);
        }
    }
}
