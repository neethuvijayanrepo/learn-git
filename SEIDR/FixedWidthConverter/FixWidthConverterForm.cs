using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SEIDR;
using SEIDR.Doc;

namespace FixedWidthConverter
{
    public partial class FixWidthConverterForm : Form
    {
        int lastSelection = 0;
        bool overridePipeGet { get{return checkBox1.Checked;}}
        const string filterOut = "FilterOutExpressions";
        const string filterIn = "FilterInExpressions";
        const string indexSet = "SetFieldIndexes";
        const string headerSet = "SetHeaderBox";
        const string deriveSet = "SetDerivedColBox";
        string folder;
        string ExpressionFocus = filterOut;
        string PreviewGrabFocus = filterOut;
        FixWidthConverter fwc;
        /// <summary>
        /// If true, close the form after the user converts the file
        /// </summary>
        public bool closeAfterConvert = false;
        /// <summary>
        /// Window form for converting fixed width to pipe delimited
        /// </summary>
        public FixWidthConverterForm()
        {
            InitializeComponent();
            PipeIndexTimer.Start();
            openFileDialog1.FileOk += new CancelEventHandler(SetFilePath);
            openFileDialog2.FileOk += new CancelEventHandler(SetFilePath);
        }
        /// <summary>
        /// Window form constructor. Allows you to start up the program with a file already chosen.
        /// </summary>
        /// <param name="FileToLoad"></param>
        public FixWidthConverterForm(string FileToLoad)
        {
            InitializeComponent();
            openFileDialog1.FileOk += new CancelEventHandler(SetFilePath);
            openFileDialog2.FileOk += new CancelEventHandler(SetFilePath);
            File.Text = FileToLoad;
            PipeIndexTimer.Start();
        }
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if(radioButton1.Checked)
                ExpressionFocus = filterOut;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
                ExpressionFocus = filterIn;
        }

        private void Space_Click(object sender, EventArgs e)
        {
            switch (ExpressionFocus)
            {
                case filterIn:
                    {
                        string text = filterKeepText.SelectedText;
                        filterKeepText.SelectedText = FixWidthConverter.makeSpaces(text);
                        return;
                    }
                case filterOut:
                    {
                        string text = filterOutText.SelectedText;
                        filterOutText.SelectedText = FixWidthConverter.makeSpaces(text);
                        return;
                    }
                case deriveSet:
                    {
                        string text = DeriveExpression.SelectedText;
                        DeriveExpression.SelectedText = FixWidthConverter.makeSpaces(text);
                        return;
                    }

            }
            return;
        }

        private void LoadPreview_Click(object sender, EventArgs e)
        {
            folder = System.IO.Path.GetDirectoryName(File.Text);
            List<string> lines = new List<string>();
            using (var sr = new System.IO.StreamReader(File.Text))
            {   
                const int LINE_COUNT = 2000;
                for (int i = 0; i < LINE_COUNT; i++)
                {
                    string temp = sr.ReadLine();
                    if (temp == null)
                    {
                        break;
                    }
                    lines.Add(temp.Replace(((char)12).ToString(), ""));
                }

            }

            PreviewBox.Lines = lines.ToArray();
            LoadedLineLabel.Visible = true;
            LoadedLineLabel.Text = "Lines: " + PreviewBox.Lines.Length;
            HighLightLineCount.Visible = true;
            Go.Enabled = true;
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if(radioButton5.Checked)
                PreviewGrabFocus = indexSet;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
            {
                PreviewGrabFocus = filterIn;
                radioButton2.Checked = true;
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                PreviewGrabFocus = filterOut;
                radioButton1.Checked = true;
            }
        }

        private void grab_Click(object sender, EventArgs e)
        {
            string textGrab;
            if (PreviewBox.SelectionStart >= 0)
                textGrab = PreviewBox.Lines[PreviewBox.GetLineFromCharIndex(PreviewBox.SelectionStart)];
            else
                return;
                      
            switch (PreviewGrabFocus)
            {
                case indexSet:
                    {
                        //textGrab = textGrab.Replace("@", FixWidthConverter.makeAnything("@"));
                        textGrab = textGrab.Replace("|", "~");  
                        IndexingBox.Text = textGrab;
                        return;
                    }
                case filterIn:
                    {
                        //textGrab = textGrab.Replace("@", FixWidthConverter.makeAnything("@"));
                        //textGrab = textGrab.Replace("|", FixWidthConverter.makeAnything("|"));  
                        filterKeepText.Text = textGrab;
                        return;
                    }
                case filterOut:
                    {
                        //textGrab = textGrab.Replace("@", FixWidthConverter.makeAnything("@"));
                        //textGrab = textGrab.Replace("|", FixWidthConverter.makeAnything("|"));  
                        filterOutText.Text = textGrab;
                        return;
                    }
                case headerSet:
                    {
                        textGrab = textGrab.Trim();
                        textGrab = System.Text.RegularExpressions.Regex.Replace(textGrab, @"\s{2,}", fwc.Delimiter.ToString());
                        Header.Text = textGrab;
                        return;
                    }
                case deriveSet:
                    {

                        //textGrab = textGrab.Replace("@", FixWidthConverter.makeAnything("@"));
                        //textGrab = textGrab.Replace("|", FixWidthConverter.makeAnything("|"));                        
                        //textGrab = textGrab.Replace(",", FixWidthConverter.makeAnything(","));
                        DeriveExpression.Text = textGrab;
                        DeriveIndexing.Text = textGrab;
                        return;
                    }

            }
            return;
        }

