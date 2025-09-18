using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class PortaOR : Portas 
    {
        public override bool CalcularSaida()
        {
            foreach (bool entrada in Entradas)
            {
                if (entrada) return true;
            }
            return false;
        }
    }
}
