﻿using DevExpress.Data.ExpressionEditor;
using DevExpress.LookAndFeel;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DXApplication1
{
    public partial class Form1 : RibbonForm
    {

        private UndoRedoManager undoRedoManager;

        private Color color1 = Color.FromArgb(255, 0, 209, 246);
        private Color color2 = Color.FromArgb(255, 83, 186, 122);
        private Color color3 = Color.FromArgb(255, 252, 109, 119);
        private string find1 { get; set; }
        private string find2 { get; set; }
        private string find3 { get; set; }

        public Form1()
        {
            InitializeComponent();

            undoRedoManager = new UndoRedoManager();

            splitContainerControl1.SplitterPosition = splitContainerControl1.Height / 12 * 7;

            Find1RepositoryItemTextEdit.Appearance.ForeColor = color1;
            Find2RepositoryItemTextEdit.Appearance.ForeColor = color2;
            Find3RepositoryItemTextEdit.Appearance.ForeColor = color3;

            barStaticItem2.Appearance.ForeColor = color1;
            barStaticItem3.Appearance.ForeColor = color2;
            barStaticItem4.Appearance.ForeColor = color3;

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Log++"))
            {
                if (key != null)
                {
                    Find1BarEditItem.EditValue = key.GetValue("FindAll1");
                    Find2BarEditItem.EditValue = key.GetValue("FindAll2");
                    Find3BarEditItem.EditValue = key.GetValue("FindAll3");
                }
            }
        }
        private void FindBarButtonItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            FromMemoEdit.SuspendLayout();
            ToMemoEdit.SuspendLayout();

            find1 = Convert.ToString(Find1BarEditItem.EditValue);
            find2 = Convert.ToString(Find2BarEditItem.EditValue);
            find3 = Convert.ToString(Find3BarEditItem.EditValue);

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Log++"))
            {
                key.SetValue("FindAll1", find1, RegistryValueKind.String);
                key.SetValue("FindAll2", find2, RegistryValueKind.String);
                key.SetValue("FindAll3", find3, RegistryValueKind.String);
            }
            var l = Convert.ToString(FromMemoEdit.EditValue).Split(Environment.NewLine.ToCharArray()).Where(f => !string.IsNullOrEmpty(f)).ToArray();
            var filters = new List<string>();

            ToMemoEdit.Text = string.Empty;
            FromMemoEdit.Select(0, 0);

            if (!string.IsNullOrEmpty(find1))
                filters.Add(find1);
            if (!string.IsNullOrEmpty(find2))
                filters.Add(find2);
            if (!string.IsNullOrEmpty(find3))
                filters.Add(find3);

            if (filters.Count > 0)
                l = l.Where(f => filters.Any(filter => f.Contains(filter))).Distinct().ToArray();

            if (!string.IsNullOrEmpty(find1))
                barStaticItem2.Caption = $"{find1} ({l.Where(f => f.Contains(find1)).Count()} hits)";
            else
                barStaticItem2.Caption = string.Empty;

            if (!string.IsNullOrEmpty(find2))
                barStaticItem3.Caption = $"{find2} ({l.Where(f => f.Contains(find2)).Count()} hits)";
            else
                barStaticItem3.Caption = string.Empty;

            if (!string.IsNullOrEmpty(find3))
                barStaticItem4.Caption = $"{find3} ({l.Where(f => f.Contains(find3)).Count()} hits)";
            else
                barStaticItem4.Caption = string.Empty;

            if (Contains1BarCheckItem.Checked)
            {
                StringComparison comparisonType1 = StringComparison.OrdinalIgnoreCase;
                l = l.Where(f => f.IndexOf(find1, comparisonType1) >= 0).ToArray();
            }

            if (Contains2BarCheckItem.Checked)
            {
                StringComparison comparisonType2 = StringComparison.OrdinalIgnoreCase;
                l = l.Where(f => f.IndexOf(find2, comparisonType2) >= 0).ToArray();
            }

            if (Contains3BarCheckItem.Checked)
            {
                StringComparison comparisonType3 = StringComparison.OrdinalIgnoreCase;
                l = l.Where(f => f.IndexOf(find3, comparisonType3) >= 0).ToArray();
            }

            FromMemoEdit.ResumeLayout();
            ToMemoEdit.ResumeLayout();
            ToMemoEdit.Text = string.Join(Environment.NewLine, l);
            FromMemoEdit.Text = FromMemoEdit.Text;
        }

        private void ToMemoEdit_CustomHighlightText(object sender, DevExpress.XtraEditors.TextEditCustomHighlightTextEventArgs e)
        {
            if (find1 != null)
                e.HighlightWords(find1, color1);
            if (find2 != null)
                e.HighlightWords(find2, color2);
            if (find3 != null)
                e.HighlightWords(find3, color3);
        }

        private void FromMemoEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(Clipboard.GetText());
                var clipBoard = Encoding.UTF8.GetString(bytes);

                FromMemoEdit.EditValue = string.Empty;

                FromMemoEdit.EditValue = clipBoard;
                //using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Log++"))
                //{
                //    key.SetValue("Text", text, RegistryValueKind.String);
                //}

                e.Handled = true;
            }
            if (e.Control && e.KeyCode == Keys.Z)
            {
                undoRedoManager.Undo(FromMemoEdit);
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.Y)
            {
                undoRedoManager.Redo(FromMemoEdit);
                e.Handled = true;
            }
        }

        private void ToMemoEdit_DoubleClick(object sender, EventArgs e)
        {
            MemoEdit memoEdit = sender as MemoEdit;
            int charIndex = memoEdit.SelectionStart;
            int lineIndex = memoEdit.GetLineFromCharIndex(charIndex);
            string searchText = memoEdit.Lines[lineIndex];

            select(FromMemoEdit, searchText);
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

        private void FromMemoEdit_TextChanged(object sender, EventArgs e)
        {
            if (!undoRedoManager.IsUndoOrRedo)
            {
                undoRedoManager.AddState(Convert.ToString(FromMemoEdit.EditValue));
            }
        }
        private void ClearBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            ToMemoEdit.Clear();
        }

        private void UpBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            ToMemoEdit.SelectionStart = 0;
            ToMemoEdit.ScrollToCaret();
        }

        private void DownBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            ToMemoEdit.SelectionStart = ToMemoEdit.Text.Length;
            ToMemoEdit.ScrollToCaret();
        }
    }
}
