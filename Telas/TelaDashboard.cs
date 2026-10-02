using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using Npgsql;
using GerenciadorEscritorio.BancoDeDados;

namespace GerenciadorEscritorio.Telas
{
    public class TelaDashboard : UserControl
    {
        private Label lblTitulo;
        private Label lblFiltro;
        private ComboBox cbFiltroPeriodo;
        private Button btnGerarPDF;
        private Panel pnlCards;

        private Label lblValorReceitas;
        private Label lblValorDespesas;
        private Label lblValorSaldo;

        private Label lblTop1Servico;
        private Label lblTop2Servico;
        private Label lblTop3Servico;
        private Label lblMelhorClienteNome;
        private Label lblMelhorClienteValor;

        private DataGridView gridReceitas;
        private DataGridView gridDespesas;

        private PictureBox picGrafico;
        private Dictionary<string, decimal> dadosGrafico = new Dictionary<string, decimal>();

        public TelaDashboard()
        {
            ConfigurarLayout();
            CarregarResumo();
        }

        private void ConfigurarLayout()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.WhiteSmoke;
            this.AutoScroll = true;

            lblTitulo = new Label { Text = "Visão Geral", Font = new System.Drawing.Font("Segoe UI", 24, FontStyle.Bold), AutoSize = true, Location = new Point(30, 25) };
            this.Controls.Add(lblTitulo);

            lblFiltro = new Label { Text = "Período:", AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(550, 35) };
            cbFiltroPeriodo = new ComboBox { Width = 150, Font = new System.Drawing.Font("Segoe UI", 12), DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(620, 32) };
            cbFiltroPeriodo.Items.AddRange(new string[] { "Esta Semana", "Este Mês", "Este Ano", "Todo o Período" });
            cbFiltroPeriodo.SelectedIndex = 1;
            cbFiltroPeriodo.SelectedIndexChanged += (s, e) => CarregarResumo();

            btnGerarPDF = new Button { Text = "📄 Gerar PDF", Width = 140, Height = 35, BackColor = Color.FromArgb(41, 53, 65), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand, Location = new Point(780, 30) };
            btnGerarPDF.FlatAppearance.BorderSize = 0;
            btnGerarPDF.Click += BtnGerarPDF_Click;

            this.Controls.Add(lblFiltro);
            this.Controls.Add(cbFiltroPeriodo);
            this.Controls.Add(btnGerarPDF);

            pnlCards = new Panel { Size = new Size(850, 780), Location = new Point((this.ClientSize.Width - 850) / 2, 90) };
            this.Controls.Add(pnlCards);

            Panel cardReceitas = CriarCartao("Receitas", Color.FromArgb(46, 204, 113), new Point(0, 0), out lblValorReceitas);
            Panel cardDespesas = CriarCartao("Despesas", Color.FromArgb(231, 76, 60), new Point(300, 0), out lblValorDespesas);
            Panel cardSaldo = CriarCartao("Saldo Líquido", Color.FromArgb(41, 128, 185), new Point(600, 0), out lblValorSaldo);
            pnlCards.Controls.Add(cardReceitas);
            pnlCards.Controls.Add(cardDespesas);
            pnlCards.Controls.Add(cardSaldo);

            Panel panelTop3 = new Panel { Location = new Point(0, 150), Size = new Size(400, 180), BackColor = Color.White };
            Label lblTop3Titulo = new Label { Text = "🏆 Top 3 Serviços (Receita)", Font = new System.Drawing.Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(15, 15), AutoSize = true };
            lblTop1Servico = new Label { Text = "1. -", Font = new System.Drawing.Font("Segoe UI", 12), Location = new Point(15, 60), AutoSize = true, ForeColor = Color.FromArgb(39, 174, 96) };
            lblTop2Servico = new Label { Text = "2. -", Font = new System.Drawing.Font("Segoe UI", 12), Location = new Point(15, 95), AutoSize = true, ForeColor = Color.FromArgb(39, 174, 96) };
            lblTop3Servico = new Label { Text = "3. -", Font = new System.Drawing.Font("Segoe UI", 12), Location = new Point(15, 130), AutoSize = true, ForeColor = Color.FromArgb(39, 174, 96) };
            panelTop3.Controls.Add(lblTop3Titulo); panelTop3.Controls.Add(lblTop1Servico); panelTop3.Controls.Add(lblTop2Servico); panelTop3.Controls.Add(lblTop3Servico);
            pnlCards.Controls.Add(panelTop3);

            Panel panelTopCliente = new Panel { Location = new Point(450, 150), Size = new Size(400, 180), BackColor = Color.White };
            Label lblClienteTitulo = new Label { Text = "⭐ Cliente Mais Rentável", Font = new System.Drawing.Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(15, 15), AutoSize = true };
            lblMelhorClienteNome = new Label { Text = "-", Font = new System.Drawing.Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(15, 65), AutoSize = true, ForeColor = Color.FromArgb(41, 53, 65) };
            lblMelhorClienteValor = new Label { Text = "R$ 0,00", Font = new System.Drawing.Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(15, 110), AutoSize = true, ForeColor = Color.FromArgb(41, 128, 185) };
            panelTopCliente.Controls.Add(lblClienteTitulo); panelTopCliente.Controls.Add(lblMelhorClienteNome); panelTopCliente.Controls.Add(lblMelhorClienteValor);
            pnlCards.Controls.Add(panelTopCliente);

