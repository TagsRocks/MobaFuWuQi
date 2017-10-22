using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;


public class MathUtil
{
    /// <summary>
    /// Clamping a value to be sure it lies between two values
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="aValue"></param>
    /// <param name="aMax"></param>
    /// <param name="aMin"></param>
    /// <returns></returns>
    public static T Clamp<T>(T aValue, T aMin, T aMax) where T : IComparable<T>
    {
        var _Result = aValue;
        if (aValue.CompareTo(aMax) > 0)
            _Result = aMax;
        else if (aValue.CompareTo(aMin) < 0)
            _Result = aMin;
        return _Result;
    }

    //http://lolengine.net/blog/2013/09/18/beautiful-maths-quaternion-from-vectors
    public static Quaternion FromTwoVector(Vector3 a, Vector3 b)
    {
        a = Vector3.Normalize(a);
        b = Vector3.Normalize(b);
        var cosTheta = Vector3.Dot(a, b);
        var halfCos = (float)Math.Sqrt(0.5f * (1 + cosTheta));
        var halfSin = (float)Math.Sqrt(0.5f * (1 - cosTheta));
        var w = Vector3.Normalize(Vector3.Cross(a, b));
        return new Quaternion(halfCos, halfSin * w.X, halfSin*w.Y, halfSin * w.Z);

    }

    //http://www.euclideanspace.com/maths/algebra/vectors/angleBetween/
    public static float RotY(Vector3 dir)
    {
        var deg = Math.Atan2(dir.Z, dir.X) - Math.Atan2(forward.Z, forward.X);
        return RadToDeg((float)deg);
    }
    public static float Math2UnityRot(float deg)
    {
        return -deg;
    }
    public static Vector3 forward = new Vector3(0, 0, 1);
    public static float RadToDeg(float rad)
    {
        return (float)(rad / Math.PI * 180);
    }
}
