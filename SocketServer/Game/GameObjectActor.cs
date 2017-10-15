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
    /// 
    /// TODO:改成单线程对象 不要Actor了
    /// </summary>
    public class GameObjectActor : Actor
    {
        public bool IsStart = false;
        public RoomActor room;
        private List<GameObjectActor> child = new List<GameObjectActor>();
        public string name;
        public GameObjectActor parent;
        public MyVec3 pos;
        public MyVec3 scale;

        public GameObjectActor[] GetChildren()
        {
            return child.ToArray();
        } 

        /// <summary>
        /// 创建父子关系 
        /// Entity需要在加入Room的时候 创建父子关系
        /// </summary>
        /// <param name="c"></param>
        public void AddChild(GameObjectActor c)
        {
            c.parent = this;
            child.Add(c);
        }

        /// <summary>
        /// 进入Room 初始化
        /// 初始化各个组件
        /// </summary>
        public void Start()
        {
            if (!IsStart)
            {
                IsStart = true;
                foreach (var component in components)
                {
                    component.Init();
                }
                foreach (var gameObjectActor in child)
                {
                    gameObjectActor.Start();
                }
            }
        }

        /// <summary>
        /// 退出Room销毁
        /// </summary>
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

        /// <summary>
        /// 单线程GameObjectActor 只能在Room中执行Task
        /// </summary>
        /// <param name="cb"></param>
        public override void RunTask(Func<Task> cb)
        {
            GetRoom().RunTask(cb);
        }
        /// <summary>
        /// 没有并行 使用Room的Actor
        /// </summary>
        public override ActorSynchronizationContext _messageQueue
        {
            get
            {
                return GetRoom()._messageQueue;
            }
        }

        /// <summary>
        /// 递归查找所在的Room
        /// </summary>
        private RoomActor roomAct = null;
        public RoomActor GetRoom()
        {
            if (roomAct == null)
            {
                var act = this;
                while (act != null && act.room == null)
                {
                    act = act.parent;
                }
                roomAct = act.room;
            }
            return roomAct;
        }
        
    }
}
