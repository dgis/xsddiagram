//    XSDDiagram - A XML Schema Definition file viewer
//    Copyright (C) 2006-2019  Regis COSNIER
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
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using XSDDiagram.Rendering;
using System.Threading;
using System.Globalization;
using XSDDiagram;

namespace XSDDiagramConsole
{
	public static class Program
	{
		static string usage = @"XSD Diagram, version {0}
Usage: {1} [-o output.svg] [-os EXTENSION] [-r RootElement[@namespace]]* [-e N] [-d] [-c] [-z N] [-f PATH,NAME,TYPE,NAMESPACE,COMMENT,SEQ,LASTCHILD,XSDTYPE] [-a] [-y] [-u USERNAME] [-p PASSWORD] [file.xsd or URL]

-o FILE
	specifies the output image. '.png','.jpg', '.svg', '.txt', '.csv' ('.emf' on Windows) are allowed.
	If not present, the GUI is shown.
-os EXTENSION
	specifies the output image is streamed through the standard
	output. EXTENSION can be: png, jpg, svg, txt, csv.
	If not present, the GUI is shown.
-r ELEMENT
	specifies the root element of the tree.
	You can put several -r options = several root elements in the tree.
    The element can have a namespace: MyElement@http://mynamespace/path
-e N
	specifies the expand level (from 0 to what you want).
	Be carefull, the result image can be huge.
-d
	Display the documentation.
-c
	Draw a compact diagram.
-z N
	specifies the zoom percentage from 10% to 1000% (only for .png image).
	Work only with the '-o', '-os png' or '-os jpg' option.
-f PATH,NAME,TYPE,NAMESPACE,COMMENT,SEQ,LASTCHILD,XSDTYPE
	specifies the fields you want to output when rendering to a txt or csv file.
-a
	outputs the attributes in text mode only (.txt and .csv).
-y
	force huge image generation without user prompt.
-u USERNAME
	specifies a username to authenticate when a xsd dependency
	(import or include) is a secured url.
-p PASSWORD
	specifies a password to authenticate when a xsd dependency
	(import or include) is a secured url.

Example 1:
> XSDDiagramConsole.exe -o file.png -r TotoRoot -r TotoComplexType@http://mynamespace -e 3 -d -z 200 ./folder1/toto.xsd
	will generate a PNG image from a diagram with a root elements
	'TotoRoot' and 'TotoComplexType', and expanding the tree
	from the root until the 3rd level, with the documentation.

Example 2:
> XSDDiagram.exe ./folder1/toto.xsd
	will load the xsd file in the GUI window.

Example 3:
> XSDDiagram.exe -r TotoRoot -e 2 ./folder1/toto.xsd
	will load the xsd file in the GUI window with a root element
	'TotoRoot' and expanding the tree from the root until the 2nd level.

Example 4:
> XSDDiagramConsole.exe -os svg -r TotoRoot -e 3 ./folder1/toto.xsd
	will write a SVG image in the standard output from a diagram with a root element
	'TotoRoot' and expanding the tree from the root until the 3rd level.

Example 5:
> XSDDiagramConsole.exe -os txt -r TotoRoot -e 3 -f PATH,TYPE,COMMENT -a ./folder1/toto.xsd
	will write a textual representation in the standard output from a diagram with a root element
	'TotoRoot' and expanding the tree from the root until the 3rd level.
";

        public static void Execute(Options options)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            var l = new Logger(options);

            bool streamToOutput = !string.IsNullOrEmpty(options.OutputFile) || options.OutputOnStdOut;

            if (options.RequestHelp || string.IsNullOrEmpty(options.InputFile) || !streamToOutput ||
                options.RootElements.Count == 0 || options.ExpandLevel < 0 || options.Zoom < 10.0 || options.Zoom > 1000.0)
            {
                string version = typeof(Program).Assembly.GetName().Version.ToString();
                l.Log(usage, version, Path.GetFileName(Environment.GetCommandLineArgs()[0]));

                return;
            }

            l.Log("Loading the file: {0}\n", options.InputFile);

            Schema schema = new Schema();
            schema.RequestCredential += delegate (string url, string realm, int attemptCount, out string username, out string password)
            {
                username = password = "";
                if (!string.IsNullOrEmpty(options.Username))
                {
                    if (attemptCount > 1)
                        return false;
                    username = options.Username;
                    password = options.Password;
                    return true;
                }
                return false;
            };

            schema.LoadSchema(options.InputFile);

            if (schema.LoadError.Count > 0)
            {
                l.LogError("There are errors while loading:\n");
                foreach (var error in schema.LoadError)
                {
                    l.LogError(error);
                }
                l.LogError("\r\n");
            }

            Diagram diagram = new Diagram(schema);
            diagram.ShowDocumentation = options.ShowDocumentation;
            diagram.Scale = options.Zoom / 100.0f;
            diagram.CompactLayoutDensity = true;

            foreach (var rootElement in options.RootElements)
            {
                string elementName = rootElement;
                string elementNamespace = null;
                if (!string.IsNullOrEmpty(elementName))
                {
                    var pos = rootElement.IndexOf("@");
                    if (pos != -1)
                    {
                        elementName = rootElement.Substring(0, pos);
                        elementNamespace = rootElement.Substring(pos + 1);
                    }
                }

                foreach (var element in schema.Elements)
                {
                    if ((elementNamespace != null && elementNamespace == element.NameSpace && element.Name == elementName) ||
                        (elementNamespace == null && element.Name == elementName))
                    {
                        l.Log("Adding '{0}' element to the diagram...\n", rootElement);
                        diagram.Add(element.Tag, element.NameSpace);
                    }
                }
            }
            Form form = new Form();
            Graphics graphics = form.CreateGraphics();
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            for (int i = 0; i < options.ExpandLevel; i++)
            {
                l.Log("Expanding to level {0}...\n", i + 1);
                if (!diagram.ExpandOneLevel())
                {
                    l.Log("Cannot expand more.\n");
                    break;
                }
            }
            diagram.Layout(graphics);
            l.Log("Saving image...\n");
            try
            {
                bool result = false;

                DiagramExporter exporter = new DiagramExporter(diagram);
                IDictionary<string, object> specificRendererParameters = new Dictionary<string, object>()
                        {
                            { "TextOutputFields", options.TextOutputFields },
                            { "DisplayAttributes", options.DisplayAttributes },
                            { "Schema", schema }
                            //For future parameters, {}
                        };
                if (options.OutputOnStdOut)
                {
                    Stream stream = Console.OpenStandardOutput();
                    result = exporter.Export(stream, "." + options.OutputOnStdOutExtension.ToLower(), graphics, new DiagramAlertHandler(ByPassSaveAlert), specificRendererParameters);
                    stream.Flush();
                }
                else
                {
                    result = exporter.Export(options.OutputFile, graphics, new DiagramAlertHandler(ByPassSaveAlert), specificRendererParameters);
                }

                if (result)
                    l.Log("The diagram is now saved in the file: {0}\n", options.OutputFile);
                else
                    l.LogError("ERROR: The diagram has not been saved!\n");
            }
            catch (Exception ex)
            {
                l.LogError("ERROR: The diagram has not been saved. {0}\n", ex.Message);
            }

            graphics.Dispose();
            form.Dispose();
        }

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		public static void Main()
		{
            Execute(new Options(Environment.GetCommandLineArgs()));
        }

		static bool ByPassSaveAlert(string title, string message)
		{
			return true;
		}
    }
}