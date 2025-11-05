using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public abstract class GateModel
    {
        public Guid Id { get; } = Guid.NewGuid(); //Guid é identificador global, aqui ta sendo criado um novo automaticamente
        public List<GateModel> Inputs { get; } = new List<GateModel>(); //cada item da lista é outro gate model, ou seja, as estradas dessa porta vêm da saída de outras portas
        public bool Output { get; protected set; } //guarda o valor lógico de saída da porta, protected indica que só a propria classe ou filhas podem alterar o valor

        protected abstract bool ComputeOutput(); 

        public virtual bool Evaluate(HashSet<Guid> visited = null) //método que avalia a saída da porta recursivamente
        {
            if (visited == null) visited = new HashSet<Guid>();
            if (visited.Contains(Id)) throw new InvalidOperationException("Ciclo detectado"); //verifica se o Id da porta já foi visitado, se sim, lança uma exceção indicando que há um ciclo
            visited.Add(Id);

            foreach (var input in Inputs)
                input.Evaluate(visited);

            Output = ComputeOutput(); 
            visited.Remove(Id); //remove o Id do conjunto de visitados para permitir outras avaliações
            return Output;

        }
    }
}
