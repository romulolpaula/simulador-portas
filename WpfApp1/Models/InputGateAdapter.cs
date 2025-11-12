using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class InputGateAdapter : GateModel
    {
        private readonly bool _value;

        public InputGateAdapter(bool value)
        {
            _value = value;
        }

        protected override bool ComputeOutput() => _value;
    }
}

