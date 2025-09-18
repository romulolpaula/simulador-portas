using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class XorGate : GateModel
    {
        protected override bool ComputeOutput() => Inputs.Count > 0 && Inputs.Count(i => i.Output) % 2 == 1;
        //retorna true se o número de entradas true for impar 
    }
}
