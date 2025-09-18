using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    class PortaXOR : Portas
    {
        public override bool CalcularSaida()
        {
            bool resultado = false;
            foreach (bool entrada in Entradas)
            {
                resultado ^= entrada; 
            }
            return resultado;
        }
    }
}
