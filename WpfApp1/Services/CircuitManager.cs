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
        public List<Wire> Wires { get; } new List<Wire>();

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
