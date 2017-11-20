using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace MyLib
{
    public sealed class WaitForNextFrameAwaiter : INotifyCompletion
    {
        private Action act;
        private readonly WaitForNextFrame _context;
        public WaitForNextFrameAwaiter(WaitForNextFrame context)
        {
            //if (context == null) throw new ArgumentNullException("context");
            _context = context;
            _context.room.AddAwaiter(this);
        }
        /// <summary>
        /// 快速检测 如果已经下一帧直接执行 否则压入等待队列
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                //已经在当前上下文里面了，就不需要再次切换上下文了
                //return SynchronizationContext.Current == _context;

                //var fid = _context.room.GetFrameId();
                //return (fid >= _context.nextFrame);
                return false;
            }
        }

        /// <summary>
        /// 下一帧到来直接调用OnComplete方法即可
        /// </summary>
        /// <param name="action">Action.</param>
        public void OnCompleted(Action action)
        {
            act = action;
        }

        public void Run()
        {
            try
            {
                act();
            }
            catch(Exception exp)
            {
                LogHelper.LogError("WaitForNextFrame", exp.ToString());
            }
        }

        public void GetResult() { }
    }

    /// <summary>
    /// 从RoomActor中捕获到下一Frame
    /// 当RoomActor FrameId
    /// 
    /// RoomActor如何通知Awaitor执行结束？
    /// </summary>
    public struct WaitForNextFrame
    {
        public RoomActor room;
        //public ulong nextFrame;
        public WaitForNextFrame(RoomActor r)
        {
            room = r;
            //nextFrame = room.GetFrameId() + 1;
        }
        public WaitForNextFrameAwaiter GetAwaiter()
        {
            return new WaitForNextFrameAwaiter(this);
        }
    }
}
