using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkPlaces.IO
{
    public struct UpdateProcessChangedArgs
    {
        public int TotalChunks { get; private set; }
        public int CurrentChunks { get; private set; }

        public UpdateProcessChangedArgs(int totalChunks, int currentChunks = 0)
        {
            TotalChunks = totalChunks > 0 ? totalChunks : 1;
            CurrentChunks = currentChunks;
        }

        public void UpdateChunks(int chunks)
        {
            CurrentChunks = chunks <= TotalChunks ? chunks : TotalChunks;
        }
    }
}