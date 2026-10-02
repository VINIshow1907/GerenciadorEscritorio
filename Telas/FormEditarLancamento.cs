using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;
using GerenciadorEscritorio.BancoDeDados;

namespace GerenciadorEscritorio.Telas
{
    public class FormEditarLancamento : Form
    {
        private int idLancamento;
        private string tipoLancamento;
        private string nomeOriginal;

        private ComboBox cbCategoria;
        private TextBox txtNome;
        private TextBox txtValor;
        private ComboBox cbPagamento;
        private DateTimePicker dtpData;
        private Button btnSalvar;

        public FormEditarLancamento(int id, string tipo, string categoria, string nome, decimal valor, string pagamento, DateTime data)
        {
            idLancamento = id;
            tipoLancamento = tipo;
            nomeOriginal = nome;

            ConfigurarLayout();
            CarregarCategorias(categoria);

            txtNome.Text = nome;
            txtValor.Text = valor.ToString("N2");
            cbPagamento.SelectedItem = pagamento;
            dtpData.Value = data;
        }

        private void ConfigurarLayout()
        {
            this.Text = $"Editar {tipoLancamento}";
            this.Size = new Size(400, 470);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.WhiteSmoke;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblTitulo = new Label { Text = $"Editar {tipoLancamento}", Font = new Font("Segoe UI", 16, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
            this.Controls.Add(lblTitulo);

            int y = 70;
            this.Controls.Add(new Label { Text = tipoLancamento == "Receita" ? "Cliente:" : "Fornecedor / Descrição:", Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
            txtNome = new TextBox { Location = new Point(20, y + 25), Width = 340, Font = new Font("Segoe UI", 10) };
            this.Controls.Add(txtNome);

            y += 65;
            this.Controls.Add(new Label { Text = "Categoria:", Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
            cbCategoria = new ComboBox { Location = new Point(20, y + 25), Width = 340, Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cbCategoria);

            y += 65;
            this.Controls.Add(new Label { Text = "Forma de Pagto:", Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
            cbPagamento = new ComboBox { Location = new Point(20, y + 25), Width = 160, Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cbPagamento.Items.AddRange(new string[] { "PIX", "Boleto", "Dinheiro", "Cartão de Débito", "Cartão de Crédito", "Cheque", "Transferência Bancária" });
            this.Controls.Add(cbPagamento);

            this.Controls.Add(new Label { Text = "Valor (R$):", Location = new Point(200, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
            txtValor = new TextBox { Location = new Point(200, y + 25), Width = 160, Font = new Font("Segoe UI", 10) };

            // ==========================================
            // MÁSCARA: Bloqueia letras no campo de valor
            // ==========================================
            txtValor.KeyPress += (s, e) =>
            {
                // Permite apenas números, backspace (apagar) e vírgula
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != ',')
                {
                    e.Handled = true;
                }

                // Impede colocar mais de uma vírgula
                if (e.KeyChar == ',' && (s as TextBox).Text.IndexOf(',') > -1)
                {
                    e.Handled = true;
                }
            };
            this.Controls.Add(txtValor);

            y += 65;
            this.Controls.Add(new Label { Text = "Data do Lançamento:", Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
            dtpData = new DateTimePicker { Location = new Point(20, y + 25), Width = 160, Font = new Font("Segoe UI", 10), Format = DateTimePickerFormat.Short };
            this.Controls.Add(dtpData);

            y += 75;
            btnSalvar = new Button { Text = "💾 Salvar Alterações", Location = new Point(20, y), Width = 340, Height = 45, BackColor = Color.FromArgb(41, 128, 185), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 12, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSalvar.FlatAppearance.BorderSize = 0;
            btnSalvar.Click += BtnSalvar_Click;
            this.Controls.Add(btnSalvar);
        }

        private void CarregarCategorias(string categoriaAtual)
        {
            try
            {
                ConexaoDB db = new ConexaoDB();
                using (NpgsqlConnection conn = db.Conectar())
                using (NpgsqlCommand cmd = new NpgsqlCommand($"SELECT id, nome FROM Categoria WHERE tipo = '{tipoLancamento}' ORDER BY nome", conn))
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cbCategoria.Items.Add(new KeyValuePair<int, string>(Convert.ToInt32(reader["id"]), reader["nome"].ToString()));
                    }
                }

                cbCategoria.DisplayMember = "Value";
                cbCategoria.ValueMember = "Key";

                for (int i = 0; i < cbCategoria.Items.Count; i++)
                {
                    var item = (KeyValuePair<int, string>)cbCategoria.Items[i];
                    if (item.Value == categoriaAtual)
                    {
                        cbCategoria.SelectedIndex = i;
                        break;
                    }
                }
            }
            catch { }
        }

        private void InitializeComponent()
        {

        }

        private void BtnSalvar_Click(object sender, EventArgs e)
        {
            if (cbCategoria.SelectedItem == null)
            {
                MessageBox.Show("Selecione uma categoria válida.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
            }

            decimal valorConvertido;
            if (!decimal.TryParse(txtValor.Text, out valorConvertido))
            {
                MessageBox.Show("Por favor, digite um valor numérico válido.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
            }

            int idCategoria = ((KeyValuePair<int, string>)cbCategoria.SelectedItem).Key;
            string tabela = tipoLancamento == "Receita" ? "Receita" : "Despesa";
            string colunaNome = tipoLancamento == "Receita" ? "cliente" : "descricao";

            try
            {
                ConexaoDB db = new ConexaoDB();
                using (NpgsqlConnection conn = db.Conectar())
                {
                    using (NpgsqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            string sqlUpdate = $"UPDATE {tabela} SET {colunaNome} = @nome, categoria_id = @cat, valor = @val, forma_pagamento = @pag, data_lancamento = @data WHERE id = @id";
                            using (NpgsqlCommand cmd = new NpgsqlCommand(sqlUpdate, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@nome", txtNome.Text);
                                cmd.Parameters.AddWithValue("@cat", idCategoria);
                                cmd.Parameters.AddWithValue("@val", valorConvertido);
                                cmd.Parameters.AddWithValue("@pag", cbPagamento.SelectedItem?.ToString() ?? "");
                                cmd.Parameters.AddWithValue("@data", dtpData.Value.Date);
                                cmd.Parameters.AddWithValue("@id", idLancamento);
                                cmd.ExecuteNonQuery();
                            }

                            string pergunta = $"Você deseja aplicar esse novo valor ({valorConvertido:C}) e atualizações para todas as parcelas e lançamentos FUTUROS registrados para '{nomeOriginal}'?";
                            if (MessageBox.Show(pergunta, "Atualização em Lote de Parcelas", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                string sqlLote = $"UPDATE {tabela} SET {colunaNome} = @nome, categoria_id = @cat, valor = @val, forma_pagamento = @pag WHERE {colunaNome} = @nomeOriginal AND data_lancamento > @dataAtual";
                                using (NpgsqlCommand cmdLote = new NpgsqlCommand(sqlLote, conn, trans))
                                {
                                    cmdLote.Parameters.AddWithValue("@nome", txtNome.Text);
                                    cmdLote.Parameters.AddWithValue("@cat", idCategoria);
                                    cmdLote.Parameters.AddWithValue("@val", valorConvertido);
                                    cmdLote.Parameters.AddWithValue("@pag", cbPagamento.SelectedItem?.ToString() ?? "");
                                    cmdLote.Parameters.AddWithValue("@nomeOriginal", nomeOriginal);
                                    cmdLote.Parameters.AddWithValue("@dataAtual", dtpData.Value.Date);

                                    int afetados = cmdLote.ExecuteNonQuery();
                                    if (afetados > 0)
                                    {
                                        MessageBox.Show($"{afetados} lançamentos futuros foram atualizados com sucesso!", "Lote Atualizado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                    else
                                    {
                                        MessageBox.Show("Nenhum lançamento futuro encontrado para este cliente/fornecedor a partir desta data.", "Sem alterações futuras", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                }
                            }

                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            throw ex;
                        }
                    }
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}