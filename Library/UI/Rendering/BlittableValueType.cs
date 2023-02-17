using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XLibrary
{
    internal class BlittableValueType
    {
        public static int StrideOf<T>(T[] array) where T : struct
        {
            return StrideOf<T>();
        }

        public static int StrideOf<T>() where T : struct
        {
            return System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
        }
    }
}
