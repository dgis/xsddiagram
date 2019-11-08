using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using XSDDiagram.Properties;

namespace XSDDiagram
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            this.propertyGrid1.SelectedObject = Settings.Default;
        }

        private void SettingsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.propertyGrid1.SelectedObject = null;
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            MainForm.Form.ChangeSetting(e.ChangedItem.PropertyDescriptor.Name);
        }
    }
}