            Label lblUltimasReceitas = new Label { Text = "📈 Últimas 6 Entradas", Font = new System.Drawing.Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(0, 350), AutoSize = true, ForeColor = Color.FromArgb(39, 174, 96) };
            pnlCards.Controls.Add(lblUltimasReceitas);
            gridReceitas = new DataGridView { Location = new Point(0, 380), Size = new Size(400, 165), BackgroundColor = Color.White, AllowUserToAddRows = false, ReadOnly = true, RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            pnlCards.Controls.Add(gridReceitas);

            Label lblUltimasDespesas = new Label { Text = "📉 Últimas 6 Saídas", Font = new System.Drawing.Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(450, 350), AutoSize = true, ForeColor = Color.FromArgb(231, 76, 60) };
            pnlCards.Controls.Add(lblUltimasDespesas);
            gridDespesas = new DataGridView { Location = new Point(450, 380), Size = new Size(400, 165), BackgroundColor = Color.White, AllowUserToAddRows = false, ReadOnly = true, RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            pnlCards.Controls.Add(gridDespesas);

            Label lblGraficoTitulo = new Label { Text = "📊 Distribuição de Despesas (Saídas)", Font = new System.Drawing.Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(0, 560), AutoSize = true, ForeColor = Color.FromArgb(41, 53, 65) };
            pnlCards.Controls.Add(lblGraficoTitulo);

            picGrafico = new PictureBox { Location = new Point(0, 590), Size = new Size(850, 180), BackColor = Color.White };
            picGrafico.Paint += PicGrafico_Paint;
            pnlCards.Controls.Add(picGrafico);

            this.Resize += (s, e) => {
                btnGerarPDF.Left = Math.Max(0, this.ClientSize.Width - btnGerarPDF.Width - 30);
                cbFiltroPeriodo.Left = Math.Max(0, btnGerarPDF.Left - cbFiltroPeriodo.Width - 15);
                lblFiltro.Left = Math.Max(0, cbFiltroPeriodo.Left - lblFiltro.Width - 10);

                // Evita que o painel seja empurrado para a esquerda (negativo) ao diminuir a tela
                pnlCards.Left = Math.Max(0, (this.ClientSize.Width - pnlCards.Width) / 2);
            };
        }

        private Panel CriarCartao(string titulo, Color corFundo, Point posicao, out Label lblValor)
        {
            Panel card = new Panel { Size = new Size(250, 130), Location = posicao, BackColor = corFundo };
            Label lblTituloCard = new Label { Text = titulo, Font = new System.Drawing.Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(15, 15) };
            lblValor = new Label { Text = "R$ 0,00", Font = new System.Drawing.Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(15, 60) };
            card.Controls.Add(lblTituloCard); card.Controls.Add(lblValor);
            return card;
        }

        private string ObterFiltroSql()
        {
            int periodo = cbFiltroPeriodo.SelectedIndex;
            if (periodo == 0) return "EXTRACT(WEEK FROM data_lancamento) = EXTRACT(WEEK FROM CURRENT_DATE) AND EXTRACT(YEAR FROM data_lancamento) = EXTRACT(YEAR FROM CURRENT_DATE)";
            if (periodo == 1) return "EXTRACT(MONTH FROM data_lancamento) = EXTRACT(MONTH FROM CURRENT_DATE) AND EXTRACT(YEAR FROM data_lancamento) = EXTRACT(YEAR FROM CURRENT_DATE)";
            if (periodo == 2) return "EXTRACT(YEAR FROM data_lancamento) = EXTRACT(YEAR FROM CURRENT_DATE)";
            return "1=1";
        }

        private void CarregarResumo()
        {
            lblTitulo.Text = "Visão Geral - " + cbFiltroPeriodo.SelectedItem.ToString().ToUpper();
            string filtroSql = ObterFiltroSql();
            decimal totalReceitas = 0, totalDespesas = 0;

            try
            {
                ConexaoDB db = new ConexaoDB();
                using (NpgsqlConnection conn = db.Conectar())
                {
                    using (NpgsqlCommand cmd = new NpgsqlCommand($"SELECT SUM(valor) FROM Receita WHERE {filtroSql}", conn))
                    {
                        object result = cmd.ExecuteScalar();
                        if (result != DBNull.Value && result != null) totalReceitas = Convert.ToDecimal(result);
                    }
                    using (NpgsqlCommand cmd = new NpgsqlCommand($"SELECT SUM(valor) FROM Despesa WHERE {filtroSql}", conn))
                    {
                        object result = cmd.ExecuteScalar();
                        if (result != DBNull.Value && result != null) totalDespesas = Convert.ToDecimal(result);
                    }

                    lblTop1Servico.Text = "1. -"; lblTop2Servico.Text = "2. -"; lblTop3Servico.Text = "3. -";
                    lblMelhorClienteNome.Text = "Nenhum cliente no período"; lblMelhorClienteValor.Text = "R$ 0,00";

                    string sqlTop3 = $"SELECT c.nome, SUM(r.valor) as total FROM Receita r INNER JOIN Categoria c ON r.categoria_id = c.id WHERE {filtroSql} GROUP BY c.nome ORDER BY total DESC LIMIT 3";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sqlTop3, conn))
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        int rank = 1;
                        while (reader.Read())
                        {
                            string texto = $"{rank}. {reader["nome"]} - {Convert.ToDecimal(reader["total"]):C}";
                            if (rank == 1) lblTop1Servico.Text = texto;
                            if (rank == 2) lblTop2Servico.Text = texto;
                            if (rank == 3) lblTop3Servico.Text = texto;
                            rank++;
                        }
                    }

                    string sqlCliente = $"SELECT cliente, SUM(valor) as total FROM Receita WHERE {filtroSql} AND TRIM(cliente) <> '' GROUP BY cliente ORDER BY total DESC LIMIT 1";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sqlCliente, conn))
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lblMelhorClienteNome.Text = reader["cliente"].ToString().ToUpper();
                            lblMelhorClienteValor.Text = Convert.ToDecimal(reader["total"]).ToString("C");
                        }
                    }

                    string sqlRec = $"SELECT cliente AS \"Cliente\", valor AS \"Valor (R$)\", data_lancamento AS \"Data\" FROM Receita WHERE {filtroSql} ORDER BY data_lancamento DESC LIMIT 6";
                    using (NpgsqlCommand cmdRec = new NpgsqlCommand(sqlRec, conn))
                    using (NpgsqlDataAdapter adapterRec = new NpgsqlDataAdapter(cmdRec))
                    {
                        DataTable dtRec = new DataTable(); adapterRec.Fill(dtRec); gridReceitas.DataSource = dtRec;
                        if (gridReceitas.Columns.Contains("Data")) gridReceitas.Columns["Data"].DefaultCellStyle.Format = "dd/MM/yyyy";
                        if (gridReceitas.Columns.Contains("Valor (R$)")) gridReceitas.Columns["Valor (R$)"].DefaultCellStyle.Format = "N2";
                    }

                    string sqlDesp = $"SELECT descricao AS \"Fornecedor\", valor AS \"Valor (R$)\", data_lancamento AS \"Data\" FROM Despesa WHERE {filtroSql} ORDER BY data_lancamento DESC LIMIT 6";
                    using (NpgsqlCommand cmdDesp = new NpgsqlCommand(sqlDesp, conn))
                    using (NpgsqlDataAdapter adapterDesp = new NpgsqlDataAdapter(cmdDesp))
                    {
                        DataTable dtDesp = new DataTable(); adapterDesp.Fill(dtDesp); gridDespesas.DataSource = dtDesp;
                        if (gridDespesas.Columns.Contains("Data")) gridDespesas.Columns["Data"].DefaultCellStyle.Format = "dd/MM/yyyy";
                        if (gridDespesas.Columns.Contains("Valor (R$)")) gridDespesas.Columns["Valor (R$)"].DefaultCellStyle.Format = "N2";
                    }

                    dadosGrafico.Clear();
                    string filtroDespesa = filtroSql.Replace("data_lancamento", "d.data_lancamento");
                    string sqlGrafico = $"SELECT c.nome, SUM(d.valor) as total FROM Despesa d INNER JOIN Categoria c ON d.categoria_id = c.id WHERE {filtroDespesa} GROUP BY c.nome ORDER BY total DESC";

                    using (NpgsqlCommand cmdGrafico = new NpgsqlCommand(sqlGrafico, conn))
                    using (NpgsqlDataReader reader = cmdGrafico.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dadosGrafico.Add(reader["nome"].ToString(), Convert.ToDecimal(reader["total"]));
                        }
                    }
                    picGrafico.Invalidate();
                }

                decimal saldo = totalReceitas - totalDespesas;
                lblValorReceitas.Text = totalReceitas.ToString("C");
                lblValorDespesas.Text = totalDespesas.ToString("C");
                lblValorSaldo.Text = saldo.ToString("C");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar resumo: " + ex.Message);
            }
        }

        private void PicGrafico_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (dadosGrafico.Count == 0)
            {
                e.Graphics.DrawString("Nenhuma despesa para exibir neste período.", new System.Drawing.Font("Segoe UI", 10), Brushes.Gray, new PointF(300, 80));
                return;
            }

            decimal total = 0;
            foreach (var valor in dadosGrafico.Values) total += valor;
            if (total == 0) return;

            Color[] cores = { Color.FromArgb(231, 76, 60), Color.FromArgb(52, 152, 219), Color.FromArgb(241, 196, 15), Color.FromArgb(155, 89, 182), Color.FromArgb(230, 126, 34), Color.FromArgb(26, 188, 156), Color.FromArgb(52, 73, 94) };

            float startAngle = 0;
            int indiceCor = 0;

            System.Drawing.Rectangle rectPizza = new System.Drawing.Rectangle(200, 10, 150, 150);
            int legendaX = 380;
            int legendaY = 15;

            foreach (var item in dadosGrafico)
            {
                float sweepAngle = (float)((item.Value / total) * 360);
                using (Brush pincel = new SolidBrush(cores[indiceCor % cores.Length]))
                {
                    e.Graphics.FillPie(pincel, rectPizza, startAngle, sweepAngle);
                    e.Graphics.FillRectangle(pincel, legendaX, legendaY, 15, 15);
                    string porcentagem = ((item.Value / total) * 100).ToString("0.0") + "%";
                    e.Graphics.DrawString($"{item.Key} - {item.Value:C} ({porcentagem})", new System.Drawing.Font("Segoe UI", 10), Brushes.Black, new PointF(legendaX + 25, legendaY - 2));
                }
                legendaY += 25;
                startAngle += sweepAngle;
                indiceCor++;
                if (legendaY > 150)
                {
                    legendaY = 15;
                    legendaX += 250;
                }
            }
        }

        private void BtnGerarPDF_Click(object sender, EventArgs e)
        {
            using (FormFiltroPDF form = new FormFiltroPDF())
            {
                form.ShowDialog();
            }
        }
    }

    public class FormFiltroPDF : Form
    {
        private ComboBox cbFiltroMes;
        private ComboBox cbFiltroAno;
        private ComboBox cbFiltroCategoria;
        private ComboBox cbFiltroPagamento;
        private TextBox txtBuscaNome;
        private ComboBox cbTipoRelatorio;
        private Button btnGerar;

        public FormFiltroPDF()
        {
            this.Text = "Opções do Relatório PDF";
            this.Size = new Size(700, 340); // Aumentei um pouquinho pra caber o menu
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.WhiteSmoke;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblFiltroMes = new Label { Text = "Mês:", Location = new Point(20, 20), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold) };
            cbFiltroMes = new ComboBox { Location = new Point(20, 45), Width = 150, Font = new System.Drawing.Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cbFiltroMes.Items.AddRange(new string[] { "Todos os Meses", "01 - Janeiro", "02 - Fevereiro", "03 - Março", "04 - Abril", "05 - Maio", "06 - Junho", "07 - Julho", "08 - Agosto", "09 - Setembro", "10 - Outubro", "11 - Novembro", "12 - Dezembro" });
            cbFiltroMes.SelectedIndex = DateTime.Now.Month;

            Label lblFiltroAno = new Label { Text = "Ano:", Location = new Point(180, 20), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold) };
            cbFiltroAno = new ComboBox { Location = new Point(180, 45), Width = 120, Font = new System.Drawing.Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cbFiltroAno.Items.AddRange(new string[] { "Todos os Anos", "2024", "2025", "2026", "2027", "2028", "2029", "2030" });
            cbFiltroAno.SelectedItem = DateTime.Now.Year.ToString();

            Label lblFiltroPag = new Label { Text = "Forma de Pagto:", Location = new Point(310, 20), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold) };
            cbFiltroPagamento = new ComboBox { Location = new Point(310, 45), Width = 150, Font = new System.Drawing.Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cbFiltroPagamento.Items.AddRange(new string[] { "Todos", "PIX", "Boleto", "Dinheiro", "Cartão de Débito", "Cartão de Crédito", "Cheque", "Transferência Bancária" });
            cbFiltroPagamento.SelectedIndex = 0;

            Label lblFiltroCat = new Label { Text = "Categoria / Serviço:", Location = new Point(20, 80), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold) };
            cbFiltroCategoria = new ComboBox { Location = new Point(20, 105), Width = 220, Font = new System.Drawing.Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cbFiltroCategoria.Items.AddRange(new string[] { "Todas", "ITR", "Imposto de Renda", "Documento de Veículo", "Mensalidade do Escritório", "Notas Fiscais", "Reembolso de Cliente", "Outras Receitas / Avulsos", "Aluguel", "Água", "Energia Elétrica", "Internet / Telefone", "Sistemas / Softwares", "Impostos e Taxas", "Manutenção e Limpeza", "Material de Escritório", "Outras Despesas / Imprevistos" });
            cbFiltroCategoria.SelectedIndex = 0;

            Label lblBusca = new Label { Text = "Pessoa / Empresa (Extrato):", Location = new Point(250, 80), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold) };
            txtBuscaNome = new TextBox { Location = new Point(250, 105), Width = 410, Font = new System.Drawing.Font("Segoe UI", 10) };

            Label lblTipoRel = new Label { Text = "Selecione o Tipo de Relatório:", Location = new Point(20, 140), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold) };
            cbTipoRelatorio = new ComboBox { Location = new Point(20, 165), Width = 640, Font = new System.Drawing.Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cbTipoRelatorio.Items.AddRange(new string[] {
                "1. Visão do Caixa do Escritório (Entradas - Saídas)",
                "2. Extrato de Cobrança (Calcula a dívida pendente abatendo os reembolsos já pagos)",
                "3. Histórico de Gastos do Cliente (Transparência total do que ele gastou no escritório)"
            });
            cbTipoRelatorio.SelectedIndex = 0;

            btnGerar = new Button { Text = "📄 Baixar Relatório PDF", Location = new Point(20, 215), Width = 640, Height = 45, BackColor = Color.FromArgb(41, 128, 185), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new System.Drawing.Font("Segoe UI", 12, FontStyle.Bold), Cursor = Cursors.Hand };
            btnGerar.FlatAppearance.BorderSize = 0;
            btnGerar.Click += BtnGerar_Click;

            this.Controls.Add(lblFiltroMes); this.Controls.Add(cbFiltroMes);
            this.Controls.Add(lblFiltroAno); this.Controls.Add(cbFiltroAno);
            this.Controls.Add(lblFiltroPag); this.Controls.Add(cbFiltroPagamento);
            this.Controls.Add(lblFiltroCat); this.Controls.Add(cbFiltroCategoria);
            this.Controls.Add(lblBusca); this.Controls.Add(txtBuscaNome);
            this.Controls.Add(lblTipoRel); this.Controls.Add(cbTipoRelatorio);
            this.Controls.Add(btnGerar);

            CarregarSugestoes();
        }

        private void CarregarSugestoes()
        {
            AutoCompleteStringCollection sugestoes = new AutoCompleteStringCollection();
            try
            {
                ConexaoDB db = new ConexaoDB();
                using (NpgsqlConnection conn = db.Conectar())
                {
                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT DISTINCT cliente FROM Receita WHERE cliente IS NOT NULL", conn))
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        while (reader.Read()) sugestoes.Add(reader["cliente"].ToString());

                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT DISTINCT descricao FROM Despesa WHERE descricao IS NOT NULL", conn))
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        while (reader.Read()) sugestoes.Add(reader["descricao"].ToString());
                }
            }
            catch { }
            txtBuscaNome.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtBuscaNome.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtBuscaNome.AutoCompleteCustomSource = sugestoes;
        }

        private void BtnGerar_Click(object sender, EventArgs e)
        {
            int mesSelecionado = cbFiltroMes.SelectedIndex;
            string anoSelecionado = cbFiltroAno.SelectedItem.ToString();
            string categoriaSelecionada = cbFiltroCategoria.SelectedItem.ToString();
            string pagamentoSelecionado = cbFiltroPagamento.SelectedItem.ToString();
            string nomeBusca = txtBuscaNome.Text.Trim();

            // Lê o índice do tipo de relatório escolhido (0, 1 ou 2)
            int tipoRelatorio = cbTipoRelatorio.SelectedIndex;

            string tituloPDF = "GERAL";
            if (!string.IsNullOrEmpty(nomeBusca)) tituloPDF = nomeBusca.ToUpper();

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Documento PDF (*.pdf)|*.pdf";
            sfd.FileName = $"Relatorio_{tituloPDF.Replace(" ", "_")}.pdf";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    iText.Kernel.Pdf.PdfWriter writer = new iText.Kernel.Pdf.PdfWriter(sfd.FileName);
                    iText.Kernel.Pdf.PdfDocument pdf = new iText.Kernel.Pdf.PdfDocument(writer);
                    iText.Layout.Document doc = new iText.Layout.Document(pdf, iText.Kernel.Geom.PageSize.A4);
                    doc.SetMargins(40, 30, 40, 30);

                    iText.Kernel.Font.PdfFont fontTitulo = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
                    iText.Kernel.Font.PdfFont fontSubtitulo = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
                    iText.Kernel.Font.PdfFont fontNormal = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

                    // ================= TÍTULOS DINÂMICOS =================
                    string textoTitulo = "EXTRATO DE MOVIMENTACOES (CAIXA)";
                    if (tipoRelatorio == 1) textoTitulo = "EXTRATO DE COBRANCA AO CLIENTE";
                    else if (tipoRelatorio == 2) textoTitulo = "HISTORICO DE GASTOS DO CLIENTE";

                    doc.Add(new iText.Layout.Element.Paragraph(textoTitulo).SetFont(fontTitulo).SetFontSize(18).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                    string textoPeriodo = "Periodo: Todos os Registros";
                    if (mesSelecionado > 0 && anoSelecionado != "Todos os Anos") textoPeriodo = $"Periodo: {cbFiltroMes.SelectedItem.ToString().Substring(5).ToUpper()} DE {anoSelecionado}";
                    else if (anoSelecionado != "Todos os Anos") textoPeriodo = $"Periodo: ANO DE {anoSelecionado}";
                    doc.Add(new iText.Layout.Element.Paragraph(textoPeriodo).SetFont(fontNormal).SetFontSize(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                    if (!string.IsNullOrEmpty(nomeBusca))
                        doc.Add(new iText.Layout.Element.Paragraph($"Referente a: {nomeBusca.ToUpper()}").SetFont(fontSubtitulo).SetFontSize(12).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                    doc.Add(new iText.Layout.Element.Paragraph("\n"));

                    decimal totalRec = 0, totalDesp = 0;

                    ConexaoDB db = new ConexaoDB();
                    using (NpgsqlConnection conn = db.Conectar())
                    {
                        // ================= RECEITAS =================
                        string sqlRec = "SELECT r.data_lancamento, r.cliente, c.nome, r.forma_pagamento, r.valor FROM Receita r INNER JOIN Categoria c ON r.categoria_id = c.id WHERE 1=1";
                        if (mesSelecionado > 0) sqlRec += " AND EXTRACT(MONTH FROM r.data_lancamento) = @mes";
                        if (anoSelecionado != "Todos os Anos") sqlRec += " AND EXTRACT(YEAR FROM r.data_lancamento) = @ano";
                        if (!string.IsNullOrEmpty(nomeBusca)) sqlRec += " AND r.cliente ILIKE @nome";
                        if (categoriaSelecionada != "Todas") sqlRec += " AND c.nome = @categoria";
                        if (pagamentoSelecionado != "Todos") sqlRec += " AND r.forma_pagamento = @pagamento";
                        sqlRec += " ORDER BY r.data_lancamento";

                        string lblRecTitle = "RECEITAS (ENTRADAS)";
                        if (tipoRelatorio == 1) lblRecTitle = "SERVICOS PRESTADOS E PAGAMENTOS (HONORARIOS)";
                        else if (tipoRelatorio == 2) lblRecTitle = "HONORARIOS E SERVICOS PAGOS AO ESCRITORIO";

                        doc.Add(new iText.Layout.Element.Paragraph(lblRecTitle).SetFont(fontSubtitulo).SetFontSize(12));
                        doc.Add(new iText.Layout.Element.Paragraph(" "));

                        iText.Layout.Element.Table tableRec = new iText.Layout.Element.Table(iText.Layout.Properties.UnitValue.CreatePercentArray(new float[] { 12f, 33f, 25f, 15f, 15f })).UseAllAvailableWidth();
                        tableRec.AddHeaderCell(new iText.Layout.Element.Cell().SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY).Add(new iText.Layout.Element.Paragraph("Data").SetFont(fontSubtitulo).SetFontSize(11)));
                        tableRec.AddHeaderCell(new iText.Layout.Element.Cell().SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY).Add(new iText.Layout.Element.Paragraph("Cliente").SetFont(fontSubtitulo).SetFontSize(11)));
                        tableRec.AddHeaderCell(new iText.Layout.Element.Cell().SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY).Add(new iText.Layout.Element.Paragraph("Categoria").SetFont(fontSubtitulo).SetFontSize(11)));
                        tableRec.AddHeaderCell(new iText.Layout.Element.Cell().SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY).Add(new iText.Layout.Element.Paragraph("Pagamento").SetFont(fontSubtitulo).SetFontSize(11)));
                        tableRec.AddHeaderCell(new iText.Layout.Element.Cell().SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY).Add(new iText.Layout.Element.Paragraph("Valor (R$)").SetFont(fontSubtitulo).SetFontSize(11)));

                        using (NpgsqlCommand cmd = new NpgsqlCommand(sqlRec, conn))
                        {
                            if (mesSelecionado > 0) cmd.Parameters.AddWithValue("@mes", mesSelecionado);
                            if (anoSelecionado != "Todos os Anos") cmd.Parameters.AddWithValue("@ano", Convert.ToInt32(anoSelecionado));
                            if (!string.IsNullOrEmpty(nomeBusca)) cmd.Parameters.AddWithValue("@nome", "%" + nomeBusca + "%");
                            if (categoriaSelecionada != "Todas") cmd.Parameters.AddWithValue("@categoria", categoriaSelecionada);
                            if (pagamentoSelecionado != "Todos") cmd.Parameters.AddWithValue("@pagamento", pagamentoSelecionado);

                            using (NpgsqlDataReader r = cmd.ExecuteReader())
                            {
                                while (r.Read())
                                {
                                    string nomeCategoria = r["nome"].ToString();
                                    decimal v = Convert.ToDecimal(r["valor"]);

                                    // LÓGICA DE TRANSPARÊNCIA: Se for modo Gasto do Cliente, ignoramos o Reembolso na tabela para não duplicar custos.
                                    if (tipoRelatorio == 2 && nomeCategoria == "Reembolso de Cliente")
                                    {
                                        continue;
                                    }

                                    tableRec.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(DateTime.Parse(r["data_lancamento"].ToString()).ToString("dd/MM/yyyy")).SetFont(fontNormal).SetFontSize(10)));
                                    tableRec.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(r["cliente"].ToString()).SetFont(fontNormal).SetFontSize(10)));
                                    tableRec.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(nomeCategoria).SetFont(fontNormal).SetFontSize(10)));
                                    tableRec.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(r["forma_pagamento"].ToString()).SetFont(fontNormal).SetFontSize(10)));

                                    // LÓGICA DE ABATIMENTO DA COBRANÇA
                                    if (tipoRelatorio == 1 && nomeCategoria == "Reembolso de Cliente")
                                    {
                                        totalRec -= v;
                                        tableRec.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("-" + v.ToString("C")).SetFont(fontNormal).SetFontSize(10).SetFontColor(iText.Kernel.Colors.ColorConstants.RED)));
                                    }
                                    else
                                    {
                                        totalRec += v;
                                        tableRec.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(v.ToString("C")).SetFont(fontNormal).SetFontSize(10)));
                                    }
                                }
                            }
                        }

                        string descRecTotal = $"Total de Receitas: {totalRec:C}\n\n";
                        if (tipoRelatorio == 1) descRecTotal = $"Total de Servicos (Abatendo Pagos): {totalRec:C}\n\n";
                        else if (tipoRelatorio == 2) descRecTotal = $"Total de Honorarios Pagos: {totalRec:C}\n\n";

                        doc.Add(tableRec);
                        doc.Add(new iText.Layout.Element.Paragraph(descRecTotal).SetFont(fontSubtitulo).SetFontSize(12).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));

                        // ================= DESPESAS =================
                        string sqlDesp = "SELECT d.data_lancamento, d.descricao, c.nome, d.forma_pagamento, d.valor FROM Despesa d INNER JOIN Categoria c ON d.categoria_id = c.id WHERE 1=1";
                        if (mesSelecionado > 0) sqlDesp += " AND EXTRACT(MONTH FROM d.data_lancamento) = @mes";
                        if (anoSelecionado != "Todos os Anos") sqlDesp += " AND EXTRACT(YEAR FROM d.data_lancamento) = @ano";
                        if (!string.IsNullOrEmpty(nomeBusca)) sqlDesp += " AND d.descricao ILIKE @nome";
                        if (categoriaSelecionada != "Todas") sqlDesp += " AND c.nome = @categoria";
                        if (pagamentoSelecionado != "Todos") sqlDesp += " AND d.forma_pagamento = @pagamento";
                        sqlDesp += " ORDER BY d.data_lancamento";

                        string lblDespTitle = "DESPESAS (SAIDAS)";
                        if (tipoRelatorio == 1) lblDespTitle = "TAXAS E IMPOSTOS PAGOS PELO ESCRITORIO (A REEMBOLSAR)";
                        else if (tipoRelatorio == 2) lblDespTitle = "TAXAS, IMPOSTOS E DOCUMENTOS PAGOS PARA O CLIENTE";

                        doc.Add(new iText.Layout.Element.Paragraph(lblDespTitle).SetFont(fontSubtitulo).SetFontSize(12));
                        doc.Add(new iText.Layout.Element.Paragraph(" "));

                        iText.Layout.Element.Table tableDesp = new iText.Layout.Element.Table(iText.Layout.Properties.UnitValue.CreatePercentArray(new float[] { 12f, 33f, 25f, 15f, 15f })).UseAllAvailableWidth();
                        tableDesp.AddHeaderCell(new iText.Layout.Element.Cell().SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY).Add(new iText.Layout.Element.Paragraph("Data").SetFont(fontSubtitulo).SetFontSize(11)));
                        tableDesp.AddHeaderCell(new iText.Layout.Element.Cell().SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY).Add(new iText.Layout.Element.Paragraph("Fornecedor / Ref.").SetFont(fontSubtitulo).SetFontSize(11)));
                        tableDesp.AddHeaderCell(new iText.Layout.Element.Cell().SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY).Add(new iText.Layout.Element.Paragraph("Categoria").SetFont(fontSubtitulo).SetFontSize(11)));
                        tableDesp.AddHeaderCell(new iText.Layout.Element.Cell().SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY).Add(new iText.Layout.Element.Paragraph("Pagamento").SetFont(fontSubtitulo).SetFontSize(11)));
                        tableDesp.AddHeaderCell(new iText.Layout.Element.Cell().SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY).Add(new iText.Layout.Element.Paragraph("Valor (R$)").SetFont(fontSubtitulo).SetFontSize(11)));

                        using (NpgsqlCommand cmd = new NpgsqlCommand(sqlDesp, conn))
                        {
                            if (mesSelecionado > 0) cmd.Parameters.AddWithValue("@mes", mesSelecionado);
                            if (anoSelecionado != "Todos os Anos") cmd.Parameters.AddWithValue("@ano", Convert.ToInt32(anoSelecionado));
                            if (!string.IsNullOrEmpty(nomeBusca)) cmd.Parameters.AddWithValue("@nome", "%" + nomeBusca + "%");
                            if (categoriaSelecionada != "Todas") cmd.Parameters.AddWithValue("@categoria", categoriaSelecionada);
                            if (pagamentoSelecionado != "Todos") cmd.Parameters.AddWithValue("@pagamento", pagamentoSelecionado);

                            using (NpgsqlDataReader r = cmd.ExecuteReader())
                            {
                                while (r.Read())
                                {
                                    tableDesp.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(DateTime.Parse(r["data_lancamento"].ToString()).ToString("dd/MM/yyyy")).SetFont(fontNormal).SetFontSize(10)));
                                    tableDesp.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(r["descricao"].ToString()).SetFont(fontNormal).SetFontSize(10)));
                                    tableDesp.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(r["nome"].ToString()).SetFont(fontNormal).SetFontSize(10)));
                                    tableDesp.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(r["forma_pagamento"].ToString()).SetFont(fontNormal).SetFontSize(10)));
                                    decimal v = Convert.ToDecimal(r["valor"]);
                                    totalDesp += v;
                                    tableDesp.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(v.ToString("C")).SetFont(fontNormal).SetFontSize(10)));
                                }
                            }
                        }

                        string descDespTotal = $"Total de Despesas: {totalDesp:C}\n\n";
                        if (tipoRelatorio == 1) descDespTotal = $"Total de Reembolsos Pendentes: {totalDesp:C}\n\n";
                        else if (tipoRelatorio == 2) descDespTotal = $"Total de Taxas Pagas p/ Cliente: {totalDesp:C}\n\n";

                        doc.Add(tableDesp);
                        doc.Add(new iText.Layout.Element.Paragraph(descDespTotal).SetFont(fontSubtitulo).SetFontSize(12).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));

                        // ================= CÁLCULO E TEXTO DO SALDO FINAL =================
                        decimal saldoFim = 0;
                        string textoSaldo = "";
                        iText.Kernel.Colors.Color corSaldo = iText.Kernel.Colors.ColorConstants.BLACK;

                        if (tipoRelatorio == 0) // Visão do Caixa (Receitas - Despesas)
                        {
                            saldoFim = totalRec - totalDesp;
                            textoSaldo = "SALDO LIQUIDO FINAL: ";
                            corSaldo = saldoFim >= 0 ? iText.Kernel.Colors.ColorConstants.GREEN : iText.Kernel.Colors.ColorConstants.RED;
                        }
                        else if (tipoRelatorio == 1) // Cobrança (Serviços Prestados + DARFs Pagos)
                        {
                            saldoFim = totalRec + totalDesp;
                            textoSaldo = "TOTAL GERAL A RECEBER DO CLIENTE: ";
                            corSaldo = iText.Kernel.Colors.ColorConstants.BLUE;
                        }
                        else if (tipoRelatorio == 2) // Gastos do Cliente (O que ele gastou de Honorário + DARFs)
                        {
                            saldoFim = totalRec + totalDesp;
                            textoSaldo = "TOTAL GERAL GASTO PELO CLIENTE: ";
                            corSaldo = iText.Kernel.Colors.ColorConstants.BLUE;
                        }

                        doc.Add(new iText.Layout.Element.Paragraph($"{textoSaldo}{saldoFim:C}\n\n").SetFont(fontSubtitulo).SetFontSize(14).SetFontColor(corSaldo).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));

                        // ================= GRÁFICO (Exibido apenas no modo de Caixa) =================
                        if (tipoRelatorio == 0)
                        {
                            Dictionary<string, decimal> dadosPie = new Dictionary<string, decimal>();
                            string sqlGrafico = "SELECT c.nome, SUM(d.valor) as total FROM Despesa d INNER JOIN Categoria c ON d.categoria_id = c.id WHERE 1=1";
                            if (mesSelecionado > 0) sqlGrafico += " AND EXTRACT(MONTH FROM d.data_lancamento) = @mes";
                            if (anoSelecionado != "Todos os Anos") sqlGrafico += " AND EXTRACT(YEAR FROM d.data_lancamento) = @ano";
                            if (!string.IsNullOrEmpty(nomeBusca)) sqlGrafico += " AND d.descricao ILIKE @nome";
                            if (categoriaSelecionada != "Todas") sqlGrafico += " AND c.nome = @categoria";
                            if (pagamentoSelecionado != "Todos") sqlGrafico += " AND d.forma_pagamento = @pagamento";
                            sqlGrafico += " GROUP BY c.nome ORDER BY total DESC";

                            using (NpgsqlCommand cmd = new NpgsqlCommand(sqlGrafico, conn))
                            {
                                if (mesSelecionado > 0) cmd.Parameters.AddWithValue("@mes", mesSelecionado);
                                if (anoSelecionado != "Todos os Anos") cmd.Parameters.AddWithValue("@ano", Convert.ToInt32(anoSelecionado));
                                if (!string.IsNullOrEmpty(nomeBusca)) cmd.Parameters.AddWithValue("@nome", "%" + nomeBusca + "%");
                                if (categoriaSelecionada != "Todas") cmd.Parameters.AddWithValue("@categoria", categoriaSelecionada);
                                if (pagamentoSelecionado != "Todos") cmd.Parameters.AddWithValue("@pagamento", pagamentoSelecionado);

                                using (NpgsqlDataReader r = cmd.ExecuteReader())
                                    while (r.Read()) dadosPie.Add(r["nome"].ToString(), Convert.ToDecimal(r["total"]));
                            }

                            if (dadosPie.Count > 0)
                            {
                                doc.Add(new iText.Layout.Element.Paragraph("DISTRIBUICAO DE DESPESAS").SetFont(fontSubtitulo).SetFontSize(12).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                                doc.Add(new iText.Layout.Element.Paragraph(" "));

                                using (Bitmap bmp = new Bitmap(850, 200))
                                using (Graphics g = Graphics.FromImage(bmp))
                                {
                                    g.Clear(Color.White);
                                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                                    decimal totalPie = 0;
                                    foreach (var val in dadosPie.Values) totalPie += val;

                                    Color[] cores = { Color.FromArgb(231, 76, 60), Color.FromArgb(52, 152, 219), Color.FromArgb(241, 196, 15), Color.FromArgb(155, 89, 182), Color.FromArgb(230, 126, 34), Color.FromArgb(26, 188, 156), Color.FromArgb(52, 73, 94) };
                                    float startAngle = 0; int indiceCor = 0;

                                    System.Drawing.Rectangle rectPizza = new System.Drawing.Rectangle(150, 10, 160, 160);
                                    int legendaX = 340, legendaY = 15;

                                    foreach (var item in dadosPie)
                                    {
                                        float sweepAngle = (float)((item.Value / totalPie) * 360);
                                        using (Brush pincel = new SolidBrush(cores[indiceCor % cores.Length]))
                                        {
                                            g.FillPie(pincel, rectPizza, startAngle, sweepAngle);
                                            g.FillRectangle(pincel, legendaX, legendaY, 15, 15);
                                            string porcentagem = ((item.Value / totalPie) * 100).ToString("0.0") + "%";
                                            g.DrawString($"{item.Key} - {item.Value:C} ({porcentagem})", new System.Drawing.Font("Segoe UI", 10), Brushes.Black, new PointF(legendaX + 25, legendaY - 2));
                                        }
                                        legendaY += 25; startAngle += sweepAngle; indiceCor++;

                                        if (legendaY > 170) { legendaY = 15; legendaX += 280; }
                                    }

                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                        iText.IO.Image.ImageData imageData = iText.IO.Image.ImageDataFactory.Create(ms.ToArray());
                                        iText.Layout.Element.Image imgGrafico = new iText.Layout.Element.Image(imageData);
                                        imgGrafico.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);
                                        imgGrafico.SetAutoScale(true);
                                        doc.Add(imgGrafico);
                                    }
                                }
                            }
                        }
                    }

                    doc.Close();
                    MessageBox.Show("Relatório gerado com sucesso!", "Documento Pronto", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao gerar PDF: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}