using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace XSDDiagram
{
	public partial class AboutForm : Form
	{
		public AboutForm()
		{
			InitializeComponent();

			this.richTextBox.Text = Properties.Resources.ReadMe;

			this.richTextBox.LinkClicked += new LinkClickedEventHandler(richTextBox_LinkClicked);
		}

		void richTextBox_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(e.LinkText);
		}
	}
}