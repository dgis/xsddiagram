//    XSDDiagram - A XML Schema Definition file viewer
//    Copyright (C) 2006  Regis COSNIER
//
//    This program is free software; you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation; either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program; if not, write to the Free Software
//    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace XSDDiagram
{
	public partial class DiagramControl : UserControl
	{
		private static int WM_SETFOCUS = 0x0007;

		public DiagramControl()
		{
			InitializeComponent();

			this.SetStyle(
			ControlStyles.UserPaint |
			ControlStyles.AllPaintingInWmPaint |
			ControlStyles.OptimizedDoubleBuffer, true);
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WM_SETFOCUS)
				return;
			base.WndProc(ref m);
		}
	}
}
