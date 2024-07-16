using DevExpress.Data.ExpressionEditor;
using DevExpress.LookAndFeel;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DXApplication1
{
    public partial class Form1 : RibbonForm
    {
        public string text { get; set; } 
        public Form1()
        {
            InitializeComponent();

            splitContainerControl1.SplitterPosition = splitContainerControl1.Height / 12 * 7;

            repositoryItemTextEdit1.Appearance.ForeColor = DXSkinColors.FillColors.Primary;
            repositoryItemTextEdit3.Appearance.ForeColor = DXSkinColors.FillColors.Success;
            repositoryItemTextEdit4.Appearance.ForeColor = DXSkinColors.FillColors.Danger;
       
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Log++"))
            {
                if (key != null)
                {
                    barEditItem1.EditValue = key.GetValue("FindAll1");
                    barEditItem3.EditValue = key.GetValue("FindAll2");
                    barEditItem4.EditValue = key.GetValue("FindAll3");
                    memoEdit2.EditValue = key.GetValue("Text");
                    text = memoEdit2.Text;
                }
            }
            find();
        }
        private void barButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            find();
        }

        private void find()
        {
            memoEdit2.Select(0,0);
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Log++"))
            {
                key.SetValue("FindAll1", Convert.ToString(barEditItem1.EditValue), RegistryValueKind.String);
                key.SetValue("FindAll2", Convert.ToString(barEditItem3.EditValue), RegistryValueKind.String);
                key.SetValue("FindAll3", Convert.ToString(barEditItem4.EditValue), RegistryValueKind.String);
            }

            var l = text.Split('\n').ToArray();
            var filters = new List<string>();

            if (!string.IsNullOrEmpty(Convert.ToString(barEditItem1.EditValue)))
                filters.Add(Convert.ToString(barEditItem1.EditValue));
            if (!string.IsNullOrEmpty(Convert.ToString(barEditItem3.EditValue)))
                filters.Add(Convert.ToString(barEditItem3.EditValue));
            if (!string.IsNullOrEmpty(Convert.ToString(barEditItem4.EditValue)))
                filters.Add(Convert.ToString(barEditItem4.EditValue));

            if (filters.Count > 0)
                l = l.Where(f => filters.Any(filter => f.Contains(filter))).ToArray();

            memoEdit1.Text = string.Join("\n", l);
        }
        private void memoEdit_CustomHighlightText(object sender, DevExpress.XtraEditors.TextEditCustomHighlightTextEventArgs e)
        {
            e.HighlightWords(Convert.ToString(barEditItem1.EditValue), DXSkinColors.FillColors.Primary);
            e.HighlightWords(Convert.ToString(barEditItem3.EditValue), DXSkinColors.FillColors.Success);
            e.HighlightWords(Convert.ToString(barEditItem4.EditValue), DXSkinColors.FillColors.Danger);
        }

        private void barButtonItem2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            memoEdit2.Text = text;
            memoEdit1.Clear();
             
        }

        private void memoEdit2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                text = Clipboard.GetText();

                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Log++"))
                {
                    key.SetValue("Text", text, RegistryValueKind.String);
                }
            }
        }

        private void memoEdit1_DoubleClick(object sender, EventArgs e)
        {
            MemoEdit memoEdit = sender as MemoEdit;
            int charIndex = memoEdit.SelectionStart;
            int lineIndex = memoEdit.GetLineFromCharIndex(charIndex);
            string searchText = memoEdit.Lines[lineIndex];

            select(memoEdit2, searchText);
        }

        private void select(MemoEdit sender, string searchText)
        {
            MemoEdit memoEdit = sender as MemoEdit;

            int startIndex = sender.Text.IndexOf(searchText);

            if (startIndex != -1)
            {
                memoEdit.Focus();
                memoEdit.SelectionStart = startIndex;
                memoEdit.SelectionLength = 1;
                memoEdit.ScrollToCaret();

                int lineStart = memoEdit.Text.LastIndexOf(Environment.NewLine, startIndex) + 1;
                int lineEnd = memoEdit.Text.IndexOf(Environment.NewLine, startIndex);
                if (lineEnd < 0)
                {
                    lineEnd = memoEdit.Text.Length;
                }
                memoEdit.Select(lineStart, lineEnd - lineStart);
            }
        }
    }
}
