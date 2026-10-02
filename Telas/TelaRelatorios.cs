using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using Npgsql;
using GerenciadorEscritorio.BancoDeDados;

namespace GerenciadorEscritorio.Telas
{
    public class TelaRelatorios : UserControl
    {
        private Panel pnlCentro;
        private DateTimePicker dtpDataInicio;
        private DateTimePicker dtpDataFim;
        private ComboBox cbFiltroPagamento;
        private TextBox txtBuscaNome;
        private ComboBox cbTipoRelatorio;
        private Button btnGerarPDF;

        private CheckedListBox clbReceitas;
        private CheckedListBox clbDespesas;

        public TelaRelatorios()
        {
            ConfigurarLayout();
            CarregarCategorias();
            CarregarSugestoes();
        }

        private void ConfigurarLayout()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.WhiteSmoke;

            Label lblTitulo = new Label { Text = "Gerador de Relatórios e Extratos", Font = new System.Drawing.Font("Segoe UI", 24, FontStyle.Bold), AutoSize = true, Location = new Point(30, 25) };
            this.Controls.Add(lblTitulo);

            pnlCentro = new Panel { Size = new Size(750, 380), Location = new Point((this.Width - 750) / 2, 100) };
            this.Controls.Add(pnlCentro);

            Label lblFiltroInicio = new Label { Text = "Data Inicial:", Location = new Point(0, 0), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold) };
            dtpDataInicio = new DateTimePicker { Location = new Point(0, 25), Width = 140, Font = new System.Drawing.Font("Segoe UI", 10), Format = DateTimePickerFormat.Short };
            dtpDataInicio.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            Label lblFiltroFim = new Label { Text = "Data Final:", Location = new Point(160, 0), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold) };
            dtpDataFim = new DateTimePicker { Location = new Point(160, 25), Width = 140, Font = new System.Drawing.Font("Segoe UI", 10), Format = DateTimePickerFormat.Short };
            dtpDataFim.Value = DateTime.Now;

            Label lblFiltroPag = new Label { Text = "Forma de Pagto:", Location = new Point(320, 0), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold) };
            cbFiltroPagamento = new ComboBox { Location = new Point(320, 25), Width = 150, Font = new System.Drawing.Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cbFiltroPagamento.Items.AddRange(new string[] { "Todos", "PIX", "Boleto", "Dinheiro", "Cartão de Débito", "Cartão de Crédito", "Cheque", "Transferência Bancária" });
            cbFiltroPagamento.SelectedIndex = 0;

            Label lblFiltroRec = new Label { Text = "Filtrar Receitas:", Location = new Point(0, 70), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold) };
            clbReceitas = new CheckedListBox { Location = new Point(0, 95), Size = new Size(190, 100), Font = new System.Drawing.Font("Segoe UI", 9), CheckOnClick = true };

            Label lblFiltroDesp = new Label { Text = "Filtrar Despesas:", Location = new Point(200, 70), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold) };
            clbDespesas = new CheckedListBox { Location = new Point(200, 95), Size = new Size(190, 100), Font = new System.Drawing.Font("Segoe UI", 9), CheckOnClick = true };

            Label lblBusca = new Label { Text = "Filtrar por Pessoa / Empresa:", Location = new Point(410, 70), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold) };
            txtBuscaNome = new TextBox { Location = new Point(410, 95), Width = 230, Font = new System.Drawing.Font("Segoe UI", 10) };

            Label lblTipoRel = new Label { Text = "Selecione o Tipo de Relatório:", Location = new Point(0, 210), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold) };
            cbTipoRelatorio = new ComboBox { Location = new Point(0, 235), Width = 640, Font = new System.Drawing.Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cbTipoRelatorio.Items.AddRange(new string[] {
                "1. Visão do Caixa do Escritório (Entradas - Saídas)",
                "2. Extrato de Cobrança (Calcula a dívida pendente abatendo os reembolsos já pagos)",
                "3. Histórico de Gastos do Cliente (Transparência total do que ele gastou no escritório)"
            });
            cbTipoRelatorio.SelectedIndex = 0;

            btnGerarPDF = new Button { Text = "📄 Gerar Extrato Personalizado", Location = new Point(0, 280), Width = 640, Height = 50, BackColor = Color.FromArgb(41, 128, 185), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new System.Drawing.Font("Segoe UI", 12, FontStyle.Bold), Cursor = Cursors.Hand };
            btnGerarPDF.FlatAppearance.BorderSize = 0;
            btnGerarPDF.Click += BtnGerarPDF_Click;

            pnlCentro.Controls.Add(lblFiltroInicio); pnlCentro.Controls.Add(dtpDataInicio);
            pnlCentro.Controls.Add(lblFiltroFim); pnlCentro.Controls.Add(dtpDataFim);
            pnlCentro.Controls.Add(lblFiltroPag); pnlCentro.Controls.Add(cbFiltroPagamento);

            pnlCentro.Controls.Add(lblFiltroRec); pnlCentro.Controls.Add(clbReceitas);
            pnlCentro.Controls.Add(lblFiltroDesp); pnlCentro.Controls.Add(clbDespesas);

            pnlCentro.Controls.Add(lblBusca); pnlCentro.Controls.Add(txtBuscaNome);
            pnlCentro.Controls.Add(lblTipoRel); pnlCentro.Controls.Add(cbTipoRelatorio);
            pnlCentro.Controls.Add(btnGerarPDF);

            this.Resize += (s, e) => pnlCentro.Left = (this.Width - pnlCentro.Width) / 2;
        }

        private void CarregarCategorias()
        {
            try
            {
                ConexaoDB db = new ConexaoDB();
                using (NpgsqlConnection conexao = db.Conectar())
                {
                    string sql = "SELECT id, nome, tipo FROM public.categoria ORDER BY nome";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conexao))
                    {
                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                CategoriaItem item = new CategoriaItem
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Nome = reader["nome"].ToString()
                                };

                                if (reader["tipo"].ToString() == "Receita")
                                {
                                    clbReceitas.Items.Add(item, true);
                                }
                                else
                                {
                                    clbDespesas.Items.Add(item, true);
                                }
                            }
                        }
                    }
                }
            }
            catch { }
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

        private void BtnGerarPDF_Click(object sender, EventArgs e)
        {
            DateTime dataInicio = dtpDataInicio.Value.Date;
            DateTime dataFim = dtpDataFim.Value.Date.AddDays(1).AddTicks(-1);

            string pagamentoSelecionado = cbFiltroPagamento.SelectedItem.ToString();
            string nomeBusca = txtBuscaNome.Text.Trim();
            int tipoRelatorio = cbTipoRelatorio.SelectedIndex;

            List<int> idsRec = new List<int>();
            foreach (CategoriaItem item in clbReceitas.CheckedItems) idsRec.Add(item.Id);
            string filtroRec = idsRec.Count > 0 ? string.Join(",", idsRec) : "0";

            List<int> idsDesp = new List<int>();
            foreach (CategoriaItem item in clbDespesas.CheckedItems) idsDesp.Add(item.Id);
            string filtroDesp = idsDesp.Count > 0 ? string.Join(",", idsDesp) : "0";

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

                    string textoTitulo = "EXTRATO DE MOVIMENTACOES (CAIXA)";
                    if (tipoRelatorio == 1) textoTitulo = "EXTRATO DE COBRANCA AO CLIENTE";
                    else if (tipoRelatorio == 2) textoTitulo = "HISTORICO DE GASTOS DO CLIENTE";

                    doc.Add(new iText.Layout.Element.Paragraph(textoTitulo).SetFont(fontTitulo).SetFontSize(18).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                    string textoPeriodo = $"Periodo: {dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}";
                    doc.Add(new iText.Layout.Element.Paragraph(textoPeriodo).SetFont(fontNormal).SetFontSize(10).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                    if (!string.IsNullOrEmpty(nomeBusca))
                        doc.Add(new iText.Layout.Element.Paragraph($"Referente a: {nomeBusca.ToUpper()}").SetFont(fontSubtitulo).SetFontSize(12).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                    doc.Add(new iText.Layout.Element.Paragraph("\n"));

                    decimal totalRec = 0, totalDesp = 0;

                    ConexaoDB db = new ConexaoDB();
                    using (NpgsqlConnection conn = db.Conectar())
                    {
                        // ================= RECEITAS =================
                        string sqlRec = $"SELECT r.data_lancamento, r.cliente, c.nome, r.forma_pagamento, r.valor FROM Receita r INNER JOIN Categoria c ON r.categoria_id = c.id WHERE c.id IN ({filtroRec}) AND r.data_lancamento BETWEEN @dataInicio AND @dataFim";
                        if (!string.IsNullOrEmpty(nomeBusca)) sqlRec += " AND r.cliente ILIKE @nome";
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
                            cmd.Parameters.AddWithValue("@dataInicio", dataInicio);
                            cmd.Parameters.AddWithValue("@dataFim", dataFim);
                            if (!string.IsNullOrEmpty(nomeBusca)) cmd.Parameters.AddWithValue("@nome", "%" + nomeBusca + "%");
                            if (pagamentoSelecionado != "Todos") cmd.Parameters.AddWithValue("@pagamento", pagamentoSelecionado);

                            using (NpgsqlDataReader r = cmd.ExecuteReader())
                            {
                                while (r.Read())
                                {
                                    string nomeCategoria = r["nome"].ToString();
                                    decimal v = Convert.ToDecimal(r["valor"]);

                                    if (tipoRelatorio == 2 && nomeCategoria == "Reembolso de Cliente") continue;

                                    tableRec.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(DateTime.Parse(r["data_lancamento"].ToString()).ToString("dd/MM/yyyy")).SetFont(fontNormal).SetFontSize(10)));
                                    tableRec.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(r["cliente"].ToString()).SetFont(fontNormal).SetFontSize(10)));
                                    tableRec.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(nomeCategoria).SetFont(fontNormal).SetFontSize(10)));
                                    tableRec.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(r["forma_pagamento"].ToString()).SetFont(fontNormal).SetFontSize(10)));

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
                        string sqlDesp = $"SELECT d.data_lancamento, d.descricao, c.nome, d.forma_pagamento, d.valor FROM Despesa d INNER JOIN Categoria c ON d.categoria_id = c.id WHERE c.id IN ({filtroDesp}) AND d.data_lancamento BETWEEN @dataInicio AND @dataFim";
                        if (!string.IsNullOrEmpty(nomeBusca)) sqlDesp += " AND d.descricao ILIKE @nome";
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
                            cmd.Parameters.AddWithValue("@dataInicio", dataInicio);
                            cmd.Parameters.AddWithValue("@dataFim", dataFim);
                            if (!string.IsNullOrEmpty(nomeBusca)) cmd.Parameters.AddWithValue("@nome", "%" + nomeBusca + "%");
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

                        if (tipoRelatorio == 0)
                        {
                            saldoFim = totalRec - totalDesp;
                            textoSaldo = "SALDO LIQUIDO FINAL: ";
                            corSaldo = saldoFim >= 0 ? iText.Kernel.Colors.ColorConstants.GREEN : iText.Kernel.Colors.ColorConstants.RED;
                        }
                        else if (tipoRelatorio == 1)
                        {
                            saldoFim = totalRec + totalDesp;
                            textoSaldo = "TOTAL GERAL A RECEBER DO CLIENTE: ";
                            corSaldo = iText.Kernel.Colors.ColorConstants.BLUE;
                        }
                        else if (tipoRelatorio == 2)
                        {
                            saldoFim = totalRec + totalDesp;
                            textoSaldo = "TOTAL GERAL GASTO PELO CLIENTE: ";
                            corSaldo = iText.Kernel.Colors.ColorConstants.BLUE;
                        }

                        doc.Add(new iText.Layout.Element.Paragraph($"{textoSaldo}{saldoFim:C}\n\n").SetFont(fontSubtitulo).SetFontSize(14).SetFontColor(corSaldo).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));

                        // ================= GRÁFICO =================
                        if (tipoRelatorio == 0)
                        {
                            Dictionary<string, decimal> dadosPie = new Dictionary<string, decimal>();
                            string sqlGrafico = $"SELECT c.nome, SUM(d.valor) as total FROM Despesa d INNER JOIN Categoria c ON d.categoria_id = c.id WHERE c.id IN ({filtroDesp}) AND d.data_lancamento BETWEEN @dataInicio AND @dataFim";
                            if (!string.IsNullOrEmpty(nomeBusca)) sqlGrafico += " AND d.descricao ILIKE @nome";
                            if (pagamentoSelecionado != "Todos") sqlGrafico += " AND d.forma_pagamento = @pagamento";
                            sqlGrafico += " GROUP BY c.nome ORDER BY total DESC";

                            using (NpgsqlCommand cmd = new NpgsqlCommand(sqlGrafico, conn))
                            {
                                cmd.Parameters.AddWithValue("@dataInicio", dataInicio);
                                cmd.Parameters.AddWithValue("@dataFim", dataFim);
                                if (!string.IsNullOrEmpty(nomeBusca)) cmd.Parameters.AddWithValue("@nome", "%" + nomeBusca + "%");
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

    public class CategoriaItem
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public override string ToString()
        {
            return Nome;
        }
    }
}