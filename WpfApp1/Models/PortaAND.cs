using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Classes
{
    public class PortaAND : Portas
    {
        public override bool CalcularSaida()
        {
            foreach (bool entrada in Entradas)
            {
                if (!entrada) return false; //!entrada inverte o valor pra true, então é o mesmo que entrada == false
            }
            return true; //só retorna true se todas forem true
        }
    }
}
