using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    class PortaNAND : Portas 
    {
        public override bool CalcularSaida()
        {
            foreach (bool entrada in Entradas)
            {
                if (!entrada) return true; //!entrada inverte o valor pra true, então é o mesmo que entrada == false
            }

            return false; //só retorna false se todas forem true
        }
    }
}
