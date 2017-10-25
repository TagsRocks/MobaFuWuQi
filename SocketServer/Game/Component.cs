using System;

namespace MyLib 
{
	public class Component
	{
		public Actor actor;
		public Component ()
		{
		}

        /// <summary>
        /// 添加组件到Actor上结束
        /// Component 的组件可以在这里初始化
        /// </summary>
	    public virtual void AfterAdd()
	    {
	        
	    }
        /// <summary>
        /// GameObjectActor  Start时调用Init 初始化组件
        /// 或者GameObjectActor 已经Start了则在 AddComponent时调用
        /// GameObject组件在这里初始化
        /// 
        /// 当GameObject 作为孩子节点添加到一个已经Start的GameObject上时 Init也会被触发
        /// </summary>
	    public virtual void Init()
	    {
	        
	    }

        /// <summary>
        /// 退出Room 摧毁GameObject时 摧毁Component
        /// </summary>
	    public virtual void Destroy()
	    {
	        
	    }
	}
}

