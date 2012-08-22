using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace XLibrary.Remote
{
    public class BandwidthLog
    {
        public CircularBuffer<int> In;
        public CircularBuffer<int> Out;

        public int InPerSec;
        public int OutPerSec;


        public BandwidthLog(int size)
        {
            In = new CircularBuffer<int>(size);
            Out = new CircularBuffer<int>(size);
        }

        public void NextSecond()
        {
            In.Add(InPerSec);
            InPerSec = 0;

            Out.Add(OutPerSec);
            OutPerSec = 0;
        }

        public void Resize(int seconds)
        {
            In.Capacity = seconds;
            Out.Capacity = seconds;
        }

        public float InOutAvg(int period)
        {
            return Average(In, period) + Average(Out, period);
        }

        public float InAvg()
        {
            return Average(In, In.Length);
        }

        public float OutAvg()
        {
            return Average(Out, Out.Length);
        }

        public float Average(CircularBuffer<int> buff, int period)
        {
            float avg = 0;

            int i = 0;
            for (; i < period && i < buff.Length; i++)
                avg += buff[i];

            return (i > 0) ? avg / i : 0;
        }
    }
}
