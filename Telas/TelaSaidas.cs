using System;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;
using GerenciadorEscritorio.BancoDeDados;

namespace GerenciadorEscritorio.Telas
{
    public class TelaSaidas : UserControl
    {
        private Panel pnlCentro;
        private TextBox txtFornecedorDescricao;
        private ComboBox cbCategoria;
        private ComboBox cbFormaPagamento;
        private NumericUpDown numParcelas;
        private Label lblParcelas;
        private NumericUpDown numValor;
        private DateTimePicker dtpData;
        private Button btnSalvar;

        public TelaSaidas()
        {
            ConfigurarLayout();
            CarregarSugestoes(); // <-- A MÁGICA PARA FORNECEDORES
        }

        private void ConfigurarLayout()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.WhiteSmoke;

            Label lblTitulo = new Label { Text = "Registrar Saída / Despesa", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Color.FromArgb(192, 57, 43), AutoSize = true, Location = new Point(30, 25) };
            this.Controls.Add(lblTitulo);

            pnlCentro = new Panel { Size = new Size(700, 250), Location = new Point((this.Width - 700) / 2, 100) };
            this.Controls.Add(pnlCentro);

            Label lblDescricao = new Label { Text = "Fornecedor / Descrição:", Location = new Point(0, 0), AutoSize = true, Font = new Font("Segoe UI", 10) };
            txtFornecedorDescricao = new TextBox { Location = new Point(0, 25), Width = 380, Font = new Font("Segoe UI", 10) };

            Label lblFormaPagamento = new Label { Text = "Forma de Pagto:", Location = new Point(400, 0), AutoSize = true, Font = new Font("Segoe UI", 10) };
            cbFormaPagamento = new ComboBox { Location = new Point(400, 25), Width = 180, Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cbFormaPagamento.Items.AddRange(new string[] { "PIX", "Boleto", "Dinheiro", "Cartão de Débito", "Cartão de Crédito", "Cheque", "Transferência Bancária" });
            cbFormaPagamento.SelectedIndex = 0;

            lblParcelas = new Label { Text = "Parcelas:", Location = new Point(600, 0), AutoSize = true, Font = new Font("Segoe UI", 10), Visible = false };
            numParcelas = new NumericUpDown { Location = new Point(600, 25), Width = 80, Font = new Font("Segoe UI", 10), Minimum = 1, Maximum = 120, Value = 1, Visible = false };

            cbFormaPagamento.SelectedIndexChanged += (s, e) =>
            {
                bool isCredito = cbFormaPagamento.SelectedItem.ToString() == "Cartão de Crédito";
                lblParcelas.Visible = isCredito;
                numParcelas.Visible = isCredito;
                if (!isCredito) numParcelas.Value = 1;
            };

            Label lblCategoria = new Label { Text = "Categoria da Despesa:", Location = new Point(0, 75), AutoSize = true, Font = new Font("Segoe UI", 10) };
            cbCategoria = new ComboBox { Location = new Point(0, 100), Width = 230, Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cbCategoria.Items.AddRange(new string[] { "Aluguel", "Água", "Energia Elétrica", "Internet / Telefone", "Material de Escritório", "Sistemas / Softwares", "Impostos e Taxas", "Manutenção e Limpeza", "Outras Despesas / Imprevistos" });
            cbCategoria.SelectedIndex = 0;

            Label lblValor = new Label { Text = "Valor Total (R$):", Location = new Point(250, 75), AutoSize = true, Font = new Font("Segoe UI", 10) };
            numValor = new NumericUpDown { Location = new Point(250, 100), Width = 150, Font = new Font("Segoe UI", 10), DecimalPlaces = 2, Maximum = 999999 };

            Label lblData = new Label { Text = "Data do Pagamento:", Location = new Point(420, 75), AutoSize = true, Font = new Font("Segoe UI", 10) };
            dtpData = new DateTimePicker { Location = new Point(420, 100), Width = 160, Font = new Font("Segoe UI", 10), Format = DateTimePickerFormat.Short };

            btnSalvar = new Button { Text = "Salvar Despesa", Location = new Point(250, 165), Width = 200, Height = 45, BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSalvar.FlatAppearance.BorderSize = 0;
            btnSalvar.Click += BtnSalvar_Click;

            pnlCentro.Controls.Add(lblDescricao); pnlCentro.Controls.Add(txtFornecedorDescricao);
            pnlCentro.Controls.Add(lblFormaPagamento); pnlCentro.Controls.Add(cbFormaPagamento);
            pnlCentro.Controls.Add(lblParcelas); pnlCentro.Controls.Add(numParcelas);
            pnlCentro.Controls.Add(lblCategoria); pnlCentro.Controls.Add(cbCategoria);
            pnlCentro.Controls.Add(lblValor); pnlCentro.Controls.Add(numValor);
            pnlCentro.Controls.Add(lblData); pnlCentro.Controls.Add(dtpData);
            pnlCentro.Controls.Add(btnSalvar);

            this.Resize += (s, e) => pnlCentro.Left = (this.Width - pnlCentro.Width) / 2;
        }

        // ====================================================================
        // NOVO: Busca fornecedores no banco
        // ====================================================================
        private void CarregarSugestoes()
        {
            AutoCompleteStringCollection sugestoes = new AutoCompleteStringCollection();
            try
            {
                ConexaoDB db = new ConexaoDB();
                using (NpgsqlConnection conn = db.Conectar())
                {
                    string sql = "SELECT DISTINCT descricao FROM Despesa WHERE descricao IS NOT NULL AND TRIM(descricao) <> ''";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sugestoes.Add(reader["descricao"].ToString());
                        }
                    }
                }
            }
            catch { }

            txtFornecedorDescricao.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtFornecedorDescricao.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtFornecedorDescricao.AutoCompleteCustomSource = sugestoes;
        }

