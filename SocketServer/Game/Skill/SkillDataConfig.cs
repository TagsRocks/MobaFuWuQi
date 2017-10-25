using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    /// <summary>
    /// 技能相关事件触发配置
    /// </summary>
    public enum SkillEvent
    {
        EventTrigger,
        EventMissileDie,
        EventStart,
    }
    /// <summary>
    ///技能层相关配置  
    /// </summary>
    [System.Serializable]
    public class EventItem
    {
        /// <summary>
        /// 事件类型： 技能开始，技能命中，子弹命中
        /// </summary>
        public SkillEvent evt;
        /// <summary>
        /// 技能层配置对象  直接指向
        /// </summary>
        public FakeGameObject layout;

        /*
        /// <summary>
        /// 技能Buff
        /// </summary>
        //public Affix affix;

        //DamageShape 会带着玩家一起移动  向前冲击技能需要带动玩家一起运动
        public bool attachOwner = false;
        //将粒子效果附加到玩家身上
        public bool attaches = false;

        //陷阱技能会衍生出子技能 创建另外一个技能的技能状态机
        public int childSkillId = -1;

        //电击爆炸出现在目标身上 粒子效果附着到目标身上
        public bool AttachToTarget = false;

        //设置Beam的粒子效果的GravityWell BeamTarget 的目标为Enemy的坐标  闪电类型的粒子瞬间攻击一定命中目标 目标头顶召唤闪电
        public bool SetBeamTarget = false;
        public Vector3 BeamOffset = new Vector3(0, 1, 0);

        //出现在目标所在位置
        public bool TargetPos = false;
        //使用上次事件标记的位置 魔兽争霸3中的 血法师的 召唤火焰的技能 
        public bool UseMarkPos = false;

        public int EvtId = 0; //当前技能的事件ID编号
        */

    }

    public class SkillDataConfig : GameObjectComponent
    {
        /// <summary>
        /// 所有技能层列表 
        /// </summary>
		public List<EventItem> eventList;
        /*
        //循环播放idle动画 按照攻击频率来 释放技能 火焰陷阱  某些技能循环播放动画 持续性的攻击技能 喷射火焰
        public bool animationLoop = false;
        //持续攻击时间长度
        public float attackDuration = 1;
        */
    }
}
