using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Numerics;

namespace MyLib 
{
    public partial class Util
    {
        public static int FramePerSecond = 100;
        public static int GameToNetNumber = 100;
        public static int FrameMSTime = 100;
        public static float FrameSecTime = 0.1f;
        public static float FixDist = 1f * 1f;
        public static float PredictTimeStep = 0.2f;

        public static bool IsClientMove(MyVec3 clientSpeed)
        {
            return (clientSpeed.x != 0 || clientSpeed.z != 0);
        }
        /// <summary>
        /// 根据客户端输入重新计算玩家的位置
        /// </summary>
        /// <param name="clientPos"></param>
        /// <param name="mePos"></param>
        /// <returns></returns>
        public static bool IsNetMove(Vector3 clientPos, AvatarInfo mePos)
        {
            var mePos2 = Util.NetPosToFloat(mePos);
            var deltaPos = clientPos - mePos2;
            deltaPos.Y = 0;
            var len = deltaPos.LengthSquared();
            return (len > FixDist);
        }

        public static float ClientFrameToServer(ulong frameId)
        {
            return frameId *1.0f / GameToNetNumber;
        }
        /// <summary>
        /// Frame 帧时间转化为秒 时间
        /// </summary>
        /// <param name="dFrame"></param>
        /// <returns></returns>
        public static float FrameToTime(float dFrame)
        {
            return dFrame * MainClass.syncFreq;
        }

        public static Vector3 DeltaPos(AvatarInfo p1, AvatarInfo p0)
        {
            var myVec = new MyVec3(p1.X-p0.X, p1.Y-p0.Y, p1.Z-p0.Z);
            return myVec.ToFloat();
        }
        public static Vector3 NetPosToFloat(AvatarInfo p)
        {
            var myVec = new MyVec3(p.X, p.Y, p.Z);
            return myVec.ToFloat();
        }
        public static MyVec3 NetPosToIntVec(AvatarInfo p)
        {
            var myVec = new MyVec3(p.X, p.Y, p.Z);
            return myVec;
        }

        public static int GameVecToNet(float v)
        {
            return (int)(v * GameToNetNumber);
        }
        public static int GameTimeToNet(float t)
        {
            return (int)(t * FramePerSecond);
        }
        public static int TimeToMS(float t)
        {
            return (int)(t * 1000);
        }
        public static int TimeToMS(double t)
        {
            return (int)(t * 1000);
        }
        
        public static double MSToSec(int t)
        {
            return t / 1000.0;
        }
        public static void Log(string msg)
        {
            //Console.WriteLine(msg);
        }

        public static int RealToNetPos(float p)
        {
            return (int)(p * 100);
        }

        public class Pair
        {
            public byte moduleId;
            public byte messageId;

            public Pair(byte a, byte b)
            {
                moduleId = a;
                messageId = b;
            }
        }



        public static Pair GetMsgID(string name)
        {
            return SaveGame.saveGame.GetMsgID(name);
        }

        public static Pair GetMsgID(string moduleName, string name)
        {
            Debug.Log("moduleName " + moduleName + " " + name);
            var mId = SaveGame.saveGame.msgNameIdMap[moduleName]["id"].AsInt;
            var pId = SaveGame.saveGame.msgNameIdMap[moduleName][name].AsInt;
            return new Pair((byte)mId, (byte)pId);
        }

        public static string FlattenException(Exception exception)
        {
            var stringBuilder = new StringBuilder();
            while (exception != null)
            {
                stringBuilder.AppendLine(exception.Message);
                stringBuilder.AppendLine(exception.StackTrace);
                exception = exception.InnerException;
            }
            return stringBuilder.ToString();
        }

        public static string PrintStackTrace()
        {
            var st = new StackTrace(true);
            StringBuilder sb = new StringBuilder();
            for (var i = 0; i < st.FrameCount; i++)
            {
                var sf = st.GetFrame(i);
                var s = sf.ToString();
                Debug.Log(s);
                sb.Append(s);
            }
            return sb.ToString();
        }