        private void IndexingBox_TextChanged(object sender, EventArgs e)
        {

        }
        #region Expression click logic
        private void PipeIndex_Click(object sender, EventArgs e)
        {            
            if (overridePipeGet)
            {
                PipeIndexes.Items.Clear(); //Reset
                string line = IndexingBox.Text;
                string[] fields = line.Split('|');
                foreach (string width in fields)
                {
                    PipeIndexes.Items.Add(width.Length);
                }
                /*while (line.Length > 0)
                {
                    string work;
                    int index = line.IndexOf("|");
                    if (index < 0)
                    {
                        PipeIndexes.Items.Add(line.Length);
                        return;
                    }
                    work = line.Substring(0, index);
                    line = line.Substring(index+1);
                    PipeIndexes.Items.Add(index);
                }//*/

            }
            else
                PipeIndexes.Items.Add(IndexingBox.SelectedText.Length);
        }

        private void digits_Click(object sender, EventArgs e)
        {
            switch (ExpressionFocus)
            {
                case filterIn:
                    {
                        string text = filterKeepText.SelectedText;
                        filterKeepText.SelectedText = FixWidthConverter.makeDigits(text, true);
                        return;
                    }
                case filterOut:
                    {
                        string text = filterOutText.SelectedText;
                        filterOutText.SelectedText = FixWidthConverter.makeDigits(text, true);
                        return;
                    }
                case deriveSet:
                    {
                        string text = DeriveExpression.SelectedText;
                        DeriveExpression.SelectedText = FixWidthConverter.makeDigits(text, true);
                        return;
                    }
            }
            return;
        }

        private void Letters_Click(object sender, EventArgs e)
        {
            switch (ExpressionFocus)
            {
                case filterIn:
                    {
                        string text = filterKeepText.SelectedText;
                        filterKeepText.SelectedText = FixWidthConverter.makeLetters(text, true);
                        return;
                    }
                case filterOut:
                    {
                        string text = filterOutText.SelectedText;
                        filterOutText.SelectedText = FixWidthConverter.makeLetters(text, true);
                        return;
                    }
                case deriveSet:
                    {
                        string text = DeriveExpression.SelectedText;
                        DeriveExpression.SelectedText = FixWidthConverter.makeLetters(text, true);
                        return;
                    }
            }
            return;
        }

        private void any_Click(object sender, EventArgs e)
        {
            switch (ExpressionFocus)
            {
                case filterIn:
                    {
                        string text = filterKeepText.SelectedText;
                        filterKeepText.SelectedText = FixWidthConverter.makeAnything(text);
                        return;
                    }
                case filterOut:
                    {
                        string text = filterOutText.SelectedText;
                        filterOutText.SelectedText = FixWidthConverter.makeAnything(text);
                        return;
                    }
                case deriveSet:
                    {
                        string text = DeriveExpression.SelectedText;
                        DeriveExpression.SelectedText = FixWidthConverter.makeAnything(text);
                        return;
                    }
            }
            return;

        }

        private void vEnd_Click(object sender, EventArgs e)
        {
            switch (ExpressionFocus)
            {
                case filterIn:
                    {
                        filterKeepText.Text = filterKeepText.Text + "%";
                        return;
                    }
                case filterOut:
                    {
                        filterOutText.Text = filterOutText.Text + "%";
                        return;
                    }
                case deriveSet:
                    {

                        DeriveExpression.Text = DeriveExpression.Text + "%";
                        return;
                    }
            }
            return;
        }

