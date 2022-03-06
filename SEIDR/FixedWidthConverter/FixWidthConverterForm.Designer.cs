namespace FixedWidthConverter
{
    /// <summary>
    /// Window form for doing conversion to fix width
    /// </summary>
    partial class FixWidthConverterForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FixWidthConverterForm));
            this.PreviewBox = new System.Windows.Forms.TextBox();
            this.Header = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.filterOutText = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.o_Filter = new System.Windows.Forms.ListBox();
            this.filterKeepText = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.i_Filter = new System.Windows.Forms.ListBox();
            this.Space = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.AlphaNumeric = new System.Windows.Forms.Button();
            this.radioButton8 = new System.Windows.Forms.RadioButton();
            this.re_edit = new System.Windows.Forms.Button();
            this.FinishExpression = new System.Windows.Forms.Button();
            this.vStart = new System.Windows.Forms.Button();
            this.vEnd = new System.Windows.Forms.Button();
            this.any = new System.Windows.Forms.Button();
            this.Letters = new System.Windows.Forms.Button();
            this.digits = new System.Windows.Forms.Button();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.File = new System.Windows.Forms.TextBox();
            this.LoadPreview = new System.Windows.Forms.Button();
            this.Go = new System.Windows.Forms.Button();
            this.PipeIndex = new System.Windows.Forms.Button();
            this.grab = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioButton7 = new System.Windows.Forms.RadioButton();
            this.radioButton6 = new System.Windows.Forms.RadioButton();
            this.radioButton5 = new System.Windows.Forms.RadioButton();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.IndexingBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.PipeIndexes = new System.Windows.Forms.ListBox();
            this.label6 = new System.Windows.Forms.Label();
            this.indexClear = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.saveSetting = new System.Windows.Forms.Button();
            this.toolTip2 = new System.Windows.Forms.ToolTip(this.components);
            this.button2 = new System.Windows.Forms.Button();
            this.settingsFile = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.button1 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.openFileDialog2 = new System.Windows.Forms.OpenFileDialog();
            this.DeriveExpression = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.DeriveIndexing = new System.Windows.Forms.TextBox();
            this.DeriveBox = new System.Windows.Forms.ListBox();
            this.label10 = new System.Windows.Forms.Label();
            this.DeriveName = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.toolTip3 = new System.Windows.Forms.ToolTip(this.components);
            this.DerivedToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.PipeIndexTimer = new System.Windows.Forms.Timer(this.components);
            this.LoadedLineLabel = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.SettingFileExecute = new System.Windows.Forms.CheckBox();
            this.DelimiterGroup = new System.Windows.Forms.GroupBox();
            this.tabOutput = new System.Windows.Forms.RadioButton();
            this.commaOutput = new System.Windows.Forms.RadioButton();
            this.pipeOutput = new System.Windows.Forms.RadioButton();
            this.anchorOffsets = new System.Windows.Forms.Button();
            this.HighLightLineCount = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.DelimiterGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // PreviewBox
            // 
            this.PreviewBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.PreviewBox.Font = new System.Drawing.Font("Courier New", 9F);
            this.PreviewBox.HideSelection = false;
            this.PreviewBox.Location = new System.Drawing.Point(10, 80);
            this.PreviewBox.Multiline = true;
            this.PreviewBox.Name = "PreviewBox";
            this.PreviewBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.PreviewBox.Size = new System.Drawing.Size(676, 740);
            this.PreviewBox.TabIndex = 0;
            this.PreviewBox.TabStop = false;
            this.PreviewBox.WordWrap = false;
            // 
            // Header
            // 
            this.Header.Font = new System.Drawing.Font("Courier New", 9F);
            this.Header.HideSelection = false;
            this.Header.Location = new System.Drawing.Point(699, 83);
            this.Header.Multiline = true;
            this.Header.Name = "Header";
            this.Header.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.Header.Size = new System.Drawing.Size(669, 64);
            this.Header.TabIndex = 1;
            this.toolTip3.SetToolTip(this.Header, "Header to use in file. If left blank, there will be no header line.");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 64);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "File Preview";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(695, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Header";
            // 
            // filterOutText
            // 
            this.filterOutText.Font = new System.Drawing.Font("Courier New", 9F);
            this.filterOutText.HideSelection = false;
            this.filterOutText.Location = new System.Drawing.Point(702, 240);
            this.filterOutText.Name = "filterOutText";
            this.filterOutText.Size = new System.Drawing.Size(517, 21);
            this.filterOutText.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(702, 221);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Filter Out";
            // 
            // o_Filter
            // 
            this.o_Filter.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.o_Filter.FormattingEnabled = true;
            this.o_Filter.HorizontalScrollbar = true;
            this.o_Filter.ItemHeight = 15;
            this.o_Filter.Location = new System.Drawing.Point(702, 266);
            this.o_Filter.Name = "o_Filter";
            this.o_Filter.Size = new System.Drawing.Size(518, 64);
            this.o_Filter.TabIndex = 6;
            this.o_Filter.TabStop = false;
            // 
            // filterKeepText
            // 
            this.filterKeepText.Font = new System.Drawing.Font("Courier New", 9F);
            this.filterKeepText.HideSelection = false;
            this.filterKeepText.Location = new System.Drawing.Point(698, 356);
            this.filterKeepText.Name = "filterKeepText";
            this.filterKeepText.Size = new System.Drawing.Size(519, 21);
            this.filterKeepText.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(700, 340);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(57, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Filter Keep";
            // 
            // i_Filter
            // 
            this.i_Filter.Font = new System.Drawing.Font("Courier New", 9F);
            this.i_Filter.FormattingEnabled = true;
            this.i_Filter.HorizontalScrollbar = true;
            this.i_Filter.ItemHeight = 15;
            this.i_Filter.Location = new System.Drawing.Point(697, 386);
            this.i_Filter.Name = "i_Filter";
            this.i_Filter.Size = new System.Drawing.Size(523, 49);
            this.i_Filter.TabIndex = 9;
            this.i_Filter.TabStop = false;
            this.toolTip3.SetToolTip(this.i_Filter, "Regular expression for lines to add to output file.");
            // 
            // Space
            // 
            this.Space.Location = new System.Drawing.Point(6, 90);
            this.Space.Name = "Space";
            this.Space.Size = new System.Drawing.Size(135, 23);
            this.Space.TabIndex = 10;
            this.Space.TabStop = false;
            this.Space.Text = "To Space";
            this.toolTip3.SetToolTip(this.Space, "Convert the selected text to a regex for that many spaces");
            this.Space.UseVisualStyleBackColor = true;
            this.Space.Click += new System.EventHandler(this.Space_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.AlphaNumeric);
            this.groupBox1.Controls.Add(this.radioButton8);
            this.groupBox1.Controls.Add(this.re_edit);
            this.groupBox1.Controls.Add(this.FinishExpression);
            this.groupBox1.Controls.Add(this.vStart);
            this.groupBox1.Controls.Add(this.vEnd);
            this.groupBox1.Controls.Add(this.any);
            this.groupBox1.Controls.Add(this.Letters);
            this.groupBox1.Controls.Add(this.digits);
            this.groupBox1.Controls.Add(this.radioButton2);
            this.groupBox1.Controls.Add(this.Space);
            this.groupBox1.Controls.Add(this.radioButton1);
            this.groupBox1.Location = new System.Drawing.Point(1226, 153);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(148, 360);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Expression Modifiers";
            this.toolTip1.SetToolTip(this.groupBox1, "Buttons will modify selected text in the appropriate box");
            this.toolTip2.SetToolTip(this.groupBox1, "Buttons will modify selected text in the appropriate box");
            // 
            // AlphaNumeric
            // 
            this.AlphaNumeric.Location = new System.Drawing.Point(6, 179);
            this.AlphaNumeric.Name = "AlphaNumeric";
            this.AlphaNumeric.Size = new System.Drawing.Size(134, 22);
            this.AlphaNumeric.TabIndex = 19;
            this.AlphaNumeric.Text = "To AlphaNumeric";
            this.AlphaNumeric.UseVisualStyleBackColor = true;
            this.AlphaNumeric.Click += new System.EventHandler(this.AlphaNumeric_Click);
            // 
            // radioButton8
            // 
            this.radioButton8.AutoSize = true;
            this.radioButton8.Location = new System.Drawing.Point(7, 67);
            this.radioButton8.Name = "radioButton8";
            this.radioButton8.Size = new System.Drawing.Size(56, 17);
            this.radioButton8.TabIndex = 18;
            this.radioButton8.TabStop = true;
            this.radioButton8.Text = "Derive";
            this.radioButton8.UseVisualStyleBackColor = true;
            this.radioButton8.CheckedChanged += new System.EventHandler(this.radioButton8_CheckedChanged);
            // 
            // re_edit
            // 
            this.re_edit.Location = new System.Drawing.Point(7, 323);
            this.re_edit.Name = "re_edit";
            this.re_edit.Size = new System.Drawing.Size(135, 23);
            this.re_edit.TabIndex = 17;
            this.re_edit.TabStop = false;
            this.re_edit.Text = "Back To EditBox";
            this.re_edit.UseVisualStyleBackColor = true;
            this.re_edit.Click += new System.EventHandler(this.re_edit_Click);
            // 
            // FinishExpression
            // 
            this.FinishExpression.Location = new System.Drawing.Point(7, 294);
            this.FinishExpression.Name = "FinishExpression";
            this.FinishExpression.Size = new System.Drawing.Size(135, 23);
            this.FinishExpression.TabIndex = 16;
            this.FinishExpression.TabStop = false;
            this.FinishExpression.Text = "Finish/Add Expression";
            this.toolTip3.SetToolTip(this.FinishExpression, "Finish the expression, add it to the appropriate expression list.");
            this.FinishExpression.UseVisualStyleBackColor = true;
            this.FinishExpression.Click += new System.EventHandler(this.FinishExpression_Click);
            // 
            // vStart
            // 
            this.vStart.Location = new System.Drawing.Point(7, 264);
            this.vStart.Name = "vStart";
            this.vStart.Size = new System.Drawing.Size(135, 23);
            this.vStart.TabIndex = 15;
            this.vStart.TabStop = false;
            this.vStart.Text = "Add Variable Start(\"%\")";
            this.toolTip3.SetToolTip(this.vStart, "Add a \'%\' to the start of the line. Used like the SQL wildcard");
            this.vStart.UseVisualStyleBackColor = true;
            this.vStart.Click += new System.EventHandler(this.vStart_Click);
            // 
            // vEnd
            // 
            this.vEnd.Location = new System.Drawing.Point(7, 234);
            this.vEnd.Name = "vEnd";
            this.vEnd.Size = new System.Drawing.Size(135, 23);
            this.vEnd.TabIndex = 14;
            this.vEnd.TabStop = false;
            this.vEnd.Text = "Add Variable Ending(\"%\")";
            this.toolTip3.SetToolTip(this.vEnd, "Add a \'%\' to the end of the line. % is used as SQL wildcard");
            this.vEnd.UseVisualStyleBackColor = true;
            this.vEnd.Click += new System.EventHandler(this.vEnd_Click);
            // 
            // any
            // 
            this.any.Location = new System.Drawing.Point(6, 205);
            this.any.Name = "any";
            this.any.Size = new System.Drawing.Size(135, 23);
            this.any.TabIndex = 13;
            this.any.TabStop = false;
            this.any.Text = "To Anything";
            this.toolTip3.SetToolTip(this.any, "Convert the selected text to a regex for that many of any character");
            this.any.UseVisualStyleBackColor = true;
            this.any.Click += new System.EventHandler(this.any_Click);
            // 
            // Letters
            // 
            this.Letters.Location = new System.Drawing.Point(6, 150);
            this.Letters.Name = "Letters";
            this.Letters.Size = new System.Drawing.Size(135, 23);
            this.Letters.TabIndex = 12;
            this.Letters.TabStop = false;
            this.Letters.Text = "To Letters";
            this.toolTip3.SetToolTip(this.Letters, "Convert the selected text to a regex for that many letters or spaces");
            this.Letters.UseVisualStyleBackColor = true;
            this.Letters.Click += new System.EventHandler(this.Letters_Click);
            // 
            // digits
            // 
            this.digits.Location = new System.Drawing.Point(6, 120);
            this.digits.Name = "digits";
            this.digits.Size = new System.Drawing.Size(135, 23);
            this.digits.TabIndex = 11;
            this.digits.TabStop = false;
            this.digits.Text = "To Digits";
            this.toolTip3.SetToolTip(this.digits, "Convert the selected text to a regex for that many spaces or digits");
            this.digits.UseVisualStyleBackColor = true;
            this.digits.Click += new System.EventHandler(this.digits_Click);
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(7, 43);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(75, 17);
            this.radioButton2.TabIndex = 1;
            this.radioButton2.Text = "Filter Keep";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Checked = true;
            this.radioButton1.Location = new System.Drawing.Point(7, 20);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(67, 17);
            this.radioButton1.TabIndex = 0;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Filter Out";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // File
            // 
            this.File.Location = new System.Drawing.Point(11, 20);
            this.File.Name = "File";
            this.File.Size = new System.Drawing.Size(565, 20);
            this.File.TabIndex = 0;
            this.File.TextChanged += new System.EventHandler(this.File_TextChanged);
            // 
            // LoadPreview
            // 
            this.LoadPreview.Enabled = false;
            this.LoadPreview.Location = new System.Drawing.Point(585, 19);
            this.LoadPreview.Name = "LoadPreview";
            this.LoadPreview.Size = new System.Drawing.Size(101, 23);
            this.LoadPreview.TabIndex = 13;
            this.LoadPreview.Text = "Load to Preview";
            this.LoadPreview.UseVisualStyleBackColor = true;
            this.LoadPreview.Click += new System.EventHandler(this.LoadPreview_Click);
            // 
            // Go
            // 
            this.Go.Enabled = false;
            this.Go.Location = new System.Drawing.Point(1089, 812);
            this.Go.Name = "Go";
            this.Go.Size = new System.Drawing.Size(132, 23);
            this.Go.TabIndex = 14;
            this.Go.Text = "Convert";
            this.toolTip2.SetToolTip(this.Go, "Settings can be saved after running.");
            this.toolTip1.SetToolTip(this.Go, "Settings can be saved after running.");
            this.Go.UseVisualStyleBackColor = true;
            this.Go.Click += new System.EventHandler(this.Go_Click);
            // 
            // PipeIndex
            // 
            this.PipeIndex.Location = new System.Drawing.Point(951, 785);
            this.PipeIndex.Name = "PipeIndex";
            this.PipeIndex.Size = new System.Drawing.Size(131, 23);
            this.PipeIndex.TabIndex = 15;
            this.PipeIndex.Text = "Add As Pipe Index";
            this.toolTip2.SetToolTip(this.PipeIndex, "Add indexes by first selecting the full text that should go into this field");
            this.toolTip1.SetToolTip(this.PipeIndex, "Add Pipes to the text in Pipe Indexes to set where you want fields to go");
            this.PipeIndex.UseVisualStyleBackColor = true;
            this.PipeIndex.Click += new System.EventHandler(this.PipeIndex_Click);
            // 
            // grab
            // 
            this.grab.Location = new System.Drawing.Point(386, 17);
            this.grab.Name = "grab";
            this.grab.Size = new System.Drawing.Size(130, 23);
            this.grab.TabIndex = 16;
            this.grab.TabStop = false;
            this.grab.Text = "Grab Line From Preview";
            this.toolTip3.SetToolTip(this.grab, "Grabs a line from the preview box and sends it to the appropriate place");
            this.grab.UseVisualStyleBackColor = true;
            this.grab.Click += new System.EventHandler(this.grab_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.radioButton7);
            this.groupBox2.Controls.Add(this.radioButton6);
            this.groupBox2.Controls.Add(this.radioButton5);
            this.groupBox2.Controls.Add(this.radioButton4);
            this.groupBox2.Controls.Add(this.radioButton3);
            this.groupBox2.Controls.Add(this.grab);
            this.groupBox2.Location = new System.Drawing.Point(699, 153);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(522, 56);
            this.groupBox2.TabIndex = 17;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "PreviewGrab";
            this.toolTip1.SetToolTip(this.groupBox2, "Add text from the selected line of the File Preview to the appropriate section");
            this.toolTip2.SetToolTip(this.groupBox2, "Add text from the selected line of the File Preview to the appropriate section");
            this.groupBox2.Enter += new System.EventHandler(this.groupBox2_Enter);
            // 
            // radioButton7
            // 
            this.radioButton7.AutoSize = true;
            this.radioButton7.Location = new System.Drawing.Point(318, 20);
            this.radioButton7.Name = "radioButton7";
            this.radioButton7.Size = new System.Drawing.Size(56, 17);
            this.radioButton7.TabIndex = 21;
            this.radioButton7.TabStop = true;
            this.radioButton7.Text = "Derive";
            this.toolTip3.SetToolTip(this.radioButton7, "Derive a new column to describe the section or subsection of the fixed width file" +
        ".");
            this.radioButton7.UseVisualStyleBackColor = true;
            this.radioButton7.CheckedChanged += new System.EventHandler(this.radioButton7_CheckedChanged);
            // 
            // radioButton6
            // 
            this.radioButton6.AutoSize = true;
            this.radioButton6.Location = new System.Drawing.Point(252, 20);
            this.radioButton6.Name = "radioButton6";
            this.radioButton6.Size = new System.Drawing.Size(60, 17);
            this.radioButton6.TabIndex = 20;
            this.radioButton6.Text = "Header";
            this.radioButton6.UseVisualStyleBackColor = true;
            this.radioButton6.CheckedChanged += new System.EventHandler(this.radioButton6_CheckedChanged);
            // 
            // radioButton5
            // 
            this.radioButton5.AutoSize = true;
            this.radioButton5.Location = new System.Drawing.Point(161, 20);
            this.radioButton5.Name = "radioButton5";
            this.radioButton5.Size = new System.Drawing.Size(86, 17);
            this.radioButton5.TabIndex = 19;
            this.radioButton5.Text = "Pipe Indexes";
            this.radioButton5.UseVisualStyleBackColor = true;
            this.radioButton5.CheckedChanged += new System.EventHandler(this.radioButton5_CheckedChanged);
            // 
            // radioButton4
            // 
            this.radioButton4.AutoSize = true;
            this.radioButton4.Location = new System.Drawing.Point(80, 20);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(75, 17);
            this.radioButton4.TabIndex = 18;
            this.radioButton4.Text = "Filter Keep";
            this.radioButton4.UseVisualStyleBackColor = true;
            this.radioButton4.CheckedChanged += new System.EventHandler(this.radioButton4_CheckedChanged);
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Checked = true;
            this.radioButton3.Location = new System.Drawing.Point(7, 20);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(67, 17);
            this.radioButton3.TabIndex = 17;
            this.radioButton3.TabStop = true;
            this.radioButton3.Text = "Filter Out";
            this.radioButton3.UseVisualStyleBackColor = true;
            this.radioButton3.CheckedChanged += new System.EventHandler(this.radioButton3_CheckedChanged);
            // 
            // IndexingBox
            // 
            this.IndexingBox.BackColor = System.Drawing.Color.White;
            this.IndexingBox.Font = new System.Drawing.Font("Courier New", 9F);
            this.IndexingBox.HideSelection = false;
            this.IndexingBox.Location = new System.Drawing.Point(699, 703);
            this.IndexingBox.Multiline = true;
            this.IndexingBox.Name = "IndexingBox";
            this.IndexingBox.ReadOnly = true;
            this.IndexingBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.IndexingBox.Size = new System.Drawing.Size(519, 76);
            this.IndexingBox.TabIndex = 18;
            this.toolTip3.SetToolTip(this.IndexingBox, "Highlight text to set or reset a field. Should highlight the full length of space" +
        " available for a field.");
            this.IndexingBox.TextChanged += new System.EventHandler(this.IndexingBox_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(701, 685);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(68, 13);
            this.label5.TabIndex = 19;
            this.label5.Text = "Pipe Indexes";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // PipeIndexes
            // 
            this.PipeIndexes.Font = new System.Drawing.Font("Courier New", 9F);
            this.PipeIndexes.FormattingEnabled = true;
            this.PipeIndexes.ItemHeight = 15;
            this.PipeIndexes.Location = new System.Drawing.Point(1226, 700);
            this.PipeIndexes.Name = "PipeIndexes";
            this.PipeIndexes.Size = new System.Drawing.Size(148, 94);
            this.PipeIndexes.TabIndex = 20;
            this.PipeIndexes.TabStop = false;
            this.PipeIndexes.SelectedIndexChanged += new System.EventHandler(this.PipeIndexes_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(1223, 684);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(146, 13);
            this.label6.TabIndex = 21;
            this.label6.Text = "Added FieldWidth/Index data";
            // 
            // indexClear
            // 
            this.indexClear.Location = new System.Drawing.Point(1089, 784);
            this.indexClear.Name = "indexClear";
            this.indexClear.Size = new System.Drawing.Size(132, 23);
            this.indexClear.TabIndex = 22;
            this.indexClear.TabStop = false;
            this.indexClear.Text = "Clear Indexes";
            this.indexClear.UseVisualStyleBackColor = true;
            this.indexClear.Click += new System.EventHandler(this.indexClear_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Enabled = false;
            this.checkBox1.Location = new System.Drawing.Point(996, 681);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(223, 17);
            this.checkBox1.TabIndex = 23;
            this.checkBox1.TabStop = false;
            this.checkBox1.Text = "Get Indexes Via Manual Pipes in Pipe box";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.Visible = false;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 1);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(48, 13);
            this.label7.TabIndex = 24;
            this.label7.Text = "File Path";
            // 
            // saveSetting
            // 
            this.saveSetting.Enabled = false;
            this.saveSetting.Location = new System.Drawing.Point(951, 811);
            this.saveSetting.Name = "saveSetting";
            this.saveSetting.Size = new System.Drawing.Size(132, 23);
            this.saveSetting.TabIndex = 25;
            this.saveSetting.TabStop = false;
            this.saveSetting.Text = "Save Settings";
            this.toolTip2.SetToolTip(this.saveSetting, "Create a .fwcs file that can be used with a console version of the Converter");
            this.toolTip1.SetToolTip(this.saveSetting, "Create a .fwcs file that can be used with a console version of the Converter");
            this.saveSetting.UseVisualStyleBackColor = true;
            this.saveSetting.Click += new System.EventHandler(this.button1_Click);
            // 
            // toolTip2
            // 
            this.toolTip2.Active = false;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Enabled = false;
            this.button2.Location = new System.Drawing.Point(1225, 20);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(142, 23);
            this.button2.TabIndex = 26;
            this.button2.Text = "Use Settings File";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // settingsFile
            // 
            this.settingsFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.settingsFile.Location = new System.Drawing.Point(699, 22);
            this.settingsFile.Name = "settingsFile";
            this.settingsFile.Size = new System.Drawing.Size(521, 20);
            this.settingsFile.TabIndex = 27;
            this.settingsFile.TextChanged += new System.EventHandler(this.settingsFile_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(698, 6);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(102, 13);
            this.label8.TabIndex = 28;
            this.label8.Text = "Settings File (*.fwcs)";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.ReadOnlyChecked = true;
            this.openFileDialog1.SupportMultiDottedExtensions = true;
            this.openFileDialog1.Title = "Choose Fixed Width File";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(585, 46);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 22);
            this.button1.TabIndex = 29;
            this.button1.Text = "Find File";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Location = new System.Drawing.Point(1128, 48);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(91, 20);
            this.button3.TabIndex = 30;
            this.button3.Text = "Find File";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // openFileDialog2
            // 
            this.openFileDialog2.Filter = "Fix Width Converter settings file|*.fwcs";
            this.openFileDialog2.SupportMultiDottedExtensions = true;
            this.openFileDialog2.Title = "Choose Settings File";
            // 
            // DeriveExpression
            // 
            this.DeriveExpression.HideSelection = false;
            this.DeriveExpression.Location = new System.Drawing.Point(700, 516);
            this.DeriveExpression.Name = "DeriveExpression";
            this.DeriveExpression.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.DeriveExpression.Size = new System.Drawing.Size(375, 20);
            this.DeriveExpression.TabIndex = 31;
            this.toolTip3.SetToolTip(this.DeriveExpression, "Filter for determining if the line should be used for making a derived column");
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(702, 543);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(73, 13);
            this.label9.TabIndex = 32;
            this.label9.Text = "Derived Index";
            // 
            // DeriveIndexing
            // 
            this.DeriveIndexing.BackColor = System.Drawing.Color.White;
            this.DeriveIndexing.Font = new System.Drawing.Font("Courier New", 9F);
            this.DeriveIndexing.HideSelection = false;
            this.DeriveIndexing.Location = new System.Drawing.Point(699, 557);
            this.DeriveIndexing.Multiline = true;
            this.DeriveIndexing.Name = "DeriveIndexing";
            this.DeriveIndexing.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.DeriveIndexing.Size = new System.Drawing.Size(522, 115);
            this.DeriveIndexing.TabIndex = 33;
            this.toolTip3.SetToolTip(this.DeriveIndexing, "Highlight text to choose the length of text to use as the column text. Or just ha" +
        "ve the cursor at an index to take  until the end of the line.");
            // 
            // DeriveBox
            // 
            this.DeriveBox.DisplayMember = "columnName";
            this.DeriveBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DeriveBox.FormattingEnabled = true;
            this.DeriveBox.HorizontalScrollbar = true;
            this.DeriveBox.ItemHeight = 15;
            this.DeriveBox.Location = new System.Drawing.Point(1228, 578);
            this.DeriveBox.Name = "DeriveBox";
            this.DeriveBox.Size = new System.Drawing.Size(146, 94);
            this.DeriveBox.TabIndex = 34;
            this.DeriveBox.TabStop = false;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(705, 500);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(98, 13);
            this.label10.TabIndex = 35;
            this.label10.Text = "Derived Expression";
            // 
            // DeriveName
            // 
            this.DeriveName.Location = new System.Drawing.Point(1082, 515);
            this.DeriveName.Name = "DeriveName";
            this.DeriveName.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.DeriveName.Size = new System.Drawing.Size(138, 20);
            this.DeriveName.TabIndex = 36;
            this.toolTip3.SetToolTip(this.DeriveName, "Do not include in the header box");
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(1082, 499);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(113, 13);
            this.label11.TabIndex = 37;
            this.label11.Text = "Derived Column Name";
            // 
            // PipeIndexTimer
            // 
            this.PipeIndexTimer.Interval = 1750;
            this.PipeIndexTimer.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // LoadedLineLabel
            // 
            this.LoadedLineLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LoadedLineLabel.AutoSize = true;
            this.LoadedLineLabel.Location = new System.Drawing.Point(9, 824);
            this.LoadedLineLabel.Name = "LoadedLineLabel";
            this.LoadedLineLabel.Size = new System.Drawing.Size(35, 13);
            this.LoadedLineLabel.TabIndex = 38;
            this.LoadedLineLabel.Text = "Lines:";
            this.LoadedLineLabel.Visible = false;
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Enabled = false;
            this.numericUpDown1.Location = new System.Drawing.Point(1226, 811);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown1.TabIndex = 39;
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // SettingFileExecute
            // 
            this.SettingFileExecute.AutoSize = true;
            this.SettingFileExecute.Location = new System.Drawing.Point(1227, 51);
            this.SettingFileExecute.Name = "SettingFileExecute";
            this.SettingFileExecute.Size = new System.Drawing.Size(105, 17);
            this.SettingFileExecute.TabIndex = 40;
            this.SettingFileExecute.Text = "Convert on Load";
            this.SettingFileExecute.UseVisualStyleBackColor = true;
            // 
            // DelimiterGroup
            // 
            this.DelimiterGroup.Controls.Add(this.tabOutput);
            this.DelimiterGroup.Controls.Add(this.commaOutput);
            this.DelimiterGroup.Controls.Add(this.pipeOutput);
            this.DelimiterGroup.Location = new System.Drawing.Point(699, 784);
            this.DelimiterGroup.Name = "DelimiterGroup";
            this.DelimiterGroup.Size = new System.Drawing.Size(246, 50);
            this.DelimiterGroup.TabIndex = 41;
            this.DelimiterGroup.TabStop = false;
            this.DelimiterGroup.Text = "Output Delimiter";
            // 
            // tabOutput
            // 
            this.tabOutput.AutoSize = true;
            this.tabOutput.Location = new System.Drawing.Point(124, 19);
            this.tabOutput.Name = "tabOutput";
            this.tabOutput.Size = new System.Drawing.Size(46, 17);
            this.tabOutput.TabIndex = 2;
            this.tabOutput.Text = "TAB";
            this.tabOutput.UseVisualStyleBackColor = true;
            this.tabOutput.CheckedChanged += new System.EventHandler(this.tabOutput_CheckedChanged);
            // 
            // commaOutput
            // 
            this.commaOutput.AutoSize = true;
            this.commaOutput.Location = new System.Drawing.Point(58, 19);
            this.commaOutput.Name = "commaOutput";
            this.commaOutput.Size = new System.Drawing.Size(60, 17);
            this.commaOutput.TabIndex = 1;
            this.commaOutput.Text = "Comma";
            this.commaOutput.UseVisualStyleBackColor = true;
            this.commaOutput.CheckedChanged += new System.EventHandler(this.commaOutput_CheckedChanged);
            // 
            // pipeOutput
            // 
            this.pipeOutput.AutoSize = true;
            this.pipeOutput.Checked = true;
            this.pipeOutput.Location = new System.Drawing.Point(6, 19);
            this.pipeOutput.Name = "pipeOutput";
            this.pipeOutput.Size = new System.Drawing.Size(46, 17);
            this.pipeOutput.TabIndex = 0;
            this.pipeOutput.TabStop = true;
            this.pipeOutput.Text = "Pipe";
            this.pipeOutput.UseVisualStyleBackColor = true;
            this.pipeOutput.CheckedChanged += new System.EventHandler(this.pipeOutput_CheckedChanged);
            // 
            // anchorOffsets
            // 
            this.anchorOffsets.Location = new System.Drawing.Point(697, 457);
            this.anchorOffsets.Name = "anchorOffsets";
            this.anchorOffsets.Size = new System.Drawing.Size(120, 23);
            this.anchorOffsets.TabIndex = 42;
            this.anchorOffsets.Text = "Anchor Offsets";
            this.anchorOffsets.UseVisualStyleBackColor = true;
            this.anchorOffsets.Click += new System.EventHandler(this.anchorOffsets_Click);
            // 
            // HighLightLineCount
            // 
            this.HighLightLineCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.HighLightLineCount.AutoSize = true;
            this.HighLightLineCount.Location = new System.Drawing.Point(247, 822);
            this.HighLightLineCount.Name = "HighLightLineCount";
            this.HighLightLineCount.Size = new System.Drawing.Size(127, 13);
            this.HighLightLineCount.TabIndex = 43;
            this.HighLightLineCount.Text = "HighLighted LineCount: 0";
            this.HighLightLineCount.Visible = false;
            // 
            // FixWidthConverterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1400, 846);
            this.Controls.Add(this.HighLightLineCount);
            this.Controls.Add(this.anchorOffsets);
            this.Controls.Add(this.DelimiterGroup);
            this.Controls.Add(this.SettingFileExecute);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.LoadedLineLabel);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.DeriveName);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.DeriveBox);
            this.Controls.Add(this.DeriveIndexing);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.DeriveExpression);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.settingsFile);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.saveSetting);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.indexClear);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.PipeIndexes);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.IndexingBox);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.PipeIndex);
            this.Controls.Add(this.Go);
            this.Controls.Add(this.LoadPreview);
            this.Controls.Add(this.File);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.i_Filter);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.filterKeepText);
            this.Controls.Add(this.o_Filter);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.filterOutText);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Header);
            this.Controls.Add(this.PreviewBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1416, 885);
            this.MinimumSize = new System.Drawing.Size(1416, 885);
            this.Name = "FixWidthConverterForm";
            this.Text = "Fix Width Converter";
            this.Load += new System.EventHandler(this.FixWidthConverterForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.DelimiterGroup.ResumeLayout(false);
            this.DelimiterGroup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox PreviewBox;
        private System.Windows.Forms.TextBox Header;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox filterOutText;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox o_Filter;
        private System.Windows.Forms.TextBox filterKeepText;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox i_Filter;
        private System.Windows.Forms.Button Space;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button digits;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.Button vStart;
        private System.Windows.Forms.Button vEnd;
        private System.Windows.Forms.Button any;
        private System.Windows.Forms.Button Letters;
        private System.Windows.Forms.Button FinishExpression;
        private System.Windows.Forms.TextBox File;
        private System.Windows.Forms.Button LoadPreview;
        private System.Windows.Forms.Button Go;
        private System.Windows.Forms.Button PipeIndex;
        private System.Windows.Forms.Button grab;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioButton4;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.TextBox IndexingBox;
        private System.Windows.Forms.RadioButton radioButton5;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ListBox PipeIndexes;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button re_edit;
        private System.Windows.Forms.Button indexClear;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.RadioButton radioButton6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolTip toolTip2;
        private System.Windows.Forms.Button saveSetting;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox settingsFile;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.OpenFileDialog openFileDialog2;
        private System.Windows.Forms.RadioButton radioButton7;
        private System.Windows.Forms.RadioButton radioButton8;
        private System.Windows.Forms.TextBox DeriveExpression;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox DeriveIndexing;
        private System.Windows.Forms.ListBox DeriveBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox DeriveName;
        private System.Windows.Forms.ToolTip toolTip3;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ToolTip DerivedToolTip;
        private System.Windows.Forms.Timer PipeIndexTimer;
        private System.Windows.Forms.Label LoadedLineLabel;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.CheckBox SettingFileExecute;
        private System.Windows.Forms.Button AlphaNumeric;
        private System.Windows.Forms.GroupBox DelimiterGroup;
        private System.Windows.Forms.RadioButton tabOutput;
        private System.Windows.Forms.RadioButton commaOutput;
        private System.Windows.Forms.RadioButton pipeOutput;
        private System.Windows.Forms.Button anchorOffsets;
        private System.Windows.Forms.Label HighLightLineCount;
    }
}