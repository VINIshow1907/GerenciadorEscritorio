using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;
using GerenciadorEscritorio.BancoDeDados;

namespace GerenciadorEscritorio.Telas
{
    public class TelaHistorico : UserControl
    {
        private Panel pnlCentro;
        private ComboBox cbTipoVisao;
        private ComboBox cbFiltroCategoria;
        private ComboBox cbFiltroPagamento;
        private ComboBox cbFiltroMes;
        private ComboBox cbFiltroAno;
        private TextBox txtBuscaNome;
        private Button btnBuscar;
        private DataGridView gridResultados;
        private Button btnExcluir;
        private Button btnEditar;

        public TelaHistorico()
        {
            ConfigurarLayout();
            CarregarDados();
        }

        private void ConfigurarLayout()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.WhiteSmoke;

            Label lblTitulo = new Label { Text = "Histórico de Lançamentos", Font = new Font("Segoe UI", 24, FontStyle.Bold), AutoSize = true, Location = new Point(30, 25) };
            this.Controls.Add(lblTitulo);

            pnlCentro = new Panel { Size = new Size(850, 540), Location = new Point((this.Width - 850) / 2, 80) };
            this.Controls.Add(pnlCentro);

            Label lblFiltro = new Label { Text = "O que você quer ver?", Location = new Point(0, 0), AutoSize = true, Font = new Font("Segoe UI", 10) };
            cbTipoVisao = new ComboBox { Location = new Point(0, 25), Width = 150, Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cbTipoVisao.Items.AddRange(new string[] { "Receitas (Entradas)", "Despesas (Saídas)" });
            cbTipoVisao.SelectedIndex = 0;

            Label lblFiltroCat = new Label { Text = "Serviço / Categoria:", Location = new Point(170, 0), AutoSize = true, Font = new Font("Segoe UI", 10) };
            cbFiltroCategoria = new ComboBox { Location = new Point(170, 25), Width = 180, Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cbFiltroCategoria.Items.AddRange(new string[] { "Todas", "ITR", "Imposto de Renda", "Documento de Veículo", "Mensalidade do Escritório", "Notas Fiscais", "Outras Receitas / Avulsos", "Aluguel", "Água", "Energia Elétrica", "Internet / Telefone", "Sistemas / Softwares", "Impostos e Taxas", "Manutenção e Limpeza", "Material de Escritório", "Outras Despesas / Imprevistos" });
            cbFiltroCategoria.SelectedIndex = 0;

            Label lblFiltroPag = new Label { Text = "Forma de Pagto:", Location = new Point(370, 0), AutoSize = true, Font = new Font("Segoe UI", 10) };
            cbFiltroPagamento = new ComboBox { Location = new Point(370, 25), Width = 130, Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cbFiltroPagamento.Items.AddRange(new string[] { "Todos", "PIX", "Boleto", "Dinheiro", "Cartão de Débito", "Cartão de Crédito", "Cheque", "Transferência Bancária" });
            cbFiltroPagamento.SelectedIndex = 0;

            Label lblBusca = new Label { Text = "Buscar por Nome:", Location = new Point(520, 0), AutoSize = true, Font = new Font("Segoe UI", 10) };
            txtBuscaNome = new TextBox { Location = new Point(520, 25), Width = 200, Font = new Font("Segoe UI", 10) };

            Label lblFiltroMes = new Label { Text = "Mês:", Location = new Point(0, 60), AutoSize = true, Font = new Font("Segoe UI", 10) };
            cbFiltroMes = new ComboBox { Location = new Point(0, 85), Width = 150, Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cbFiltroMes.Items.AddRange(new string[] { "Todos", "01 - Janeiro", "02 - Fevereiro", "03 - Março", "04 - Abril", "05 - Maio", "06 - Junho", "07 - Julho", "08 - Agosto", "09 - Setembro", "10 - Outubro", "11 - Novembro", "12 - Dezembro" });
            cbFiltroMes.SelectedIndex = 0;

            Label lblFiltroAno = new Label { Text = "Ano:", Location = new Point(170, 60), AutoSize = true, Font = new Font("Segoe UI", 10) };
            cbFiltroAno = new ComboBox { Location = new Point(170, 85), Width = 100, Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cbFiltroAno.Items.AddRange(new string[] { "Todos", "2024", "2025", "2026", "2027", "2028", "2029", "2030" });
            cbFiltroAno.SelectedIndex = 0;

            btnBuscar = new Button { Text = "🔍 Buscar", Location = new Point(290, 83), Width = 100, Height = 30, BackColor = Color.FromArgb(41, 53, 65), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnBuscar.Click += (s, e) => CarregarDados();

            gridResultados = new DataGridView();
            gridResultados.Location = new Point(0, 130);
            gridResultados.Size = new Size(850, 340);
            gridResultados.BackgroundColor = Color.White;
            gridResultados.AllowUserToAddRows = false;
            gridResultados.AllowUserToDeleteRows = false;
            gridResultados.ReadOnly = true;
            gridResultados.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridResultados.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            btnExcluir = new Button { Text = "🗑️ Excluir Selecionado", Location = new Point(215, 490), Width = 200, Height = 40, BackColor = Color.Crimson, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnExcluir.Click += BtnExcluir_Click;

            btnEditar = new Button { Text = "✏️ Editar Selecionado", Location = new Point(435, 490), Width = 200, Height = 40, BackColor = Color.Orange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnEditar.Click += BtnEditar_Click;

            pnlCentro.Controls.Add(lblFiltro); pnlCentro.Controls.Add(cbTipoVisao);
            pnlCentro.Controls.Add(lblFiltroCat); pnlCentro.Controls.Add(cbFiltroCategoria);
            pnlCentro.Controls.Add(lblFiltroPag); pnlCentro.Controls.Add(cbFiltroPagamento);
            pnlCentro.Controls.Add(lblBusca); pnlCentro.Controls.Add(txtBuscaNome);
            pnlCentro.Controls.Add(lblFiltroMes); pnlCentro.Controls.Add(cbFiltroMes);
            pnlCentro.Controls.Add(lblFiltroAno); pnlCentro.Controls.Add(cbFiltroAno);
            pnlCentro.Controls.Add(btnBuscar);
            pnlCentro.Controls.Add(gridResultados);
            pnlCentro.Controls.Add(btnExcluir);
            pnlCentro.Controls.Add(btnEditar);

            this.Resize += (s, e) => pnlCentro.Left = (this.Width - pnlCentro.Width) / 2;
        }

        private void CarregarDados()
        {
            string tipo = cbTipoVisao.SelectedIndex == 0 ? "Receita" : "Despesa";
            string categoriaSelecionada = cbFiltroCategoria.SelectedItem.ToString();
            string pagamentoSelecionado = cbFiltroPagamento.SelectedItem.ToString();
            int mesSelecionado = cbFiltroMes.SelectedIndex;
            string anoSelecionado = cbFiltroAno.SelectedItem.ToString();
            string nomeBusca = txtBuscaNome.Text.Trim();

            // A MÁGICA ACONTECE AQUI: Adicionado r.operador e d.operador nas consultas SQL
            string sql = tipo == "Receita"
                ? @"SELECT r.id AS ""ID"", c.nome AS ""Categoria"", r.cliente AS ""Cliente/Descrição"", r.valor AS ""Valor (R$)"", r.forma_pagamento AS ""Pagamento"", r.data_lancamento AS ""Data"", r.operador AS ""Operador"" FROM Receita r INNER JOIN Categoria c ON r.categoria_id = c.id WHERE 1=1 "
                : @"SELECT d.id AS ""ID"", c.nome AS ""Categoria"", d.descricao AS ""Cliente/Descrição"", d.valor AS ""Valor (R$)"", d.forma_pagamento AS ""Pagamento"", d.data_lancamento AS ""Data"", d.operador AS ""Operador"" FROM Despesa d INNER JOIN Categoria c ON d.categoria_id = c.id WHERE 1=1 ";

            string alias = tipo == "Receita" ? "r" : "d";

            if (categoriaSelecionada != "Todas") sql += " AND c.nome = @categoria ";
            if (pagamentoSelecionado != "Todos") sql += $" AND {alias}.forma_pagamento = @pagamento ";
            if (mesSelecionado > 0) sql += $" AND EXTRACT(MONTH FROM {alias}.data_lancamento) = @mes ";
            if (anoSelecionado != "Todos") sql += $" AND EXTRACT(YEAR FROM {alias}.data_lancamento) = @ano ";

            if (!string.IsNullOrEmpty(nomeBusca))
            {
                if (tipo == "Receita") sql += " AND r.cliente ILIKE @nome ";
                else sql += " AND d.descricao ILIKE @nome ";
            }

            sql += $" ORDER BY {alias}.data_lancamento DESC";

            try
            {
                ConexaoDB db = new ConexaoDB();
                using (NpgsqlConnection conn = db.Conectar())
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    if (categoriaSelecionada != "Todas") cmd.Parameters.AddWithValue("@categoria", categoriaSelecionada);
                    if (pagamentoSelecionado != "Todos") cmd.Parameters.AddWithValue("@pagamento", pagamentoSelecionado);
                    if (mesSelecionado > 0) cmd.Parameters.AddWithValue("@mes", mesSelecionado);
                    if (anoSelecionado != "Todos") cmd.Parameters.AddWithValue("@ano", Convert.ToInt32(anoSelecionado));
                    if (!string.IsNullOrEmpty(nomeBusca)) cmd.Parameters.AddWithValue("@nome", "%" + nomeBusca + "%");

                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        gridResultados.DataSource = dt;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Erro ao carregar dados: " + ex.Message); }
        }

        private void BtnExcluir_Click(object sender, EventArgs e)
        {
            if (gridResultados.SelectedRows.Count == 0) return;
            int idSelecionado = Convert.ToInt32(gridResultados.SelectedRows[0].Cells["ID"].Value);
            string tipo = cbTipoVisao.SelectedIndex == 0 ? "Receita" : "Despesa";

            if (MessageBox.Show($"Tem certeza que deseja excluir este lançamento?", "Confirmação", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    ConexaoDB db = new ConexaoDB();
                    using (NpgsqlConnection conn = db.Conectar())
                    using (NpgsqlCommand cmd = new NpgsqlCommand($"DELETE FROM {tipo} WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", idSelecionado);
                        cmd.ExecuteNonQuery();
                    }
                    CarregarDados();
                }
                catch (Exception ex) { MessageBox.Show("Erro ao excluir: " + ex.Message); }
            }
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            if (gridResultados.SelectedRows.Count == 0) { MessageBox.Show("Por favor, selecione uma linha.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            try
            {
                DataGridViewRow linha = gridResultados.SelectedRows[0];
                int id = Convert.ToInt32(linha.Cells["ID"].Value);
                string tipo = cbTipoVisao.SelectedIndex == 0 ? "Receita" : "Despesa";
                string categoria = linha.Cells["Categoria"].Value?.ToString() ?? "";
                string cliente = linha.Cells["Cliente/Descrição"].Value?.ToString() ?? "";
                string pagamento = linha.Cells["Pagamento"].Value?.ToString() ?? "";

                decimal valor = 0;
                decimal.TryParse(linha.Cells["Valor (R$)"].Value?.ToString(), System.Globalization.NumberStyles.Any, new System.Globalization.CultureInfo("pt-BR"), out valor);

                DateTime data;
                if (!DateTime.TryParse(linha.Cells["Data"].Value?.ToString(), new System.Globalization.CultureInfo("pt-BR"), System.Globalization.DateTimeStyles.None, out data)) data = DateTime.Now;

                FormEditarLancamento formEdit = new FormEditarLancamento(id, tipo, categoria, cliente, valor, pagamento, data);
                if (formEdit.ShowDialog() == DialogResult.OK) CarregarDados();
            }
            catch (Exception ex) { MessageBox.Show("Erro ao preparar a edição: " + ex.Message, "Erro Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}