using System;
using System.Windows;
using WpfApp1.Banco_de_Dados;

namespace WpfApp1
{
    public partial class Login : Window
    {
        private UsuarioDAO usuarioDAO = new UsuarioDAO();

        public Login()
        {
            InitializeComponent();
        }

        private void btnEntrar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string email = txtEmail.Text.Trim();
                string senha = txtSenha.Password.Trim();

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(senha))
                {
                    MessageBox.Show("Preencha todos os campos!", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var usuario = usuarioDAO.VerificarLogin(email, senha);

                if (usuario != null)
                {
                    App.CurrentUsername = usuario.UsuarioLogin;
                    App.CurrentUserId = usuario.Id;

                    MessageBox.Show($"Login realizado com sucesso! Bem-vindo, {usuario.Nome}", "Bem-vindo", MessageBoxButton.OK, MessageBoxImage.Information);

                    var menu = new Menu(usuario.Nome); // seu Menu já usa nome
                    menu.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("E-mail ou senha incorretos!", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao tentar realizar login:\n" + ex.Message,
                                "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void btnCadastrar_Click(object sender, RoutedEventArgs e)
        {
            Cadastro cad = new Cadastro();
            cad.ShowDialog();
        }
    }
}
