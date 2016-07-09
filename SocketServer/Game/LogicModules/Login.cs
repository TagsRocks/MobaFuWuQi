using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using MySql.Data.MySqlClient;
using SimpleJSON;

namespace MyLib
{
    public delegate List<object[]> GetResult(MySqlDataReader reader);

    public class Login
    {
        /// <summary>
        /// MySql 资源的释放
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public static bool CheckNewUser(string deviceId)
        {
			return false;
        }

        private static List<object[]> RunReaderTank(string sql, Dictionary<string, object> kv, GetResult getFunc)
        {
            try
            {
                LogHelper.Log("MySql", "sql: " + sql);
                using (var connection = new MySqlConnection())
                {
                    connection.ConnectionString = ServerConfig.instance.configMap["DatabaseConnection"];
                    connection.Open();

                    using (var cmd = new MySqlCommand(sql, connection))
                    {
                        foreach (var o in kv)
                        {
                            //LogHelper.Log("MySql", "kv: "+o.Key+" v "+o.Value);
                            cmd.Parameters.AddWithValue("?" + o.Key, o.Value);
                        }
                        using (var reader = cmd.ExecuteReader())
                        {
                            return getFunc(reader);
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                LogHelper.Log("Error", exp.ToString());
            }
            return null;
        }


        private static void RunQueryTank(string sql, Dictionary<string, object> kv)
        {
            try
            {
                LogHelper.Log("MySql", "sql: " + sql);
                using (var connection = new MySqlConnection())
                {
                    connection.ConnectionString = ServerConfig.instance.configMap["DatabaseConnection"];
                    connection.Open();

                    using (var cmd = new MySqlCommand(sql, connection))
                    {
                        foreach (var o in kv)
                        {
                            //LogHelper.Log("MySql", "kv: "+o.Key+" v "+o.Value);
                            cmd.Parameters.AddWithValue("?" + o.Key, o.Value);
                        }
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception exp)
            {
                LogHelper.Log("Error", exp.ToString());
            }
        }


        public static void AddNewUser(NameValueCollection nvc, IPAddress addr)
        {
        }

        public static void UnBan(string[] ipList)
        {
            foreach (var s in ipList)
            {
                var dict = new Dictionary<string, object>()
                {
                    {"did", s},
                };
                var sql = "delete from banip where did = ?did ";
                RunQueryTank(sql, dict);
            }
        }

        public static void BanDid(string[] ipList, long endTime)
        {
            foreach (var s in ipList)
            {
                var dict = new Dictionary<string, object>()
                {
                    {"did", s},
                    {"end_time", endTime},
                };
                var sql = "insert into banip set ";

                var kvStr = new List<string>();
                foreach (var o in dict)
                {
                    kvStr.Add(string.Format("{0}=?{1}", o.Key, o.Key));
                }
                var kv = string.Join(",", kvStr.ToArray());
                sql += kv;
                RunQueryTank(sql, dict);
            }
        }

        public static bool IsBlack(string did)
        {
            var now = Util.GetServerTime();
            var dict = new Dictionary<string, object>()
            {
                {"did", did},
                {"end_time", now},
            };
            var sql = "select did from banip where did = ?did and end_time > ?end_time ";
            var result = RunReaderTank(sql, dict, DumpParse);
            return result.Count > 0;
        }

        private static void InsertRole(NameValueCollection nvc, IPAddress addr)
        {
        }

        private static string GetKV(Dictionary<string, object> dict)
        {
            var kvStr = new List<string>();
            foreach (var o in dict)
            {
                kvStr.Add(string.Format("{0}=?{1}", o.Key, o.Key));
            }
            
            var kv = string.Join(",", kvStr.ToArray());
            return kv;
        } 

        private static Dictionary<string, int> lastLogin = new Dictionary<string, int>();

        public static void UpdateLogin(NameValueCollection nvc, HttpListenerRequest req)
        {
        }

        private static void InsertLogin(NameValueCollection nvc, HttpListenerRequest req)
        {
        }

        public static void StartMatch(DeviceInfo info, string user_ip)
        {
        }

        public static void QuitRoom(DeviceInfo info, string user_ip)
        {
        }

        public static void QuitGame(HttpListenerRequest req)
        {
        }

        private static List<object[]> ParseRoleInfo(MySqlDataReader reader)
        {
            var ret = new List<object[]>();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    var an = reader.GetString(0);
                    var rn = reader.GetString(1);
                    var lt = reader.GetInt32(2);
                    ret.Add(new object[]
                    {
                        an, rn, lt,
                    });
                }
            }
            return ret;
        }

        public static List<object[]> GetRoleInfo(int startTime, int endTime)
        {
			return null;
        }

        private static List<object[]> DumpParse(MySqlDataReader reader)
        {
            var ret = new List<object[]>();
            if (reader.HasRows)
            {
                ret.Add(null);
            }
            return ret;
        }

        public static bool CheckInWhiteList(string ipOrName)
        {
            var dict = new Dictionary<string, object>
            {
                {"ip", ipOrName},
            };
            var sql = "select ip from ipwhite where ip = ?ip";
            var result = RunReaderTank(sql, dict, DumpParse);
            return (result.Count > 0);
        }

        private static List<object[]> ParseUserInfo(MySqlDataReader reader)
        {
            var ret = new List<object[]>();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    var rid = reader.GetString(0);
                    var rname = reader.GetString(1);
                    var aname = reader.GetString(2);
                    var regTime = reader.GetInt32(3);
                    var lev = reader.GetInt32(4);
                    var ltim = reader.GetInt32(5);
                    var did = reader.GetString(6);
                    ret.Add(new object[]
                    {
                        rid, rname, aname,
                        regTime, lev, ltim, did,
                    });
                }
            }
            return ret;
        }

        public static string UserInfo(string roleName, string roleId, string accountName)
        {
			return "";
        }

        private static void VerifyLoginInfo(string uid)
        {
            try
            {

                MySqlConnection connection = new MySqlConnection();
                connection.ConnectionString = ServerConfig.instance.GetMySqlConnectionString();
                connection.Open();

                string select = "select *  from login where uid = ?uid";
                MySqlCommand queryUser = new MySqlCommand(select, connection);
                queryUser.Parameters.AddWithValue("?uid", uid);

                var reader = queryUser.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    int length = reader.GetInt32(4);
                    if (length > 0)
                    {
                        byte[] buffer = new byte[length];
                        reader.GetBytes(3, 0, buffer, 0, length); //用户数据
                    }
                }
                else
                {
                    reader.Close();
                    string insert = "insert into login(uid) values (?uid)";
                    MySqlCommand insertUser = new MySqlCommand(insert, connection);
                    insertUser.Parameters.AddWithValue("?uid", uid);
                    insertUser.ExecuteNonQuery();
                }

                connection.Close();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static int LoginGame(CGPlayerCmd playerLoginInfo)
        {
            var accountInfo = playerLoginInfo.Account;
            if (string.IsNullOrEmpty(accountInfo))
            {
                return 1;
            }
            if (Encoding.UTF8.GetBytes(accountInfo).Length > 30)
            {
                return 2;
            }
            VerifyLoginInfo(accountInfo);
            return 0;
        }

        public static void OnlineUser(int num)
        {
        }

        public static void Error(string content)
        {
        }

        private static List<object[]> ParseLoginInfo(MySqlDataReader reader)
        {
            var ret = new List<object[]>();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    var an1 = reader.GetString(0);
                    var an2 = reader.GetString(1);
                    var an3 = reader.GetString(2);
                    var an4 = reader.GetInt32(3);
                    var an5 = reader.GetInt32(4);
                    var an6 = reader.GetInt32(5);
                    var an7 = reader.GetInt32(6);
                    var an8 = reader.GetInt32(7);
                    var an9 = reader.GetInt32(8);
                    var an10 = reader.GetInt32(9);
                    ret.Add(new object[]
                    {
                        an1, an2, an3, an4, an5, an6, an7, an8, an9, an10
                    });
                }
            }
            return ret;
        }

