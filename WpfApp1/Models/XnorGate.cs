using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class XnorGate : GateModel
    {
        protected override bool ComputeOutput() => Inputs.Count > 0 && Inputs.Count(i => i.Output) % 2 == 0;
    }
}
