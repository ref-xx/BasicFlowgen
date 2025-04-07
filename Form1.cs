using System.Data;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.RegularExpressions;

namespace FlowGen
{
    public partial class Form1 : Form
    {
        public enum ColumnKey
        {
            FlowMarker,     // akış işareti →, ⏹️, ↩ vb.
            LineNumber,     // örn: 5060
            Command,        // BASIC komutu gosub 500 gibi
            IncomingRefs,   // bu satıra gelen GOTO/GOSUB referansları
            FunctionName,   // varsa: “InitMap”, “GameLoop” gibi tanım
            Comment         // el ile yazılmış yorumlar
        }

        private Dictionary<ColumnKey, int> ColumnMap;
        private void InitGridColumns()
        {
            dataGridView1.Columns.Clear();
            ColumnMap = new();

            AddGridColumn(ColumnKey.FlowMarker, "", 40, false);
            AddGridColumn(ColumnKey.LineNumber, "Line", 60, false);
            AddGridColumn(ColumnKey.Command, "Command", 400, false);
            AddGridColumn(ColumnKey.IncomingRefs, "← From", 300, false);
            AddGridColumn(ColumnKey.FunctionName, "Function", 120, true);
            AddGridColumn(ColumnKey.Comment, "Comment", 200, true);

            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            Font monoFont = new Font("Fira Code Retina", 10, FontStyle.Bold);
            dataGridView1.Columns[ColumnMap[ColumnKey.LineNumber]].DefaultCellStyle.Font = monoFont;
            dataGridView1.Columns[ColumnMap[ColumnKey.Command]].DefaultCellStyle.Font = monoFont;
            /*
            var col = dataGridView1.Columns[ColumnMap[ColumnKey.IncomingRefs]];
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            col.FillWeight = 100; // Yüzde gibi davranır
            */
            dataGridView1.Columns[ColumnMap[ColumnKey.Comment]].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            foreach (var key in ColumnMap.Keys)
            {
                if (key != ColumnKey.Comment)
                {
                    dataGridView1.Columns[ColumnMap[key]].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }
            }

        }
        private void AddGridColumn(ColumnKey key, string header, int width, bool editable)
        {
            var col = new DataGridViewTextBoxColumn
            {
                HeaderText = header,
                Width = width,
                ReadOnly = !editable,
                Name = key.ToString()
            };

            int index = dataGridView1.Columns.Add(col);
            ColumnMap[key] = index;
        }