        public static string LoginServer(string pid, string uid)
        {
            Console.WriteLine("pid = " + pid + " , uid = " + uid);
            var dict = new Dictionary<string, object>()
            {
                {"pid", pid},
                {"uid", uid},
            };
            var sql = "select * from login where uid = ?uid and pid = ?pid";

            var data = RunReaderTank(sql, dict, ParseLoginInfo);

            if (data.Count == 0)
            {
                sql = "insert into login(pid,uid) values (?pid,?uid)";
                dict = new Dictionary<string, object>()
                {
                    {"pid", pid},
                    {"uid", uid},
                };
                RunQueryTank(sql, dict);

                JSONClass jsonClass = new JSONClass();
                jsonClass.Add("ret", new JSONData(0));
                jsonClass.Add("info", new JSONData(""));
                return jsonClass.ToString();
            }
            else
            {
                if (string.IsNullOrEmpty(data[0][2] as string))
                {
                    JSONClass jc = new JSONClass();
                    jc.Add("ret", new JSONData(0));
                    jc.Add("info", new JSONData(""));
                    return jc.ToString();
                }

                JSONClass jsonClass = new JSONClass();
                jsonClass.Add("ret", new JSONData(1));
                JSONClass info = new JSONClass();
                info.Add("name", new JSONData(data[0][2] as string));
                info.Add("level", new JSONData((int) data[0][3]));
                info.Add("exp", new JSONData((int) data[0][4]));
                info.Add("medal", new JSONData((int) data[0][5]));
                jsonClass.Add("info", info);
                return jsonClass.ToString();
            }
        }

