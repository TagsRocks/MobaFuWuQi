using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MyLib;
using System.Net.Http;
using System.Web;
using SimpleJSON;

namespace SocketServer.Game
{
    public class HttpServerActor : Actor
    {
        private HttpListener httpListener;
        public override void Init()
        {
            RunTask(RunHttp);
        }

        private async Task RunHttp()
        {
            var httpPort = ServerConfig.instance.configMap["HttpServerListenPort"].AsInt;
            httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://*:" + httpPort + "/");
            LogHelper.Log("Http", "StartServer: " + httpPort);
            httpListener.Start();
            while (!IsStop() && httpListener.IsListening)
            {
                var context = await httpListener.GetContextAsync();
                //var context = httpListener.GetContext();
                var req = context.Request;
                LogHelper.Log("Http",
                    "HttpRequest: " + req.Url + " qu " + req.QueryString + " raw " + req.RawUrl + " me " +
                    req.HttpMethod);
                Handle(context, req);
            }
            httpListener.Stop();
            httpListener.Close();
        }

        private async Task Handle(HttpListenerContext context, HttpListenerRequest req)
        {
            var resp = await HandleGet(req);
            var buf = Encoding.UTF8.GetBytes(resp);
            context.Response.AddHeader("Content-Encoding", "utf-8");
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.ContentLength64 = buf.Length;
            LogHelper.Log("Http", "SendResponse");
            try
            {
                context.Response.OutputStream.Write(buf, 0, buf.Length);
            }
            catch (Exception exp)
            {
                LogHelper.Log("Http", exp.ToString());
            }
            finally
            {
                context.Response.OutputStream.Close();
            }
        }

