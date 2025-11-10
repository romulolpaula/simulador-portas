using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class InputGateAdapter : GateModel
    {
        private readonly InputNode inputNode;

        public InputGateAdapter(InputNode node)
        {
            inputNode = node;
        }

        public override bool Evaluate(HashSet<Guid> visited)
        {
            // garante que o valor do InputNode seja propagado como saída da "porta"
            Output = inputNode.Value;
            return Output;
        }

        public void Reset()
        {
            // reseta igual à lógica normal, se precisar
            Output = false;
        }

        protected override bool ComputeOutput()
        {
            return inputNode.Value;
        }

    }
}
