using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XTestLib
{
    public class LibClass
    {
        public int TestMe;

        public void DoMoreStuff()
        {
            int i = 0;
            i++;

            int j = 1;
            j++;

            DoAnonStuff();
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
        }
    }
}
