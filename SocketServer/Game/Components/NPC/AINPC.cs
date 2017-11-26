using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace MyLib
{
    /// <summary>
    /// 初始化AI状态机
    /// 联系玩家属性和AI
    /// 组装玩家各种属性
    /// </summary>
    public abstract class AINPC : GameObjectComponent
    {
        public AICharacter aiCharacter;
        public PhysicManager physic;
        public EntityProxy proxy;
        public NpcConfig npcConfig;
        public UnitData unitData;
        public ActorInRoom mySelf;
        public NpcAttribute attribute;
        public override void Init()
        {
            base.Init();
            mySelf = gameObject as ActorInRoom;
            physic = GetRoom().GetComponent<PhysicManager>();
            proxy = physic.AddEntity(mySelf);

            gameObject.AddComponent<MoveController>();
            gameObject.AddComponent<SkillComponent>();
            attribute = gameObject.AddComponent<NpcAttribute>();
        }

        //玩家需要等待选择了角色之后才能初始化
        public virtual void AfterSelectHeroInit()
        {
            npcConfig = NpcDataManager.Instance.GetConfig(mySelf.GetUnitId());
            unitData = Util.GetUnitData(mySelf.IsPlayer, mySelf.GetUnitId(), mySelf.Level);
            mySelf.DuckInfo.HP = unitData.HP;
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

        public float GetAttackRadiusSquare()
        {
            return npcConfig.attackRangeDist * npcConfig.attackRangeDist;
        }

      
        public void Move(Vector3 disp)
        {
            physic.MoveEntity(ref proxy, disp);
        }
    }
}
