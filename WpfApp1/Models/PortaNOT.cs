using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class PortaNOT : Portas
    {
        public override bool CalcularSaida()
        {
            if (Entradas.Count != 1)
                throw new ArgumentException("A Porta NOT só aceita uma entrada!");

            return !Entradas[0]; // inverte
        }
    }
}
