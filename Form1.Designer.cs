namespace FlowGen
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            button1 = new Button();
            listBox2 = new ListBox();
            textBox1 = new TextBox();
            button2 = new Button();
            checkBox1 = new CheckBox();
            contextMenuStrip1 = new ContextMenuStrip(components);
            editToolStripMenuItem = new ToolStripMenuItem();
            addEmptyLineToolStripMenuItem = new ToolStripMenuItem();
            pickColorToolStripMenuItem = new ToolStripMenuItem();
            addDescriptionToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem2 = new ToolStripSeparator();
            copyToolStripMenuItem = new ToolStripMenuItem();
            pasteToolStripMenuItem = new ToolStripMenuItem();
            cutToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripSeparator();
            exportToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem3 = new ToolStripSeparator();
            followGotoToolStripMenuItem = new ToolStripMenuItem();
            label1 = new Label();
            button3 = new Button();
            checkBox2 = new CheckBox();
            checkBox3 = new CheckBox();
            groupBox1 = new GroupBox();
            button4 = new Button();
            dataGridView1 = new DataGridView();
            checkBox4 = new CheckBox();
            contextMenuStrip1.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(12, 19);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 0;
            button1.Text = "Open";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // listBox2
            // 
            listBox2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            listBox2.FormattingEnabled = true;
            listBox2.ItemHeight = 15;
            listBox2.Location = new Point(512, 6);
            listBox2.Name = "listBox2";
            listBox2.Size = new Size(75, 34);
            listBox2.TabIndex = 3;
            listBox2.Visible = false;
            listBox2.SelectedIndexChanged += listBox2_SelectedIndexChanged;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(182, 19);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(160, 23);
            textBox1.TabIndex = 4;
            textBox1.KeyDown += textBox1_KeyDown;
            textBox1.MouseDown += textBox1_MouseDown;
            // 
            // button2
            // 
            button2.Location = new Point(348, 19);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 5;
            button2.Text = "Find";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(429, 21);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(77, 19);
            checkBox1.TabIndex = 6;
            checkBox1.Text = "From Top";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { editToolStripMenuItem, addEmptyLineToolStripMenuItem, pickColorToolStripMenuItem, addDescriptionToolStripMenuItem, toolStripMenuItem2, copyToolStripMenuItem, pasteToolStripMenuItem, cutToolStripMenuItem, toolStripMenuItem1, exportToolStripMenuItem, toolStripMenuItem3, followGotoToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(160, 220);
            contextMenuStrip1.Opening += contextMenuStrip1_Opening;
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new Size(159, 22);
            editToolStripMenuItem.Tag = "1";
            editToolStripMenuItem.Text = "Edit";
            editToolStripMenuItem.Visible = false;
            editToolStripMenuItem.Click += GeneralMenuItem_Click;
            // 
            // addEmptyLineToolStripMenuItem
            // 
            addEmptyLineToolStripMenuItem.Name = "addEmptyLineToolStripMenuItem";
            addEmptyLineToolStripMenuItem.Size = new Size(159, 22);
            addEmptyLineToolStripMenuItem.Tag = "2";
            addEmptyLineToolStripMenuItem.Text = "Add Empty Line";
            addEmptyLineToolStripMenuItem.Click += GeneralMenuItem_Click;
            // 
            // pickColorToolStripMenuItem
            // 
            pickColorToolStripMenuItem.Name = "pickColorToolStripMenuItem";
            pickColorToolStripMenuItem.Size = new Size(159, 22);
            pickColorToolStripMenuItem.Tag = "10";
            pickColorToolStripMenuItem.Text = "Change Color";
            pickColorToolStripMenuItem.Click += GeneralMenuItem_Click;
            // 
            // addDescriptionToolStripMenuItem
            // 
            addDescriptionToolStripMenuItem.Name = "addDescriptionToolStripMenuItem";
            addDescriptionToolStripMenuItem.Size = new Size(159, 22);
            addDescriptionToolStripMenuItem.Tag = "7";
            addDescriptionToolStripMenuItem.Text = "Add Description";
            addDescriptionToolStripMenuItem.Click += GeneralMenuItem_Click;
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(156, 6);
            toolStripMenuItem2.Click += toolStripMenuItem2_Click;
            // 
            // copyToolStripMenuItem
            // 
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            copyToolStripMenuItem.Size = new Size(159, 22);
            copyToolStripMenuItem.Tag = "4";
            copyToolStripMenuItem.Text = "Copy";
            copyToolStripMenuItem.Click += GeneralMenuItem_Click;
            // 
            // pasteToolStripMenuItem
            // 
            pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            pasteToolStripMenuItem.Size = new Size(159, 22);
            pasteToolStripMenuItem.Tag = "5";
            pasteToolStripMenuItem.Text = "Paste";
            pasteToolStripMenuItem.Click += GeneralMenuItem_Click;
            // 
            // cutToolStripMenuItem
            // 
            cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            cutToolStripMenuItem.Size = new Size(159, 22);
            cutToolStripMenuItem.Tag = "6";
            cutToolStripMenuItem.Text = "Cut";
            cutToolStripMenuItem.Click += GeneralMenuItem_Click;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(156, 6);
            // 
            // exportToolStripMenuItem
            // 
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            exportToolStripMenuItem.Size = new Size(159, 22);
            exportToolStripMenuItem.Tag = "3";
            exportToolStripMenuItem.Text = "Export...";
            exportToolStripMenuItem.Click += GeneralMenuItem_Click;
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            toolStripMenuItem3.Size = new Size(156, 6);
            // 
            // followGotoToolStripMenuItem
            // 
            followGotoToolStripMenuItem.Name = "followGotoToolStripMenuItem";
            followGotoToolStripMenuItem.Size = new Size(159, 22);
            followGotoToolStripMenuItem.Tag = "8";
            followGotoToolStripMenuItem.Text = "Follow Goto...";
            followGotoToolStripMenuItem.Visible = false;
            followGotoToolStripMenuItem.Click += GeneralMenuItem_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(512, 23);
            label1.Name = "label1";
            label1.Size = new Size(234, 15);
            label1.TabIndex = 7;
            label1.Text = "Alt+click to follow |  Right Click for Actions";
            // 
            // button3
            // 
            button3.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button3.Location = new Point(316, 16);
            button3.Name = "button3";
            button3.Size = new Size(124, 23);
            button3.TabIndex = 8;
            button3.Text = "Save Project...";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // checkBox2
            // 
            checkBox2.AutoSize = true;
            checkBox2.Checked = true;
            checkBox2.CheckState = CheckState.Checked;
            checkBox2.Location = new Point(153, 18);
            checkBox2.Name = "checkBox2";
            checkBox2.Size = new Size(136, 19);
            checkBox2.TabIndex = 9;
            checkBox2.Text = "Remove (->pointers)";
            checkBox2.UseVisualStyleBackColor = false;
            checkBox2.Visible = false;
            // 
            // checkBox3
            // 
            checkBox3.AutoSize = true;
            checkBox3.Location = new Point(14, 18);
            checkBox3.Name = "checkBox3";
            checkBox3.Size = new Size(142, 19);
            checkBox3.TabIndex = 10;
            checkBox3.Text = "Export as plain bas file";
            checkBox3.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            groupBox1.Controls.Add(button4);
            groupBox1.Controls.Add(checkBox3);
            groupBox1.Controls.Add(checkBox2);
            groupBox1.Controls.Add(button3);
            groupBox1.Location = new Point(771, -3);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(460, 45);
            groupBox1.TabIndex = 11;
            groupBox1.TabStop = false;
            // 
            // button4
            // 
            button4.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button4.Location = new Point(164, 16);
            button4.Name = "button4";
            button4.Size = new Size(124, 23);
            button4.TabIndex = 11;
            button4.Text = "Export as Txt...";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(12, 48);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(1219, 439);
            dataGridView1.TabIndex = 13;
            dataGridView1.MouseClick += dataGridView1_MouseClick;
            // 
            // checkBox4
            // 
            checkBox4.AutoSize = true;
            checkBox4.Checked = true;
            checkBox4.CheckState = CheckState.Checked;
            checkBox4.Location = new Point(93, 22);
            checkBox4.Name = "checkBox4";
            checkBox4.Size = new Size(81, 19);
            checkBox4.TabIndex = 14;
            checkBox4.Text = "AutoColor";
            checkBox4.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1243, 499);
            Controls.Add(checkBox4);
            Controls.Add(dataGridView1);
            Controls.Add(groupBox1);
            Controls.Add(label1);
            Controls.Add(checkBox1);
            Controls.Add(button2);
            Controls.Add(textBox1);
            Controls.Add(listBox2);
            Controls.Add(button1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            Text = "FlowGen";
            contextMenuStrip1.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private ListBox listBox2;
        private TextBox textBox1;
        private Button button2;
        private CheckBox checkBox1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem addEmptyLineToolStripMenuItem;
        private ToolStripMenuItem exportToolStripMenuItem;
        private ToolStripMenuItem copyToolStripMenuItem;
        private ToolStripMenuItem pasteToolStripMenuItem;
        private ToolStripMenuItem cutToolStripMenuItem;
        private ToolStripMenuItem addDescriptionToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem2;
        private ToolStripSeparator toolStripMenuItem1;
        private Label label1;
        private ToolStripSeparator toolStripMenuItem3;
        private ToolStripMenuItem followGotoToolStripMenuItem;
        private Button button3;
        private CheckBox checkBox2;
        private CheckBox checkBox3;
        private GroupBox groupBox1;
        private DataGridView dataGridView1;
        private ToolStripMenuItem pickColorToolStripMenuItem;
        private CheckBox checkBox4;
        private Button button4;
    }
}
