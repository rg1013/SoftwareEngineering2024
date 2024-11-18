/******************************************************************************
 * Filename    = FileClonerLogger.cs
 *
 * Author      = Neeraj Krishna N
 * 
 * Project     = FileCloner
 *
 * Description = A custom implementation of Logger class, which logs to a file
 *              named FileClonerModuleName.log in the log directory. Helps 
 *              developers in analysing any unforeseen error during the usage
 *              of the application
 *****************************************************************************/

using FileCloner.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FileCloner.FileClonerLogging
{
    public class FileClonerLogger
    {
        // base path defined by folder
        private static string _basePath = Constants.defaultFolderPath;
        private static readonly string _logDirectory = Path.Combine(_basePath, "FileClonerLogs");
        private string? _logFile;
        private string _moduleName;

        // lock to write to Log Files
        private object _syncLock = new();

        /// <summary>
        /// Creates a Log File with the name moduleName.log inside the _logDirectory
        /// </summary>
        /// <param name="moduleName"></param>
        public FileClonerLogger(string moduleName)
        {
            _moduleName = moduleName;
            _syncLock = new();

            try
            {
                if (!Directory.Exists(_logDirectory))
                {
                    Directory.CreateDirectory(_logDirectory);
                }
                _logFile = $"{_logDirectory}\\FileCloner{_moduleName}.log";
                if (!File.Exists(_logFile))
                {
                    File.Create(_logFile).Close();
                }
                lock (_syncLock)
                {
                    File.WriteAllText(_logFile, $"{_moduleName} : Logging Started\n");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message + "\n");
            }
        }

        /// <summary>
        /// API to be called to Log onto the file
        /// </summary>
        /// <param name="message"></param>
        /// <param name="memberName"></param>
        /// <param name="filePath"></param>
        /// <param name="lineNumber"></param>
        /// <param name="isErrorMessage"></param>
        public void Log(string message,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string filePath = "",
                [CallerLineNumber] int lineNumber = 0,
                bool isErrorMessage = false)
        {
            // string logToBeWritten = $"{_moduleName}:{filePath}->{memberName}->{lineNumber} :: {message}";
            string logToBeWritten = $"{Path.GetFileName(filePath)}->{memberName}->{lineNumber} :: {message}";
            if (isErrorMessage)
            {
                logToBeWritten = $"ERROR : {logToBeWritten}";
            }
            Write(logToBeWritten);
        }

        /// <summary>
        /// Function which writes to the log File
        /// </summary>
        /// <param name="logToBeWritten"></param>
        private void Write(string logToBeWritten)
        {
            if (_logFile != null)
            {
                lock (_syncLock)
                {
                    try
                    {
                        File.AppendAllText(_logFile, logToBeWritten + "\n");
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"FILECLONERTRACE : {ex.Message}\n");
                    }
                }
            }
        }


    }
}
