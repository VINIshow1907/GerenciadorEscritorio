using System;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;
using GerenciadorEscritorio.BancoDeDados;

namespace GerenciadorEscritorio
{
    public class FormSelecionarOperador : Form
    {
        private FlowLayoutPanel painelBotoes;

        public FormSelecionarOperador()
        {
            this.Text = "Quem está operando?";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblTitulo = new Label { Text = "Quem está operando o sistema?", Font = new Font("Segoe UI", 16, FontStyle.Bold), AutoSize = true, Location = new Point(80, 20), ForeColor = Color.FromArgb(41, 53, 65) };
            this.Controls.Add(lblTitulo);

            painelBotoes = new FlowLayoutPanel { Location = new Point(50, 80), Size = new Size(400, 250), FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
            this.Controls.Add(painelBotoes);

            CarregarOperadores();
        }

        private void CarregarOperadores()
        {
            try
            {
                ConexaoDB db = new ConexaoDB();
                using (NpgsqlConnection conn = db.Conectar())
                {
                    string sql = "SELECT nome, telefone FROM Operador ORDER BY nome";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string nome = reader["nome"].ToString();
                            string telefone = reader["telefone"].ToString();

                            // Formata o telefone para ficar bonitão na tela
                            string telefoneFormatado = telefone.Length == 11 ? $"({telefone.Substring(0, 2)}) {telefone.Substring(2, 5)}-{telefone.Substring(7)}" : telefone;

                            Button btnOperador = new Button { Text = $"{nome}\n{telefoneFormatado}", Width = 380, Height = 60, Margin = new Padding(0, 0, 0, 10), Font = new Font("Segoe UI", 12, FontStyle.Bold), BackColor = Color.White, ForeColor = Color.FromArgb(41, 128, 185), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
                            btnOperador.FlatAppearance.BorderColor = Color.FromArgb(41, 128, 185);
                            btnOperador.FlatAppearance.BorderSize = 2;

                            // Quando clicar, salva quem é a pessoa na Sessão e fecha a tela
                            btnOperador.Click += (s, e) =>
                            {
                                Sessao.OperadorAtual = nome;
                                this.DialogResult = DialogResult.OK;
                                this.Close();
                            };

                            painelBotoes.Controls.Add(btnOperador);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar os operadores: " + ex.Message);
            }
        }

        private void InitializeComponent()
        {

        }
    }
}