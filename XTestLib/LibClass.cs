using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XTestLib
{
    public class LibClass
    {
        public int TestMe;


        public int PublicVar = 2;
        private int PrivateVar = 3;
        static int StaticVar = 4;

        delegate void TestDelegate(int x);

        TestDelegate RunDelegate;

        public LibClass()
        {
            RunDelegate += DelegateTarget;
        }

        public void DoMoreStuff()
        {
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

        public static void HolyCow()
        {
            int x = 0;
            x++;
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
        }
    }

}
