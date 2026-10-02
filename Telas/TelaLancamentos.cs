using System;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;
using GerenciadorEscritorio.BancoDeDados;

namespace GerenciadorEscritorio.Telas
{
    public class TelaLancamentos : UserControl
    {
        private Panel pnlCentro;
        private ComboBox cbTipo;
        private ComboBox cbCategoria;
        private TextBox txtClienteDescricao;
        private NumericUpDown numValor;
        private DateTimePicker dtpData;
        private ComboBox cbFormaPagamento;
        private NumericUpDown numParcelas;
        private Label lblParcelas;
        private Button btnSalvar;

        public TelaLancamentos(string tipoPadrao = "Receita")
        {
            ConfigurarLayout(tipoPadrao);
            CarregarSugestoes();
        }

        private void ConfigurarLayout(string tipoPadrao)
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.WhiteSmoke;

            Label lblTitulo = new Label { Text = "Registrar Novo Lançamento", Font = new Font("Segoe UI", 24, FontStyle.Bold), AutoSize = true, Location = new Point(30, 25) };
            this.Controls.Add(lblTitulo);

            pnlCentro = new Panel { Size = new Size(900, 250), Location = new Point((this.Width - 900) / 2, 100) };
            this.Controls.Add(pnlCentro);

            Label lblTipo = new Label { Text = "Tipo de Movimentação:", Location = new Point(0, 0), AutoSize = true, Font = new Font("Segoe UI", 10) };
            cbTipo = new ComboBox { Location = new Point(0, 25), Width = 200, Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cbTipo.Items.AddRange(new string[] { "Receita", "Despesa" });
            cbTipo.SelectedItem = tipoPadrao;

            Label lblCliente = new Label { Text = "Cliente / Descrição:", Location = new Point(220, 0), AutoSize = true, Font = new Font("Segoe UI", 10) };
            txtClienteDescricao = new TextBox { Location = new Point(220, 25), Width = 350, Font = new Font("Segoe UI", 10) };

            Label lblFormaPagamento = new Label { Text = "Forma de Pagto:", Location = new Point(590, 0), AutoSize = true, Font = new Font("Segoe UI", 10) };
            cbFormaPagamento = new ComboBox { Location = new Point(590, 25), Width = 180, Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cbFormaPagamento.Items.AddRange(new string[] { "PIX", "Dinheiro", "Cartão de Débito", "Cartão de Crédito", "Cheque", "Transferência Bancária" });
            cbFormaPagamento.SelectedIndex = 0;

            lblParcelas = new Label { Text = "Parcelas:", Location = new Point(790, 0), AutoSize = true, Font = new Font("Segoe UI", 10), Visible = false };
            numParcelas = new NumericUpDown { Location = new Point(790, 25), Width = 80, Font = new Font("Segoe UI", 10), Minimum = 1, Maximum = 120, Value = 1, Visible = false };

            cbFormaPagamento.SelectedIndexChanged += (s, e) =>
            {
                bool isCredito = cbFormaPagamento.SelectedItem.ToString() == "Cartão de Crédito";
                lblParcelas.Visible = isCredito;
                numParcelas.Visible = isCredito;
                if (!isCredito) numParcelas.Value = 1;
            };

            Label lblCategoria = new Label { Text = "Serviço / Categoria:", Location = new Point(0, 75), AutoSize = true, Font = new Font("Segoe UI", 10) };
            cbCategoria = new ComboBox { Location = new Point(0, 100), Width = 200, Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cbCategoria.Items.AddRange(new string[] { "ITR", "Imposto de Renda", "Documento de Veículo", "Mensalidade do Escritório", "Notas Fiscais", "Outras Receitas / Avulsos", "Aluguel", "Água", "Internet", "Material de Escritório", "Outras Despesas / Imprevistos" });
            cbCategoria.SelectedIndex = 0;

            Label lblValor = new Label { Text = "Valor Total (R$):", Location = new Point(220, 75), AutoSize = true, Font = new Font("Segoe UI", 10) };
            numValor = new NumericUpDown { Location = new Point(220, 100), Width = 150, Font = new Font("Segoe UI", 10), DecimalPlaces = 2, Maximum = 999999 };

            Label lblData = new Label { Text = "Data da 1ª Parcela:", Location = new Point(390, 75), AutoSize = true, Font = new Font("Segoe UI", 10) };
            dtpData = new DateTimePicker { Location = new Point(390, 100), Width = 180, Font = new Font("Segoe UI", 10), Format = DateTimePickerFormat.Short };

            btnSalvar = new Button { Text = "Salvar Lançamento", Location = new Point(350, 165), Width = 200, Height = 45, BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSalvar.FlatAppearance.BorderSize = 0;
            btnSalvar.Click += BtnSalvar_Click;

            pnlCentro.Controls.Add(lblTipo); pnlCentro.Controls.Add(cbTipo);
            pnlCentro.Controls.Add(lblCliente); pnlCentro.Controls.Add(txtClienteDescricao);
            pnlCentro.Controls.Add(lblFormaPagamento); pnlCentro.Controls.Add(cbFormaPagamento);
            pnlCentro.Controls.Add(lblParcelas); pnlCentro.Controls.Add(numParcelas);
            pnlCentro.Controls.Add(lblCategoria); pnlCentro.Controls.Add(cbCategoria);
            pnlCentro.Controls.Add(lblValor); pnlCentro.Controls.Add(numValor);
            pnlCentro.Controls.Add(lblData); pnlCentro.Controls.Add(dtpData);
            pnlCentro.Controls.Add(btnSalvar);

            this.Resize += (s, e) => pnlCentro.Left = (this.Width - pnlCentro.Width) / 2;
        }

        private void CarregarSugestoes()
        {
            AutoCompleteStringCollection sugestoes = new AutoCompleteStringCollection();
            try
            {
                ConexaoDB db = new ConexaoDB();
                using (NpgsqlConnection conn = db.Conectar())
                {
                    string sql = "SELECT DISTINCT cliente FROM Receita WHERE cliente IS NOT NULL AND TRIM(cliente) <> ''";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sugestoes.Add(reader["cliente"].ToString());
                        }
                    }
                }
            }
            catch { }

            txtClienteDescricao.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtClienteDescricao.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtClienteDescricao.AutoCompleteCustomSource = sugestoes;
        }

        private void BtnSalvar_Click(object sender, EventArgs e)
        {
            string tipo = cbTipo.SelectedItem.ToString();
            string clienteDescricao = txtClienteDescricao.Text;
            string categoriaNome = cbCategoria.SelectedItem.ToString();
            string formaPagamento = cbFormaPagamento.SelectedItem.ToString();
            decimal valorTotal = numValor.Value;
            DateTime dataInicial = dtpData.Value;
            int parcelas = (int)numParcelas.Value;

            if (string.IsNullOrWhiteSpace(clienteDescricao))
            {
                MessageBox.Show("Por favor, preencha o campo Cliente / Descrição.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
            }
            if (valorTotal <= 0)
            {
                MessageBox.Show("O valor tem de ser maior que zero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
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
                        DateTime dataDaParcelaAtual = dataInicial.AddMonths(i);

                        // O motor SQL agora insere o operador automaticamente
                        string sqlInsert = tipo == "Receita"
                            ? "INSERT INTO Receita (categoria_id, cliente, valor, data_lancamento, descricao, forma_pagamento, operador) VALUES (@catId, @cliente, @valor, @data, @desc, @forma, @operador)"
                            : "INSERT INTO Despesa (categoria_id, valor, data_lancamento, descricao, forma_pagamento, operador) VALUES (@catId, @valor, @data, @desc, @forma, @operador)";

                        using (NpgsqlCommand cmdInsert = new NpgsqlCommand(sqlInsert, conn))
                        {
                            cmdInsert.Parameters.AddWithValue("@catId", categoriaId);
                            cmdInsert.Parameters.AddWithValue("@valor", valorPorParcela);
                            cmdInsert.Parameters.AddWithValue("@data", dataDaParcelaAtual);
                            cmdInsert.Parameters.AddWithValue("@forma", formaPagamento);
                            cmdInsert.Parameters.AddWithValue("@operador", Sessao.OperadorAtual); // Carimba o usuário!

                            if (tipo == "Receita")
                            {
                                cmdInsert.Parameters.AddWithValue("@cliente", clienteDescricao);
                                cmdInsert.Parameters.AddWithValue("@desc", parcelas > 1 ? $"Parcela {i + 1}/{parcelas}" : "");
                            }
                            else
                            {
                                cmdInsert.Parameters.AddWithValue("@desc", parcelas > 1 ? $"{clienteDescricao} (Parcela {i + 1}/{parcelas})" : clienteDescricao);
                            }
                            cmdInsert.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("Lançamento guardado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtClienteDescricao.Text = ""; numValor.Value = 0; dtpData.Value = DateTime.Now; cbFormaPagamento.SelectedIndex = 0; numParcelas.Value = 1;
                CarregarSugestoes();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao guardar na base de dados: " + ex.Message, "Erro Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}