        /// <summary>
        /// 全局的静态数据表加载会存在问题
        /// 因此多线程要避免全局静态存储
        /// </summary>
        private static Dictionary<int, UnitData> monsterData = new Dictionary<int, UnitData>();
        public static UnitData GetUnitData(bool isPlayer, int mid, int level)
        {
            //玩家才需要level 怪物的level都是0， 因此mid为玩家的job的时候*10足够了
            int key = Convert.ToInt32(isPlayer)*1000000 + mid*10 + level;
            lock (monsterData)
            {
                if (monsterData.ContainsKey(key))
                {
                    return monsterData[key];
                }
                UnitData ud = new UnitData(isPlayer, mid, level);
                monsterData[key] = ud;
                return ud;
            }
        }


        /// <summary>
        /// 避免使用全局静态对象 否则需要考虑线程安全性
        /// </summary>
        static Dictionary<int, SkillData> skillData = new Dictionary<int, SkillData>();
        public static SkillData GetSkillData(int skillId, int level)
        {
            lock (skillData)
            {
                int key = skillId * 1000000 + level;
                if (skillData.ContainsKey(key))
                {
                    return skillData[key];
                }
                var sd = new SkillData(skillId, level);
                skillData[key] = sd;
                return sd;
            }
        }

        public static int RangeInt(int a, int b)
        {
            var rd = new Random();
            return rd.Next(a, b);
        }

        public static float RangeFloat(float a, float b)
        {
            var rd = new Random();
            return (float)(rd.NextDouble())*(b-a)+a;
        }



        public static List<List<float>> ParseConfig(string config)
        {
            var ret = new List<List<float>>();
            var g = config.Split(char.Parse("|"));
            foreach (var s in g)
            {
                var c = s.Split(char.Parse("_"));
                var c1 = new List<float>();
                ret.Add(c1);
                foreach (var c2 in c)
                {
                    var f = Convert.ToSingle(c2);
                    c1.Add(f);
                }
            }
            return ret;
        }

        public static double GetTimeNow()
        {
	        return DateTime.UtcNow.Ticks/10000000.0;
        }

        public static double startTime;
        /// <summary>
        /// 距离服务器启动的时间 降低时间大小
        /// </summary>
        public static float GetTimeSinceServerStart()
        {
            return (float)(Util.GetTimeNow() - startTime);
        }

        /// <summary>
        /// 服务器上距离2016年1月1日过去的时间
        /// </summary>
        /// <returns></returns>
        public static int GetServerTime()
        {
            /*
            var gameBegin = new DateTime(2016, 1, 1);
            var now = DateTime.Now;
            //var passTicks = now.Ticks - gameBegin.Ticks;
            var passTicks = now.Ticks;
            var sec = passTicks/10000000.0f;
            return (int) sec;
            */
            var span = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            return (int)span.TotalSeconds;
        }
        public static string GetBinPath()
        {
            var path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            return path.Substring(6);
        }
        public static float XZDistSqrt(Vector3 p1, Vector3 p2)
        {
            var delta = (p2 - p1);
            delta.Y = 0;
            return delta.LengthSquared();
        }
    }

    public static class Log
    {
        public static void AI(string s)
        {
            LogHelper.Log("Info", s);
        }

        public static void Important(string s)
        {
            LogHelper.Log("Info", s);
        }

        public static void Sys(string s)
        {
            LogHelper.Log("Info", s);
        }

        public static void Critical(string s)
        {
            LogHelper.Log("Info", s);
        }
        public static void Info(string s)
        {
            LogHelper.Log("Info", s);
        }
        public static void Error(string s)
        {
            LogHelper.LogError("Info", s);
        }
    }
    public class Debug{
		private static StringBuilder sb = new StringBuilder();

        [Conditional("DEBUG_LOG")]
        public static void Log(string msg) {
			//Console.WriteLine(msg);
            WriteFile(msg);
        }
        [Conditional("DEBUG_LOG")]
        public static void LogError(string msg) {
            //Console.WriteLine("Error:"+msg);
            var sb2 = Util.PrintStackTrace();
            WriteFile(msg+sb2);
        }
        [Conditional("DEBUG_LOG")]
        public static void LogWarning(string msg){
            //Console.WriteLine(msg);
            WriteFile(msg);
        }
         
        private static readonly  object Locker = new object();
        [Conditional("DEBUG_LOG")]
        private static void WriteFile(string msg)
        {
            LogHelper.Log("Info", msg);
            /*
            lock (Locker)
            {
                sb.Append(msg);
                if (sb.Length > 1000)
                {
                    File.AppendAllText("log.txt", sb.ToString());
                    sb.Clear();
                }
            }
            */
        }
      
    }
}

