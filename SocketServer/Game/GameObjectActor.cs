using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    /// <summary>
    /// 如何处理GameObject 和 Actor的关系
    /// GameObject 可以作为独立的Actor 来执行逻辑
    /// GameObject 作为Actor 的一个成员附着到Actor 上面
    /// 解析配置结构 挂上不同的组件执行代码逻辑
    /// 
    /// 但是不受ActorManager 管理
    /// </summary>
    public class GameObjectActor : Actor
    {
        public RoomActor room;
        private List<GameObjectActor> child = new List<GameObjectActor>();
        public string name;
        public GameObjectActor parent;
        public Vector3 pos;
        public Vector3 scale;

        public GameObjectActor[] GetChildren()
        {
            return child.ToArray();
        } 
        public void AddChild(GameObjectActor c)
        {
            c.parent = this;
            child.Add(c);
        }

        public void Start()
        {
            foreach (var component in components)
            {
                component.Init();
            }
            foreach (var gameObjectActor in child)
            {
                gameObjectActor.Start();
            }
        }

        public void Destroy()
        {
            ActorManager.Instance.RemoveActor(this.Id);
            foreach (var component in components)
            {
                component.Destroy();
            }
            foreach (var gameObjectActor in child)
            {
                gameObjectActor.Destroy();
            }
        }
    }
}
