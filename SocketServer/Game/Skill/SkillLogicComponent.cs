using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    abstract class SkillLogicComponent : GameObjectComponent
    {
        public SkillLayoutRunner runner;
        /// <summary>
        /// 开始执行技能逻辑
        /// </summary>
        public abstract void Run();
    }
}
