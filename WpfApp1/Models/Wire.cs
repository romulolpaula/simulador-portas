using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class Wire //classe para o fio de conexão entre as portas
    {
        public GateModel Source { get; set; }
        public GateModel Target { get; set; }
        public int TargetInputIndex { get; set; } //se quiser indexar entradas específicas em portas com múltiplas entradas
    }
}
