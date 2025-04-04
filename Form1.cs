using System.Text;
using System.Text.RegularExpressions;

namespace FlowGen
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
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
                }
            }
        }
        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
        }


        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        public bool parseJump()
        {
            if (listBox1.SelectedItem == null)
                return false;

            string selectedText = listBox1.SelectedItem.ToString().ToLower();

            // Eğer "return" varsa, stack'e geri dön
            if (selectedText == "return" || selectedText.Contains("return"))
            {
                if (listBox2.Items.Count > 0)
                {
                    if (int.TryParse(listBox2.Items[0].ToString(), out int returnIndex))
                    {
                        listBox1.SelectedIndex = returnIndex;
                        listBox1.TopIndex = returnIndex;
                        listBox2.Items.RemoveAt(0); // pop
                    }
                }
                return true;
            }

            // GOTO veya GOSUB kontrolü
            string keyword = null;
            if (selectedText.Contains("gosub"))
                keyword = "gosub";
            else if (selectedText.Contains("goto"))
                keyword = "goto";
            else if (selectedText.Contains("go sub"))
                keyword = "go sub";
            else if (selectedText.Contains("go to"))
                keyword = "go to";

            if (keyword != null)
            {
                var match = System.Text.RegularExpressions.Regex.Match(selectedText, $@"{keyword}\s+(\d+)");
                if (match.Success)
                {
                    string targetLine = match.Groups[1].Value;
                    string statementLine = match.Groups[2].Value;


                    if (keyword == "gosub")
                    {
                        int currentIndex = listBox1.SelectedIndex;
                        listBox2.Items.Insert(0, currentIndex); // stack push
                    }

                    JumpToLine(targetLine, statementLine);
                    return true;
                }
            }

            // GOTO/GOSUB yoksa ama (->xxx) varsa — ilk hedefe atla
            var jumpHintMatch = System.Text.RegularExpressions.Regex.Match(selectedText, @"\(->(\d+)\)");
            if (jumpHintMatch.Success)
            {
                string hintedLine = jumpHintMatch.Groups[1].Value;
                string statementLine = jumpHintMatch.Groups[2].Value;
                JumpToLine(hintedLine, statementLine);
                return true;
            }
            return false;
        }

        private void JumpToLine(string targetLine, string statement)
        {
            int st = 0;
            if (!int.TryParse(statement, out st))
                st = 0;  // sayı değilse varsayılan
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                string itemText = listBox1.Items[i].ToString().TrimStart();
                if (itemText.StartsWith(targetLine + " "))
                {
                    listBox1.SelectedIndex = i + st;
                    listBox1.TopIndex = i;
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

            listBox1.Items.Clear();

            var lines = File.ReadAllLines(filename);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                List<string> statements = new List<string>();
                StringBuilder current = new StringBuilder();
                bool insideQuotes = false;

                for (int i = 0; i < line.Length; i++)
                {
                    char c = line[i];

                    if (c == '"')
                        insideQuotes = !insideQuotes;

                    if (c == ':' && !insideQuotes)
                    {
                        statements.Add(current.ToString().Trim());
                        current.Clear();
                    }
                    else
                    {
                        current.Append(c);
                    }
                }

                // C64 stili: Satır sonu varsa tırnak kapalı sayılır
                if (insideQuotes)
                    insideQuotes = false;

                if (current.Length > 0)
                    statements.Add(current.ToString().Trim());

                // Satır numarası + statements
                if (statements.Count > 0)
                {
                    listBox1.Items.Add(statements[0]);

                    for (int i = 1; i < statements.Count; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(statements[i]))
                            listBox1.Items.Add(": " + statements[i]);

                        // Okunabilirlik için return sonrası boşluk
                        if (statements[i].ToLower().Contains("return"))
                            listBox1.Items.Add("");
                    }
                }
            }
        }


        private void listBox1_Click(object sender, EventArgs e)
        {
            //if (listBox1.SelectedIndex > -1) parseJump();
        }

        private void listBox2_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex > -1) popJump();
        }


        private void popJump()
        {

            if (listBox2.SelectedItem == null)
                return;

            // Index bilgisini al
            if (int.TryParse(listBox2.SelectedItem.ToString(), out int returnIndex))
            {
                // Geri zıpla
                listBox1.SelectedIndex = returnIndex;
                listBox1.TopIndex = returnIndex;
            }

            // Stack'ten çıkar (pop)
            listBox2.Items.RemoveAt(listBox2.Items.Count - 1);
        }

        private void AnalyzeJumps()
        {

            // Şimdi her item'ı tek tek analiz et
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                string item = listBox1.Items[i].ToString().Trim();
                string sourceLine = "";
                int statementIndex = 0;

                // Satır numarası içeren satır mı?
                var match = System.Text.RegularExpressions.Regex.Match(item, @"^(\d+)\b");
                if (match.Success)
                {
                    sourceLine = match.Groups[1].Value;
                    statementIndex = 0; // ilk statement
                }
                else if (item.StartsWith(":")) // sub-statement
                {
                    // önceki satırı bulup line numarasını al
                    for (int j = i - 1; j >= 0; j--)
                    {
                        string prev = listBox1.Items[j].ToString().Trim();
                        var prevMatch = System.Text.RegularExpressions.Regex.Match(prev, @"^(\d+)\b");
                        if (prevMatch.Success)
                        {
                            sourceLine = prevMatch.Groups[1].Value;
                            break;
                        }
                    }

                    // statement index = kaçıncı : satırı olduğunu say
                    statementIndex = 0;
                    for (int j = i - 1; j >= 0; j--)
                    {
                        string prev = listBox1.Items[j].ToString().Trim();
                        if (!prev.StartsWith(":"))
                            break;
                        statementIndex++;
                    }
                    statementIndex++; // 1-based index
                }

                // Eğer bu item içinde jump varsa, hedef satırı bul
                var jumpMatch = System.Text.RegularExpressions.Regex.Match(item.ToLower(), @"\b(goto|gosub|go to|go sub)\s+(\d+)\b");
                if (jumpMatch.Success)
                {
                    string target = jumpMatch.Groups[2].Value;
                    string callerRef = $"->{sourceLine}:{statementIndex}";

                    // hedef satırın index'ini bul ve (->X:Y) ekle
                    for (int k = 0; k < listBox1.Items.Count; k++)
                    {
                        string targetItem = listBox1.Items[k].ToString().TrimStart();
                        if (targetItem.StartsWith(target + " "))
                        {
                            string existing = listBox1.Items[k].ToString();
                            if (!existing.Contains(callerRef))
                            {
                                // ÖNCE: Gerekirse bir üst satıra boşluk ekle
                                if (k > 0 && !string.IsNullOrWhiteSpace(listBox1.Items[k - 1].ToString()))
                                {
                                    listBox1.Items.Insert(k, ""); // boş satır ekle
                                    k++; // eklenen boşluk yüzünden hedef satır bir aşağıya kaydı
                                }

                                // Artık işaretlemeyi ekleyebiliriz
                                listBox1.Items[k] = listBox1.Items[k].ToString() + " (" + callerRef + ")";
                            }
                            break;
                        }
                    }

                }
            }
        }
        bool mouseHeld = false;
        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
            {
                if (mouseHeld) return;

                mouseHeld = true;
                int index = listBox1.IndexFromPoint(e.Location);
                if (index < 0) return;



                string text = listBox1.Items[index].ToString();

                Rectangle itemRect = listBox1.GetItemRectangle(index);
                int clickX = e.X - itemRect.X;
                // Ortalama karakter genişliği hesapla
                //Size avgCharSize = TextRenderer.MeasureText(" ", listBox1.Font);
                var s = listBox1.Font.Size;
                int charIndex = (int)(clickX / s);// avgCharSize.Width;
                if (charIndex > text.Length) { parseJump(); return; }

                if (charIndex < Math.Min(12, text.Length)) if (parseJump()) return;
                if (charIndex < 5) charIndex = 5;


                // Şimdi satırdaki tüm (->XXXX) eşleşmelerini bulalım
                string substring = text.Substring(charIndex - 4); // charIndex'ten itibaren kalan kısmı al
                var matches = System.Text.RegularExpressions.Regex.Matches(substring, @"\(->(\d+):(\d+)\)");
                if (matches.Count > 0)
                {
                    string targetLine = matches[0].Groups[1].Value;
                    string statementLine = matches[0].Groups[2].Value;

                    JumpToLine(targetLine, statementLine);
                }
            }
        }
        private void listBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int index = listBox1.IndexFromPoint(e.Location);
                if (index == ListBox.NoMatches) return;

                listBox1.SelectedIndices.Clear();
                listBox1.SelectedIndex = index;

                string text = listBox1.Items[index].ToString();

                BuildDynamicJumpMenuItems(text);

                contextMenuStrip1.Show(listBox1, e.Location);
            }
        }

        private void listBox1_MouseUp2(object sender, MouseEventArgs e)
        {
            mouseHeld = false;

            // Sadece sağ tıklamada göster
            if (e.Button == MouseButtons.Right)
            {
                // Menü konumunda göster
                contextMenuStrip1.Show(listBox1, e.Location);

            }

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

            if (!checkBox1.Checked && listBox1.SelectedIndex >= 0)
            {
                // Devam eden arama için, şu anki seçilinin bir sonrasından başla
                startIndex = listBox1.SelectedIndex + 1;
            }

            for (int i = startIndex; i < listBox1.Items.Count; i++)
            {
                string line = listBox1.Items[i].ToString().ToLower();
                if (line.Contains(query))
                {
                    listBox1.SelectedIndex = i;
                    listBox1.TopIndex = i;
                    return;
                }
            }

            // Eşleşme bulunamadı
            MessageBox.Show("Metin bulunamadı.", "Arama", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void BuildDynamicJumpMenuItems(string text)
        {
            // Önce önceki dinamikleri sil
            for (int i = contextMenuStrip1.Items.Count - 1; i >= 0; i--)
            {
                var item = contextMenuStrip1.Items[i];
                if (item.Tag is string tag && tag.StartsWith("jump:"))
                {
                    contextMenuStrip1.Items.RemoveAt(i);
                }
            }

            // Tüm (->XXXX:YYY) eşleşmelerini bul
            var matches = Regex.Matches(text, @"\(->(\d+):(\d+)\)");

            if (matches.Count > 0)
            {
                //contextMenuStrip1.Items.Add(new ToolStripSeparator());

                foreach (Match match in matches)
                {
                    string line = match.Groups[1].Value;
                    string statement = match.Groups[2].Value;

                    var jumpItem = new ToolStripMenuItem($"→ Goto {line}:{statement}");
                    jumpItem.Tag = $"jump:{line}:{statement}";
                    jumpItem.Click += DynamicJumpMenuItem_Click;

                    contextMenuStrip1.Items.Add(jumpItem);
                }
            }
        }
        private void DynamicJumpMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is string tag && tag.StartsWith("jump:"))
            {
                var parts = tag.Split(':');
                if (parts.Length == 3)
                {
                    string targetLine = parts[1];
                    string statement = parts[2];
                    JumpToLine(targetLine, statement);
                }
            }
        }


        private void GeneralMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem && int.TryParse(menuItem.Tag?.ToString(), out int tag))
            {
                switch (tag)
                {
                    case 1: // Edit
                        EditSelectedLine();
                        break;

                    case 2: // Add empty line
                        AddEmptyLine();
                        break;

                    case 3: // Export to file...
                        ExportSelectionToFile();

                        break;

                    case 4: // Copy
                        CopySelection();
                        break;

                    case 5: // Paste
                        PasteToSelection();
                        break;

                    case 6: // Cut
                        CutSelection();
                        break;
                    case 7: //add description
                        addDescription();
                        break;
                    case 8:
                        follow();
                        break;

                    default:
                        MessageBox.Show("Bilinmeyen işlem");
                        break;
                }
            }
        }
        private void follow()
        {
            parseJump();
        }

        private void EditSelectedLine()
        {
            if (listBox1.SelectedIndices.Count == 0) return;

            int index = listBox1.SelectedIndices[0];
            string current = listBox1.Items[index].ToString();

            string input = Microsoft.VisualBasic.Interaction.InputBox("Düzenle:", "Item Edit", current);
            if (!string.IsNullOrEmpty(input))
                listBox1.Items[index] = input;
        }

        private void addDescription()
        {
            if (listBox1.SelectedIndices.Count == 0) return;

            int index = listBox1.SelectedIndices[0];
            string current = "# ";

            string input = Microsoft.VisualBasic.Interaction.InputBox("Düzenle:", "Item Edit", current);
            if (!string.IsNullOrEmpty(input))
                listBox1.Items[index] = input;
        }

        private void AddEmptyLine()
        {
            if (listBox1.SelectedIndices.Count == 0) return;
            int index = listBox1.SelectedIndices[0];
            listBox1.Items.Insert(index, "");
        }

        private void ExportToFile2()
        {
            using SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                FileName = "export.txt"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllLines(sfd.FileName, listBox1.Items.Cast<string>());
                MessageBox.Show("Dosya dışa aktarıldı.");
            }
        }

        private void ExportToFile()
        {
            using SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                FileName = "export.txt"
            };

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            var lines = listBox1.Items.Cast<string>().ToList();
            var output = new List<string>();
            string lastBaseLine = null;

            foreach (var rawLine in lines)
            {
                string line = rawLine;

                // checkBox2: (->...) kısmını kırp
                if (checkBox2.Checked)
                {
                    int idx = line.IndexOf(" (->");
                    if (idx > -1)
                        line = line.Substring(0, idx).TrimEnd();
                }

                // checkBox3: : ile başlayan satırı birleştir
                if (checkBox3.Checked && line.TrimStart().StartsWith(":"))
                {
                    if (output.Count > 0)
                    {
                        output[output.Count - 1] += " " + line.Trim();
                    }
                    else
                    {
                        // önceki satır yoksa, tek başına ekle
                        output.Add(line.Trim());
                    }
                }
                else
                {
                    output.Add(line);
                }
            }

            File.WriteAllLines(sfd.FileName, output);
            MessageBox.Show("Dosya dışa aktarıldı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExportSelectionToFile()
        {
            if (listBox1.SelectedItems.Count == 0)
            {
                MessageBox.Show("Hiçbir satır seçilmedi.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                FileName = "selection.txt"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllLines(sfd.FileName, listBox1.SelectedItems.Cast<string>());
                MessageBox.Show("Seçili satırlar dışa aktarıldı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


        private void CopySelection()
        {
            if (listBox1.SelectedItems.Count == 0) return;
            string joined = string.Join(Environment.NewLine, listBox1.SelectedItems.Cast<string>());
            if (joined != "") Clipboard.SetText(joined);
        }

        private void PasteToSelection()
        {
            if (!Clipboard.ContainsText()) return;
            string[] lines = Clipboard.GetText().Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            int index = listBox1.SelectedIndices.Count > 0 ? listBox1.SelectedIndices[0] : listBox1.Items.Count;
            foreach (var line in lines.Reverse())
            {
                listBox1.Items.Insert(index, line);
            }
        }

        private void CutSelection()
        {
            CopySelection();
            for (int i = listBox1.SelectedIndices.Count - 1; i >= 0; i--)
            {
                listBox1.Items.RemoveAt(listBox1.SelectedIndices[i]);
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            ExportToFile();
        }
    }
}
