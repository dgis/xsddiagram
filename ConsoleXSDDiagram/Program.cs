using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XSDDiagram;
using System.Drawing;
using System.Windows.Forms;

namespace ConsoleXSDDiagram
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> arguments = new List<string>();
            // Convert "--option:params" or "/option params" to "-option params"
            foreach (var argument in args)
	        {
                string command = null;
                if (argument.StartsWith("/") || argument.StartsWith("-"))
                    command = argument.Substring(1);
                else if (argument.StartsWith("--"))
                    command = argument.Substring(2);
                if (!string.IsNullOrEmpty(command))
                {
                    int indexOfColon = command.IndexOf(';');
                    if (indexOfColon > 0)
                    {
                        string parameter = command.Substring(indexOfColon + 1);
                        command = command.Substring(0, indexOfColon);
                        arguments.Add("-" + command);
                        arguments.Add(parameter);
                    }
                    else
                        arguments.Add("-" + command);
                }
                else
                    arguments.Add(argument);
            }

            string inputFile = null;
            string outputFile = null;
            List<string> rootElements = new List<string>();
            int expandLevel = -1;
            float zoom = 100.0f;

            int currentArgument = 0;
            while (currentArgument < arguments.Count)
            {
                string argument = arguments[currentArgument++];
                if (string.Compare("-i", argument, true) == 0)
                {
                    if (currentArgument < arguments.Count)
                        inputFile = args[currentArgument++];
                }
                else if (string.Compare("-o", argument, true) == 0)
                {
                    if (currentArgument < arguments.Count)
                        outputFile = args[currentArgument++];
                }
                else if (string.Compare("-r", argument, true) == 0)
                {
                    if (currentArgument < arguments.Count)
                        rootElements.Add(args[currentArgument++]);
                }
                else if (string.Compare("-e", argument, true) == 0)
                {
                    if (currentArgument < arguments.Count)
                    {
                        try
                        {
                            expandLevel = int.Parse(args[currentArgument++]);
                        }
                        catch { }
                    }
                }
                else if (string.Compare("-z", argument, true) == 0)
                {
                    if (currentArgument < arguments.Count)
                    {
                        try
                        {
                            zoom = (float)int.Parse(args[currentArgument++]);
                        }
                        catch { }
                    }
                }

            }

            if (string.IsNullOrEmpty(inputFile) || string.IsNullOrEmpty(outputFile) || rootElements.Count == 0 || expandLevel < 0 || zoom < 10.0 || zoom > 1000.0)
            {
                Console.WriteLine("Usage: > XSDDiagramConsole.exe -i file.xsd -o output.svg -r RootElement -e N [-z N]");
                Console.WriteLine("\t-i specifies the input XSD file.");
                Console.WriteLine("\t-o specifies the output image. Only '.svg' or '.png' are allowed.");
                Console.WriteLine("\t-r specifies the root element of the tree (You can put several -r option = several root element in the tree).");
                Console.WriteLine("\t-e specifies the expand level (from 0 to what you want).");
                Console.WriteLine("\t-z specifies the zoom percentage (from 10% to 1000%).");
                Console.WriteLine("Example: > XSDDiagramConsole.exe -i ./folder1/toto.xsd -o toto-diagram.svg -r TotoRoot -e 5 -z 200");
                Console.WriteLine("\twill generate a SVG image from a diagram with a root element 'TotoRoot' and expanding the tree from the root until the 5th level.");

                return;
            }

            Console.WriteLine("Loading the file: {0}", inputFile);

            Schema schema = new Schema();
            schema.LoadSchema(inputFile);

            if (schema.LoadError.Count > 0)
            {
                Console.WriteLine("There are errors while loading:");
                foreach (var error in schema.LoadError)
                {
                    Console.WriteLine(error);
                }
            }

            Diagram diagram = new Diagram();
            diagram.RequestAnyElement += new Diagram.RequestAnyElementEventHandler(diagram_RequestAnyElement);
            diagram.Scale = zoom / 100.0f;

            foreach (var element in schema.Elements)
            {
                foreach (var rootElement in rootElements)
                {
                    if (element.Name == rootElement)
                    {
                        diagram.Add(element.Tag, element.NameSpace);
                    }
                }
            }
            Form form = new Form();
            Graphics graphics = form.CreateGraphics();
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            for (int i = 0; i < expandLevel; i++)
            {
                diagram.ExpandOneLevel();
                diagram.Layout(graphics);
            }
            diagram.Layout(graphics);
            diagram.SaveToImage(outputFile, graphics, new Diagram.AlerteDelegate(SaveAlert));
            graphics.Dispose();
            form.Dispose();
        }

        static void diagram_RequestAnyElement(DiagramBase diagramElement, out XMLSchema.element element, out string nameSpace)
        {
            element = null;
            nameSpace = "";
        }

        static bool SaveAlert(string title, string message)
        {
            Console.WriteLine(string.Format("{0}. {1} [Yn]", title, message));
            ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(false);
            return consoleKeyInfo.Key == ConsoleKey.Y;
        }
    }
}
