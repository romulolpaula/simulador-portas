using System;
using System.Windows;
using WpfApp1.Banco_de_Dados;

namespace WpfApp1
{
    public partial class Cadastro : Window
    {
        private UsuarioDAO usuarioDAO = new UsuarioDAO();

        public Cadastro()
        {
            InitializeComponent();
        }

        private void BtnCadastrar_Click(object sender, RoutedEventArgs e)
        {
            string nome = txtNome.Text.Trim();
            string usuario = txtUsuario.Text.Trim();
            string senha = txtSenha.Password.Trim();

            if (usuario.Contains(" "))
            {
                MessageBox.Show("O nome de usuário não pode conter espaços!", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(senha))
            {
                MessageBox.Show("Preencha todos os campos!", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (usuarioDAO.Cadastrar(nome, usuario, senha))
            {
                MessageBox.Show("Cadastro realizado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Usuário já existe!", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