        private async Task<string> HandleGet(HttpListenerRequest req)
        {
            var rawUrl = req.RawUrl;
            var queryPart = req.RawUrl.Substring(rawUrl.IndexOf("?") + 1);

            if (req.HttpMethod == "GET")
            {
                if (rawUrl == "/")
                {

                    var rets = ("<html><body><h1>Server Status Query</h1>");
                    rets += ("<form method=get action=/query>");
                    rets += ("<input type=text name=flags value=all>");
                    rets += ("<input type=submit value=query>");
                    rets += ("</form>");

                    rets += ("<h1>停服通告 IsClose true 停服 false 开服</h1>");
                    rets += ("<form method=get action=/SetClose>");
                    rets += ("<h2>IsClose</h2>");
                    rets += ("<input type=text name=IsClose value=false>");
                    rets += ("<h2>消息</h2>");
                    rets += ("<input type=text name=CloseMsg value=小学生不要玩游戏哦>");
                    rets += ("<input type=submit value=Run>");
                    rets += ("</form>");
                    rets += ("</body></html>");

                    return rets;
                }
                else if (rawUrl.StartsWith("/api/user_online.php"))
                {
                    var url = rawUrl;
                    var iqs = url.IndexOf('?');
                    if (iqs >= 0)
                    {
                        /*
                    var queryStr = HttpUtility.ParseQueryString(url.Substring(iqs + 1));
                    var tm = queryStr["time"];
                    var gid = queryStr["game_id"];
                    var sid = queryStr["server_id"];
                    var pid = queryStr["platform_id"];
                    var flag = queryStr["flag"];
                    */

                        var ss = ActorManager.Instance.GetActor<MyLib.SocketServer>();
                        var ac = ss.AgentCount;
                        var mact = ActorManager.Instance.GetActor<MyLib.MonitorActor>();
                        var maxPlayer = mact.maxPlayer;
                        var lowPlayer = mact.lowPlayer;

                        return (ac + "," + maxPlayer + "," + lowPlayer);
                    }

                }
                else if (rawUrl.StartsWith("/api/user_role_info.php"))
                {
                    var qs = req.QueryString;
                    var st = Convert.ToInt32(qs["start_time"]);
                    var et = Convert.ToInt32(qs["end_time"]);
                    var result = Login.GetRoleInfo(st, et);

                    var js = new JSONArray();
                    foreach (var objectse in result)
                    {
                        var arr = new JSONArray();
                        arr.Add(new JSONData((string)objectse[0]));
                        arr.Add(new JSONData((string)objectse[1]));
                        arr.Add(new JSONData((int)objectse[2]));
                        js.Add(arr);
                    }
                    return js.ToString();
                }
                else if (rawUrl.StartsWith("/api/user_upgrade.php"))
                {
                    var js = new JSONArray();
                    return (js.ToString());
                }else if (rawUrl.StartsWith("/idleServer"))
                {
                    var ms = ActorManager.Instance.GetActor<MasterServerActor>();
                    var idle = await ms.GetIdleServer();
                    return idle;
                }else if (rawUrl.StartsWith("/isFull"))
                {
                    var ss = ActorManager.Instance.GetActor<SlaveServerActor>();
                    var isFull = await ss.GetStatus();
                    return isFull;
                }
                else if (rawUrl.StartsWith("/query"))
                {
                    var qs = req.QueryString;
                    var flags = qs["flags"];

                    string result = "";
                    string actorStatus = "";
                    string roomStatus = "";
                    if (flags == "all")
                    {
                        LogHelper.Log("QueryStatus", "");
                        var server = ActorManager.Instance.GetActor<MyLib.SocketServer>();
                        result = server.ToString();
                        actorStatus = ActorManager.Instance.ToString();
                        roomStatus = ActorManager.Instance.GetActor<Lobby>().ToString();

                        var rets = ("<html><body><h1>Server Status</h1>");
                        rets += string.Format("Server Status : {0}\n", result);
                        rets += string.Format("Actor Status : {0}\n", actorStatus);
                        rets += string.Format("Room Status : {0}\n", roomStatus);
                        LogHelper.Log("QueryFinish", "");
                        return rets;
                    }
                    else if (flags == "close")
                    {
                        ActorManager.Instance.Stop();
                        var server = ActorManager.Instance.GetActor<MyLib.SocketServer>();
                        server.AcceptConnnection = false;
                        lock (server.agents)
                        {
                            foreach (var agent in server.agents)
                            {
                                agent.Value.Close();
                            }
                        }
                        LogHelper.LogCloseServer();
                        var rets = ("<html><body><h1>Close Server Successfully</h1>");
                        return rets;
                    }
                    else if (flags == "agent")
                    {
                        var server = ActorManager.Instance.GetActor<MyLib.SocketServer>();
                        result = server.ToString();
                        return result;
                    }
                    else if (flags == "GameOver")
                    {
                        var lobby = ActorManager.Instance.GetActor<Lobby>();
                        var rooms = await lobby.GetRooms();
                        foreach (var roomActor in rooms)
                        {
                            roomActor.GameOver();
                        }
                        return "GameOver";
                    }else if (flags == "room")
                    {
                        roomStatus = ActorManager.Instance.GetActor<Lobby>().ToString();
                        return roomStatus;
                    }
                }//研发Http后台接口
                else if (rawUrl.StartsWith("/ban"))
                {
                    var qs = req.QueryString;
                    var bt = qs["banType"];
                    var status = qs["status"];
                    var banDate = qs["banDate"];

                    //if (bt == "BanImei")
                    //{
                    var d = qs["data"].Split(',');
                    if (status == "0")
                    {
                        Login.UnBan(d);
                    }
                    else if(status == "1")
                    {
                        Login.BanDid(d, Convert.ToInt64(banDate));
                    }
                    //}
                    var js = new SimpleJSON.JSONClass();
                    js.Add("ret", new JSONData(0));
                    js.Add("msg", new JSONData("成功"));
                    var seg = new JSONClass();
                    seg.Add("role_name", "角色名");
                    js.Add("desc", seg);
                    var darr = new JSONArray();
                    js.Add("data", darr);
                    return js.ToString();
                }else if (rawUrl.StartsWith("/news_broadcast"))//所有房间内广播
                {
                    var qs = req.QueryString;
                    var con = qs["content"];
                    var lob = ActorManager.Instance.GetActor<Lobby>();
                    lob.BroadcastNews(con);
                    var js = new JSONClass();
                    js.Add("ret", new JSONData(0));
                    js.Add("msg", new JSONData("成功"));
                    return js.ToString();
                }
                else if (rawUrl.StartsWith("/user_info_list"))
                {
                    var qs = req.QueryString;
                    var roleName = qs["roleName"];
                    var roleId = qs["roleId"];
                    var accountName = qs["accountName"];
                    var ret = Login.UserInfo(roleName, roleId, accountName);
                    return ret;
                }
                else if (rawUrl.StartsWith("/CheckNewUser"))
                {
                    var qs = req.QueryString;
                    var deviceID = qs["did"];
                    var result = Login.CheckNewUser(deviceID);
                    var js = new JSONClass();
                    js.Add("ret", new JSONData(result));
                    return js.ToString();
                }else if (rawUrl.StartsWith("/AddNewUser"))
                {
                    var addr = req.RemoteEndPoint.Address;
                    var qs = req.QueryString;
                    Login.AddNewUser(qs, addr);
                    return "";
                }else if (rawUrl.StartsWith("/UpdateLogin"))
                {
                    var qs = req.QueryString;
                    Login.UpdateLogin(qs, req);
                    return string.Empty;
                }else if (rawUrl.StartsWith("/QuitGame"))
                {
                    Login.QuitGame(req);
                    return string.Empty;
                }else if (rawUrl.StartsWith("/TestError"))
                {
                    Login.Error("Test");
                    return string.Empty;
                }else if (rawUrl.StartsWith("/LoginVerify"))
                {
                    return "";
                }
                else if (rawUrl.StartsWith("/ViewRoom"))
                {
                    var rid = req.QueryString["id"];
                    var ridI = Convert.ToInt32(rid);
                    var room = ActorManager.Instance.GetActor(ridI) as RoomActor;
                    if (room != null)
                    {
                        room.ShowPhysic();
                    }
                }else if (rawUrl.StartsWith("/IsClose"))
                {
                    //黑名单
                    var ip = req.RemoteEndPoint.Address.ToString();
                    var did = req.QueryString["did"];
                    var ret = Login.IsBlack(did);
                    if (ret)
                    {
                        var js = new JSONClass();
                        js.Add("IsClose", new JSONData(true));
                        js.Add("IsBlack", new JSONData(true));
                        return js.ToString();
                    }

                    //白名单
                    var inWhite = Login.CheckInWhiteList(ip);
                    if (inWhite)
                    {
                        var js = new JSONClass();
                        js.Add("IsClose", new JSONData(false));
                        return js.ToString();
                    }
                    else //正常服务器关闭
                    {
                        var js = new JSONClass();
                        js.Add("IsClose", new JSONData(ServerIsClose));
                        js.Add("CloseMsg", new JSONData(CloseMsg));
                        return js.ToString();
                    }

                }else if (rawUrl.StartsWith("/SetClose"))
                {
                    var qs = req.QueryString;
                    var isClose = System.Convert.ToBoolean(qs["IsClose"]);
                    ServerIsClose = isClose;
                    //CloseMsg = qs["CloseMsg"];
                    CloseMsg = HttpUtility.ParseQueryString(queryPart).Get("CloseMsg");
                    LogHelper.Log("Http", "Close: "+CloseMsg);
                }
                else if (rawUrl.StartsWith("/LoginServer"))
                {
                    var pid = req.QueryString["pid"];
                    var uid = req.QueryString["uid"];
                    return Login.LoginServer(pid,uid);
                }
                else if (rawUrl.StartsWith("/CreateChar"))
                {
                    var pid = req.QueryString["pid"];
                    var uid = req.QueryString["uid"];
                    var name = HttpUtility.ParseQueryString(queryPart).Get("name");
                    return Login.CreateChar(pid, uid, name);
                }
                else if (rawUrl.StartsWith("/QueryUserInfo"))
                {
                    var pid = req.QueryString["pid"];
                    var uid = req.QueryString["uid"];
                    return Login.QueryUserInfo(pid, uid);
                }
                else if (rawUrl.StartsWith("/ExchangeMedal"))
                {
                    var pid = req.QueryString["pid"];
                    var uid = req.QueryString["uid"];
                    return Login.ExchangeMedal(pid, uid);
                }
                else if (rawUrl.StartsWith("/GMFeedback"))
                {
                    var roleId = req.QueryString["RoleId"];
                    var roleName = HttpUtility.ParseQueryString(queryPart).Get("RoleName");
                    var accountName = req.QueryString["AccountName"];
                    var complaintType = req.QueryString["ComplaintType"];
                    var content = req.QueryString["Content"];
                    var platform = req.QueryString["Platform"];
                    return Login.GMFeedback(roleId,roleName,accountName,complaintType,content,platform);
                }
                else if (rawUrl.StartsWith("/RenameUserName"))
                {
                    var pid = req.QueryString["pid"];
                    var uid = req.QueryString["uid"];
                    var name = HttpUtility.ParseQueryString(queryPart).Get("name");
                    return Login.RenameUsername(pid, uid, name);
                }
                else if (rawUrl.StartsWith("/GetAllMail"))
                {
                    var pid = req.QueryString["pid"];
                    var uid = req.QueryString["uid"];
                    return Login.GetAllMail(pid, uid);
                }
                else if (rawUrl.StartsWith("/ReadMail"))
                {
                    var mailId = req.QueryString["mailid"];
                    return Login.ReadMail(mailId);
                }
                else if (rawUrl.StartsWith("/DeleteMail"))
                {
                    var mailId = req.QueryString["mailid"];
                    return Login.DeleteMail(mailId);
                }
                else if (rawUrl.StartsWith("/OneKeyDeleteMail"))
                {
                    var pid = req.QueryString["pid"];
                    var uid = req.QueryString["uid"];
                    return Login.DeleteAllMail(pid, uid);
                }
                else if (rawUrl.StartsWith("/CheckMailTip"))
                {
                    var pid = req.QueryString["pid"];
                    var uid = req.QueryString["uid"];
                    return Login.CheckNewMail(pid, uid);
                }else if (rawUrl.StartsWith("/EmptyRoom"))
                {
                    var lob = ActorManager.Instance.GetActor<Lobby>();
                    var rooms = await lob.GetRooms();
                    var tp = Convert.ToInt32(req.QueryString["type"]);
                    var isNew = false;
                    if (tp == 0)
                    {
                        isNew = true;
                    }

                    var js = new JSONClass();
                    foreach (var roomActor in rooms)
                    {
                        if (roomActor.IsNewUser() == isNew)
                        {
                            var need = await roomActor.IsNeedRobot();
                            if (need)
                            {
                                js.Add("ret", new JSONData(true));
                                return js.ToString();
                            }
                        }
                    }
                    js.Add("ret", new JSONData(false));
                    return js.ToString();
                }else if (rawUrl.StartsWith("/Record"))
                {
                    var uid = req.QueryString["uid"];
                    return Login.QueryRecord(uid);
                }
                else if (rawUrl.StartsWith("/UpdateRecord"))
                {
                    return Login.UpdateRecord(req.QueryString);
                }else if (rawUrl.StartsWith("/Chat"))
                {
                    var ca = ActorManager.Instance.GetActor<ChatActor>();
                    //var qs = req.QueryString;
                    var qs = HttpUtility.ParseQueryString(queryPart);
                    ca.AddChat(qs["who"], qs["content"]);
                    return "suc";
                }else if (rawUrl.StartsWith("/GetMsg"))
                {
                    var ca = ActorManager.Instance.GetActor<ChatActor>();
                    var qs = req.QueryString;
                    var ord = Convert.ToInt32(qs["ord"]);
                    var ret = await ca.GetChatMsg(ord);
                    return ret;
                }
                else if (rawUrl.StartsWith("/send_mail"))
                {
                    var sendType = req.QueryString["sendType"];

                    int st;
                    if (int.TryParse(sendType, out st))
                    {
                        var qs = HttpUtility.ParseQueryString(queryPart);
                        if (st == 1)
                        {
                            var roleName = qs.Get("roleName");
                            var title = qs.Get("mailTitle");
                            var content = qs.Get("mailContent");
                            return Login.SendMailToName(roleName, title,content);
                        }
                        else if (st == 4)
                        {
                            var title = qs.Get("mailTitle");
                            var content = qs.Get("mailContent");
                            return Login.SendMailToAll(title, content);
                        }
                    }
                      
                }
            }
            return "NoUse2";
        }

        private bool ServerIsClose = false;
        private string CloseMsg = "目前停服2016-6-3日13:00到2016-6-4日2:00";

    }
}
