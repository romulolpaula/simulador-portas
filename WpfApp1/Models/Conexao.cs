using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Classes
{
    class Conexao
    {
        public int IdConexao { get; set; }
        public int IdCircuito { get; set; }
        public int IdPortaOrigem { get; set; }
        public int OrigemPin { get; set; }
        public int IdPortaDestino { get; set; }
        public int DestinoPin { get; set; }
        public string? PathJSON { get; set; }
    }
}
