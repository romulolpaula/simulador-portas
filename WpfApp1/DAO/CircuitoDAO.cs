using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Classes;

namespace WpfApp1.DAO
{
    public class CircuitoDAO
    {
        private string connectionString = "Data Source=simulador.db;Version=3;";

        public void Inserir(Circuito c)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "INSERT INTO Circuito (Nome, Descricao, IdUsuario) VALUES (@n, @d, @u)";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@n", c.Nome);
                    cmd.Parameters.AddWithValue("@d", c.Descricao);
                    cmd.Parameters.AddWithValue("@u", c.IdUsuario);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Circuito> ListarPorUsuario(int idUsuario)
        {
            List<Circuito> lista = new List<Circuito>();
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Circuito WHERE IdUsuario=@u";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@u", idUsuario);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Circuito
                            {
                                IdCircuito = Convert.ToInt32(reader["IdCircuito"]),
                                Nome = reader["Nome"].ToString(),
                                Descricao = reader["Descricao"].ToString(),
                                IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                                DataCriacao = Convert.ToDateTime(reader["DataCriacao"])
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public void Excluir(int idCircuito)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "DELETE FROM Circuito WHERE IdCircuito=@id";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idCircuito);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