        private void BtnSalvar_Click(object sender, EventArgs e)
        {
            string descricao = txtFornecedorDescricao.Text;
            string categoriaNome = cbCategoria.SelectedItem.ToString();
            string formaPagamento = cbFormaPagamento.SelectedItem.ToString();
            decimal valorTotal = numValor.Value;
            DateTime dataInicial = dtpData.Value;
            int parcelas = (int)numParcelas.Value;

            if (string.IsNullOrWhiteSpace(descricao))
            {
                MessageBox.Show("Por favor, informe a descrição ou fornecedor.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
            }
            if (valorTotal <= 0)
            {
                MessageBox.Show("O valor precisa ser maior que zero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
            }

            try
            {
                ConexaoDB db = new ConexaoDB();
                using (NpgsqlConnection conn = db.Conectar())
                {
                    int categoriaId = 0;
                    using (NpgsqlCommand cmdCat = new NpgsqlCommand("SELECT id FROM Categoria WHERE nome = @nome LIMIT 1", conn))
                    {
                        cmdCat.Parameters.AddWithValue("@nome", categoriaNome);
                        object result = cmdCat.ExecuteScalar();
                        if (result != null) categoriaId = Convert.ToInt32(result);
                    }

                    decimal valorPorParcela = valorTotal / parcelas;
                    for (int i = 0; i < parcelas; i++)
                    {
                        DateTime dataAtual = dataInicial.AddMonths(i);
                        string descFinal = parcelas > 1 ? $"{descricao} (Parcela {i + 1}/{parcelas})" : descricao;

                        using (NpgsqlCommand cmdInsert = new NpgsqlCommand("INSERT INTO Despesa (categoria_id, valor, data_lancamento, descricao, forma_pagamento) VALUES (@catId, @valor, @data, @desc, @forma)", conn))
                        {
                            cmdInsert.Parameters.AddWithValue("@catId", categoriaId);
                            cmdInsert.Parameters.AddWithValue("@valor", valorPorParcela);
                            cmdInsert.Parameters.AddWithValue("@data", dataAtual);
                            cmdInsert.Parameters.AddWithValue("@desc", descFinal);
                            cmdInsert.Parameters.AddWithValue("@forma", formaPagamento);
                            cmdInsert.ExecuteNonQuery();
                        }
                    }
                }
                MessageBox.Show("Despesa registrada com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtFornecedorDescricao.Text = ""; numValor.Value = 0; dtpData.Value = DateTime.Now; cbFormaPagamento.SelectedIndex = 0; numParcelas.Value = 1;
                CarregarSugestoes(); // Atualiza a lista
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar despesa: " + ex.Message, "Erro Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}