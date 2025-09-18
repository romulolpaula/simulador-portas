using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Classes
{
    public abstract class Portas
    {
        public List<bool> Entradas { get; set; }
        public abstract bool CalcularSaida();
    }
}
