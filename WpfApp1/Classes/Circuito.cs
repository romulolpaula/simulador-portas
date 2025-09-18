using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Classes
{
    class Circuito
    {
        public int IdCircuito { get; set; }
        public required string Nome { get; set; }
        public string? Descricao { get; set; }
        public int IdUsuario { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
