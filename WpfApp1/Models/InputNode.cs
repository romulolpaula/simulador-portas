using System;
using System.Collections.Generic;

namespace WpfApp1.Models
{
    public class InputNode : GateModel
    {
        public bool Value { get; set; } = false;

        protected override bool ComputeOutput()
        {
            return Value;
        }
    }
}
