namespace FixedWidthConverter
{
    partial class AnchorLineSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AnchorLineSettings));
            this.anchorSet = new System.Windows.Forms.ComboBox();
            this.lineSource = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.colName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.AnchorOffsetCount = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.LineOffsetTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.HighLightInfo = new System.Windows.Forms.Label();
            this.ExtractStartUpDown = new System.Windows.Forms.NumericUpDown();
            this.ExtractStartLabel = new System.Windows.Forms.Label();
            this.ExtractEndLabel = new System.Windows.Forms.Label();
            this.ExtractEndupDown = new System.Windows.Forms.NumericUpDown();
            this.endOfLine = new System.Windows.Forms.CheckBox();
            this.save = new System.Windows.Forms.Button();
            this.selectionPoll = new System.Windows.Forms.Timer(this.components);
            this.finish = new System.Windows.Forms.Button();
            this.delete = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.AnchorOffsetCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ExtractStartUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ExtractEndupDown)).BeginInit();
            this.SuspendLayout();
            // 
            // anchorSet
            // 
            this.anchorSet.FormattingEnabled = true;
            this.anchorSet.Location = new System.Drawing.Point(12, 23);
            this.anchorSet.Name = "anchorSet";
            this.anchorSet.Size = new System.Drawing.Size(344, 21);
            this.anchorSet.TabIndex = 0;
            this.anchorSet.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // lineSource
            // 
            this.lineSource.HideSelection = false;
            this.lineSource.Location = new System.Drawing.Point(12, 75);
            this.lineSource.Name = "lineSource";
            this.lineSource.Size = new System.Drawing.Size(704, 20);
            this.lineSource.TabIndex = 1;
            this.lineSource.TextChanged += new System.EventHandler(this.lineSource_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Text Source";
            // 
            // colName
            // 
            this.colName.Location = new System.Drawing.Point(381, 24);
            this.colName.Name = "colName";
            this.colName.Size = new System.Drawing.Size(336, 20);
            this.colName.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(381, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Column Name";
            // 
            // AnchorOffsetCount
            // 
            this.AnchorOffsetCount.Location = new System.Drawing.Point(9, 148);
            this.AnchorOffsetCount.Minimum = new decimal(new int[] {
            70,
            0,
            0,
            -2147483648});
            this.AnchorOffsetCount.Name = "AnchorOffsetCount";
            this.AnchorOffsetCount.Size = new System.Drawing.Size(120, 20);
            this.AnchorOffsetCount.TabIndex = 5;
            this.LineOffsetTooltip.SetToolTip(this.AnchorOffsetCount, resources.GetString("AnchorOffsetCount.ToolTip"));
            this.AnchorOffsetCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.AnchorOffsetCount.ValueChanged += new System.EventHandler(this.AnchorOffsetCount_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 126);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(132, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Anchor Offset (Line Count)";
            // 
            // HighLightInfo
            // 
            this.HighLightInfo.AutoSize = true;
            this.HighLightInfo.Location = new System.Drawing.Point(381, 102);
            this.HighLightInfo.Name = "HighLightInfo";
            this.HighLightInfo.Size = new System.Drawing.Size(172, 13);
            this.HighLightInfo.TabIndex = 7;
            this.HighLightInfo.Text = "Selected Position: 0, Highlighted: 0";
            // 
            // ExtractStartUpDown
            // 
            this.ExtractStartUpDown.Location = new System.Drawing.Point(381, 148);
            this.ExtractStartUpDown.Maximum = new decimal(new int[] {
            4000,
            0,
            0,
            0});
            this.ExtractStartUpDown.Name = "ExtractStartUpDown";
            this.ExtractStartUpDown.Size = new System.Drawing.Size(120, 20);
            this.ExtractStartUpDown.TabIndex = 8;
            this.ExtractStartUpDown.ValueChanged += new System.EventHandler(this.ExtractInfo_ValueChanged);
            // 
            // ExtractStartLabel
            // 
            this.ExtractStartLabel.AutoSize = true;
            this.ExtractStartLabel.Location = new System.Drawing.Point(381, 127);
            this.ExtractStartLabel.Name = "ExtractStartLabel";
            this.ExtractStartLabel.Size = new System.Drawing.Size(65, 13);
            this.ExtractStartLabel.TabIndex = 9;
            this.ExtractStartLabel.Text = "Extract Start";
            // 
            // ExtractEndLabel
            // 
            this.ExtractEndLabel.AutoSize = true;
            this.ExtractEndLabel.Location = new System.Drawing.Point(513, 129);
            this.ExtractEndLabel.Name = "ExtractEndLabel";
            this.ExtractEndLabel.Size = new System.Drawing.Size(116, 13);
            this.ExtractEndLabel.TabIndex = 11;
            this.ExtractEndLabel.Text = "# Characters to Extract";
            // 
            // ExtractEndupDown
            // 
            this.ExtractEndupDown.Location = new System.Drawing.Point(516, 148);
            this.ExtractEndupDown.Maximum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.ExtractEndupDown.Name = "ExtractEndupDown";
            this.ExtractEndupDown.Size = new System.Drawing.Size(120, 20);
            this.ExtractEndupDown.TabIndex = 10;
            this.ExtractEndupDown.ValueChanged += new System.EventHandler(this.ExtractInfo_ValueChanged);
            // 
            // endOfLine
            // 
            this.endOfLine.AutoSize = true;
            this.endOfLine.Location = new System.Drawing.Point(381, 174);
            this.endOfLine.Name = "endOfLine";
            this.endOfLine.Size = new System.Drawing.Size(159, 17);
            this.endOfLine.TabIndex = 12;
            this.endOfLine.Text = "Extract Through End of Line";
            this.endOfLine.UseVisualStyleBackColor = true;
            this.endOfLine.CheckedChanged += new System.EventHandler(this.endOfLine_CheckedChanged);
            // 
            // save
            // 
            this.save.Location = new System.Drawing.Point(381, 225);
            this.save.Name = "save";
            this.save.Size = new System.Drawing.Size(209, 30);
            this.save.TabIndex = 13;
            this.save.Text = "Save Anchor Offset Extract";
            this.save.UseVisualStyleBackColor = true;
            this.save.Click += new System.EventHandler(this.save_Click);
            // 
            // selectionPoll
            // 
            this.selectionPoll.Tick += new System.EventHandler(this.selectionPoll_Tick);
            // 
            // finish
            // 
            this.finish.Location = new System.Drawing.Point(596, 225);
            this.finish.Name = "finish";
            this.finish.Size = new System.Drawing.Size(120, 30);
            this.finish.TabIndex = 15;
            this.finish.Text = "Save and Finish";
            this.finish.UseVisualStyleBackColor = true;
            this.finish.Click += new System.EventHandler(this.finish_Click);
            // 
            // delete
            // 
            this.delete.Location = new System.Drawing.Point(173, 225);
            this.delete.Name = "delete";
            this.delete.Size = new System.Drawing.Size(183, 30);
            this.delete.TabIndex = 16;
            this.delete.Text = "Delete Currently selected";
            this.delete.UseVisualStyleBackColor = true;
            this.delete.Click += new System.EventHandler(this.delete_Click);
            // 
            // AnchorLineSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.AliceBlue;
            this.ClientSize = new System.Drawing.Size(729, 267);
            this.Controls.Add(this.delete);
            this.Controls.Add(this.finish);
            this.Controls.Add(this.save);
            this.Controls.Add(this.endOfLine);
            this.Controls.Add(this.ExtractEndLabel);
            this.Controls.Add(this.ExtractEndupDown);
            this.Controls.Add(this.ExtractStartLabel);
            this.Controls.Add(this.ExtractStartUpDown);
            this.Controls.Add(this.HighLightInfo);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.AnchorOffsetCount);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.colName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lineSource);
            this.Controls.Add(this.anchorSet);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AnchorLineSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AnchorLineSettings";
            ((System.ComponentModel.ISupportInitialize)(this.AnchorOffsetCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ExtractStartUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ExtractEndupDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox anchorSet;
        private System.Windows.Forms.TextBox lineSource;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox colName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown AnchorOffsetCount;
        private System.Windows.Forms.ToolTip LineOffsetTooltip;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label HighLightInfo;
        private System.Windows.Forms.NumericUpDown ExtractStartUpDown;
        private System.Windows.Forms.Label ExtractStartLabel;
        private System.Windows.Forms.Label ExtractEndLabel;
        private System.Windows.Forms.NumericUpDown ExtractEndupDown;
        private System.Windows.Forms.CheckBox endOfLine;
        private System.Windows.Forms.Button save;
        private System.Windows.Forms.Timer selectionPoll;
        private System.Windows.Forms.Button finish;
        private System.Windows.Forms.Button delete;
    }
}