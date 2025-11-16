using System;
using System.Data.SQLite;
using System.Windows;

namespace WpfApp1.Banco_de_Dados
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string UsuarioLogin { get; set; }
    }

    public class UsuarioDAO
    {
        public UsuarioDAO() { }

        public bool Cadastrar(string nome, string usuario, string senha)
        {
            try
            {
                using var conexao = Database.GetConnection();

                string sqlVerificar = "SELECT COUNT(*) FROM Usuarios WHERE Username = @usuario";
                using var cmdVerificar = new SQLiteCommand(sqlVerificar, conexao);
                cmdVerificar.Parameters.AddWithValue("@usuario", usuario);
                long qtd = (long)cmdVerificar.ExecuteScalar();

                if (qtd > 0) return false;

                string sql = "INSERT INTO Usuarios (Nome, Username, Senha) VALUES (@nome, @usuario, @senha)";
                using var cmd = new SQLiteCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@nome", nome);
                cmd.Parameters.AddWithValue("@usuario", usuario);
                cmd.Parameters.AddWithValue("@senha", senha);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao cadastrar: " + ex.Message, "Erro");
                return false;
            }
        }

        public Usuario VerificarLogin(string usuario, string senha)
        {
            try
            {
                using var conexao = Database.GetConnection();

                string sql = "SELECT Id, Nome, Username FROM Usuarios WHERE Username = @usuario AND Senha = @senha LIMIT 1";
                using var cmd = new SQLiteCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@usuario", usuario);
                cmd.Parameters.AddWithValue("@senha", senha);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new Usuario
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Nome = reader["Nome"].ToString(),
                        UsuarioLogin = reader["Username"].ToString()
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao verificar login: " + ex.Message, "Erro");
                return null;
            }
        }

        public int GetUsuarioId(string username)
        {
            using var conn = Database.GetConnection();
            using var cmd = new SQLiteCommand("SELECT Id FROM Usuarios WHERE Username=@username", conn);
            cmd.Parameters.AddWithValue("@username", username);
            var result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : -1;
        }

        public string GetNomeUsuario(string username)
        {
            using var conn = Database.GetConnection();
            using var cmd = new SQLiteCommand("SELECT Nome FROM Usuarios WHERE Username=@username", conn);
            cmd.Parameters.AddWithValue("@username", username);
            var result = cmd.ExecuteScalar();
            return result != null ? result.ToString() : "";
        }
    }
}
