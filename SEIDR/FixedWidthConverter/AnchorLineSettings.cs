using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FixedWidthConverter
{
    public partial class AnchorLineSettings : Form
    {
        public AnchorLineSettings(string newLineSource, List<AnchorOffsetHelper> helperSource, int initialStart, int initialStop)
        {
            InitializeComponent();
            //comboBox1.Enabled = false;
            delete.Enabled = false;
            //helperSource = AnchorOffsetHelper.GetFromFixWidthSettings(fwcSource);
            this.HelperSource = new List<AnchorOffsetHelper>(helperSource);
            this.HelperSource.Insert(0, null);
            //anchorSet.DataSource = helperSource;
            foreach (var anchor in HelperSource)
            {
                anchorSet.Items.Add(anchor ?? "(NEW)" as object);
            }
            anchorSet.SelectedIndex = 0;
            //source = fwcSource;
            lineSource.Text = newLineSource;
            ExtractStartUpDown.Value = initialStart;
            ExtractEndupDown.Value = initialStop;
            if (initialStop == 0)
                endOfLine.Checked = true;
            selectionPoll.Start();
        }
        public List<AnchorOffsetHelper> HelperSource;
        //SEIDR.FixWidthConverter source;        
        private void endOfLine_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb.Checked)
            {
                ExtractEndupDown.Enabled = false;
                ExtractEndupDown.Value = 0;
            }
            else
            {
                ExtractEndupDown.Enabled = true;
                ExtractEndupDown.Value = 1;
            }
            //ExtractInfo_ValueChanged(sender, e); //Updating value property raises event already
        }

        private void lineSource_TextChanged(object sender, EventArgs e)
        {

        }

        private void selectionPoll_Tick(object sender, EventArgs e)
        {
            HighLightInfo.Text = "Selected Position: " + lineSource.SelectionStart + ", Highlighted: " + lineSource.SelectionLength;
        }

        private void delete_Click(object sender, EventArgs e)
        {
            if (anchorSet.SelectedIndex > 0)//Note: Index 0 should be null.
            {                
                HelperSource.RemoveAt(anchorSet.SelectedIndex); 
                anchorSet.Items.RemoveAt(anchorSet.SelectedIndex);
                anchorSet.SelectedIndex = -1; //Remove selection.                
            }
        }

        private void save_Click(object sender, EventArgs e)
        {
            AnchorOffsetHelper anchorHelp;
            bool setNewSelected = false;
            int idx = anchorSet.SelectedIndex;
            if (anchorSet.SelectedIndex > 0) //Cannot unselect, so index 0 is a dummy record.
            {
                anchorHelp = anchorSet.SelectedItem as AnchorOffsetHelper;
            }
            else
            {
                anchorHelp = new AnchorOffsetHelper();
                HelperSource.Add(anchorHelp); //helperSource[0] = null.
                setNewSelected = true;
            }
            if (anchorHelp != null)
            {
                anchorHelp.StartPosition = (int)ExtractStartUpDown.Value;
                anchorHelp.EndPosition = endOfLine.Checked ? null as int? : (int)ExtractEndupDown.Value;
                anchorHelp.Offset = (int)AnchorOffsetCount.Value;
                anchorHelp.ColumnName = colName.Text;
            }
            //Reset position display
            if (setNewSelected)
            {
                anchorSet.Items.Add(anchorHelp);
                anchorSet.SelectedIndex = HelperSource.Count - 1;
            }
            else
            {
                anchorSet.Items.RemoveAt(idx); //Remove and add back so that the list display string is forced to update. (Refresh does not seem to cover)
                anchorSet.Items.Insert(idx, anchorHelp);
                anchorSet.SelectedIndex = idx;
            }
        }
        


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            delete.Enabled = anchorSet.SelectedIndex > 0;
            var anchorHelp = anchorSet.SelectedItem as AnchorOffsetHelper;
            if (anchorHelp != null)
            {
                ExtractStartUpDown.Value = anchorHelp.StartPosition;
                lineSource.SelectionStart = anchorHelp.StartPosition;
                if (anchorHelp.EndPosition.HasValue)
                {
                    endOfLine.Checked = false;
                    ExtractEndupDown.Value = anchorHelp.EndPosition.Value;
                    lineSource.SelectionLength = anchorHelp.EndPosition.Value;
                }
                else
                {
                    ExtractEndupDown.Value = 0;
                    endOfLine.Checked = true;
                    lineSource.SelectionLength = 0;
                }
                AnchorOffsetCount.Value = anchorHelp.Offset;
                colName.Text = anchorHelp.ColumnName;
                
            }
            else
            {
                colName.Text = string.Empty; //unselected.     
                ExtractStartUpDown.Value = 0;
                ExtractEndupDown.Value = 0;
                endOfLine.Checked = true;           
            }
        }

        private void finish_Click(object sender, EventArgs e)
        {
            selectionPoll.Stop();
            //AnchorOffsetHelper.UpdateConverter(source, helperSource);
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ExtractInfo_ValueChanged(object sender, EventArgs e)
        {
            int start = (int)ExtractStartUpDown.Value;
            int? end = endOfLine.Checked ? null as int?: (int)ExtractEndupDown.Value;

            lineSource.SelectionStart = start;
            if (end.HasValue)
                lineSource.SelectionLength = end.Value;
            else
                lineSource.SelectionLength = lineSource.TextLength;
        }
        int lastOffset = 1;
        private void AnchorOffsetCount_ValueChanged(object sender, EventArgs e)
        {
            if (AnchorOffsetCount.Value == 0)
            {
                AnchorOffsetCount.Value = lastOffset > 0 ? -1 : 1;
            }
            else
                lastOffset = (int)AnchorOffsetCount.Value;
        }
    }
}
