using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SimpleJSON;

namespace MyLib
{
    public class SlaveServer 
    {
        public string serverAdd;
        public string httpAddress;

        public SlaveServer(JSONArray ipConfig)
        {

            serverAdd = ipConfig[0];
            var ip = ipConfig[0].Value.Split(':');
            var lb = IPAddress.Parse(ip[0]);
            var port = Convert.ToInt32(ip[1]);

            httpAddress = ipConfig[1];
            var port2 = Convert.ToInt32(ipConfig[1].Value.Split(':')[1]);

            /*
            if (IPAddress.IsLoopback(lb))
            {
                var udp = new UdpClient(0);
                var le = udp.Client.LocalEndPoint as IPEndPoint;
                var nad = le.Address;
                var replacePort = new IPEndPoint(nad, port);
                serverAdd = replacePort.ToString();

                var htnew = new IPEndPoint(nad, port2);
                httpAddress = htnew.ToString();
            }
            */

            LogHelper.Log("Master", "Server Http: "+serverAdd+" http "+httpAddress);
        }

        public bool IsFull = false;
        public int RoomNum = 0;

        public bool IsOpen = false;
        public async Task QueryState()
        {
            var hc = new HttpClient();
            //LogHelper.Log("Master", "QueryState:"+httpAddress);
            var url = string.Format("http://{0}/isFull", httpAddress);
            JSONNode status = null;
            try
            {
                var resp = await hc.GetStringAsync(url);
                status = SimpleJSON.JSON.Parse(resp);
                LogHelper.Log("Master", "Status: "+resp);
            }
            catch (Exception exp)
            {
                LogHelper.Log("Master", exp.ToString());
            }
            if (status == null)
            {
                IsOpen = false;
            }
            else
            {
                IsOpen = true;
                var so = status.AsObject;
                IsFull =  so["IsFull"].AsBool;
                RoomNum = so["RoomNum"].AsInt;
            }
        }  
    }

    public class MasterServerActor : Actor
    {
        private List<SlaveServer> slaveServers = new List<SlaveServer>();
        private SlaveServer idleServer;

        private async Task CheckSlaveState()
        {
            var slave = ServerConfig.instance.configMap["SlaveList"].AsArray;
            foreach (JSONNode s in slave)
            {
                var ipConfig = s.AsArray;
                var ss = new SlaveServer(ipConfig);
                slaveServers.Add(ss);
            }
            idleServer = slaveServers[0];

            while (!IsStop())
            {
                var find = false;
                foreach (var slaveServer in slaveServers)
                {
                    await slaveServer.QueryState();
                    if (!slaveServer.IsFull)
                    {
                        idleServer = slaveServer;
                        find = true;
                        break;
                    }
                }

                if (!find)
                {
                    var minServer = slaveServers[0];
                    foreach (var slaveServer in slaveServers)
                    {
                        if (slaveServer.IsOpen && slaveServer.RoomNum < minServer.RoomNum)
                        {
                            minServer = slaveServer;
                        }
                    }
                    idleServer = minServer;
                }
                await Task.Delay(60000);
            }
        }

        public async Task<string> GetIdleServer()
        {
            await this._messageQueue;
            return idleServer.serverAdd;
        } 

        public override void Init()
        {
            InitMessageQueue();
            RunTask(CheckSlaveState);
        }
    }
}