        private void vStart_Click(object sender, EventArgs e)
        {
            switch (ExpressionFocus)
            {
                case filterIn:
                    {
                        filterKeepText.Text = "%" + filterKeepText.Text ;
                        return;
                    }
                case filterOut:
                    {
                        filterOutText.Text = "%" + filterOutText.Text;
                        return;
                    }
                case deriveSet:
                    {

                        DeriveExpression.Text = "%" + DeriveExpression.Text;
                        return;
                    }
            }
            return;
        }

        private void FinishExpression_Click(object sender, EventArgs e)
        {
            switch (ExpressionFocus)
            {
                case filterIn:
                    {
                        i_Filter.Items.Add(filterKeepText.Text);
                        filterKeepText.Text = "";
                        return;
                    }
                case filterOut:
                    {
                        o_Filter.Items.Add(filterOutText.Text);
                        filterOutText.Text = "";
                        return;
                    }
                case deriveSet:
                    {
                        DerivedColumnInfo di = new DerivedColumnInfo(DeriveExpression.Text, DeriveName.Text, DeriveIndexing.SelectionStart, DeriveIndexing.SelectionLength);
                        DeriveBox.Items.Add(di);
                        DeriveName.Text = "";                        
                        return;
                    }
            }
            return;
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void re_edit_Click(object sender, EventArgs e)
        {
            switch (ExpressionFocus)
            {
                case filterIn:
                    {
                        if (i_Filter.SelectedIndex >= 0)
                        {
                            filterKeepText.Text = i_Filter.SelectedItem.ToString();
                            i_Filter.Items.RemoveAt(i_Filter.SelectedIndex);
                        }
                        return;
                    }
                case filterOut:
                    {
                        if (o_Filter.SelectedIndex >= 0)
                        {
                            filterOutText.Text = o_Filter.SelectedItem.ToString();
                            o_Filter.Items.RemoveAt(o_Filter.SelectedIndex);
                        }
                        return;
                    }
                case deriveSet:
                    {
                        if (DeriveBox.SelectedIndex >= 0)
                        {
                            DerivedColumnInfo dci = (DerivedColumnInfo)DeriveBox.SelectedItem;
                            DeriveName.Text = dci.columnName;
                            DeriveExpression.Text = dci.expression;
                            DeriveIndexing.Text = "".PadLeft(dci.maxLength, '#');
                            DeriveBox.Items.RemoveAt(DeriveBox.SelectedIndex);
                        }
                        return;
                    }
            }
            return;
        }
        #endregion

        private void Go_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            Go.Enabled = false;
            fwc = new FixWidthConverter(File.Text);
            fwc.SetDelimiter(_currentDelimiter);
            fwc.NewHeader = Header.Text;
            if (fwc.NewHeader.Trim() == "")
            {
                MessageBox.Show("Need Header.");
                Go.Enabled = true;
                return;
            }
            foreach (var filter_Keep in i_Filter.Items)
            {
                fwc.filterIn.Add(filter_Keep.ToString());
            }
            foreach (var filter_Out in o_Filter.Items)
            {
                fwc.filterOut.Add(filter_Out.ToString());
            }
            foreach (var ind in PipeIndexes.Items)
            {
                fwc.fieldWidths.Add((int)ind);
            }
            foreach (var derived in DeriveBox.Items)
            {
                fwc.InsertDerived((DerivedColumnInfo)derived);
            }
            fwc.hasHeader = false;
            fwc.HeaderDifferentIndexes = true;
            AnchorOffsetHelper.UpdateConverter(fwc, anchorModifiers);
            fwc.ConvertFile();
            Go.Enabled = true;
            saveSetting.Enabled = true;
            System.Diagnostics.Process.Start(folder);
            Cursor = Cursors.Default;
            if (closeAfterConvert)
                this.Close();
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void PipeIndexes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (PipeIndexes.SelectedIndex >= 0)
            {
                numericUpDown1.Enabled = true;
                var temp = Convert.ToInt32(PipeIndexes.SelectedItem);
                if (temp < numericUpDown1.Maximum)
                    numericUpDown1.Value = temp;
                else
                    numericUpDown1.Enabled = false;
            }
            else
                numericUpDown1.Enabled = false;
        }

        private void indexClear_Click(object sender, EventArgs e)
        {
            PipeIndexes.Items.Clear();
        }

