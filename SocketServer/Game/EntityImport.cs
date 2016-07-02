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
            g.pos = Vector3.Parse(jobj["Pos"].Value);
            g.scale = Vector3.Parse(jobj["Scale"].Value);
            Debug.Log("InitGameObject: "+g.name);
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
                Debug.Log("InitComponent: "+g.name+" tp "+tp);
                var ac =g.GetType().GetMethod("AddComponent");
                var ge = ac.MakeGenericMethod(tp);
                var com = ge.Invoke(g, null);
                ReadAttr(com, jobj); 
            }
        }

        private static void ReadAttr(object com, JSONClass jobj)
        {
            var comTp = com.GetType();
            foreach (KeyValuePair<string, JSONNode> j in jobj)
            {
                var k = j.Key;
                var jd = j.Value as JSONData;
                object retv;
                var tp  =jd.GetValueType(out retv);
                if (tp == typeof(string))
                {
                    var rv = retv as string;
                    if (rv.StartsWith("<vec>"))
                    {
                        retv = Vector3.Parse(rv);
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