        private static List<object[]> ParseDetailLoginInfo(MySqlDataReader reader)
        {
            var ret = new List<object[]>();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    var an0 = reader.GetInt32(3);
                    var an1 = reader.GetInt32(4);
                    var an2 = reader.GetInt32(5);

                    var an4 = reader.GetInt32(8);

                    ret.Add(new object[]
                    {
                        an0,
                        an1,
                        an2,
                        an4,
                    });
                }
            }
            return ret;
        }

        public static List<object[]> LoginQueryInfo(string pid, string uid)
        {
            Console.WriteLine("pid = " + pid + " , uid = " + uid);
            var dict = new Dictionary<string, object>()
            {
                {"pid", pid},
                {"uid", uid},
            };

            var sql = "select *  from login where uid = ?uid and pid = ?pid";
            var data = RunReaderTank(sql, dict, ParseDetailLoginInfo);
            if (data.Count == 0)
            {
                sql = "insert into login(pid,uid) values (?pid,?uid)";
                dict = new Dictionary<string, object>()
                {
                    {"pid", pid},
                    {"uid", uid},
                };
                RunQueryTank(sql, dict);
                return null;
            }
            return data;
        }

        public static void SaveUserInfo(string pid, string uid, int level, long exp, int medal, int dayBattleCount)
        {
            Console.WriteLine("pid = " + pid + " , uid = " + uid);
            var dict = new Dictionary<string, object>()
            {
                {"pid", pid},
                {"uid", uid},
                {"level", level},
                {"exp", exp},
                {"medal", medal},
                {"dayBattleCount", dayBattleCount},
            };

            var sql =
                "update login set level = ?level, exp = ?exp, medal = ?medal, dayBattleCount = ?dayBattleCount where uid = ?uid and pid = ?pid";
            RunQueryTank(sql, dict);
        }

        public static string CreateChar(string pid, string uid, string name)
        {
            var dict = new Dictionary<string, object>()
            {
                {"name", name},
            };
            var sql = "select * from login where uname =  ?name";

            var data = RunReaderTank(sql, dict, ParseLoginInfo);
            if (data.Count == 0)
            {
                dict = new Dictionary<string, object>()
                {
                    {"pid", pid},
                    {"uid", uid},
                };

                sql = "select * from login where uid = ?uid and pid = ?pid";
                data = RunReaderTank(sql, dict, ParseLoginInfo);
                if (data.Count == 0)
                {
                    JSONClass jsonClass = new JSONClass();
                    jsonClass.Add("ret", new JSONData(1));
                    jsonClass.Add("info", new JSONData(""));
                    return jsonClass.ToString();
                }
                else
                {
                    sql = "update login set uname = ?uname where uid = ?uid and pid = ?pid";
                    dict = new Dictionary<string, object>()
                    {
                        {"pid", pid},
                        {"uid", uid},
                        {"uname", name},
                    };
                    RunQueryTank(sql, dict);

                    JSONClass jsonClass = new JSONClass();
                    jsonClass.Add("ret", new JSONData(0));
                    JSONClass info = new JSONClass();
                    info.Add("name", new JSONData(name));
                    info.Add("level", new JSONData((int) data[0][3]));
                    info.Add("exp", new JSONData((int) data[0][4]));
                    info.Add("medal", new JSONData((int) data[0][5]));
                    jsonClass.Add("info", info);

                    return jsonClass.ToString();
                }
            }
            else
            {
                JSONClass jsonClass = new JSONClass();
                jsonClass.Add("ret", new JSONData(2));
                jsonClass.Add("info", new JSONData(""));
                Console.WriteLine(jsonClass.ToString());
                return jsonClass.ToString();
            }
        }

        public static string QueryUserInfo(string pid, string uid)
        {
            var dict = new Dictionary<string, object>()
            {
                {"pid", pid},
                {"uid", uid},
            };
            var sql = "select * from login where uid = ?uid and pid = ?pid";

            var data = RunReaderTank(sql, dict, ParseLoginInfo);

            if (data.Count == 0)
            {
                JSONClass jsonClass = new JSONClass();
                jsonClass.Add("ret", new JSONData(0));
                return jsonClass.ToString();
            }
            else
            {
                JSONClass jsonClass = new JSONClass();
                jsonClass.Add("ret", new JSONData(1));
                JSONClass info = new JSONClass();
                info.Add("name", new JSONData(data[0][2] as string));
                info.Add("level", new JSONData((int) data[0][3]));
                info.Add("exp", new JSONData((int) data[0][4]));
                info.Add("medal", new JSONData((int) data[0][5]));
                jsonClass.Add("info", info);
                return jsonClass.ToString();
            }
        }

        public static string ExchangeMedal(string pid, string uid)
        {
            var dict = new Dictionary<string, object>()
            {
                {"pid", pid},
                {"uid", uid},
            };
            var sql = "select * from login where uid = ?uid and pid = ?pid";

            var data = RunReaderTank(sql, dict, ParseLoginInfo);

            var level = (int) data[0][3];
            if (data.Count == 0 || level < 40)
            {
                JSONClass jsonClass = new JSONClass();
                jsonClass.Add("ret", new JSONData(1));
                return jsonClass.ToString();
            }
            else
            {
                sql = "update login set level = 1,exp = 0,medal = ?medal where uid = ?uid and pid = ?pid";
                dict = new Dictionary<string, object>()
                {
                    {"pid", pid},
                    {"uid", uid},
                    {"medal", ((int) data[0][5] + 1)},
                };
                RunQueryTank(sql, dict);

                JSONClass jsonClass = new JSONClass();
                jsonClass.Add("ret", new JSONData(0));
                JSONClass info = new JSONClass();
                info.Add("name", new JSONData(data[0][2] as string));
                info.Add("level", new JSONData(1));
                info.Add("exp", new JSONData(0));
                info.Add("medal", new JSONData((int) data[0][5] + 1));
                jsonClass.Add("info", info);
                return jsonClass.ToString();
            }
        }

        public static string GMFeedback(string roleId, string roleName, string accountName, string complaintType,
            string content, string platform)
        {
            var sql =
                "insert into gmfeedback(RoleId,RoleName,AccountName,ComplaintSubmitTime,ComplaintType,Content,Platform) values (?roleId,?roleName,?accountName,UNIX_TIMESTAMP(),?complaintType,?content,?platform)";
            var dict = new Dictionary<string, object>()
            {
                {"roleId", roleId},
                {"roleName", roleName},
                {"accountName", accountName},
                {"complaintType", complaintType},
                {"content", content},
                {"platform", platform},
            };
            RunQueryTank(sql, dict);

            JSONClass jsonClass = new JSONClass();
            jsonClass.Add("ret", new JSONData(0));
            return jsonClass.ToString();
        }

        public static string RenameUsername(string pid, string uid, string name)
        {
            var dict = new Dictionary<string, object>()
            {
                {"pid", pid},
                {"uid", uid},
                {"uname", name }
            };

            var sql = "select * from login where uname = ?uname";

            var data = RunReaderTank(sql, dict, ParseLoginInfo);
            if (data.Count > 0)
            {
                JSONClass jsonClass = new JSONClass();
                jsonClass.Add("ret", new JSONData(3));
                jsonClass.Add("desc", new JSONData("已经存在相同名称的角色名,改名失败"));
                return jsonClass.ToString();
            }

            sql = "select * from login where uid = ?uid and pid = ?pid";
            data = RunReaderTank(sql, dict, ParseLoginInfo);

            if (data.Count > 0)
            {
                if ((int) data[0][9] > 0)
                {
                    JSONClass jsonClass = new JSONClass();
                    jsonClass.Add("ret", new JSONData(1));
                    jsonClass.Add("desc", new JSONData("已经改过名,无法再次改名"));
                    return jsonClass.ToString();
                }

                sql = "update login set uname = ?uname, userRename = 1 where uid = ?uid and pid = ?pid";
                dict = new Dictionary<string, object>()
                {
                    {"pid", pid},
                    {"uid", uid},
                    {"uname", name},
                };
                RunQueryTank(sql, dict);

                JSONClass jsonClass1 = new JSONClass();
                jsonClass1.Add("ret", new JSONData(0));
                jsonClass1.Add("name", new JSONData(name));
                jsonClass1.Add("rename", new JSONData(1));
                jsonClass1.Add("desc", new JSONData("改名成功"));
                return jsonClass1.ToString();
            }
            else
            {
                JSONClass jsonClass = new JSONClass();
                jsonClass.Add("ret", new JSONData(2));
                jsonClass.Add("desc", new JSONData("查询不到玩家,改名失败"));
                return jsonClass.ToString();
            }
        }

        private static List<object[]> ParseRecordInfo(MySqlDataReader reader)
        {
            var ret = new List<object[]>();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    var total = reader.GetInt32(1);
                    var mvp = reader.GetInt32(2);
                    var threeKill = reader.GetInt32(3);
                    var fourKill = reader.GetInt32(4);
                    var fiveKill = reader.GetInt32(5);
                    var totalKill = reader.GetInt32(6);
                    var dieNum = reader.GetInt32(7);
                    ret.Add(new object[]
                    {
                        total,
                        mvp,
                        threeKill,
                        fourKill,
                        fiveKill,
                        totalKill,
                        dieNum,
                    });
                }
            }
            return ret;
        }

        private static List<object[]> ParseMainInfo(MySqlDataReader reader)
        {
            var ret = new List<object[]>();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    var an1 = reader.GetInt32(0);
                    var an2 = reader.GetString(1);
                    var an3 = reader.GetString(2);
                    var an4 = reader.GetString(3);
                    var an5 = reader.GetString(4);
                    var an6 = reader.GetInt32(5);
                    var an7 = reader.GetInt32(6);
                    var an8 = reader.GetString(7);
 
                    ret.Add(new object[]
                    {
                        an1,an2,an3,an4,an5,an6,an7,an8 
                    });
                }
            }
            return ret;
        }

        public static string GetAllMail(string pid, string uid)
        {
            var dict = new Dictionary<string, object>()
            {
                {"pid", pid},
                {"uid", uid},
            };
            var sql = "select * from mail where uid = ?uid and pid = ?pid";

            var data = RunReaderTank(sql, dict, ParseMainInfo);

            if (data.Count > 0)
            {
                JSONClass jsonClass = new JSONClass();
                jsonClass.Add("ret", new JSONData(0));
                var jsonArray = new JSONArray();;
                foreach (var d in data)
                {
                    var tempJsonClass = new JSONClass();
                    tempJsonClass.Add("mailid", new JSONData((int)d[0]));
                    tempJsonClass.Add("title",new JSONData(d[3].ToString()));
                    tempJsonClass.Add("sender", new JSONData(d[4].ToString()));
                    tempJsonClass.Add("sendtime", new JSONData(d[5].ToString()));
                    tempJsonClass.Add("state", new JSONData((int)d[6]));
                    tempJsonClass.Add("content", new JSONData(d[7].ToString()));
                    jsonArray.Add(tempJsonClass);
                }
                jsonClass.Add("result", jsonArray);
                return jsonClass.ToString();
            }
            else
            {
                JSONClass jsonClass = new JSONClass();
                jsonClass.Add("ret", new JSONData(1));
                return jsonClass.ToString();
            }
        }

        public static string SendMailToName(string roleNames,string title, string content)
        {
            var names = roleNames.Split(',');
            if (names == null || names.Length == 0)
            {
                JSONClass jsonClass = new JSONClass();
                jsonClass.Add("ret", new JSONData(1));
                jsonClass.Add("msg", new JSONData("查询不到相关玩家"));
                jsonClass.Add("desc", new JSONClass());
                jsonClass.Add("data", new JSONClass());
                return jsonClass.ToString();
            }
            var dict = new Dictionary<string, object>()
            {

            };

            StringBuilder sb1 = new StringBuilder();
            foreach (var name in names)
            {
                string formatStr = "'{0}',";
                sb1.Append(string.Format(formatStr, name));
            }
            sb1.Remove(sb1.Length - 1, 1);

            var sql = "select * from login where uname in ("+ sb1+ ")";
            var data = RunReaderTank(sql, dict, ParseMainInfo);

            if (data.Count == 0)
            {
                //error
                JSONClass jsonClass = new JSONClass();
                jsonClass.Add("ret", new JSONData(1));
                jsonClass.Add("msg", new JSONData("查询不到相关玩家"));
                jsonClass.Add("desc", new JSONClass());
                jsonClass.Add("data", new JSONClass());
                return jsonClass.ToString();
            }
            else
            {
                sql = "insert into mail(pid,uid,title,sendtime,state,content) values";

                StringBuilder sb = new StringBuilder();
                sb.Append(sql);
                for (int i = 0; i < data.Count; ++i)
                {
                    string formatStr = "({0},'{1}','{2}',{3},0,'{4}'),";
                    var pid = data[i][0];
                    var uid = data[i][1];
                    var timeStr = "UNIX_TIMESTAMP()";
                    sb.Append(string.Format(formatStr,pid,uid,title,timeStr,content));
                }
                sb.Remove(sb.Length - 1,1);
                RunQueryTank(sb.ToString(), dict);
                JSONClass jsonClass = new JSONClass();
                jsonClass.Add("ret", new JSONData(0));
                jsonClass.Add("msg", new JSONData("发送邮件成功"));
                jsonClass.Add("desc", new JSONClass());
                jsonClass.Add("data", new JSONClass());
                return jsonClass.ToString();
            }
        }
        
        public static string SendMailToAll(string title, string content)
        {
            var dict = new Dictionary<string, object>()
            {

            };

            var sql = "select * from login";
            var data = RunReaderTank(sql, dict, ParseMainInfo);

            if (data.Count == 0)
            {
                //error
                JSONClass jsonClass = new JSONClass();
                jsonClass.Add("ret", new JSONData(1));
                jsonClass.Add("msg", new JSONData("查询不到相关玩家"));
                jsonClass.Add("desc", new JSONClass());
                jsonClass.Add("data", new JSONClass());
                return jsonClass.ToString();
            }
            else
            {
                sql = "insert into mail(pid,uid,title,sendtime,state,content) values";

                StringBuilder sb = new StringBuilder();
                sb.Append(sql);
                for (int i = 0; i < data.Count; ++i)
                {
                    string formatStr = "({0},'{1}','{2}',{3},0,'{4}'),";
                    var pid = data[i][0];
                    var uid = data[i][1];
                    var timeStr = "UNIX_TIMESTAMP()";
                    sb.Append(string.Format(formatStr, pid, uid, title, timeStr, content));
                }
                sb.Remove(sb.Length - 1, 1);
                RunQueryTank(sb.ToString(), dict);
                JSONClass jsonClass = new JSONClass();
                jsonClass.Add("ret", new JSONData(0));
                jsonClass.Add("msg", new JSONData("发送邮件成功"));
                jsonClass.Add("desc", new JSONClass());
                jsonClass.Add("data", new JSONClass());
                return jsonClass.ToString();
            }
        }

        public static string ReadMail(string mailId)
        {
            var dict = new Dictionary<string, object>()
            {
                {"mailid", mailId},
 
            };
            var sql = "update mail set state = 1 where mailid = ?mailid";
            RunQueryTank(sql, dict);

            JSONClass jsonClass = new JSONClass();
            jsonClass.Add("ret", new JSONData(0));
            jsonClass.Add("mailid", new JSONData(int.Parse(mailId)));
            jsonClass.Add("mailstate", new JSONData(1));
            return jsonClass.ToString();
        }

        public static string DeleteMail(string mailId)
        {
            var dict = new Dictionary<string, object>()
            {
                {"mailid", mailId},

            };
            var sql = "delete from mail where mailid = ?mailid";
            RunQueryTank(sql, dict);

            JSONClass jsonClass = new JSONClass();
            jsonClass.Add("ret", new JSONData(0));
            jsonClass.Add("mailid", new JSONData(int.Parse(mailId)));
            return jsonClass.ToString();
        }


        public static string DeleteAllMail(string pid, string uid)
        {
            var dict = new Dictionary<string, object>()
            {
                {"pid", pid},
                {"uid", uid},
            };

            var sql = "select * from mail where uid = ?uid and pid = ?pid and state = 1";
            var data = RunReaderTank(sql, dict, ParseMainInfo);

            sql = "delete from mail where uid = ?uid and pid = ?pid and state = 1";
            RunQueryTank(sql, dict);

            var jsonArray = new JSONArray(); ;
            foreach (var d in data)
            {
                var tempJsonClass = new JSONClass();
                tempJsonClass.Add("mailid", new JSONData((int)d[0]));
                jsonArray.Add(tempJsonClass);
            }
            JSONClass jsonClass = new JSONClass();
            jsonClass.Add("ret", new JSONData(0));
            jsonClass.Add("result", jsonArray);
            return jsonClass.ToString();
        }

        public static string CheckNewMail(string pid, string uid)
        {
            var dict = new Dictionary<string, object>()
            {
                {"pid", pid},
                {"uid", uid},
            };

            var sql = "select * from mail where uid = ?uid and pid = ?pid and state = 0";
            var data = RunReaderTank(sql, dict, ParseMainInfo);

            if (data.Count > 0)
            {
                JSONClass jsonClass1 = new JSONClass();
                jsonClass1.Add("ret", new JSONData(0));
                return jsonClass1.ToString();
            }
            JSONClass jsonClass = new JSONClass();
            jsonClass.Add("ret", new JSONData(1));
            return jsonClass.ToString();
        }

        public static string QueryRecord(string uid)
        {
            var dict = new Dictionary<string, object>()
            {
                {"uid", uid},
            };
            var sql = "select * from record where uid = ?uid";
            var data = RunReaderTank(sql, dict, ParseRecordInfo);
            if (data.Count > 0)
            {
                var d0 = data[0];
                var jc = new JSONClass();
                jc.Add("total", new JSONData((int)d0[0]));
                jc.Add("mvp", new JSONData((int)d0[1]));
                jc.Add("threeKill", new JSONData((int)d0[2]));
                jc.Add("fourKill", new JSONData((int)d0[3]));
                jc.Add("fiveKill", new JSONData((int)d0[4]));
                jc.Add("totalKill", new JSONData((int)d0[5]));
                jc.Add("dieNum", new JSONData((int)d0[6]));
                return jc.ToString();
            }
            var jc1 = new JSONClass();
            return jc1.ToString();
        }

        public static string UpdateRecord(NameValueCollection nvc)
        {
            var uid = nvc["uid"];
            var dict = new Dictionary<string, object>()
            {
                {"uid", uid},
            };
            var sql = "select uid from record where uid = ?uid";
            var data = RunReaderTank(sql, dict, DumpParse);
            if (data.Count == 0)
            {
                InsertRecord(uid);
            }

            var total = nvc["total"];
            if (total != null)
            {
                UpdateTotal(uid);
            }
            var mvp = nvc["mvp"];
            if (mvp != null)
            {
                UpdateMVP(uid);
            }
            var threeKill = nvc["threeKill"];
            if (threeKill != null)
            {
                UpdateThreeKill(uid);
            }
            var fourKill = nvc["fourKill"];
            if (fourKill != null)
            {
                UpdateFourKill(uid);
            }
            var fiveKill = nvc["fiveKill"];
            if (fiveKill != null)
            {
                UpdateFiveKill(uid);
            }
            var totalKill = nvc["totalKill"];
            if (totalKill != null)
            {
                UpdateTotalKill(uid);
            }
            var dieNum = nvc["dieNum"];
            if (dieNum != null)
            {
                UpdateDieNum(uid);
            }

            var js = new JSONClass();
            js.Add("ret", new JSONData(true));
            return js.ToString();
        }

        private static void UpdateTotal(string uid)
        {
            var dict = new Dictionary<string, object>()
            {
                {"uid", uid},
            };
            var sql = "update record set total = total+1 where uid = ?uid";
            RunQueryTank(sql, dict);
        }

        private static void UpdateMVP(string uid)
        {
            var dict = new Dictionary<string, object>()
            {
                {"uid", uid},
            };
            var sql = "update record set mvp = mvp+1 where uid = ?uid";
            RunQueryTank(sql, dict);
        }

        private static void UpdateThreeKill(string uid)
        {
            var dict = new Dictionary<string, object>()
            {
                {"uid", uid},
            };
            var sql = "update record set threeKill = threeKill+1 where uid = ?uid";
            RunQueryTank(sql, dict);
        }

        private static void UpdateFourKill(string uid)
        {
            var dict = new Dictionary<string, object>()
            {
                {"uid", uid},
            };
            var sql = "update record set fourKill = fourKill+1 where uid = ?uid";
            RunQueryTank(sql, dict);
        }

        private static void UpdateFiveKill(string uid)
        {
            var dict = new Dictionary<string, object>()
            {
                {"uid", uid},
            };
            var sql = "update record set fiveKill = fiveKill+1 where uid = ?uid";
            RunQueryTank(sql, dict);
        }

        private static void UpdateTotalKill(string uid)
        {
            var dict = new Dictionary<string, object>()
            {
                {"uid", uid},
            };
            var sql = "update record set totalKill = totalKill+1 where uid = ?uid";
            RunQueryTank(sql, dict);
        }
        private static void UpdateDieNum(string uid)
        {
            var dict = new Dictionary<string, object>()
            {
                {"uid", uid},
            };
            var sql = "update record set dieNum = dieNum+1 where uid = ?uid";
            RunQueryTank(sql, dict);
        }


        private static void InsertRecord(string uid)
        {
            var dict = new Dictionary<string, object>()
            {
                {"uid", uid},
            };
            var sql = "insert into record set ";
            var kv = GetKV(dict);
            sql += kv;
            RunQueryTank(sql, dict);
        }
    }
}
