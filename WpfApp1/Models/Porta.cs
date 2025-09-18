using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Classes
{
    class Porta
    {
        public int IdPorta { get; set; }
        public int IdCircuito { get; set; }
        public required string Tipo { get; set; }
        public required string Label { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
        public int Inputs { get; set; }
        public int Outputs { get; set; }
        public required string Orientation { get; set; }
        public int ZIndex { get; set; }
    }
}
