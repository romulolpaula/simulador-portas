using System;
using System.Collections.Generic;

namespace WpfApp1.Models
{
    public class OutputNode : GateModel
    {
        public override bool Evaluate(HashSet<Guid> visited)
        {
            if (Inputs.Count > 0)
            {
                // Avalia o nó de entrada antes de pegar o valor
                Inputs[0].Evaluate(visited);
                Output = Inputs[0].Output;
            }
            return Output;
        }

        // Saída não faz nenhuma computação lógica
        protected override bool ComputeOutput()
        {
            return Output;
        }
    }
}
