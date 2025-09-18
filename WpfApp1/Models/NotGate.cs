using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class NotGate : GateModel
    {
        protected override bool ComputeOutput() => Inputs.Count > 0 && !Inputs[0].Output : true;
                //retorna true se a única entrada for false, caso contrário retorna false
    }
}
