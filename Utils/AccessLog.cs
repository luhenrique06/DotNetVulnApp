using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace brokenaccesscontrol.Utils
{
    public class AccessLog
    {
        public static string _appLogPath = "logs/Access.log";

        // A09 - Logging & Alerting Failures: entrada não sanitizada -> log injection
        // (CRLF permite forjar linhas). Além disso, senhas/PAN chegam aqui em claro.
        public static void writeLine(string level, string message){
            var line = $"[{DateTime.Now}] [{level}] {message}" + Environment.NewLine;
            File.AppendAllText(_appLogPath, line);
        }

        public static void Debug(string message){
            writeLine("DEBUG", message);
        }

        public static void Info(string message){
            writeLine("INFO", message);
        }

        public static void Error(string message){
            writeLine("ERROR", message);
        }

        public static void Warning(string message){
            writeLine("WARNING", message);
        }
    }
}