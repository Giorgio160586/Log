using DevExpress.LookAndFeel;
using DevExpress.XtraBars.Ribbon;
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

            repositoryItemTextEdit1.Appearance.ForeColor = DXSkinColors.FillColors.Question;
            repositoryItemTextEdit3.Appearance.ForeColor = DXSkinColors.FillColors.Warning;
            repositoryItemTextEdit4.Appearance.ForeColor = DXSkinColors.FillColors.Success;
       
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Log++"))
            {
                if (key != null)
                {
                    barEditItem1.EditValue = key.GetValue("FindAll1");
                    barEditItem3.EditValue = key.GetValue("FindAll2");
                    barEditItem4.EditValue = key.GetValue("FindAll3");
                    memoEdit1.EditValue = key.GetValue("Text");
                }
            }

        }
        private void barButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Log++"))
            {
                key.SetValue("FindAll1", Convert.ToString(barEditItem1.EditValue), RegistryValueKind.String);
                key.SetValue("FindAll2", Convert.ToString(barEditItem3.EditValue), RegistryValueKind.String);
                key.SetValue("FindAll3", Convert.ToString(barEditItem4.EditValue), RegistryValueKind.String);
                key.SetValue("Text", Convert.ToString(memoEdit1.EditValue), RegistryValueKind.String);
            }

            text = memoEdit1.Text;
            var l = memoEdit1.Text.Split('\n').ToArray();
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
        private void memoEdit1_CustomHighlightText(object sender, DevExpress.XtraEditors.TextEditCustomHighlightTextEventArgs e)
        {
            e.HighlightWords(Convert.ToString(barEditItem1.EditValue), DXSkinColors.FillColors.Question);
            e.HighlightWords(Convert.ToString(barEditItem3.EditValue), DXSkinColors.FillColors.Warning);
            e.HighlightWords(Convert.ToString(barEditItem4.EditValue), DXSkinColors.FillColors.Success);
        }

        private void barButtonItem2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            memoEdit1.Text = text;
        }
    }
}
