using System;
using System.Collections.Generic;

namespace XSDDiagram
{
    public class Options
    {
		public static string[] Arguments { get; private set; }
		public static string InputFile { get; private set; }
		public static string OutputFile { get; private set; }
		public static bool OutputOnStdOut { get; private set; }
		public static string OutputOnStdOutExtension { get; private set; }
		public static IList<string> RootElements { get; private set; }
		public static int ExpandLevel { get; private set; }
		public static float Zoom { get; private set; }
		public static bool RequestHelp { get; private set; }
		public static bool IsRunningOnMono { get; private set; }

		static Options()
		{
			InputFile = null;
			OutputFile = null;
			OutputOnStdOut = false;
			OutputOnStdOutExtension = "png";
			RootElements = new List<string>();
			ExpandLevel = 0;
			Zoom = 100.0f;
			RequestHelp = false;
			IsRunningOnMono = Type.GetType("Mono.Runtime") != null;

			string[] args = Environment.GetCommandLineArgs();
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
			Arguments = arguments.ToArray();

			int currentArgument = 1;
			while (currentArgument < arguments.Count)
			{
				string argument = arguments[currentArgument++];
				if (string.Compare("-h", argument, true) == 0)
				{
					RequestHelp = true;
				}
				else if (string.Compare("-o", argument, true) == 0)
				{
					if (currentArgument < arguments.Count)
						OutputFile = args[currentArgument++];
				}
				else if (string.Compare("-os", argument, true) == 0)
				{
					OutputOnStdOut = true;
					if (currentArgument < arguments.Count)
						OutputOnStdOutExtension = args[currentArgument++];
				}
				else if (string.Compare("-r", argument, true) == 0)
				{
					if (currentArgument < arguments.Count)
						RootElements.Add(args[currentArgument++]);
				}
				else if (string.Compare("-e", argument, true) == 0)
				{
					if (currentArgument < arguments.Count)
					{
						try
						{
							ExpandLevel = int.Parse(args[currentArgument++]);
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
							Zoom = (float)int.Parse(args[currentArgument++]);
						}
						catch { }
					}
				}
				else
					InputFile = argument;
			}
		}
    }
}
