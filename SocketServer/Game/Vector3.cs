using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    public class Vector3
    {
        //Unity 坐标*100 厘米
        public int x, y, z;

        public static Vector3 Parse(string s)
        {
            var t = s.IndexOf('>');
            Debug.Log("SubStringIs: "+s);
            var v = s.Substring(t+1).Split(',');
            var xx = Convert.ToInt32(v[0]);
            var yy = Convert.ToInt32(v[1]);
            var zz = Convert.ToInt32(v[2]);
            var vc = new Vector3()
            {
                x=xx,
                y=yy,
                z=zz,
            };
            return vc;

        }
    }
}
