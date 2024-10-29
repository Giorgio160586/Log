using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DXApplication1
{
    public partial class Form1 : RibbonForm
    {
        private UndoRedoManager undoRedoManager;

        private Color color1 = Color.FromArgb(255, 0, 209, 246);
        private Color color2 = Color.FromArgb(255, 83, 186, 122);
        private Color color3 = Color.FromArgb(255, 252, 109, 119);

        private Color selectionColor = Color.FromArgb(255, 33, 66, 131);

        private const string subkey = @"SOFTWARE\Log++";
        private List<string> find1 { get; set; }
        private List<string> find2 { get; set; }
        private List<string> find3 { get; set; }
        public static string version
        {
            get
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                return versionInfo.FileVersion;
            }
        }

        private int searchStartIndex = 0;

        private bool formLoaded = false;
        public Form1()
        {
            InitializeComponent();

            ResizeFormToScreenPercentage(0.75);

            var process = Environment.Is64BitProcess ? "x64" : "x86";
            this.Text += $" - v{version} ({process})";

            FromMemoEdit.Properties.AdvancedModeOptions.SelectionColor = selectionColor;
            ToMemoEdit.Properties.AdvancedModeOptions.SelectionColor = selectionColor;

            undoRedoManager = new UndoRedoManager();

            splitContainerControl1.SplitterPosition = splitContainerControl1.Height / 12 * 4;

            repositoryItemCheckedComboBoxEdit1.Appearance.ForeColor = color1;
            repositoryItemCheckedComboBoxEdit2.Appearance.ForeColor = color2;
            repositoryItemCheckedComboBoxEdit3.Appearance.ForeColor = color3;

            barStaticItem2.Appearance.ForeColor = color1;
            barStaticItem3.Appearance.ForeColor = color2;
            barStaticItem4.Appearance.ForeColor = color3;
        }
        private void Form1_Activated(object sender, EventArgs e)
        {
            if (!formLoaded)
            {
                Application.DoEvents();
                formLoaded = true;

                LoadFromRegistry($@"{subkey}\FindAll1", Find1BarEditItem);
                LoadFromRegistry($@"{subkey}\FindAll2", Find2BarEditItem);
                LoadFromRegistry($@"{subkey}\FindAll3", Find3BarEditItem);


                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(subkey))
                {
                    if (key != null)
                    {
                        SetText(Convert.ToString(key.GetValue("Text")));
                    }
                }
            }
        }

        private void LoadFromRegistry(string registryPath, BarEditItem barEditItem)
        {
            var comboBox = ((RepositoryItemCheckedComboBoxEdit)barEditItem.Edit);
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryPath))
            {
                if (key != null)
                {
                    comboBox.Items.Clear();
                    foreach (string subKeyName in key.GetSubKeyNames())
                    {
                        using (RegistryKey itemKey = key.OpenSubKey(subKeyName))
                        {
                            if (itemKey != null)
                            {
                                string itemValue = (string)itemKey.GetValue("Item", string.Empty);
                                bool isChecked = (string)itemKey.GetValue("Checked", "False") == "True";

                                if (!string.IsNullOrEmpty(itemValue))
                                    comboBox.Items.Add(itemValue, isChecked);
                            }
                        }
                    }
                }
            }
            barEditItem.EditValue = comboBox.GetCheckedItems();
        }
        private void ResizeFormToScreenPercentage(double percentage)
        {
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
            int newWidth = (int)(screenBounds.Width * percentage);
            int newHeight = (int)(screenBounds.Height * percentage);
            this.Width = newWidth;
            this.Height = newHeight;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(
                (screenBounds.Width - this.Width) / 2,
                (screenBounds.Height - this.Height) / 2
            );
        }
        private void FindBarButtonItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            DevExpress.XtraSplashScreen.SplashScreenManager.ShowForm(this, typeof(DXWaitForm), true, true, false);

            FromMemoEdit.SuspendLayout();
            ToMemoEdit.SuspendLayout();

            AddItemToRepository(Convert.ToString(Find1BarEditItem.EditValue), repositoryItemCheckedComboBoxEdit1);
            AddItemToRepository(Convert.ToString(Find2BarEditItem.EditValue), repositoryItemCheckedComboBoxEdit2);
            AddItemToRepository(Convert.ToString(Find3BarEditItem.EditValue), repositoryItemCheckedComboBoxEdit3);

            SaveReg();

            var l = Convert.ToString(FromMemoEdit.EditValue)
                .Split(Environment.NewLine.ToCharArray())
                .Where(f => !string.IsNullOrEmpty(f))
                .ToArray();

            find1 = GetSelectedFilters(repositoryItemCheckedComboBoxEdit1);
            find2 = GetSelectedFilters(repositoryItemCheckedComboBoxEdit2);
            find3 = GetSelectedFilters(repositoryItemCheckedComboBoxEdit3);

            var filters = find1.Concat(find2).Concat(find3).ToList();

            if (filters.Count > 0)
                l = l.Where(f => filters.Any(filter => f.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)).ToArray();

            UpdateBarCaption(barStaticItem2, find1, l);
            UpdateBarCaption(barStaticItem3, find2, l);
            UpdateBarCaption(barStaticItem4, find3, l);

            l = FilterListByCheckItem(l, Contains1BarCheckItem, find1);
            l = FilterListByCheckItem(l, Contains2BarCheckItem, find2);
            l = FilterListByCheckItem(l, Contains3BarCheckItem, find3);

            FromMemoEdit.ResumeLayout();
            ToMemoEdit.ResumeLayout();
            ToMemoEdit.Text = string.Join("\n", l.Distinct());
            FromMemoEdit.Text = FromMemoEdit.Text;

            DevExpress.XtraSplashScreen.SplashScreenManager.CloseForm(false);
        }

        private void AddItemToRepository(string filter, RepositoryItemCheckedComboBoxEdit repository)
        {
            var list = filter.Split(',').Select(s=>s.Trim());
            foreach (var item in list)
            {
                if (!string.IsNullOrEmpty(item) && !repository.Items.Contains(item))
                    repository.Items.Add(item,true);

            }
        }

        private List<string> GetSelectedFilters(RepositoryItemCheckedComboBoxEdit repository)
        {
            return repository.Items
                .Where(f => f.CheckState == CheckState.Checked)
                .Select(s => (string)s.Value)
                .ToList();
        }

        private void UpdateBarCaption(BarStaticItem barItem, List<string> findTerms, string[] lines)
        {
            if (findTerms != null && findTerms.Any())
                barItem.Caption = string.Join(", ",
                    findTerms.Select(term =>
                        $"{term} ({lines.Count(f => f.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)} hits)"));
            else
                barItem.Caption = string.Empty;
        }
        private string[] FilterListByCheckItem(string[] list, BarCheckItem checkItem, List<string> terms)
        {
            if (checkItem.Checked && terms != null && terms.Any())
                return list.Where(f => terms.Any(term => f.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)).ToArray();
            return list;
        }

        private void ToMemoEdit_CustomHighlightText(object sender, DevExpress.XtraEditors.TextEditCustomHighlightTextEventArgs e)
        {
            if (find1 != null && find1.Any())
                foreach (var term in find1)
                {
                    e.HighlightWords(term, color1);
                }

            if (find2 != null && find2.Any())
                foreach (var term in find2)
                {
                    e.HighlightWords(term, color2);
                }

            if (find3 != null && find3.Any())
                foreach (var term in find3)
                {
                    e.HighlightWords(term, color3);
                }
        }
        private void SetText(string text)
        {
            DevExpress.XtraSplashScreen.SplashScreenManager.ShowForm(this, typeof(DXWaitForm), true, true, false);
            FromMemoEdit.SuspendLayout();
            FromMemoEdit.EditValue = text;
            FromMemoEdit.ResumeLayout();
            DevExpress.XtraSplashScreen.SplashScreenManager.CloseForm(false);
        }
        private void FromMemoEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                SetText(Clipboard.GetText());
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            if (e.Control && e.KeyCode == Keys.Z)
            {
                undoRedoManager.Undo(FromMemoEdit);
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.Y)
            {
                undoRedoManager.Redo(FromMemoEdit);
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }
        private void ToMemoEdit_DoubleClick(object sender, EventArgs e)
        {
            MemoEdit memoEdit = sender as MemoEdit;
            int charIndex = memoEdit.SelectionStart;
            int lineIndex = memoEdit.GetLineFromCharIndex(charIndex);
            string searchText = memoEdit.Lines[lineIndex];
            SelectRow(FromMemoEdit, searchText);
        }
        private void SelectRow(MemoEdit sender, string searchText)
        {
            MemoEdit memoEdit = sender as MemoEdit;

            memoEdit.SuspendLayout();
            int startIndex = sender.Text.IndexOf(searchText);

            if (startIndex != -1)
            {
                memoEdit.SelectionStart = startIndex;
                memoEdit.SelectionLength = 1;
                memoEdit.ScrollToCaret(); 
            }

            memoEdit.ResumeLayout();
        }
        private void FromMemoEdit_TextChanged(object sender, EventArgs e)
        {
            if (!undoRedoManager.IsUndoOrRedo)
            {
                undoRedoManager.AddState(Convert.ToString(FromMemoEdit.EditValue));
            }
        }
        private void Clear2BarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            ToMemoEdit.Clear();
        }
        private void UpBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            ToMemoEdit.SelectionStart = 0;
            ToMemoEdit.ScrollToCaret();
            ToMemoEdit.Focus();
        }
        private void DownBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            var last = ToMemoEdit.Text.LastIndexOf(Environment.NewLine) + 2;
            if (last > ToMemoEdit.Text.Length)
                last = ToMemoEdit.Text.Length;

            ToMemoEdit.SelectionStart = last;
            ToMemoEdit.ScrollToCaret();
            ToMemoEdit.Focus();
        }
        private void Clear1BarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            FromMemoEdit.Clear();
            SaveReg();
        }
        private void SaveReg()
        {
            SaveToRegistry($@"{subkey}\FindAll1", Find1BarEditItem);
            SaveToRegistry($@"{subkey}\FindAll2", Find2BarEditItem);
            SaveToRegistry($@"{subkey}\FindAll3", Find3BarEditItem);

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(subkey))
            {
                key.SetValue("Text", Convert.ToString(FromMemoEdit.EditValue), RegistryValueKind.String);
            }
        }

        private void SaveToRegistry(string registryPath, BarEditItem barEditItem)
        {
            var comboBox = ((RepositoryItemCheckedComboBoxEdit)barEditItem.Edit);
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(registryPath))
            {
                if (key != null)
                {
                    foreach (string subKeyName in key.GetSubKeyNames())
                    {
                        key.DeleteSubKeyTree(subKeyName);
                    }
                    for (int i = 0; i < comboBox.Items.Count; i++)
                    {
                        CheckedListBoxItem item = comboBox.Items[i];
                        using (RegistryKey itemKey = key.CreateSubKey($"Item{i}"))
                        {
                            itemKey.SetValue("Item", item.Value.ToString());
                            itemKey.SetValue("Checked", item.CheckState == CheckState.Checked ? "True" : "False");
                        }
                    }
                }
            }
        }

        private void UndoBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            undoRedoManager.Undo(FromMemoEdit);
        }
        private void RedoBarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            undoRedoManager.Redo(FromMemoEdit);
        }
        private void repositoryItemButtonEdit1_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            if (e.Button.Kind == DevExpress.XtraEditors.Controls.ButtonPredefines.Search)
            {
                string searchText = ((DevExpress.XtraEditors.ButtonEdit)sender).Text;
                if (!string.IsNullOrEmpty(searchText))
                {
                    searchStartIndex = FindAndHighlightText(FromMemoEdit.Text, searchText, searchStartIndex);
                    if (searchStartIndex == -1)
                        searchStartIndex = FindAndHighlightText(FromMemoEdit.Text, searchText, 0);
                }
            }
            else
            {
                ((DevExpress.XtraEditors.ButtonEdit)sender).EditValue = string.Empty;
                SaveReg();
            }

        }
        private int FindAndHighlightText(string memoText, string searchText, int startIndex)
        {
            if (startIndex < 0) startIndex = 0;
            int index = memoText.IndexOf(searchText, startIndex, StringComparison.OrdinalIgnoreCase);
            if (index != -1)
            {
                FromMemoEdit.SelectionStart = index;
                FromMemoEdit.SelectionLength = searchText.Length;
                FromMemoEdit.ScrollToCaret();
                FromMemoEdit.Focus();
                return index + searchText.Length;
            }
            return -1;
        }

        private void repositoryItemCheckedComboBoxEdit1_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button.Index == 1)
            {
                ((CheckedComboBoxEdit)sender).Clear();
                ((CheckedComboBoxEdit)sender).Properties.Items.Clear();
            }
        }
    }
}
