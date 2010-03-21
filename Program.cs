
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace XSDDiagram
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.ThreadException += HandleThreadException;
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}

		static void HandleThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			System.Diagnostics.Trace.WriteLine(e.ToString());
		}
	}
}