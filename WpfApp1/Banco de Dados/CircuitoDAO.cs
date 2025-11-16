using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace WpfApp1.Banco_de_Dados
{
    // DTOs simples para comunicação entre UI e DAO
    public class PortaRecord
    {
        public int TempIndex;      // índice temporário na memória (0..N-1)
        public string Tipo;       // nome do tipo, ex: "AndGate"
        public double PosX;
        public double PosY;
        public int Coluna;
        public int IndexNaColuna;
        public int DbId;          // preenchido após salvar (Id da tabela Portas)
    }

    public class ConexaoRecord
    {
        public int SourceTempIndex; // PortaRecord.TempIndex da porta de saída
        public int SourcePortIndex; // índice do pino de saída (geralmente 0)
        public int TargetTempIndex; // PortaRecord.TempIndex da porta de entrada
        public int TargetPortIndex; // índice do pino de entrada
    }

    public class CircuitoData
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Username { get; set; }
        public DateTime DataCriacao { get; set; }

        public List<PortaRecord> Portas { get; set; } = new();
        public List<ConexaoRecord> Conexoes { get; set; } = new();
    }

    public class CircuitoDAO
    {
        public int SalvarCircuito(CircuitoData data)
        {
            using var conn = Database.GetConnection();
            using var tx = conn.BeginTransaction();

            try
            {
                // inserir circuito
                using (var cmd = new SQLiteCommand("INSERT INTO Circuitos (Nome, Username) VALUES (@nome, @user)", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@nome", data.Nome);
                    cmd.Parameters.AddWithValue("@user", data.Username);
                    cmd.ExecuteNonQuery();
                }

                long circuitoId = conn.LastInsertRowId;

                // inserir portas (e anotar dbId)
                foreach (var p in data.Portas)
                {
                    using var cmd = new SQLiteCommand(
                        "INSERT INTO Portas (CircuitoId, Tipo, PosX, PosY, Coluna, IndexNaColuna) VALUES (@cid, @tipo, @x, @y, @col, @idx)",
                        conn, tx);
                    cmd.Parameters.AddWithValue("@cid", circuitoId);
                    cmd.Parameters.AddWithValue("@tipo", p.Tipo);
                    cmd.Parameters.AddWithValue("@x", p.PosX);
                    cmd.Parameters.AddWithValue("@y", p.PosY);
                    cmd.Parameters.AddWithValue("@col", p.Coluna);
                    cmd.Parameters.AddWithValue("@idx", p.IndexNaColuna);
                    cmd.ExecuteNonQuery();
                    p.DbId = (int)conn.LastInsertRowId;
                }

                // inserir conexões — precisamos mapear tempIndex -> dbId
                foreach (var c in data.Conexoes)
                {
                    var src = data.Portas.Find(p => p.TempIndex == c.SourceTempIndex);
                    var tgt = data.Portas.Find(p => p.TempIndex == c.TargetTempIndex);
                    if (src == null || tgt == null) continue;

                    using var cmd = new SQLiteCommand(
                        @"INSERT INTO Conexoes (CircuitoId, PortaSaidaId, PortaSaidaIndice, PortaEntradaId, PortaEntradaIndice)
                          VALUES (@cid, @psid, @psi, @peid, @pei)",
                        conn, tx);
                    cmd.Parameters.AddWithValue("@cid", circuitoId);
                    cmd.Parameters.AddWithValue("@psid", src.DbId);
                    cmd.Parameters.AddWithValue("@psi", c.SourcePortIndex);
                    cmd.Parameters.AddWithValue("@peid", tgt.DbId);
                    cmd.Parameters.AddWithValue("@pei", c.TargetPortIndex);
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();
                return (int)circuitoId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public List<CircuitoData> ListarCircuitosDoUsuario(string username)
        {
            var lista = new List<CircuitoData>();
            using var conn = Database.GetConnection();

            using var cmd = new SQLiteCommand("SELECT Id, Nome, Username, DataCriacao FROM Circuitos WHERE Username = @u ORDER BY DataCriacao DESC", conn);
            cmd.Parameters.AddWithValue("@u", username);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var c = new CircuitoData
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Nome = reader["Nome"].ToString(),
                    Username = reader["Username"].ToString(),
                    DataCriacao = DateTime.Parse(reader["DataCriacao"].ToString())
                };
                lista.Add(c);
            }
            return lista;
        }

        public CircuitoData CarregarCircuito(int circuitoId)
        {
            var data = new CircuitoData();

            using var conn = Database.GetConnection();

            using (var cmd = new SQLiteCommand("SELECT Id, Nome, Username, DataCriacao FROM Circuitos WHERE Id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", circuitoId);
                using var r = cmd.ExecuteReader();
                if (!r.Read()) return null;
                data.Id = Convert.ToInt32(r["Id"]);
                data.Nome = r["Nome"].ToString();
                data.Username = r["Username"].ToString();
                data.DataCriacao = DateTime.Parse(r["DataCriacao"].ToString());
            }

            // carregar portas
            using (var cmd = new SQLiteCommand("SELECT Id, Tipo, PosX, PosY, Coluna, IndexNaColuna FROM Portas WHERE CircuitoId = @cid ORDER BY Id ASC", conn))
            {
                cmd.Parameters.AddWithValue("@cid", circuitoId);
                using var r = cmd.ExecuteReader();
                int tempIndex = 0;
                while (r.Read())
                {
                    var p = new PortaRecord
                    {
                        TempIndex = tempIndex++,
                        Tipo = r["Tipo"].ToString(),
                        PosX = Convert.ToDouble(r["PosX"]),
                        PosY = Convert.ToDouble(r["PosY"]),
                        Coluna = Convert.ToInt32(r["Coluna"]),
                        IndexNaColuna = Convert.ToInt32(r["IndexNaColuna"]),
                        DbId = Convert.ToInt32(r["Id"])
                    };
                    data.Portas.Add(p);
                }
            }

            // carregar conexoes
            using (var cmd = new SQLiteCommand("SELECT PortaSaidaId, PortaSaidaIndice, PortaEntradaId, PortaEntradaIndice FROM Conexoes WHERE CircuitoId = @cid", conn))
            {
                cmd.Parameters.AddWithValue("@cid", circuitoId);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    var psid = Convert.ToInt32(r["PortaSaidaId"]);
                    var peid = Convert.ToInt32(r["PortaEntradaId"]);
                    // map dbId -> tempIndex
                    var src = data.Portas.Find(p => p.DbId == psid);
                    var tgt = data.Portas.Find(p => p.DbId == peid);
                    if (src == null || tgt == null) continue;

                    var c = new ConexaoRecord
                    {
                        SourceTempIndex = src.TempIndex,
                        SourcePortIndex = Convert.ToInt32(r["PortaSaidaIndice"]),
                        TargetTempIndex = tgt.TempIndex,
                        TargetPortIndex = Convert.ToInt32(r["PortaEntradaIndice"])
                    };
                    data.Conexoes.Add(c);
                }
            }

            return data;
        }
    }
}
