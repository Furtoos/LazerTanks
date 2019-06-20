using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazerTanks
{
    class JsonTanks
    {
        public JsonTank[] tanks { get; set; } 
        public int number { get; set; }
    }
    class JsonTank
    {
        public string slot { get; set;}
        public string HP { get; set; }
        public string rotationAngle { get; set; }
        public string positionX { get; set; }
        public string positionY { get; set; }
        public List<JsonShell>shells { get; set; } 
        public string shellOnBaraban { get; set; }
    }
    class JsonShell
    {
        public string positionX { get; set; }
        public string positionY { get; set; }
        public string  angle { get; set; }
    }
}
