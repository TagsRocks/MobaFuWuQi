using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    public class FakeGameObject
    {
        public GameObjectActor go;
        public int InstId;
    }
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
        public bool IsDestroy = false;

        public int InstId;

        private RoomActor room;
        public void SetRoom(RoomActor r)
        {
            room = r;
        }
        private List<GameObjectActor> child = new List<GameObjectActor>();
        public string name;
        public GameObjectActor parent;
        /// <summary>
        /// 世界绝对坐标 Unity坐标*100
        /// </summary>
        public MyVec3 pos;
        public MyVec3 scale;
        public int GoDir = 0;//子弹的朝向

        public List<GameObjectActor> GetChildren()
        {
            return child;
        }
        /// <summary>
        /// GameObject不需要Dispatch
        /// 拒绝调用父类方法
        /// </summary>
        public override void Init()
        {
        }

        /// <summary>
        /// 创建父子关系 
        /// Entity需要在加入Room的时候 创建父子关系
        /// </summary>
        /// <param name="c"></param>
        public void AddChild(GameObjectActor c)
        {
            if (c.parent != null)
            {
                LogHelper.LogError("GameObject", "ReAddError:"+c.name+":"+c.parent.name+":"+this.name);
            }
            else
            {
                c.parent = this;
                child.Add(c);
                if (IsStart)
                {
                    c.Start();
                }
            }
        }
        public void RemoveChild(GameObjectActor c)
        {
            c.parent = null;
            child.Remove(c);
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
                //避免在Init过程中添加Component造成的循环遍历问题
                for(var i = 0; i < components.Count; i++)
                {
                    var component = components[i];
                    component.Init();
                }
                for(var i = 0; i < child.Count; i++)
                {
                    var gameObjectActor = child[i];
                    gameObjectActor.Start();
                }
            }
        }

        public override void Stop()
        {
            isStop = true;
            Destroy();
        }

        /// <summary>
        /// 退出Room销毁
        /// </summary>
        public void Destroy()
        {
            IsDestroy = true;
            if(parent != null)
            {
                //父亲没有删除 自己被删除了
                if (!parent.IsDestroy)
                {
                    parent.RemoveChild(this);
                }
            }
            //局部没有添加进入全局管理
            //ActorManager.Instance.RemoveActor(this.Id);
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
                //return GetRoom()._messageQueue;
                return null;
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
