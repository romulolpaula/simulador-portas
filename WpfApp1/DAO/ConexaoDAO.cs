using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Classes;

namespace WpfApp1.DAO
{
    public class ConexaoDAO
    {
        private string connectionString = "Data Source=simulador.db;Version=3;";

        public void Inserir(Conexao c)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "INSERT INTO Conexao (IdCircuito, IdPortaOrigem, OrigemPin, IdPortaDestino, DestinoPin, PathJSON) " +
                             "VALUES (@c, @po, @op, @pd, @dp, @pj)";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@c", c.IdCircuito);
                    cmd.Parameters.AddWithValue("@po", c.IdPortaOrigem);
                    cmd.Parameters.AddWithValue("@op", c.OrigemPin);
                    cmd.Parameters.AddWithValue("@pd", c.IdPortaDestino);
                    cmd.Parameters.AddWithValue("@dp", c.DestinoPin);
                    cmd.Parameters.AddWithValue("@pj", c.PathJSON);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Conexao> ListarPorCircuito(int idCircuito)
        {
            List<Conexao> lista = new List<Conexao>();
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Conexao WHERE IdCircuito=@c";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@c", idCircuito);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Conexao
                            {
                                IdConexao = Convert.ToInt32(reader["IdConexao"]),
                                IdCircuito = Convert.ToInt32(reader["IdCircuito"]),
                                IdPortaOrigem = Convert.ToInt32(reader["IdPortaOrigem"]),
                                OrigemPin = Convert.ToInt32(reader["OrigemPin"]),
                                IdPortaDestino = Convert.ToInt32(reader["IdPortaDestino"]),
                                DestinoPin = Convert.ToInt32(reader["DestinoPin"]),
                                PathJSON = reader["PathJSON"].ToString()
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public void ExcluirPorCircuito(int idCircuito)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "DELETE FROM Conexao WHERE IdCircuito=@c";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@c", idCircuito);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}