using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleJSON;

namespace MyLib
{
    public static class EntityImport
    {
        public static GameObjectActor InitGameObject(JSONClass jobj)
        {
            var g = new GameObjectActor();
            g.name = jobj["Name"];
            g.pos = MyVec3.Parse(jobj["Pos"].Value);
            g.scale = MyVec3.Parse(jobj["Scale"].Value);
            //Debug.Log("InitGameObject: "+g.name);
            LogHelper.Log("EntityImport", "InitGameObject:"+g.name);
            //避免启动Task 应该使用 Room的Task来执行
            //ActorManager.Instance.AddActor(g);
            foreach (JSONNode com in jobj["Component"].AsArray)
            {
                InitComponent(g, com.AsObject);
            }
            foreach (JSONNode c in jobj["Child"].AsArray) 
            {
                var cobj = InitGameObject(c.AsObject);
                g.AddChild(cobj);
            }
            return g;
        }

        private static void InitComponent(GameObjectActor g, JSONClass jobj)
        {
            var t = jobj["Type"].Value;
            var tp = Type.GetType("MyLib." + t);
            if (tp != null)
            {
                LogHelper.Log("EntityImport", "InitComponent: "+g.name+" tp "+tp);
                var ac = g.GetType().GetMethod("AddComponent");
                var ge = ac.MakeGenericMethod(tp);
                var com = ge.Invoke(g, null);
                ReadAttr(com, jobj); 
            }else
            {
                LogHelper.Log("EntityImport", "Not Find Component:"+g.name+":"+t);
            }
        }

        private static void SetArray(object com, string k, JSONArray array)
        {
            Log.Sys("SetArray:"+com+":"+k+":"+array.Count);
            var comTp = com.GetType();
            var fi = comTp.GetField(k);
            Type valueType = typeof(string);
             
            if(array.Count > 0)
            {
                var value = array[0] as JSONData;
                object retv;
                var tp = value.GetValueType(out retv);
                if (tp == typeof(string))
                {
                    var rv = retv as string;
                    if(rv.StartsWith("<vec>"))
                    {
                        valueType = typeof(MyVec3);
                    }
                }
            }
            if(valueType == typeof(MyVec3))
            {
                var setV = new List<MyVec3>();
                foreach(JSONNode n in array)
                {
                    var nv = MyVec3.Parse(n);
                    setV.Add(nv);
                }
                if(fi != null)
                {
                    fi.SetValue(com, setV);
                }
            }
        }

        private static void ReadAttr(object com, JSONClass jobj)
        {
            var comTp = com.GetType();
            foreach (KeyValuePair<string, JSONNode> j in jobj)
            {
                var k = j.Key;
                //读取数组属性
                if (j.Value.AsArray != null)
                {
                    SetArray(com, k, j.Value.AsArray);
                }
                else
                {
                    var jd = j.Value as JSONData;
                    object retv;
                    var tp = jd.GetValueType(out retv);
                    if (tp == typeof(string))
                    {
                        var rv = retv as string;
                        if (rv.StartsWith("<vec>"))
                        {
                            retv = MyVec3.Parse(rv);
                        }
                    }
                    var fi = comTp.GetField(k);
                    if (fi != null)
                    {
                        fi.SetValue(com, retv);
                    }
                }
            }
        }
    }
}
