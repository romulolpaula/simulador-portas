using System;
using System.Data.SQLite;
using System.IO;
using System.Windows;

namespace WpfApp1.Banco_de_Dados
{
    public static class Database
    {
        private static readonly string DatabaseFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SimuladorPortas.db");
        private static readonly string ConnectionString = $"Data Source={DatabaseFile};Version=3;";

        // cria o arquivo e as tabelas (chame uma vez no App startup)
        public static void InicializarBanco()
        {
            try
            {
                if (!File.Exists(DatabaseFile))
                {
                    SQLiteConnection.CreateFile(DatabaseFile);
                }

                using var conn = new SQLiteConnection(ConnectionString);
                conn.Open();

                // tabela Usuarios (note nomes e colunas compatíveis com DAOs)
                string sqlUsuarios = @"
                    CREATE TABLE IF NOT EXISTS Usuarios (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Nome TEXT NOT NULL,
                        Username TEXT NOT NULL UNIQUE,
                        Senha TEXT NOT NULL
                    );";
                using (var cmd = new SQLiteCommand(sqlUsuarios, conn)) { cmd.ExecuteNonQuery(); }

                // tabela Circuitos
                string sqlCircuitos = @"
                    CREATE TABLE IF NOT EXISTS Circuitos (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Nome TEXT NOT NULL,
                        Username TEXT NOT NULL,
                        DataCriacao DATETIME DEFAULT CURRENT_TIMESTAMP
                    );";
                using (var cmd = new SQLiteCommand(sqlCircuitos, conn)) { cmd.ExecuteNonQuery(); }

                // tabela Portas
                string sqlPortas = @"
                    CREATE TABLE IF NOT EXISTS Portas (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        CircuitoId INTEGER NOT NULL,
                        Tipo TEXT NOT NULL,
                        PosX REAL NOT NULL,
                        PosY REAL NOT NULL,
                        Coluna INTEGER NOT NULL,
                        IndexNaColuna INTEGER NOT NULL,
                        FOREIGN KEY (CircuitoId) REFERENCES Circuitos(Id)
                    );";
                using (var cmd = new SQLiteCommand(sqlPortas, conn)) { cmd.ExecuteNonQuery(); }

                // tabela Conexoes
                string sqlConexoes = @"
                    CREATE TABLE IF NOT EXISTS Conexoes (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        CircuitoId INTEGER NOT NULL,
                        PortaSaidaId INTEGER NOT NULL,
                        PortaSaidaIndice INTEGER NOT NULL,
                        PortaEntradaId INTEGER NOT NULL,
                        PortaEntradaIndice INTEGER NOT NULL,
                        FOREIGN KEY (CircuitoId) REFERENCES Circuitos(Id),
                        FOREIGN KEY (PortaSaidaId) REFERENCES Portas(Id),
                        FOREIGN KEY (PortaEntradaId) REFERENCES Portas(Id)
                    );";
                using (var cmd = new SQLiteCommand(sqlConexoes, conn)) { cmd.ExecuteNonQuery(); }

                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao inicializar banco: " + ex.Message, "Erro DB", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // retorna conexão aberta — use THIS everywhere (padroniza)
        public static SQLiteConnection GetConnection()
        {
            try
            {
                if (!File.Exists(DatabaseFile))
                {
                    // caso alguém chame GetConnection sem InicializarBanco, cria o arquivo
                    SQLiteConnection.CreateFile(DatabaseFile);
                }

                var conn = new SQLiteConnection(ConnectionString);
                conn.Open();
                return conn;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao abrir conexão com o banco: " + ex.Message, "Erro DB", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }
    }
}
