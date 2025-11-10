using System;
using System.Collections.Generic;

namespace WpfApp1.Models
{
    public abstract class GateModel
    {
        public Guid Id { get; } = Guid.NewGuid(); // identificador único da porta
        public List<GateModel> Inputs { get; } = new List<GateModel>(); // portas que alimentam esta
        public bool Output { get; protected set; } // valor lógico de saída

        protected abstract bool ComputeOutput();

        public bool DependsOn(GateModel other)
        {
            if (Inputs.Contains(other))
                return true;

            foreach (var input in Inputs)
            {
                if (input.DependsOn(other))
                    return true;
            }

            return false;
        }

        public virtual bool Evaluate(HashSet<Guid> visited = null)
        {
            if (visited == null)
                visited = new HashSet<Guid>();

            // se já foi avaliado antes, evita ciclos e reavaliações desnecessárias
            if (visited.Contains(Id))
                return Output;

            visited.Add(Id);

            // avalia recursivamente as entradas (se houver)
            foreach (var input in Inputs)
                input.Evaluate(visited);

            // calcula a saída da porta atual
            Output = ComputeOutput();

            return Output;
        }
    }
}
