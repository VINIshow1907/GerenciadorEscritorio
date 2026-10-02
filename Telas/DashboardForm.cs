using System;
using System.Drawing;
using System.Windows.Forms;
using GerenciadorEscritorio.Telas;

namespace GerenciadorEscritorio
{
    public partial class DashboardForm : Form
    {
        private Panel menuLateral;
        private Panel painelCentral;
        private Label lblEndereco;

        public DashboardForm()
        {
            ConfigurarLayout();
        }

        private void ConfigurarLayout()
        {
            this.Text = "Controle Financeiro - Escritório";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(900, 600); // Impede que a tela seja espremida demais

            menuLateral = new Panel();
            menuLateral.Dock = DockStyle.Left;
            menuLateral.Width = 65; // Inicia recolhido
            menuLateral.BackColor = Color.FromArgb(41, 53, 65);

            // Botões agora criados com o Ícone e o Texto separados para o controle inteligente
            Button btnDashboard = CriarBotaoMenu("📊", "  Dashboard", 60);
            Button btnEntradas = CriarBotaoMenu("💰", "  Lançamentos", 120);
            Button btnSaidas = CriarBotaoMenu("📉", "  Saídas", 180);
            Button btnHistorico = CriarBotaoMenu("📅", "  Histórico e Filtros", 240);
            Button btnRelatorios = CriarBotaoMenu("📑", "  Relatórios", 300);
            Button btnConfig = CriarBotaoMenu("⚙️", "  Configurações", 360);

            btnDashboard.Click += (sender, e) => MostrarTela(new TelaDashboard());
            btnEntradas.Click += (sender, e) => MostrarTela(new TelaLancamentos());
            btnSaidas.Click += (sender, e) => MostrarTela(new TelaSaidas());
            btnHistorico.Click += (sender, e) => MostrarTela(new TelaHistorico());
            btnRelatorios.Click += (sender, e) => MostrarTela(new TelaRelatorios());
            btnConfig.Click += (sender, e) => MostrarTela(new TelaConfiguracoes());

            lblEndereco = new Label();
            lblEndereco.Text = "Grasiela: (17) 99735-9288\nPablo: (17) 99791-3937\nAv. Princesa Isabel, 2338\nCentro - Dirce Reis";
            lblEndereco.ForeColor = Color.Gray;
            lblEndereco.Dock = DockStyle.Bottom;
            lblEndereco.Height = 80;
            lblEndereco.TextAlign = ContentAlignment.MiddleCenter;
            lblEndereco.Visible = false; // Esconde o texto quando o menu estiver recolhido

            menuLateral.Controls.Add(btnDashboard);
            menuLateral.Controls.Add(btnEntradas);
            menuLateral.Controls.Add(btnSaidas);
            menuLateral.Controls.Add(btnHistorico);
            menuLateral.Controls.Add(btnRelatorios);
            menuLateral.Controls.Add(btnConfig);
            menuLateral.Controls.Add(lblEndereco);

            // Adiciona a inteligência de expandir/recolher
            menuLateral.MouseEnter += ExpandirMenu;
            menuLateral.MouseLeave += RecolherMenu;
            foreach (Control c in menuLateral.Controls)
            {
                c.MouseEnter += ExpandirMenu;
                c.MouseLeave += RecolherMenu;
            }

            painelCentral = new Panel();
            painelCentral.Dock = DockStyle.Fill;
            painelCentral.BackColor = Color.WhiteSmoke;

            this.Controls.Add(painelCentral);
            this.Controls.Add(menuLateral);

            MostrarTela(new TelaDashboard());
        }

        private Button CriarBotaoMenu(string icone, string texto, int topo)
        {
            Button btn = new Button();

            // Guarda o ícone na posição 0 e o texto na posição 1 dentro da Tag
            btn.Tag = new string[] { icone, texto };
            btn.Text = icone; // Como o menu inicia recolhido (65px), mostra apenas o ícone

            btn.Top = topo;
            btn.Left = 0;
            btn.Width = 250;
            btn.Height = 50;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 11, FontStyle.Regular);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(15, 0, 0, 0); // Alinha o ícone perfeitamente com a margem
            btn.Cursor = Cursors.Hand;
            return btn;
        }

        private void ExpandirMenu(object sender, EventArgs e)
        {
            menuLateral.Width = 250;
            lblEndereco.Visible = true;

            // Restaura o texto completo nos botões
            foreach (Control c in menuLateral.Controls)
            {
                if (c is Button btn && btn.Tag is string[] tags)
                {
                    btn.Text = tags[0] + tags[1]; // Junta o ícone + texto
                }
            }
        }

        private void RecolherMenu(object sender, EventArgs e)
        {
            // Confirma se o mouse realmente saiu do painel lateral
            if (!menuLateral.ClientRectangle.Contains(menuLateral.PointToClient(Cursor.Position)))
            {
                menuLateral.Width = 65;
                lblEndereco.Visible = false;

                // Limpa o texto, deixando apenas o ícone
                foreach (Control c in menuLateral.Controls)
                {
                    if (c is Button btn && btn.Tag is string[] tags)
                    {
                        btn.Text = tags[0]; // Puxa apenas o ícone da Tag
                    }
                }
            }
        }

        private void MostrarTela(UserControl tela)
        {
            painelCentral.Controls.Clear();
            tela.Dock = DockStyle.Fill;
            painelCentral.Controls.Add(tela);
        }
    }
}