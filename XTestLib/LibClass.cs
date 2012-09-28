using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XTestLib
{
    public class TestBase
    {
        public int BaseA = 4;
        public int BaseB = 3;
        public static int StaticBaseC = 5;
        public static int[] StaticArray = new int[] { 1, 2, 3, 4, 5 };
    }

    public class LibClass : TestBase
    {
        public int TestMe;

        public int[] arrayTest;
        public int[][] arrayTest2;
        public int[,] arrayTest3;

        
        int dummy = 0;

        public int PublicVar = 2;
        private int PrivateVar = 3;
        static int StaticVar = 4;

        public int[] TestArray = new int[] { 6, 7, 8, 9, 10 };

        delegate void TestDelegate(int x);

        TestDelegate RunDelegate;


        Dictionary<int, string> TestMap1 = new Dictionary<int, string>();
        Dictionary<int, int> TestMap2 = new Dictionary<int, int>();

        public LibClass()
        {
            RunDelegate += DelegateTarget;

            var x = TestMap1.Values;
            var y = TestMap2.Values;
           
            // also try this
            var z = TestMap1.Keys;

            TestFunc();
            TestFunc2();
            TestFunc3();
            TestFunc4();
            TestFunc5(5);
            TestFunc6<int, string>();
            TestFunc7();
            TestFunc8();
            TestFunc9(1, "adsf");
            TestFunc10(2, "dsf");
            TestFunc11(new StructY<StructZ<int>>());

            int testRef = 5;
            TestFunc12(ref testRef);

            TestFunc13(IntPtr.Zero);

            double testRef2 = 5.4;
            string stringRef = "dafd";
            TestFunc14(3.3, 4, ref testRef2, 6.6, ref stringRef);

            TestFunc15(ref testRef);

            IntPtr ptr = TestFunc16();
            int w = TestFunc17(ref testRef);

            TestFunc18();
            TestFunc19();

            var test20 = new T3Class<int>();
            test20.TClassTestFunc3();
        }

        int TestFunc19()
        {
            // test GenericInstanceMethod gets wraped on method exit

            return T2Class<int>.T2ClassTest<string>(3, "hello");
        }

        int TestFunc18()
        {
            // testing returning a generic param defined by a generic instance type and not a generic method

            return TClass<int>.TClassTestFunc();
        }

        public static class TClass<T>
        {
            public static T TClassTestFunc()
            {
                return default(T);
            }
        }

        public static class T2Class<T>
        {
            public static T T2ClassTest<U>(T t, U u)
            {
                U testU = default(U);

                return default(T);
            }
        }

        public class T3Class<T>
        {
            public void TClassTestFunc3()
            {
                int[] testAddingWrapperToGenericStaticClass = new int[] { 3, 3, 4, 5 };

                testAddingWrapperToGenericStaticClass.Reverse();

                SpecialTestMethod();
            }

            public static void SpecialTestMethod()
            {

            }
        }

        int TestFunc17(ref int x)
        {
            return x;
        }

        IntPtr TestFunc16()
        {
            return IntPtr.Zero;
        }

        private void TestFunc15<T>(ref T x)
        {
            x = default(T);
        }

        private void TestFunc12(ref int testRef)
        {
            testRef = 13;
        }

 
        private IntPtr TestFunc13(IntPtr x)
        {
            return x;
        }

        private int TestFunc14(double dT, int groundDistance, ref double vel, double accel, ref string teststringref)
        {
            var x = new object[] { dT, groundDistance, vel, accel, teststringref };

            TestMethodEnter(x, 3);

            return 5;
        }

        public string TestFunc()
        {
            string x = "adfa";

            return (string)TestMethodExit(x);
        }

        public int TestFunc2()
        {
            int x = 5;

            return (int)TestMethodExit(x);
        }

        public void TestFunc3()
        {
            TestMethodExit(null);
        }

        public StructX TestFunc4()
        {
            var x = new StructX();

            return (StructX)TestMethodExit(x);
        }

        public static T TestFunc5<T>(T x)
        {

            return (T)TestMethodExit(x);
        }

        public Dictionary<T, V> TestFunc6<T, V>()
        {
            return (Dictionary<T, V>)TestMethodExit(TestMap1);
        }

        public void TestFunc7()
        {
            Dictionary<int, string> x = new Dictionary<int,string>();

            var y = x.GetEnumerator();

            TestMethodExit(y);
        }

        public void TestFunc8()
        {
            var x = new SortedList<int, string>();

            var y = x.GetEnumerator();
        }

        public void TestFunc9(int x, object b)
        {

        }
        
        public void TestFunc10(int x, object b)
        {
            var args = new object[] { x, b };

            TestMethodEnter(args, 3);
        }

        public void TestFunc11<T>(StructY<StructZ<T>> b)
        {
            var args = new object[] { b };

            TestMethodEnter(args, 3);
        }

        public void TestMethodEnter(object[] args, int id)
        {

        }

        public struct StructX
        {
            public int x;
        }

        public struct StructY<T>
        {
            public T yx;
        }

        public struct StructZ<T>
        {
            public T zx;
        }

        public static object TestMethodExit(object x)
        {
            return x;
        }

        public void DoMoreStuff()
        {
            LibClass x = new LibClass();
            x.DelegateTarget(3);

            int i = 0;
            i++;

            int j = 1;
            j++;

            DoAnonStuff();

            RunDelegate(5);
        }

        public void DoAnonStuff()
        {
            int lazyThings = 4;

            var makeCake = new Func<int, int, int>((eggs, flour) => 
                {
                    int jumboEggs = eggs;
                    int wheatflour = flour;


                    var putInOven = new Func<int, int>((hands) =>
                        {
                            int fingers = 5;
                            
                            return fingers * hands + jumboEggs + lazyThings;
                        }
                    );


                    return jumboEggs + wheatflour + lazyThings * putInOven(2);

                }
            );

            var cakes = makeCake(5, 3);

            cakes++;


            var bakeAnotherCake = new Func<int>(() => 5);

            cakes += bakeAnotherCake();
        }

        public void DelegateTarget(int x)
        {
            x++;
        }
    }

    public class LibConstructClass
    {
        LibConstructClass()
        {
            var x = 0;
            x++;
        }

        void TestX(int x)
        {
            x = 0;
            x++;
        }

        void TestY()
        {
            TestX(3);
        }
    }

    public class LibDeconstructClass
    {
        ~LibDeconstructClass()
        {
            var x = 0;
            x++;
        }
    }

    public class CopyConstructor
    {
        CopyConstructor()
        {

            TestMethod(0, this);
        }

        CopyConstructor(int x)
        {
            x = 0;
            x++;
        }

        void TestMethod(int index, Object y)
        {
            index++;
        }
    }

    public static class StaticMan
    {
        static int test = 4;

        static StaticMan()
        {
            int x = 0;
            x++;
        }

        public static void HolyCow(bool recurse = true)
        {
            int x = 0;
            x++;

            if(recurse)
                Recurse1();
        }

        public static void Recurse1()
        {
            int x = StaticTemplateClass<int>.d;
            x++;

            string z = StaticTemplateClass<string>.d;
            z += "dfa"; 

            Recurse2();
        }

        public static void Recurse2()
        {
            int x = 0;
            x++;

            HolyCow(false);
        }
    }

    public static class SmallStatic
    {
        static int x = 5;

        static SmallStatic()
        {
            
        }

        static void MockConstuctor()
        {
            TestMethod(5, typeof(SmallStatic));
        }

        static void TestMethod(int index, object y)
        {
            x++;

            var z = new TemplateClass<string, string>();
        }
    }

    public static class StaticTemplateClass<T>
    {
        public static int a = 1;
        public static int b = 2;
        public static int c = 3;
        public static T d;

        static StaticTemplateClass()
        {
            var x = typeof(StaticTemplateClass<T>);
        }
    }

    public class TemplateClass<T1, T2>
    {
        int FillerFunction()
        {
            int x = 1;
            int y = 2;
            int z = 3;
            return x + y + z;
        }
    }

}
