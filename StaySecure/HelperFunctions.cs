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
        public void OperateOnDb(string sqlCommandText)
        {
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(connectionString);

            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = sqlCommandText;
            cmd.Connection = connection;

            connection.Open();
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        public static void WriteLinesToTxtFile(string[] lines, string fileLocation)
        {
            DirectoryInfo directory = new DirectoryInfo(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\TestProgramOperations.txt")));
            fileLocation = directory.ToString();
            if (File.Exists(fileLocation))
            {
                using (System.IO.StreamWriter file = File.AppendText(fileLocation))
                {
                    file.WriteLine("--" + DateTime.Now.ToShortDateString() + "--");
                    foreach (string line in lines)
                    {
                        file.WriteLine(line);
                    }
                }
            }
        }

        public static void ClearTxtFile(string fileLocation)
        {
            DirectoryInfo directory = new DirectoryInfo(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\TestProgramOperations.txt")));
            fileLocation = directory.ToString();

            if (File.Exists(fileLocation))
            {
                File.WriteAllText(fileLocation, String.Empty);
            }
        }

        public static void WriteSingleLineToTxtFile(string line, string fileLocation)
        {
            DirectoryInfo directory = new DirectoryInfo(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\TestProgramOperations.txt")));
            fileLocation = directory.ToString();

            if (File.Exists(fileLocation))
            {
                using (System.IO.StreamWriter file = File.AppendText(fileLocation))
                {
                    //file.WriteLine("--" + DateTime.Now.ToShortDateString() + "--");
                    file.WriteLine(line);
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
