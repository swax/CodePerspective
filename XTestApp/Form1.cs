using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace XTestApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public void TryStuff()
        {
            try
            {
                var a = new NestedClass();

                a.DoStuff();

                var x = new int[] {1,2,3,4};

                var y = x.Where(i => i > 2).ToArray();
            }
            catch (Exception ex)
            {

            }
        }

        public class NestedClass
        {
            public void DoStuff()
            {
                int i = 0;
                i += 5;

                int j = 5;
                i += j;

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
    }
}
