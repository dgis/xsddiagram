using System;

namespace XSDDiagramConsole
{
    internal class Logger
    {
        private bool OutputOnStdOut;
        public Logger(Options config)
        {
            OutputOnStdOut = config.OutputOnStdOut;
        }

        public void Log(string format, params object[] arg)
        {
            if (!OutputOnStdOut)
            {
                Console.Write(format, arg);
            }
        }
        public void LogError(string format, params object[] arg)
        {
            Console.Error.Write(format, arg);
        }

    }
}
