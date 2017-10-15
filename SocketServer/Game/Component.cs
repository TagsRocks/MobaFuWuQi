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
        /// </summary>
	    public virtual void AfterAdd()
	    {
	        
	    }
        /// <summary>
        /// GameObjectActor  Start时调用Init 初始化组件
        /// 或者GameObjectActor 已经Start了则在 AddComponent时调用
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

