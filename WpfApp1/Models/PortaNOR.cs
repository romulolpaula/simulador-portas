using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Classes
{
    class PortaNOR : Portas
    {
        public override bool CalcularSaida()
        {
            foreach (bool entrada in Entradas)
            {
                if (entrada) return false; //se alguma entrada for true, a saída é false
            }

            return true; //só retorna true se todas forem false
        }
    }
}
