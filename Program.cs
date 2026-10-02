using System;
using System.Windows.Forms;

namespace GerenciadorEscritorio
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 1. Abre a fechadura do escritório
            FormLogin telaLogin = new FormLogin();
            if (telaLogin.ShowDialog() == DialogResult.OK)
            {
                // 2. Pergunta quem está usando a máquina agora
                FormSelecionarOperador telaOperador = new FormSelecionarOperador();
                if (telaOperador.ShowDialog() == DialogResult.OK)
                {
                    // 3. Tudo certo! Abre o painel principal
                    Application.Run(new DashboardForm());
                }
                else
                {
                    Application.Exit(); // Se fechar a tela de escolher o nome, sai
                }
            }
            else
            {
                Application.Exit(); // Se errar o login ou fechar, sai
            }
        }
    }
}