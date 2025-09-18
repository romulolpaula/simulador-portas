using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    class PortaXNOR : Portas
    {
        public override bool CalcularSaida()
        {
            if (Entradas.Count != 2)
                throw new ArgumentException("A Porta XNOR só aceita duas entradas");

            return !(Entradas[0] ^ Entradas[1]); // nega o XOR
        }
    }
}
