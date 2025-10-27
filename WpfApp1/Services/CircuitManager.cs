using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Models;

namespace WpfApp1.Services
{
    public class CircuitManager
    {
        public List<GateModel> Gates { get; } = new List<GateModel>();
        public List<Wire> Wires { get; } = new List<Wire>();

        public bool CreatesCycle(GateModel source, GateModel target)
        {
            if (source == target) return true; //se a source e a target forem iguais é um ciclo direto

            return HasPathToSource(target, source, new HashSet<GateModel>());
        }

        private bool HasPathToSource(GateModel current, GateModel target, HashSet<GateModel> visited)
        {
            if (current == null || visited.Contains(current)) return false;

            if (current == target) return true;

            visited.Add(current);

            foreach (var wire in Wires.Where(w => w.Source == current))
            {
                if (HasPathToSource(wire.Target, target, visited))
                    return true;
            }

            return false; 
        }

        public void EvaluateAll()
        {
            var visited = new HashSet<Guid>();
            foreach (var g in Gates) 
            {
                try { g.Evaluate(visited); } catch { /*tratar ciclos */}
            }
        }
    }
}
