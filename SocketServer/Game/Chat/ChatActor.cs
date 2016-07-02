using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SimpleJSON;

namespace MyLib
{
    public class ChatInfo
    {
        public string who;
        public string content;
        public long ord;
    }

    internal class ChatActor : Actor
    {
        private Queue<ChatInfo> messages = new Queue<ChatInfo>();
        private long curOrd = 0;
        private BroadcastBlock<long> broadcaster = new BroadcastBlock<long>(null);

        public override void Init()
        {

        }

        public async Task AddChat(string who, string content)
        {
            await this._messageQueue;
            LogHelper.Log("Chat", "Add: "+who+" con "+content);
            messages.Enqueue(new ChatInfo()
            {
                who = who,
                content = content,
                ord = curOrd++,
            });

            while (messages.Count > 30)
            {
                messages.Dequeue();
            }
            broadcaster.SendAsync(curOrd - 1);
        }


        public async Task<string> GetChatMsg(int ord)
        {
            await this._messageQueue;
            if (ord >= curOrd)
            {
                while (!isStop)
                {
                    long ret = 0;
                    ret = await broadcaster.ReceiveAsync();
                    if (ret < ord)
                    {
                        broadcaster = new BroadcastBlock<long>(null); //重新建立一个流
                        ret = await broadcaster.ReceiveAsync();
                    }
                    else
                    {
                        break;
                    }
                }
            }

            var js = new JSONClass();
            var jarr = new JSONArray();
            js.Add("chat", jarr);
            foreach (var chatInfo in messages)
            {
                if (chatInfo.ord >= ord)
                {
                    var jobj = new JSONArray();
                    jobj.Add(chatInfo.who);
                    jobj.Add(chatInfo.content);
                    jobj.Add(new JSONData(chatInfo.ord));
                    jarr.Add(jobj);
                }
            }
            return js.ToString();
        }

    }
}
