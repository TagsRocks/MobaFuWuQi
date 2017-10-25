using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleJSON;
using System.IO;

namespace MyLib
{
    class Singleton<T> where T : class, new()
    {
        private static T _Instance;
        public static T Instance
        {
            get
            {
                if(_Instance == null)
                {
                    _Instance = new T();
                }
                return _Instance;
            }
        }

        public virtual void Init()
        {

        }
    }

    class SkillDataManager : Singleton<SkillDataManager>
    {
        private JSONClass data;
        public override void Init()
        {
            using (var f = new StreamReader(string.Format("ConfigData/AllSkill.json")))
            {
                var con = f.ReadToEnd();
                data = JSON.Parse(con).AsObject;
            }
        }
        public JSONClass GetConfig(string key)
        {
            return data[key].AsObject;
        }
    }
}
