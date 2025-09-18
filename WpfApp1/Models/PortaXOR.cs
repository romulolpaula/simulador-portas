using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Classes
{
    class PortaXOR : Portas
    {
        public override bool CalcularSaida()
        {
            if (Entradas.Count != 2)
                throw new ArgumentException("A Porta XOR só aceita duas entradas!");

            return Entradas[0] ^ Entradas[1]; // ^ significa Ou Exclusivo
        }
    }
}