        private void File_TextChanged(object sender, EventArgs e)
        {
            
            if (System.IO.File.Exists(File.Text))
            {
                LoadPreview.Enabled = true;
            }
            else
            {
                LoadPreview.Enabled = false;
                Go.Enabled = false;
                fwc = null;
                if(File.Text.IndexOf('"')>= 0)
                    File.Text = File.Text.Replace("\"", "");
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            toolTip1.Active = checkBox1.Checked;
            toolTip2.Active = !checkBox1.Checked;
            
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            PreviewGrabFocus = headerSet;
        }

        private void button1_Click(object sender, EventArgs e)
        {            
            Cursor = Cursors.WaitCursor;
            saveSetting.Enabled = false;
            string settingFile = System.IO.Path.GetFileNameWithoutExtension(File.Text);            
            settingFile = System.Text.RegularExpressions.Regex.Replace(settingFile, "[ 0-9]+", "") + ".fwcs";
            //settingFile = folder + "\\" + settingFile;
            //if (System.IO.File.Exists(settingFile))
            //{
            SaveFileDialog sfd = new SaveFileDialog();
            if (!string.IsNullOrWhiteSpace(settingsFile.Text))
            {
                sfd.InitialDirectory = System.IO.Path.GetDirectoryName(settingsFile.Text);
                sfd.FileName = System.IO.Path.GetFileName(settingsFile.Text);
            }
            else
            {
                sfd.InitialDirectory = folder;
                sfd.FileName = settingFile;
            }

            sfd.CheckFileExists = false;
            sfd.OverwritePrompt = true;
                
            //System.IO.File.Move(settingFile, settingFile + System.DateTime.Now.ToString("_yyyyMMdd_hhmmss"));
            sfd.ShowHelp = false;
            sfd.Filter = "Fix Width File Converter Settings file | *.fwcs";
            sfd.SupportMultiDottedExtensions = true;
            sfd.ValidateNames = true;
            sfd.Title = "Save Settings file";
            sfd.DefaultExt = ".fwcs";
            sfd.CreatePrompt = false;
            sfd.AddExtension = true;
            DialogResult dr = sfd.ShowDialog();
            if (dr == DialogResult.OK)
            {
                System.IO.File.WriteAllText(sfd.FileName, fwc.ToString());
                //}
                /*
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(settingFile, false))
                {
                    sw.Write(fwc.ToString());
                }*/
            }
            else
                saveSetting.Enabled = true;
            Cursor = Cursors.Default;
        }
        private void PopulateControlsFromSettingFile(string path, string sourceFile = "")
        {
            fwc = FixWidthConverter.construct(sourceFile, path);
            Header.Text = fwc.NewHeader;
            i_Filter.Items.Clear();
            foreach (string filter_keep in fwc.filterIn)
            {
                i_Filter.Items.Add(filter_keep);
            }
            o_Filter.Items.Clear();
            o_Filter.Items.AddRange(fwc.filterOut.ToArray());
            PipeIndexes.Items.Clear();
            foreach (var ind in fwc.fieldWidths)
            {
                PipeIndexes.Items.Add(ind);
            }
            DeriveBox.Items.Clear();
            foreach (var der in fwc.CheckDerivedColumns())
            {
                DeriveBox.Items.Add(der);
            }
            switch (fwc.Delimiter)
            {
                case ',':
                    commaOutput.Checked = true;
                    break;
                case '\t':
                    tabOutput.Checked = true;
                    break;
                default:
                    pipeOutput.Checked = true;
                    break;
            }
            anchorModifiers = AnchorOffsetHelper.GetFromFixWidthSettings(fwc);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(File.Text) && SettingFileExecute.Checked)
            {
                MessageBox.Show("Must specify a file to convert.");
            }
            button2.Enabled = false;
            Cursor = Cursors.WaitCursor;
            PopulateControlsFromSettingFile(settingsFile.Text, File.Text);
            if(folder == null || !File.Text.StartsWith(folder) || SettingFileExecute.Checked == false)
            {
                button2.Enabled = true;
                Cursor = Cursors.Default;
                return;
            }
            fwc.ConvertFile();
            try
            {
                System.Diagnostics.Process.Start(folder);
            }
            finally
            {
                button2.Enabled = true;
                Cursor = Cursors.Default;
            }
        }
        /// <summary>
        /// Allows you to populate the settings textbox with a file path before showing the dialog box.
        /// </summary>
        /// <param name="path"></param>
        public void InitSettingsFile(string path){
            settingsFile.Text = path;
            PopulateControlsFromSettingFile(path, null);
        }
        private void settingsFile_TextChanged(object sender, EventArgs e)
        {
            if (!System.IO.File.Exists(settingsFile.Text))
            {
                button2.Enabled = false;
                if (settingsFile.Text.IndexOf('"') >= 0)
                {
                    settingsFile.Text = settingsFile.Text.Replace("\"", "");
                }
            }
            else
                button2.Enabled = true;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            
        }
        private void SetFilePath(object sender, EventArgs e)
        {
            if (sender == this.openFileDialog1)
            {
                File.Text = openFileDialog1.FileName;
                openFileDialog1.FileName = "";
            }
            else if (sender == this.openFileDialog2)
            {
                settingsFile.Text = openFileDialog2.FileName;
                openFileDialog2.FileName = "";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
            
        }

        private void FixWidthConverterForm_Load(object sender, EventArgs e)
        {

        }

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton7.Checked)
            {
                radioButton8.Checked = true;
                PreviewGrabFocus = deriveSet;
                DerivedToolTip.Active = true;
            }
            else
            {
                DerivedToolTip.Active = false;
            }
        }

        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton8.Checked)
            {
                radioButton7.Checked = true;
                ExpressionFocus = deriveSet;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (PreviewBox.SelectionLength == 0)
            {
                HighLightLineCount.Text = "HighLighted LineCount: 0";
            }
            else
            {
                HighLightLineCount.Text = "HighLighted LineCount: " +
                                          PreviewBox.SelectedText.Split(new string[] { "\r\n", "\r", "\n" },
                                              StringSplitOptions.None).Length;
            }
            if (IndexingBox.SelectionLength + IndexingBox.SelectionStart >= IndexingBox.Text.Length)
                return;
            if (IndexingBox.SelectionLength == lastSelection && IndexingBox.SelectionLength > 0)
            {
                string replacement = IndexingBox.SelectedText.Replace("|", "") + "|";
                IndexingBox.SelectedText = replacement;
                IndexingBox.SelectionStart = IndexingBox.SelectionStart + IndexingBox.SelectionLength;
                IndexingBox.SelectionLength = 0;
                lastSelection = -1;
            }
            else if (IndexingBox.SelectionLength > 0)
            {
                lastSelection = IndexingBox.SelectionLength;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        { 
            
            PipeIndexes.Items[PipeIndexes.SelectedIndex] = Convert.ToInt32(numericUpDown1.Value);
        }

        private void AlphaNumeric_Click(object sender, EventArgs e)
        {
            switch (ExpressionFocus)
            {
                case filterIn:
                    {
                        string text = filterKeepText.SelectedText;
                        filterKeepText.SelectedText = FixWidthConverter.makeLetters(text, false, includeNumeric:true);
                        return;
                    }
                case filterOut:
                    {
                        string text = filterOutText.SelectedText;
                        filterOutText.SelectedText = FixWidthConverter.makeLetters(text, false, includeNumeric: true);
                        return;
                    }
                case deriveSet:
                    {
                        string text = DeriveExpression.SelectedText;
                        DeriveExpression.SelectedText = FixWidthConverter.makeLetters(text, false, includeNumeric: true);
                        return;
                    }
            }
            return;
        }

        char _currentDelimiter = '|';
        private void pipeOutput_CheckedChanged(object sender, EventArgs e)
        {
            const char delim = '|';
            if (pipeOutput.Checked)
            {
                commaOutput.Checked = false;
                tabOutput.Checked = false;
                Header.Text = Header.Text.Replace(_currentDelimiter, delim);
                _currentDelimiter = delim;
            }
        }

        private void commaOutput_CheckedChanged(object sender, EventArgs e)
        {
            const char delim = ',';
            if (commaOutput.Checked)
            {
                pipeOutput.Checked = false;
                tabOutput.Checked = false;
                Header.Text = Header.Text.Replace(_currentDelimiter, delim);
                _currentDelimiter = delim;
            }
        }

        private void tabOutput_CheckedChanged(object sender, EventArgs e)
        {
            const char delim = '\t';
            if (tabOutput.Checked)
            {
                pipeOutput.Checked = false;
                commaOutput.Checked = false;
                Header.Text = Header.Text.Replace(_currentDelimiter, delim);
                _currentDelimiter = delim;
            }
        }
        List<AnchorOffsetHelper> anchorModifiers = new List<AnchorOffsetHelper>();
        private void anchorOffsets_Click(object sender, EventArgs e)
        {
            var text = PreviewBox.Lines[PreviewBox.GetLineFromCharIndex(PreviewBox.SelectionStart)];
            int startPosition = 0;
            int endPosition = 0;
            if(PreviewBox.SelectionLength > 0)
            {
                startPosition = text.IndexOf(PreviewBox.SelectedText);                
                endPosition = PreviewBox.SelectionLength;
                if (text.Length - 1 - startPosition == endPosition)
                    endPosition = 0; //through end of line?   
            }            
            AnchorLineSettings als = new AnchorLineSettings(text, anchorModifiers, startPosition, endPosition);
            if(als.ShowDialog() == DialogResult.OK)
            {
                anchorModifiers = als.HelperSource.Where(a => a != null).ToList();
            }
        }
    }
}
