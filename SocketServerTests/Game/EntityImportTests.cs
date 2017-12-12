using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MyLib.Tests
{
    [TestClass()]
    public class EntityImportTests
    {
        [AssemblyInitialize]
        public static void Configure(TestContext tc)
        {
            LogHelper.Init();
        }
        public TestContext TestContext { get; set; }

        [TestMethod()]
        public void InitGameObjectTest()
        {
            /*
            var path = Util.GetBinPath();
            var fullPath = System.IO.Path.Combine(path, string.Format("level_1_4_{0}.json", 103));
            
            var solution_dir = Path.GetDirectoryName(Path.GetDirectoryName(TestContext.TestDir));
            LogHelper.Log("Test", solution_dir);
            Console.WriteLine("hahaa");
            LogHelper.Log("Test", fullPath);
            using (var f = new System.IO.StreamReader(fullPath))
            {
                var con = f.ReadToEnd();
                var jobj = SimpleJSON.JSON.Parse(con).AsObject;
                var entityConfig = EntityImport.InitGameObject(jobj);
            }
            */
            using(var f = new System.IO.StreamReader("ConfigData/monsterSingle.json"))
            {
                var con = f.ReadToEnd();
                var jobj = SimpleJSON.JSON.Parse(con).AsObject;
                var ety = EntityImport.InitGameObject(jobj);
            }
        }
        
    }
}