using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class OrGate : GateModel
    {
        protected override bool ComputeOutput() => Inputs.Count > 0 && Inputs.Any(i => i.Output);
    }
}
