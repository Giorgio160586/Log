using DevExpress.Data.ExpressionEditor;
using DevExpress.Drawing.Internal;
using DevExpress.LookAndFeel;
using DevExpress.XtraBars;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DXApplication1
{
    public partial class Form1 : RibbonForm
    {
        public string text { get; set; }

        private UndoRedoManager undoRedoManager;

        public Form1()
        {
            InitializeComponent();

            undoRedoManager = new UndoRedoManager();

            splitContainerControl1.SplitterPosition = splitContainerControl1.Height / 12 * 7;

            repositoryItemTextEdit1.Appearance.ForeColor = DXSkinColors.FillColors.Primary;
            repositoryItemTextEdit3.Appearance.ForeColor = DXSkinColors.FillColors.Success;
            repositoryItemTextEdit4.Appearance.ForeColor = DXSkinColors.FillColors.Danger;

            barStaticItem2.Appearance.ForeColor = Color.FromArgb(255, 0, 209, 246);
            barStaticItem3.Appearance.ForeColor = DXSkinColors.FillColors.Success;
            barStaticItem4.Appearance.ForeColor = DXSkinColors.FillColors.Danger;

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

            var l = Convert.ToString(memoEdit2.EditValue).Split('\n').ToArray();
            var filters = new List<string>();

            if (!string.IsNullOrEmpty(Convert.ToString(barEditItem1.EditValue)))
                filters.Add(Convert.ToString(barEditItem1.EditValue));
            if (!string.IsNullOrEmpty(Convert.ToString(barEditItem3.EditValue)))
                filters.Add(Convert.ToString(barEditItem3.EditValue));
            if (!string.IsNullOrEmpty(Convert.ToString(barEditItem4.EditValue)))
                filters.Add(Convert.ToString(barEditItem4.EditValue));

            if (filters.Count > 0)
                l = l.Where(f => filters.Any(filter => f.Contains(filter))).ToArray();

            if (!string.IsNullOrEmpty(Convert.ToString(barEditItem1.EditValue)))
                barStaticItem2.Caption = $"{Convert.ToString(barEditItem1.EditValue)} ({l.Where(f => f.Contains(Convert.ToString(barEditItem1.EditValue))).Count()} hits)";
            else
                barStaticItem2.Caption = string.Empty;

            if (!string.IsNullOrEmpty(Convert.ToString(barEditItem3.EditValue)))
                barStaticItem3.Caption = $"{Convert.ToString(barEditItem3.EditValue)} ({l.Where(f => f.Contains(Convert.ToString(barEditItem3.EditValue))).Count()} hits)";
            else
                barStaticItem3.Caption = string.Empty;

            if (!string.IsNullOrEmpty(Convert.ToString(barEditItem4.EditValue)))
                barStaticItem4.Caption = $"{Convert.ToString(barEditItem4.EditValue)} ({l.Where(f => f.Contains(Convert.ToString(barEditItem4.EditValue))).Count()} hits)";
            else
                barStaticItem4.Caption = string.Empty;


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
            barStaticItem2.Caption = string.Empty;
            barStaticItem3.Caption = string.Empty;
            barStaticItem4.Caption = string.Empty;
        }

        private void memoEdit2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(Clipboard.GetText());
                var clipBoard = Encoding.UTF8.GetString(bytes);

                text = clipBoard;

                memoEdit2.EditValue = text;
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Log++"))
                {
                    key.SetValue("Text", text, RegistryValueKind.String);
                }
            }
            if (e.Control && e.KeyCode == Keys.Z)
            {
                undoRedoManager.Undo(memoEdit2);
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.Y)
            {
                undoRedoManager.Redo(memoEdit2);
                e.Handled = true;
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

        private void ClearBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            memoEdit2.Clear();
            memoEdit1.Clear();
            find();
            text = string.Empty;
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Log++"))
            {
                key.SetValue("Text", text, RegistryValueKind.String);
            }
        }


        private void memoEdit2_TextChanged(object sender, EventArgs e)
        {
            if (!undoRedoManager.IsUndoOrRedo)
            {
                undoRedoManager.AddState(memoEdit2.Text);
            }

        }
    }
}
