using System;
using System.Collections.Generic;
using System.Text;

namespace Spectrogram_Plus.Design
{
    public class SelectedWindowIndices
    {
        private int timeIndex1, timeIndex2, freqIndex1, freqIndex2;
        public SelectedWindowIndices() 
        {
            this.timeIndex1 = 0;
            this.timeIndex2 = 0;
            this.freqIndex1 = 0;
            this.freqIndex2 = 0;
        }
        public SelectedWindowIndices(int timeIndex1, int timeIndex2, int freqIndex1, int freqIndex2)
        {
            this.timeIndex1 = timeIndex1;
            this.timeIndex2 = timeIndex2;
            this.freqIndex1 = freqIndex1;
            this.freqIndex2 = freqIndex2;
        }

        public (int timeIndex1, int timeIndex2, int freqIndex1, int freqIndex2) Indices()
        {
            return (timeIndex1, timeIndex2, freqIndex1, freqIndex2);
        }
        public void Remove()
        {
            this.timeIndex1 = 0;
            this.timeIndex2 = 0;
            this.freqIndex1 = 0;
            this.freqIndex2 = 0;
        }

        public bool Exists()
        {
            return !(timeIndex1 == 0 && timeIndex2 == 0 && freqIndex1 == 0 && freqIndex2 == 0);
        }

    }
}