        public Form1()
        {
            InitializeComponent();
            InitGridColumns();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //LineSplit("PICK_REF-XX.TXT");
            //AnalyzeJumps();

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "BASIC veya metin dosyaları (*.bas;*.txt)|*.bas;*.txt|Tüm dosyalar (*.*)|*.*";
                openFileDialog.Title = "BASIC Dosyası Aç";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    LineSplit(openFileDialog.FileName);
                    AnalyzeJumps();
                    AddSeparators();

                }
            }
        }
        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
        }


        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }



        private void JumpToLine(string lineNum, string statementIndex)
        {
            int stIndex = int.TryParse(statementIndex, out int st) ? st : 0;

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                var row = dataGridView1.Rows[i];
                string currentLine = row.Cells[ColumnMap[ColumnKey.LineNumber]].Value?.ToString() ?? "";
                if (currentLine == lineNum)
                {
                    int jumpIndex = i + stIndex;
                    if (jumpIndex >= dataGridView1.Rows.Count) jumpIndex = i;

                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[jumpIndex].Selected = true;
                    dataGridView1.FirstDisplayedScrollingRowIndex = i;
                    break;
                }
            }
        }



        private void LineSplit(string filename)
        {
            if (!File.Exists(filename))
            {
                MessageBox.Show("File not found: " + filename);
                return;
            }

            dataGridView1.Rows.Clear();
            var lines = File.ReadAllLines(filename);

            Color? currentColor = null;
            string currentFlow = "";
            string currentFunc = "";
            string currentComment = "";
            bool afterReturn = false;
            bool loadingFlowGen = false;

            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string rawLine = lines[lineIndex];
                string line = rawLine.Trim();

                // --- METADATA SATIRI (#...) ---
                if (line.StartsWith("#"))
                {
                    string meta = line.Substring(1);

                    // Renk
                    var colorMatch = Regex.Match(meta, @"0x([0-9A-Fa-f]{6})");
                    if (colorMatch.Success)
                    {
                        string hex = colorMatch.Groups[1].Value;
                        currentColor = ColorTranslator.FromHtml("#" + hex);
                        loadingFlowGen = true;
                    }

                    // Flow işareti
                    var flowMatch = Regex.Match(meta, @"@([^\%""]*)");
                    if (flowMatch.Success)
                        currentFlow = flowMatch.Groups[1].Value;

                    // Function name
                    var funcMatch = Regex.Match(meta, @"%([^\\""]*)");
                    if (funcMatch.Success)
                        currentFunc = funcMatch.Groups[1].Value;

                    // Comment
                    var commentMatch = Regex.Match(meta, "\"(.*)\"");
                    if (commentMatch.Success)
                        currentComment = commentMatch.Groups[1].Value;


                    if (line.EndsWith("{"))
                    {
                        var metaRow = AddGridRow(new Dictionary<ColumnKey, string>()); // Grid'e boş bir satır olarak ekle
                        if (currentColor != null)
                            dataGridView1.Rows[metaRow.Index].DefaultCellStyle.ForeColor = currentColor.Value;
                    }



                    continue; // satır veri satırı değil, atla
                }

                // RETURN sonrası işaretleme
                if (afterReturn && checkBox4.Checked && !loadingFlowGen)
                {
                    // Rastgele ama okunabilir renk
                    currentColor = GetRandomLightColor();
                }

                afterReturn = false;

                if (string.IsNullOrWhiteSpace(line))
                {
                    AddGridRow(new Dictionary<ColumnKey, string>());
                    continue;
                }

                // Satırı : ile böl
                List<string> statements = new();
                StringBuilder current = new();
                bool insideQuotes = false;

                for (int i = 0; i < line.Length; i++)
                {
                    char c = line[i];

                    if (c == '"') insideQuotes = !insideQuotes;

                    if (c == ':' && !insideQuotes)
                    {
                        if (current.ToString().Trim().Length > 0) statements.Add(current.ToString().Trim());
                        current.Clear();
                    }
                    else
                    {
                        current.Append(c);
                    }
                }

                if (current.Length > 0)
                    statements.Add(current.ToString().Trim());

                // Ana satır
                if (statements.Count > 0)
                {
                    var match = Regex.Match(statements[0], @"^(\d+)\s*(.*)");
                    string lineNum = "";
                    if (match.Success)
                    {
                        lineNum = match.Groups[1].Value;
                    }

                    string command = match.Success ? match.Groups[2].Value : statements[0];

                    var row = AddGridRow(new Dictionary<ColumnKey, string>
                    {
                        { ColumnKey.FlowMarker, currentFlow },
                        { ColumnKey.LineNumber, lineNum },
                        { ColumnKey.Command, command },
                        { ColumnKey.FunctionName, currentFunc },
                        { ColumnKey.Comment, currentComment }
                    });

                    if (currentColor != null)
                        dataGridView1.Rows[row.Index].DefaultCellStyle.ForeColor = currentColor.Value;
                    //row.DefaultCellStyle.ForeColor = currentColor.Value;

                    if (command.ToLower().Contains("return"))
                        if (ContainsKeywordOutsideQuotes(command, "return")) afterReturn = true;

                }

                // Alt ifadeler
                for (int i = 1; i < statements.Count; i++)
                {
                    string sub = statements[i];
                    if (string.IsNullOrWhiteSpace(sub))
                        continue;

                    var subRow = AddGridRow(new Dictionary<ColumnKey, string>
            {

                { ColumnKey.Command, sub },
                { ColumnKey.FunctionName, currentFunc },
                { ColumnKey.Comment, currentComment }
            });

                    if (currentColor != null)
                        subRow.DefaultCellStyle.ForeColor = currentColor.Value;

                    if (sub.ToLower().Contains("return"))
                        if (ContainsKeywordOutsideQuotes(sub, "return")) afterReturn = true;
                }

                // RETURN sonrası satır boş değilse boşluk ekle
                if (afterReturn && (lineIndex + 1 >= lines.Length || !string.IsNullOrWhiteSpace(lines[lineIndex + 1])))
                {
                    AddGridRow(new Dictionary<ColumnKey, string>());
                }
            }
        }

        private bool ContainsKeywordOutsideQuotes(string input, string keyword)
        {
            bool insideQuotes = false;
            string lowerKeyword = keyword.ToLower();

            for (int i = 0; i <= input.Length - keyword.Length; i++)
            {
                char c = input[i];
                if (c == '"')
                    insideQuotes = !insideQuotes;

                if (!insideQuotes)
                {
                    // keyword'e denk gelen kısmı kontrol et
                    string fragment = input.Substring(i, keyword.Length).ToLower();
                    if (fragment == lowerKeyword)
                    {
                        // harf sınırında mı?
                        bool leftOk = (i == 0 || !char.IsLetterOrDigit(input[i - 1]));
                        bool rightOk = (i + keyword.Length >= input.Length || !char.IsLetterOrDigit(input[i + keyword.Length]));
                        if (leftOk && rightOk)
                            return true;
                    }
                }
            }

            return false;
        }


        private DataGridViewRow AddGridRow(Dictionary<ColumnKey, string> data)
        {
            object[] cells = new object[dataGridView1.Columns.Count];

            foreach (var pair in data)
            {
                if (ColumnMap.TryGetValue(pair.Key, out int colIndex))
                {
                    cells[colIndex] = pair.Value;
                }
            }

            int rowIndex = dataGridView1.Rows.Add(cells);
            //System.Diagnostics.Debug.WriteLine(data[ColumnKey.Command]);

            return dataGridView1.Rows[rowIndex];
        }


        private void AddSeparators()
        {
            for (int i = dataGridView1.Rows.Count - 1; i > 0; i--)
            {
                var current = dataGridView1.Rows[i];
                var above = dataGridView1.Rows[i - 1];

                string incoming = GetCell(i, ColumnKey.IncomingRefs);
                string previousCommand = GetCell(i - 1, ColumnKey.Command);

                if (!string.IsNullOrWhiteSpace(incoming) &&
                    !string.IsNullOrWhiteSpace(previousCommand))
                {
                    // Boş satırı araya ekle
                    dataGridView1.Rows.Insert(i, new DataGridViewRow());

                    // Araya eklenen satır varsayılan stillerle boş kalır
                }
            }

        }



        private void AnalyzeJumps()
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                var row = dataGridView1.Rows[i];
                string command = GetCell(i, ColumnKey.Command);
                string sourceLine = "";
                int statementIndex = 0;

                // Satır numarası var mı?
                string lineNum = GetCell(i, ColumnKey.LineNumber);
                if (!string.IsNullOrEmpty(lineNum))
                {
                    sourceLine = lineNum;
                    statementIndex = 0;
                }
                else
                {
                    // Önceki line number'ı bul
                    for (int j = i - 1; j >= 0; j--)
                    {
                        string prevLine = GetCell(j, ColumnKey.LineNumber);
                        if (!string.IsNullOrEmpty(prevLine))
                        {
                            sourceLine = prevLine;
                            break;
                        }
                    }

                    // Bu statement kaçıncı alt satır?
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (!string.IsNullOrEmpty(GetCell(j, ColumnKey.LineNumber)))
                            break;

                        statementIndex++;
                    }

                    statementIndex++; // 1-based
                }


                // ON ... GOTO/GOSUB ... durumu (hesaplamalı da olabilir)
                var onMatch = Regex.Match(command.ToLower(), @"\bon\s*.+?\s+(goto|gosub)\s+(.+)");

                if (onMatch.Success)
                {
                    string list = onMatch.Groups[2].Value;
                    string callerRef = $"{sourceLine}:{statementIndex}";

                    var targets = list.Split(',')
                                      .Select(s => s.Trim())
                                      .Where(s => Regex.IsMatch(s, @"^\d+$")) // sadece sabit sayı satırlar
                                      .ToList();

                    foreach (string targetLines in targets)
                    {
                        for (int k = 0; k < dataGridView1.Rows.Count; k++)
                        {
                            if (GetCell(k, ColumnKey.LineNumber) == targetLines)
                            {
                                string existing = GetCell(k, ColumnKey.IncomingRefs);
                                var refs = existing.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(s => s.Trim())
                                                   .ToList();

                                if (!refs.Contains(callerRef))
                                {
                                    refs.Add(callerRef);
                                    SetCell(k, ColumnKey.IncomingRefs, string.Join(", ", refs));
                                }
                                break;
                            }
                        }
                    }

                    continue; // zaten işlendiyse kalan kısmı atla
                }


                // Hedef satır tespiti: GOTO/GOSUB ya da THEN <number>
                string targetLine = null;

                // 1. GOTO / GOSUB
                var jumpMatch = Regex.Match(command.ToLower(), @"\b(go\s*to|go\s*sub|goto|gosub)\s+(\d+)\b");
                if (jumpMatch.Success)
                {
                    targetLine = jumpMatch.Groups[2].Value;
                }

                // 2. THEN <number> (oto-GOTO kabul)
                if (targetLine == null)
                {
                    var thenMatch = Regex.Match(command.ToLower(), @"\bthen\s+(\d+)\b");
                    if (thenMatch.Success)
                    {
                        targetLine = thenMatch.Groups[1].Value;
                    }
                }

                // Hedef bulunduysa referans olarak işaretle
                if (!string.IsNullOrEmpty(targetLine))
                {
                    string callerRef = $"{sourceLine}:{statementIndex}";
                    bool found = false;


                    int targetLineNum = int.Parse(targetLine);
                    int bestMatchIndex = -1;
                    int bestMatchValue = int.MaxValue;

                    for (int k = 0; k < dataGridView1.Rows.Count; k++)
                    {
                        string candidate = GetCell(k, ColumnKey.LineNumber);
                        if (int.TryParse(candidate, out int lineNumb))
                        {
                            if (lineNumb == targetLineNum)
                            {
                                bestMatchIndex = k;
                                break; // tam eşleşme varsa onu seç
                            }
                            else if (lineNumb > targetLineNum && lineNumb < bestMatchValue)
                            {
                                bestMatchIndex = k;       // en küçük büyük değer
                                bestMatchValue = lineNumb;
                            }
                        }
                    }

                    if (bestMatchIndex != -1)
                    {
                        string existing = GetCell(bestMatchIndex, ColumnKey.IncomingRefs);
                        var refs = existing.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                           .Select(s => s.Trim())
                                           .ToList();

                        if (!refs.Contains(callerRef))
                        {
                            refs.Add(callerRef);
                            SetCell(bestMatchIndex, ColumnKey.IncomingRefs, string.Join(", ", refs));
                        }
                        found = true;
                    }




                    if (!found) { MessageBox.Show("Satır bulunamadı.", callerRef, MessageBoxButtons.OK, MessageBoxIcon.Information); }
                }
            }
        }



        private void SetCell(int rowIndex, ColumnKey key, string value)
        {
            dataGridView1.Rows[rowIndex].Cells[ColumnMap[key]].Value = value;
        }




        private void button2_Click(object sender, EventArgs e)
        {
            findText();
        }

        private void findText()
        {
            string query = textBox1.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(query))
                return;

            int startIndex = 0;

            if (!checkBox1.Checked && dataGridView1.CurrentCell != null)
            {
                startIndex = dataGridView1.CurrentCell.RowIndex + 1;
            }

            for (int i = startIndex; i < dataGridView1.Rows.Count; i++)
            {
                // Aranacak sütunlar — istersek genişletebiliriz
                string combined = (
                    GetCell(i, ColumnKey.LineNumber) + " " +
                    GetCell(i, ColumnKey.Command) + " " +
                    GetCell(i, ColumnKey.FunctionName) + " " +
                    GetCell(i, ColumnKey.Comment)
                ).ToLower();

                if (combined.Contains(query))
                {
                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[i].Selected = true;
                    dataGridView1.CurrentCell = dataGridView1.Rows[i].Cells[ColumnMap[ColumnKey.Command]];
                    dataGridView1.FirstDisplayedScrollingRowIndex = Math.Max(0, i - 2);
                    return;
                }
            }

            MessageBox.Show("Metin bulunamadı.", "Arama", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private string GetCell(int rowIndex, ColumnKey key)
        {
            var cell = dataGridView1.Rows[rowIndex].Cells[ColumnMap[key]];
            return cell?.Value?.ToString() ?? "";
        }

        private void textBox1_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Enter basıldığında ding sesi olmasın
                findText();
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {




        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void BuildDynamicJumpMenuItems(string commandText, string incomingRefs)
        {
            // Önceki dinamik öğeleri sil
            for (int i = contextMenuStrip1.Items.Count - 1; i >= 0; i--)
            {
                if (contextMenuStrip1.Items[i].Tag is string tag && tag.StartsWith("jump:"))
                    contextMenuStrip1.Items.RemoveAt(i);
            }

            bool added = false;

            // GOTO/GOSUB varsa en üste ekle
            var match = Regex.Match(commandText.ToLower(), @"\b(go\s*to|go\s*sub|goto|gosub)\s+(\d+)\b");
            if (match.Success)
            {
                string targetLine = match.Groups[2].Value;

                var jumpItem = new ToolStripMenuItem($"→ GOTO/GOSUB {targetLine}")
                {
                    Tag = $"jump:{targetLine}"
                };
                jumpItem.Click += DynamicJumpMenuItem_Click;
                //contextMenuStrip1.Items.Insert(0, jumpItem);
                contextMenuStrip1.Items.Add(jumpItem);
                added = true;
            }

            // Gelen referanslar varsa altına ekle
            var refs = incomingRefs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(s => s.Trim());

            foreach (var reference in refs)
            {
                string[] parts = reference.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[0], out int line))
                {
                    var refItem = new ToolStripMenuItem($"↩ From {reference}")
                    {
                        Tag = $"jump:{parts[0]}:{parts[1]}"
                    };
                    refItem.Click += DynamicJumpMenuItem_Click;
                    contextMenuStrip1.Items.Add(refItem);
                    added = true;
                }
            }

            // Eğer hiç eklenmemişse boş gösterim
            if (!added)
            {
                var refItem = new ToolStripMenuItem("(No references)")
                {
                    Tag = $"jump:0:0",
                    Enabled = false
                };
                contextMenuStrip1.Items.Add(refItem);
            }
        }


        private void DynamicJumpMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is string tag && tag.StartsWith("jump:"))
            {
                string[] parts = tag.Split(':');
                string targetLine = parts[1];

                string statementIndex = (parts.Length > 2) ? parts[2] : "0";

                JumpToLine(targetLine, statementIndex);
            }
        }



        private void GeneralMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem && int.TryParse(menuItem.Tag?.ToString(), out int tag))
            {
                switch (tag)
                {
                    case 2: // Add empty line
                        AddEmptyGridLine();
                        break;

                    case 3: // Export to file...
                        ExportGridSelectionToFile();
                        break;

                    case 4: // Copy
                        CopyGridSelection();
                        break;

                    case 5: // Paste
                        PasteToGridSelection();
                        break;

                    case 6: // Cut
                        CutGridSelection();
                        break;

                    case 7: // Add description (konuşulacak)
                        addGridDescription();
                        break;

                    case 10: //seçili hücrelerin yazı rengini değiştirir
                        changeGridSelection();
                        break;

                    default:
                        MessageBox.Show("Unknown Selection");
                        break;
                }
            }
        }

        private void changeGridSelection()
        {
            if (dataGridView1.SelectedCells.Count == 0)
            {
                MessageBox.Show("Pick some cells first.", "Text Color", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using ColorDialog cd = new ColorDialog
            {
                AllowFullOpen = true,
                FullOpen = true
            };

            if (cd.ShowDialog() != DialogResult.OK)
                return;

            foreach (DataGridViewCell cell in dataGridView1.SelectedCells)
            {
                //cell.Style.ForeColor = cd.Color;
                dataGridView1.Rows[cell.RowIndex].DefaultCellStyle.ForeColor = cd.Color;


            }
        }


        private void AddEmptyGridLine()
        {
            // Hedef satır indeksi: seçili hücrenin satırı
            int insertIndex = dataGridView1.SelectedCells.Count > 0
                ? dataGridView1.SelectedCells[0].RowIndex
                : dataGridView1.Rows.Count;

            // Tüm sütunlar için boşluklar oluştur
            object[] emptyRow = new object[dataGridView1.Columns.Count];
            for (int i = 0; i < emptyRow.Length; i++)
                emptyRow[i] = "";

            dataGridView1.Rows.Insert(insertIndex, emptyRow);

            // Yeni satırı seç ve göster
            dataGridView1.ClearSelection();
            dataGridView1.Rows[insertIndex].Selected = true;
            //dataGridView1.FirstDisplayedScrollingRowIndex = Math.Max(0, insertIndex - 2);
        }


        private void ExportGridSelectionToFile()
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select at least one line", "Wake up", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                FileName = "export.txt"
            };

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            List<string> lines = new();

            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                string line = GetCell(row.Index, ColumnKey.LineNumber).Trim();
                string command = GetCell(row.Index, ColumnKey.Command).Trim();

                if (!string.IsNullOrEmpty(command))
                {
                    if (!string.IsNullOrEmpty(line))
                        lines.Add($"{line} {command}");
                    else
                        lines.Add($": {command}");
                }
            }

            // Satır sırasını korumak için sıralıyoruz
            lines.Reverse(); // çünkü SelectedRows sondan başa sıralı gelir

            File.WriteAllLines(sfd.FileName, lines);
            MessageBox.Show("File Exported successfully.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }



        private void CopyGridSelection()
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select some rows first", "Copy", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Seçilen satırları sıralı hale getirelim
            var selectedRows = dataGridView1.SelectedRows.Cast<DataGridViewRow>()
                                 .OrderBy(r => r.Index)
                                 .ToList();

            StringBuilder sb = new();

            foreach (var row in selectedRows)
            {
                string line = GetCell(row.Index, ColumnKey.LineNumber).Trim();
                string command = GetCell(row.Index, ColumnKey.Command).Trim();

                if (!string.IsNullOrEmpty(command))
                {
                    if (!string.IsNullOrEmpty(line))
                        sb.AppendLine($"{line} {command}");
                    else
                        sb.AppendLine($": {command}");
                }
            }

            if (sb.Length > 0)
            {
                Clipboard.SetText(sb.ToString());
                //MessageBox.Show("Seçim panoya kopyalandı.", "Kopyala", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void PasteToGridSelection()
        {
            string clipText = Clipboard.GetText();

            if (string.IsNullOrWhiteSpace(clipText))
            {
                MessageBox.Show("Clipboard is empty!", "Oh no!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var lines = clipText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var selected = dataGridView1.SelectedRows.Cast<DataGridViewRow>()
                             .OrderBy(r => r.Index)
                             .ToList();

            if (selected.Count == 0)
            {
                MessageBox.Show("Select some rows to paste into.", "Paste", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int pasteCount = Math.Min(lines.Length, selected.Count);

            for (int i = 0; i < pasteCount; i++)
            {
                string line = lines[i].Trim();

                string lineNumber = "";
                string command = "";

                if (line.StartsWith(":"))
                {
                    // Sadece komut
                    command = line.Substring(1).Trim();
                }
                else
                {
                    // Line number + command ayrıştır
                    var match = Regex.Match(line, @"^(\d+)\s*(.*)");
                    if (match.Success)
                    {
                        lineNumber = match.Groups[1].Value;
                        command = match.Groups[2].Value;
                    }
                    else
                    {
                        // Line yoksa tamamı command sayılır
                        command = line;
                    }
                }

                SetCell(selected[i].Index, ColumnKey.LineNumber, lineNumber);
                SetCell(selected[i].Index, ColumnKey.Command, command);
            }
        }


        private void CutGridSelection()
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Nothing to cut.", "Pala", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 1. Adım: Kopyala
            CopyGridSelection();

            // 2. Adım: Seçilen satırları sırayla temizle
            var selectedRows = dataGridView1.SelectedRows.Cast<DataGridViewRow>()
                                 .OrderBy(r => r.Index);

            foreach (var row in selectedRows)
            {
                SetCell(row.Index, ColumnKey.LineNumber, "");
                SetCell(row.Index, ColumnKey.Command, "");
            }


        }

        private void addGridDescription()
        {
            if (dataGridView1.SelectedRows.Count < 2)
            {
                MessageBox.Show("Select a continous range of rows first.", "Describe function", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Satırları sıraya sok
            var selected = dataGridView1.SelectedRows.Cast<DataGridViewRow>()
                            .OrderBy(r => r.Index)
                            .ToList();

            // Seçimler ardışık mı?
            for (int i = 1; i < selected.Count; i++)
            {
                if (selected[i].Index != selected[i - 1].Index + 1)
                {
                    MessageBox.Show("Needs continous range of rows.", "Why you!?", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            string funcName = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter short function name/description:", "Tag range", "");

            if (string.IsNullOrWhiteSpace(funcName))
                return;

            // İşaretleme: üst ┌, orta │, alt └
            for (int i = 0; i < selected.Count; i++)
            {
                int rowIndex = selected[i].Index;

                string marker = "│";
                if (i == 0)
                    marker = "┌";
                else if (i == selected.Count - 1)
                    marker = "└";

                SetCell(rowIndex, ColumnKey.FlowMarker, marker);
                SetCell(rowIndex, ColumnKey.FunctionName, funcName);
            }

        }

        private Color GetRandomLightColor()
        {
            Random rnd = new Random();
            int r = rnd.Next(0, 130);
            int g = rnd.Next(0, 130);
            int b = rnd.Next(0, 130);
            return Color.FromArgb(r, g, b);
        }


        private void ExportFullText(string filename)
        {
            int colFlow = 2;
            int colLine = 6;
            int colCommand = 40;
            int colIncoming = 32;
            int colFunction = 32;
            int colComment = 32;

            List<string> lines = new();

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                string flow = GetCell(i, ColumnKey.FlowMarker);
                string lineNum = GetCell(i, ColumnKey.LineNumber);
                string command = GetCell(i, ColumnKey.Command);
                string incoming = GetCell(i, ColumnKey.IncomingRefs);
                string function = GetCell(i, ColumnKey.FunctionName);
                string comment = GetCell(i, ColumnKey.Comment);

                List<string> flowParts = WrapText(flow, colFlow);
                List<string> lineParts = WrapText(lineNum, colLine);
                List<string> commandParts = WrapText(command, colCommand);
                List<string> incomingParts = WrapText(incoming, colIncoming);
                List<string> functionParts = WrapText(function, colFunction);
                List<string> commentParts = WrapText(comment, colComment);

                int maxLines = new[] {
            flowParts.Count,
            lineParts.Count,
            commandParts.Count,
            incomingParts.Count,
            functionParts.Count,
            commentParts.Count
        }.Max();

                for (int l = 0; l < maxLines; l++)
                {
                    string f =  l < flowParts.Count ? flowParts[l] : "|";
                    string ln = l < lineParts.Count ? lineParts[l] : "";
                    string cmd = l < commandParts.Count ? commandParts[l] : "";
                    string inc = l < incomingParts.Count ? incomingParts[l] : "";
                    string fn = l < functionParts.Count ? functionParts[l] : "";
                    string com = l < commentParts.Count ? commentParts[l] : "";

                    // LineNo logic
                    if (l == 0)
                    {
                        string mark = ":";
                        if (string.IsNullOrWhiteSpace(command)) mark = "";
                        ln = string.IsNullOrWhiteSpace(lineNum) ? mark.PadRight(colLine) : ln;
                    }
                    else
                    {
                        ln = "".PadRight(colLine);
                    }

                    // Flow logic
                    if (!string.IsNullOrWhiteSpace(flow))
                        f = f.PadRight(colFlow);
                    else
                        f = "".PadRight(colFlow);

                    string line = f +
                                  ln.PadRight(colLine) +
                                  cmd.PadRight(colCommand) +
                                  inc.Trim().PadRight(colIncoming) +
                                  fn.PadRight(colFunction) +
                                  com.PadRight(colComment);

                    lines.Add(line);
                }

                // Satır sonu
                //lines.Add("");
            }

            File.WriteAllLines(filename, lines);
        }

        private List<string> WrapText(string text, int maxWidth)
        {
            List<string> result = new();
            if (string.IsNullOrEmpty(text))
            {
                result.Add("");
                return result;
            }

            for (int i = 0; i < text.Length; i += maxWidth)
            {
                int len = Math.Min(maxWidth, text.Length - i);
                result.Add(text.Substring(i, len));
            }

            return result;
        }

        private void ExportToText()
        {
            using SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                FileName = "export.txt"
            };

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            var output = new List<string>();

            if (checkBox3.Checked)
            {
                // Mod 1: Classic BASIC dump
                string currentLine = "";
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    string lineNum = GetCell(i, ColumnKey.LineNumber).Trim();
                    string command = GetCell(i, ColumnKey.Command).Trim();

                    if (!string.IsNullOrWhiteSpace(lineNum))
                    {
                        // önceki satırı yaz
                        if (!string.IsNullOrEmpty(currentLine))
                            output.Add(currentLine);

                        currentLine = $"{lineNum} {command}";
                    }
                    else if (!string.IsNullOrWhiteSpace(command))
                    {
                        currentLine += $": {command}";
                    }
                }

                if (!string.IsNullOrEmpty(currentLine))
                    output.Add(currentLine);

                File.WriteAllLines(sfd.FileName, output);
                MessageBox.Show("Exported.", "Yeah!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                ExportFullText(sfd.FileName);
            }



        }


        private void ExportToFile()
        {
            using SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Flowgen Project Text Files (*.fgn)|*.fgn|All Files (*.*)|*.*",
                FileName = "exported_project.fgn"
            };

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            var output = new List<string>();


            {
                // Mod 2: Meta destekli gelişmiş export
                Color? lastColor = null;
                string lastFlow = "";
                string lastFunc = "";
                string lastComment = "";

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    string lineNum = GetCell(i, ColumnKey.LineNumber).Trim();
                    string command = GetCell(i, ColumnKey.Command).Trim();
                    string flow = GetCell(i, ColumnKey.FlowMarker).Trim();
                    string func = GetCell(i, ColumnKey.FunctionName).Trim();
                    string comment = GetCell(i, ColumnKey.Comment).Trim();

                    bool isEmptyLine = string.IsNullOrWhiteSpace(lineNum) && string.IsNullOrWhiteSpace(command);
                    if (isEmptyLine)
                    {
                        output.Add(""); // gerçek boşluk
                        continue;
                    }

                    // Özellik farkı varsa meta satırı üret
                    var rowColor = dataGridView1.Rows[i].DefaultCellStyle.ForeColor;
                    bool colorChanged = lastColor == null || !rowColor.Equals(lastColor.Value);
                    bool flowChanged = flow != lastFlow;
                    bool funcChanged = func != lastFunc;
                    bool commentChanged = comment != lastComment;

                    if (colorChanged || flowChanged || funcChanged || commentChanged)
                    {
                        string metaLine = "#";

                        if (colorChanged)
                        {
                            metaLine += $"0x{rowColor.R:X2}{rowColor.G:X2}{rowColor.B:X2}";
                            lastColor = rowColor;
                        }

                        if (flowChanged)
                        {
                            metaLine += $"@{flow}";
                            lastFlow = flow;
                        }

                        if (funcChanged)
                        {
                            metaLine += $"%{func}";
                            lastFunc = func;
                        }

                        if (commentChanged)
                        {
                            metaLine += $"\"{comment}\"";
                            lastComment = comment;
                        }

                        output.Add(metaLine);
                    }

                    // Satır çıktısı
                    string lineOut = string.IsNullOrEmpty(lineNum)
                        ? $": {command}"
                        : $"{lineNum} {command}";

                    output.Add(lineOut);
                }
            }

            File.WriteAllLines(sfd.FileName, output);
            MessageBox.Show("Exported.", "Yeah!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }



        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            ExportToFile();
        }

        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // En az bir satır seçili mi?
                if (dataGridView1.SelectedRows.Count == 0)
                    return;

                // İlk seçili satır
                var row = dataGridView1.SelectedRows[0];
                string command = row.Cells[ColumnMap[ColumnKey.Command]].Value?.ToString() ?? "";
                string incoming = row.Cells[ColumnMap[ColumnKey.IncomingRefs]].Value?.ToString() ?? "";

                BuildDynamicJumpMenuItems(command, incoming);
                contextMenuStrip1.Show(dataGridView1, e.Location);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ExportToText();
        }
    }
}
