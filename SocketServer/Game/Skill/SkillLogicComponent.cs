using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    abstract class SkillLogicComponent : GameObjectComponent
    {
        public abstract void Run();
    }
}
