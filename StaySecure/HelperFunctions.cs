using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using System.Windows.Forms;
using System.IO;
using OpenQA.Selenium.Interactions;
using System.Net;
using System.Xml;
using System.Web;

namespace StaySecure
{
    class HelperFunctions
    {
        public static void ClearTxtFiles()
        {
            DirectoryInfo directory = new DirectoryInfo(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\TestProgramOperations.txt")));
            string fileLocation = directory.ToString();

            if (File.Exists(fileLocation))
            {
                File.WriteAllText(fileLocation, String.Empty);
            }

            directory = new DirectoryInfo(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\ErrorLog.txt")));
            fileLocation = directory.ToString();

            if (File.Exists(fileLocation))
            {
                File.WriteAllText(fileLocation, String.Empty);
            }
        }

        public static void WriteSingleLineToTxtFile(string line)
        {
            DirectoryInfo directory = new DirectoryInfo(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\TestProgramOperations.txt")));
            string fileLocation = directory.ToString();

            if (File.Exists(fileLocation))
            {
                using (System.IO.StreamWriter file = File.AppendText(fileLocation))
                {
                    file.WriteLine(line);
                }
            }
        }
        public static void AddToErrorLog(string errorText)
        {
            DirectoryInfo directory = new DirectoryInfo(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\ErrorLog.txt")));
            string fileLocation = directory.ToString();

            if (File.Exists(fileLocation))
            {
                using (System.IO.StreamWriter file = File.AppendText(fileLocation))
                {
                    file.WriteLine(errorText);
                }
            }
        }

        public static string FixUserAssumptions(string url)
        {
            url.Replace(" ", "");
            if (url.IndexOf("www.") < 0)
            {
                url = "http://www." + url;
            }
            else if (url.IndexOf("www.") > -1 && (url.IndexOf("https") < 0 || url.IndexOf("http") < 0))
            {
                url = "http://" + url;
            }
            return url;
        }
    }
}
