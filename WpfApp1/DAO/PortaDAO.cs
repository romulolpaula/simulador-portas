using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Classes;

namespace WpfApp1.DAO
{
    public class PortaDAO
    {
        private string connectionString = "Data Source=simulador.db;Version=3;";

        public void Inserir(Porta p)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "INSERT INTO Porta (IdCircuito, Tipo, Label, PosX, PosY, Inputs, Outputs, Orientation, ZIndex) " +
                             "VALUES (@c, @t, @l, @x, @y, @i, @o, @or, @z)";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@c", p.IdCircuito);
                    cmd.Parameters.AddWithValue("@t", p.Tipo);
                    cmd.Parameters.AddWithValue("@l", p.Label);
                    cmd.Parameters.AddWithValue("@x", p.PosX);
                    cmd.Parameters.AddWithValue("@y", p.PosY);
                    cmd.Parameters.AddWithValue("@i", p.Inputs);
                    cmd.Parameters.AddWithValue("@o", p.Outputs);
                    cmd.Parameters.AddWithValue("@or", p.Orientation);
                    cmd.Parameters.AddWithValue("@z", p.ZIndex);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Porta> ListarPorCircuito(int idCircuito)
        {
            List<Porta> lista = new List<Porta>();
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Porta WHERE IdCircuito=@c";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@c", idCircuito);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Porta
                            {
                                IdPorta = Convert.ToInt32(reader["IdPorta"]),
                                IdCircuito = Convert.ToInt32(reader["IdCircuito"]),
                                Tipo = reader["Tipo"].ToString(),
                                Label = reader["Label"].ToString(),
                                PosX = Convert.ToInt32(reader["PosX"]),
                                PosY = Convert.ToInt32(reader["PosY"]),
                                Inputs = Convert.ToInt32(reader["Inputs"]),
                                Outputs = Convert.ToInt32(reader["Outputs"]),
                                Orientation = reader["Orientation"].ToString(),
                                ZIndex = Convert.ToInt32(reader["ZIndex"])
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
                string sql = "DELETE FROM Porta WHERE IdCircuito=@c";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@c", idCircuito);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}