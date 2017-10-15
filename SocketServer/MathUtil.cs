using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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
}
