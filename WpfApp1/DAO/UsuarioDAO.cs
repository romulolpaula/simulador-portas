using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Classes;

namespace WpfApp1.DAO
{
    public class UsuarioDAO
    {
        private string connectionString = "Data Source=simulador.db;Version=3;";

        public void Inserir(Usuario u)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "INSERT INTO Usuario (Nome, Email, SenhaHash) VALUES (@n, @e, @s)";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@n", .Nome);
                    cmd.Parameters.AddWithValue("@e", u.Email);
                    cmd.Parameters.AddWithValue("@s", u.SenhaHash);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public Usuario BuscarPorEmail(string email)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Usuario WHERE Email=@e";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@e", email);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Usuario
                            {
                                IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                                Nome = reader["Nome"].ToString(),
                                Email = reader["Email"].ToString(),
                                SenhaHash = reader["SenhaHash"].ToString()
                            };
                        }
                    }
                }
            }
            return null;
        }

        public List<Usuario> ListarTodos()
        {
            List<Usuario> lista = new List<Usuario>();
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Usuario";
                using (var cmd = new SQLiteCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Usuario
                        {
                            IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                            Nome = reader["Nome"].ToString(),
                            Email = reader["Email"].ToString(),
                            SenhaHash = reader["SenhaHash"].ToString()
                        });
                    }
                }
            }
            return lista;
        }

        public void Excluir(int idUsuario)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "DELETE FROM Usuario WHERE IdUsuario=@id";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idUsuario);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
