using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace XLibrary
{
    public partial class ViewModel
    {

        public void DrawTimeline(Graphics buffer)
        {

            // iterate back from current pos timeline until back to start, or start time is newer than start pos start time


            // for each stack item
                // organize
                // filter anything that ended more than 5s ago
                // Dictionary<ThreadID, List<Depth, List<items> > >

            // each row is 20 pix, put row break between threads
            

            // draw nodes as 100px blocks, starting at startpoint

        }
    }
}
