using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class NandGate : GateModel
    {
        protected override bool ComputeOutput() => !(Inputs.Count > 0 && Inputs.All(i => i.Output));
    }
}
