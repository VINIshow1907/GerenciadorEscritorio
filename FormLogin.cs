using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Npgsql;
using GerenciadorEscritorio.BancoDeDados;

namespace GerenciadorEscritorio
{
    public class FormLogin : Form
    {
        private TextBox txtUsuario;
        private TextBox txtSenha;
        private Button btnEntrar;
        private Button btnSair;

        public FormLogin()
        {
            this.Text = "Acesso Restrito";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(41, 53, 65);
            this.FormBorderStyle = FormBorderStyle.None;

            Label lblTitulo = new Label { Text = "Controle Financeiro", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(65, 50) };
            this.Controls.Add(lblTitulo);

            Label lblSubtitulo = new Label { Text = "Acesso do Escritório", Font = new Font("Segoe UI", 10), ForeColor = Color.LightGray, AutoSize = true, Location = new Point(130, 90) };
            this.Controls.Add(lblSubtitulo);

            Panel pnlCentro = new Panel { Size = new Size(320, 250), Location = new Point(40, 150), BackColor = Color.White };
            this.Controls.Add(pnlCentro);

            Label lblUsuario = new Label { Text = "Usuário:", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(41, 53, 65), AutoSize = true, Location = new Point(20, 20) };
            txtUsuario = new TextBox { Location = new Point(20, 45), Width = 280, Font = new Font("Segoe UI", 12), Text = "escritorio dirce reis" };
            pnlCentro.Controls.Add(lblUsuario);
            pnlCentro.Controls.Add(txtUsuario);

            Label lblSenha = new Label { Text = "Senha:", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(41, 53, 65), AutoSize = true, Location = new Point(20, 90) };
            txtSenha = new TextBox { Location = new Point(20, 115), Width = 280, Font = new Font("Segoe UI", 12), UseSystemPasswordChar = true };
            pnlCentro.Controls.Add(lblSenha);
            pnlCentro.Controls.Add(txtSenha);

            btnEntrar = new Button { Text = "ENTRAR", Location = new Point(20, 170), Width = 280, Height = 45, BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 12, FontStyle.Bold), Cursor = Cursors.Hand };
            btnEntrar.FlatAppearance.BorderSize = 0;
            btnEntrar.Click += BtnEntrar_Click;
            pnlCentro.Controls.Add(btnEntrar);

            btnSair = new Button { Text = "Sair do Sistema", Location = new Point(140, 430), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Underline), ForeColor = Color.LightGray, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnSair.FlatAppearance.BorderSize = 0;
            btnSair.Click += (s, e) => Application.Exit();
            this.Controls.Add(btnSair);

            this.AcceptButton = btnEntrar;
        }

        // ========================================================
        // NOVO: Método que transforma a senha digitada em HASH
        // ========================================================
        private string GerarHashSenha(string senhaPadrao)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senhaPadrao));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormLogin));
            SuspendLayout();
            // 
            // FormLogin
            // 
            ClientSize = new Size(282, 253);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FormLogin";
            ResumeLayout(false);

        }

        private void BtnEntrar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsuario.Text) || string.IsNullOrEmpty(txtSenha.Text))
            {
                MessageBox.Show("Preencha o usuário e a senha.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                ConexaoDB db = new ConexaoDB();
                using (NpgsqlConnection conn = db.Conectar())
                {
                    // Transforma a senha digitada no mesmo formato salvo no banco
                    string senhaHashed = GerarHashSenha(txtSenha.Text.Trim());

                    string sql = "SELECT COUNT(*) FROM Usuario WHERE login = @login AND senha = @senha";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@login", txtUsuario.Text.Trim());
                        cmd.Parameters.AddWithValue("@senha", senhaHashed); // Passa o Hash para comparar!

                        long count = (long)cmd.ExecuteScalar();
                        if (count > 0)
                        {
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Usuário ou senha incorretos!", "Acesso Negado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            txtSenha.Clear();
                            txtSenha.Focus();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao conectar no banco de dados.\n" + ex.Message, "Erro Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}