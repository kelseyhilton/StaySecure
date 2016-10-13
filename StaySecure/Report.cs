﻿using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using System.Windows.Forms;
using System.IO;
using OpenQA.Selenium.Interactions;
using System.Net;

namespace StaySecure
{
    public class Report
    {
        public Report(string url) {
            string sql = "insert into Reports (CreatedDate, Url) values ("+ DateTime.Now +"'" + url + "')";
            //OperateOnDb(sql);
        }
        public Guid Id { get; set; }
        public string Url { get; set; }
        public int NumVulnerabilitiesDetected { get; set; }
        public List<Vulnerability> Vulnerabilities { get; set; }
        public string fileLocation { get; set; }

        private void OperateOnDb(string sqlCommandText) {
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

        public void GenerateReport(string url)
        {
            HelperFunctions.ClearTxtFile("");//eventually you will want to remove the empty string
            //assumptions, that the html is correct and able to be parsed

            if (IsUserUrlValid(url)) {
                //create report
                InjectQuery(url);
            }
            else { return; }
            //else indicate to user that their website has failed and ask them to enter a different url
        }

        public bool IsUserUrlValid(string url)
        {
            //assumption: the user enters a valid url
            url = FixUserAssumptions(url);
            DirectoryInfo directory = new DirectoryInfo(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\TestProgramOperations.txt")));
            string fileLocation = directory.ToString();

            Uri uriResult;
            bool isValid = Uri.IsWellFormedUriString(url, UriKind.Absolute);
            if (isValid)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    isValid = false;
                    HelperFunctions.WriteSingleLineToTxtFile("Server returned Bad Request for: " + url, fileLocation);
                }
                else if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    HelperFunctions.WriteSingleLineToTxtFile("Potential vulnerability at " + url + ". Returned Internal Server Error", fileLocation);
                }
                response.Close();
            }
            else { HelperFunctions.WriteSingleLineToTxtFile("That url is not valid.", fileLocation); }      
            return isValid;
        }

        public string FixUserAssumptions(string url)
        {
            url.Replace(" ", "");
            if(url.IndexOf("www.") < 0) {
                url = "http://www." + url;
            }
            else if (url.IndexOf("www.") > -1 && (url.IndexOf("https") < 0 || url.IndexOf("http") < 0)) {
                url = "http://" + url;
            }
            return url;
        }
        
        public void InjectQuery(string url)
        {
            HelperFunctions helper = new HelperFunctions();
            List<string> listInputFieldNames = new List<string>();
            List<string> invalidInputs = new List<string>() {
                "'",//single quote
                "''",//two single quotes
                "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"//overflow
             };

            IWebDriver driver = new PhantomJSDriver();
            driver.Url = url;
            driver.Navigate();
            string html = driver.PageSource;
            //parse inputs and come up with submit values            
            ///----------- I'm moving this all here because I don't want to open two instances of driver
            HtmlAgilityPack.HtmlDocument htmldoc = new HtmlAgilityPack.HtmlDocument();
            htmldoc.LoadHtml(html);
            if (htmldoc.DocumentNode != null)
            {
                var inputNodes = htmldoc.DocumentNode.SelectNodes("//input");
                if (inputNodes != null)               
                {
                    foreach (var node in inputNodes)
                    {
                        var name = node.Attributes["name"] != null ? node.Attributes["name"].Value : "";
                        if (node.Attributes["type"] == null || node.Attributes["type"].Value == "" || node.Attributes["type"].Value == "text")//I really want this to display if type = text
                            listInputFieldNames.Add(name);
                    }
                }
            }
            ///--------------
            if(listInputFieldNames.Count == 0)
            {
                HelperFunctions.WriteSingleLineToTxtFile("There are no visible inputs at that url.", fileLocation);
            }
            foreach (var name in listInputFieldNames)
            {             
                foreach (var input in invalidInputs)
                {
                    //check that the elements exist before the "sendkeys"
                    IWebElement element = driver.FindElement(By.Name(name));
                    if (element != null)
                    {
                        HelperFunctions.WriteSingleLineToTxtFile("Invalid input: " + input, "");
                        try
                        {
                            element.Clear();
                            Actions actions = new Actions(driver);

                            actions.MoveToElement(element).Click().Perform();
                            //exception:element is not selectable
                            element.SendKeys(input);
                            element.Submit();//--this is going to be an issue because the page will have been submitted....But I don't want to create the driver everytime
                            string pageAfterSubmit = driver.PageSource;
                            //parse for vulnerabilities
                            CheckForVulnerability(pageAfterSubmit);
                        }
                        catch (Exception e) {
                            HelperFunctions.WriteSingleLineToTxtFile("ERROR: " + e, "");
                        } 
                        //reset to original page
                        driver.Url = url;
                        driver.Navigate();
                    }
                    else
                    {
                        HelperFunctions.WriteSingleLineToTxtFile("No input fields found.", "");
                    }
                }
            }
            driver.Dispose();
        }

        public void CheckForVulnerability(string html)
        {
            HelperFunctions helper = new HelperFunctions();
            List<string> ErrorKeywords = new List<string>() {
                "error",
                "incorrect syntax",
                "server",
                "privlege",
                "identity",
                "select",
                "insert",
                "update",
                "datetime",
                "Arithmetic overflow",
                "statement",
               // "invalid",
                "column",
                "conversion failed",
                //"table", -table is going to show up in most html markup
                "cast", 
                "convert",
                "fails",
                "runtime",
                "null",
                "sql",
                "iis",
                "microsoft"
            };
            var markup = html;
            List<string> potentialVulnerability = new List<string>();
            foreach(var keyword in ErrorKeywords)
            {
                if(markup.IndexOf(keyword) > -1)
                {
                    //save value to database
                    potentialVulnerability.Add(keyword);
                    HelperFunctions.WriteSingleLineToTxtFile("Keyword found: "  + keyword, "");
                }
            }
            foreach(var vulnerability in potentialVulnerability)
            {              
                //helper.OperateOnDb("insert into vulnerabilities(reportId, description) values (1, 'keyword found: " + vulnerability + "')");//reportId 1 is a dummy variable
            }
        }

    }


    public class HelperFunctions
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

            if (File.Exists(fileLocation)){
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
    }
}
