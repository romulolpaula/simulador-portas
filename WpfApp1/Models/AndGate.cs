using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class AndGate : GateModel
    {
        protected override bool ComputeOutput() => Inputs.Count > 0 && Inputs.All(i => i.Output);
        //retorna true se todas as entradas forem true, caso contrário retorna false
    }
}
