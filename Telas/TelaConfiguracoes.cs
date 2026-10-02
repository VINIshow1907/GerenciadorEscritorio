using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Npgsql;
using GerenciadorEscritorio.BancoDeDados;

namespace GerenciadorEscritorio.Telas
{
    public class TelaConfiguracoes : UserControl
    {
        private Panel pnlCentro;
        private TextBox txtServidor, txtPorta, txtUsuario, txtSenha, txtBanco;
        private Button btnTestar, btnSalvar, btnBackup;

        public TelaConfiguracoes()
        {
            ConfigurarLayout();
            CarregarDados();
        }

        private void ConfigurarLayout()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.WhiteSmoke;
            this.AutoScroll = true;

            Label lblTitulo = new Label { Text = "Configurações do Banco de Dados", Font = new Font("Segoe UI", 24, FontStyle.Bold), AutoSize = true, Location = new Point(30, 25) };
            this.Controls.Add(lblTitulo);

            pnlCentro = new Panel { Size = new Size(500, 480), Location = new Point((this.Width - 500) / 2, 100), BackColor = Color.White };
            this.Controls.Add(pnlCentro);

            int y = 20;
            pnlCentro.Controls.Add(new Label { Text = "Servidor (Host):", Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
            txtServidor = new TextBox { Location = new Point(20, y + 25), Width = 460, Font = new Font("Segoe UI", 10) };
            pnlCentro.Controls.Add(txtServidor);

            y += 65;
            pnlCentro.Controls.Add(new Label { Text = "Porta:", Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
            txtPorta = new TextBox { Location = new Point(20, y + 25), Width = 150, Font = new Font("Segoe UI", 10) };
            pnlCentro.Controls.Add(txtPorta);

            pnlCentro.Controls.Add(new Label { Text = "Nome do Banco:", Location = new Point(190, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
            txtBanco = new TextBox { Location = new Point(190, y + 25), Width = 290, Font = new Font("Segoe UI", 10) };
            pnlCentro.Controls.Add(txtBanco);

            y += 65;
            pnlCentro.Controls.Add(new Label { Text = "Usuário:", Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
            txtUsuario = new TextBox { Location = new Point(20, y + 25), Width = 220, Font = new Font("Segoe UI", 10) };
            pnlCentro.Controls.Add(txtUsuario);

            pnlCentro.Controls.Add(new Label { Text = "Senha:", Location = new Point(260, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
            txtSenha = new TextBox { Location = new Point(260, y + 25), Width = 220, Font = new Font("Segoe UI", 10), UseSystemPasswordChar = true };
            pnlCentro.Controls.Add(txtSenha);

            y += 80;
            btnTestar = new Button { Text = "🔄 Testar", Location = new Point(20, y), Width = 220, Height = 45, BackColor = Color.Orange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
            btnTestar.FlatAppearance.BorderSize = 0;
            btnTestar.Click += BtnTestar_Click;
            pnlCentro.Controls.Add(btnTestar);

            btnSalvar = new Button { Text = "💾 Salvar", Location = new Point(260, y), Width = 220, Height = 45, BackColor = Color.FromArgb(41, 128, 185), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSalvar.FlatAppearance.BorderSize = 0;
            btnSalvar.Click += BtnSalvar_Click;
            pnlCentro.Controls.Add(btnSalvar);

            // ==========================================
            // NOVA SEÇÃO: ROTINA DE BACKUP DO SISTEMA
            // ==========================================
            y += 75;
            Panel linhaSep = new Panel { Size = new Size(460, 2), Location = new Point(20, y), BackColor = Color.LightGray };
            pnlCentro.Controls.Add(linhaSep);

            y += 15;
            Label lblSeguranca = new Label { Text = "Segurança de Dados", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(41, 53, 65), AutoSize = true, Location = new Point(20, y) };
            pnlCentro.Controls.Add(lblSeguranca);

            y += 25;
            Label lblDescricaoBackup = new Label { Text = "Gere uma cópia completa de todos os clientes e lançamentos.", Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, AutoSize = true, Location = new Point(20, y) };
            pnlCentro.Controls.Add(lblDescricaoBackup);

            y += 30;
            btnBackup = new Button { Text = "📦  Gerar Backup do Sistema", Location = new Point(20, y), Width = 460, Height = 45, BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
            btnBackup.FlatAppearance.BorderSize = 0;
            btnBackup.Click += BtnBackup_Click;
            pnlCentro.Controls.Add(btnBackup);

            this.Resize += (s, e) => {
                pnlCentro.Left = Math.Max(0, (this.Width - pnlCentro.Width) / 2);
                pnlCentro.Top = Math.Max(20, (this.Height - pnlCentro.Height) / 2);
            };
        }

        private void CarregarDados()
        {
            var config = ConexaoDB.LerConfiguracao();
            txtServidor.Text = config.Servidor;
            txtPorta.Text = config.Porta;
            txtBanco.Text = config.Banco;
            txtUsuario.Text = config.Usuario;
            txtSenha.Text = config.Senha;
        }

        private void BtnTestar_Click(object sender, EventArgs e)
        {
            string connString = $"Server={txtServidor.Text};Port={txtPorta.Text};User Id={txtUsuario.Text};Password={txtSenha.Text};Database={txtBanco.Text};";
            try
            {
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();
                    MessageBox.Show("Conexão bem-sucedida! O sistema consegue se comunicar com o banco de dados.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao conectar no banco:\n" + ex.Message, "Falha de Conexão", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSalvar_Click(object sender, EventArgs e)
        {
            var config = new ConfigDados
            {
                Servidor = txtServidor.Text,
                Porta = txtPorta.Text,
                Banco = txtBanco.Text,
                Usuario = txtUsuario.Text,
                Senha = txtSenha.Text
            };
            ConexaoDB.SalvarConfiguracao(config);
            MessageBox.Show("Configurações salvas permanentemente!", "Salvo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnBackup_Click(object sender, EventArgs e)
        {
            // Caminho padrão do utilitário de Backup do PostgreSQL 16
            string pgDumpPath = @"C:\Program Files\PostgreSQL\16\bin\pg_dump.exe";

            if (!File.Exists(pgDumpPath))
            {
                MessageBox.Show("Não foi possível encontrar a ferramenta de backup do PostgreSQL no computador.\n\nVerifique se o PostgreSQL 16 está instalado no caminho padrão (C:\\Program Files\\PostgreSQL\\16).", "Ferramenta não encontrada", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Arquivo de Backup SQL (*.sql)|*.sql";
            sfd.FileName = $"Backup_Financeiro_{DateTime.Now:yyyy_MM_dd}.sql";
            sfd.Title = "Onde deseja salvar o Backup do Sistema?";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    this.Cursor = Cursors.WaitCursor;
                    var config = ConexaoDB.LerConfiguracao();

                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = pgDumpPath;

                    // Comandos para o PostgreSQL fazer uma cópia limpa do banco
                    psi.Arguments = $"-h {config.Servidor} -p {config.Porta} -U {config.Usuario} --inserts --clean -f \"{sfd.FileName}\" {config.Banco}";

                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardError = true;
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;

                    // Passa a senha de forma segura para o processo (escondido)
                    psi.EnvironmentVariables["PGPASSWORD"] = config.Senha;

                    using (Process process = Process.Start(psi))
                    {
                        process.WaitForExit();
                        if (process.ExitCode == 0)
                        {
                            MessageBox.Show("Backup gerado com sucesso!\n\nRecomendamos guardar este arquivo em um PenDrive ou no Google Drive do escritório.", "Segurança Garantida", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            string erro = process.StandardError.ReadToEnd();
                            MessageBox.Show("Ocorreu um erro na ferramenta do banco de dados:\n" + erro, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro inesperado ao gerar backup:\n" + ex.Message, "Erro Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                }
            }
        }
    }
}