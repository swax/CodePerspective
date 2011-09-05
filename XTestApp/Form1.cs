using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using XTestLib;

namespace XTestApp
{
    public partial class Form1 : Form
    {
        string testVar = null;

        public Form1()
        {
            InitializeComponent();
        }

        public void TryStuff()
        {

            try
            {
                StaticMan.HolyCow();

                var a = new NestedClass();

                string yy = null;
                yy = testVar;
                //testVar = "hello";

                a.DoStuff();

                var x = new int[] { 1, 2, 3, 4 };

                var y = x.Where(i => i > 2).ToArray();
            }
            catch (Exception ex)
            {

            }


            var t = new LibClass();
            t.TestMe = 5;
            t.DoMoreStuff();
        }

        public class NestedClass
        {
            int testVar2 = 33;

            public void DoStuff()
            {
                int i = 0;
                i += 5;

                int j = 5;
                i += j;

                i = testVar2;

                j = i;

                DivideByZero();
            }

            public int DivideByZero()
            {
                int x = 0;
                x++;

                int y = 1;
                y--;

                return x / y;
            }
        }

        private void throwButton_Click(object sender, EventArgs e)
        {
            TryStuff();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GC.Collect();
        }
    }
}