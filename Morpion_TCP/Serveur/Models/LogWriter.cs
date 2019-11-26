using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serveur.Models
{
    public class LogWriter
    {
        public readonly string LogFile;
        private readonly object logLock;

        public LogWriter(string logFile)
        {
            LogFile = logFile;
            logLock = new object();
        }


        public void Write(string log)
        {
            DateTime localDate = DateTime.Now;
            string logDate = localDate.ToString("s");

            lock (logLock)
            {
                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(LogFile, true))
                    {
                        file.WriteLine(logDate + " " + log);
                    }
            }
        }

    }
}
