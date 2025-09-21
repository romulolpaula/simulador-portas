using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class InputNode : GateModel 
    {
        public bool Value { get; set; }
        protected override bool ComputeOutput() => Value; //retorna o valor lógico definido para o nó de entrada

        public override bool Evaluate(HashSet<Guid> visited)
        {
            return Value; //retorna o valor definido pelo usuário
        }
    }
}
