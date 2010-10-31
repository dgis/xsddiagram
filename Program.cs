
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Reflection;

namespace XSDDiagram
{
	static class Program
	{
        [DllImport("kernel32")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32")]
        static extern bool AllocConsole();

        static string usage = @"Usage: XSDDiagram.exe [-o output.svg] [-r RootElement]* [-e N] [-z N] [file.xsd]

-o specifies the output image. Only '.svg' or '.png' are allowed.
	If not present, the GUI is shown.
-r specifies the root element of the tree.
	You can put several -r option = several root elements in the tree.
-e specifies the expand level (from 0 to what you want).
	Be carefull, the result image can be huge.
-z specifies the zoom percentage from 10% to 1000% (only for .png image).
	Work only with the -o option.

Example 1:
> XSDDiagram.exe -o file.svg -r TotoRoot -e 3 -z 200 ./folder1/toto.xsd
	will generate a SVG image from a diagram with a root element
	'TotoRoot' and expanding the tree from the root until the 3rd level.

Example 2:
> XSDDiagramConsole.exe ./folder1/toto.xsd
	will load the xsd file in the GUI window.";

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			if (Options.RequestHelp || !string.IsNullOrEmpty(Options.OutputFile))
            {
                IntPtr hConsole = GetConsoleWindow();
                if (hConsole == IntPtr.Zero)
                {
                    bool result = AllocConsole();
                }

				if (Options.RequestHelp || string.IsNullOrEmpty(Options.InputFile) || string.IsNullOrEmpty(Options.OutputFile) ||
					Options.RootElements.Count == 0 || Options.ExpandLevel < 0 || Options.Zoom < 10.0 || Options.Zoom > 1000.0)
                {
					string version = typeof(Program).Assembly.GetName().Version.ToString();
					Console.WriteLine("XSD Diagram, version {0}\n{1}", version, usage);

                    return;
                }

				Console.WriteLine("Loading the file: {0}", Options.InputFile);

                Schema schema = new Schema();
				schema.LoadSchema(Options.InputFile);

                if (schema.LoadError.Count > 0)
                {
                    Console.WriteLine("There are errors while loading:");
                    foreach (var error in schema.LoadError)
                    {
                        Console.WriteLine(error);
                    }
                }

                Diagram diagram = new Diagram();
                diagram.ElementsByName = schema.ElementsByName;
				diagram.Scale = Options.Zoom / 100.0f;

				foreach (var rootElement in Options.RootElements)
				{
					foreach (var element in schema.Elements)
					{
                        if (element.Name == rootElement)
                        {
                            Console.WriteLine("Adding '{0}' element to the diagram...", rootElement);
                            diagram.Add(element.Tag, element.NameSpace);
                        }
                    }
                }
                Form form = new Form();
                Graphics graphics = form.CreateGraphics();
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

				for (int i = 0; i < Options.ExpandLevel; i++)
                {
                    Console.WriteLine("Expanding to level {0}...", i + 1);
                    diagram.ExpandOneLevel();
                }
                diagram.Layout(graphics);
                Console.WriteLine("Saving image...");
                try
                {
					if (diagram.SaveToImage(Options.OutputFile, graphics, new Diagram.AlerteDelegate(SaveAlert)))
						Console.WriteLine("The diagram is now saved in the file: {0}", Options.OutputFile);
                    else
                        Console.WriteLine("ERROR: The diagram has not been saved!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: The diagram has not been saved!");
                    Console.WriteLine(ex.ToString());
                }

                graphics.Dispose();
                form.Dispose();
            }
            else
            {
				if (Options.RequestHelp)
                    MessageBox.Show(usage);

                Application.ThreadException += HandleThreadException;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
        }

        static bool SaveAlert(string title, string message)
        {
            Console.Write(string.Format("{0}. {1} [Yn] > ", title, message));
            ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(false);
            Console.WriteLine("");
            Console.WriteLine("Ok, relax... It can take time!");
            return consoleKeyInfo.Key == ConsoleKey.Y || consoleKeyInfo.Key == ConsoleKey.Enter;
        }

		static void HandleThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			System.Diagnostics.Trace.WriteLine(e.ToString());
		}
	}
}