using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;


namespace StaySecure
{
    public class Report
    {       
        public Report(string url) {
            string sql = "insert into Reports (Url, NumVulnerabilities) values ('" + url + "', 0)";
            //OperateOnDb(sql);
        }
        public Guid Id { get; set; }
        public string Url { get; set; }
        public int NumVulnerabilitiesDetected { get; set; }
        public List<Vulnerability> Vulnerabilities { get; set; }

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


        public void CreateReport(string url)  {
            //id, reportId, description, type           
            string sql = "insert into Reports (Url, NumVulnerabilities) values ('" + url + "', 0)";
            OperateOnDb(sql);
        }

        public void GenerateReport(string url)
        {
            //assumptions, that the html is correct and able to be parsed
            url = "https://google.com/";
            List<string> listInputIds = new List<string>();
            List<string> listInputNames = new List<string>();

            HtmlAgilityPack.HtmlDocument htmldoc = GetHtmlDocumentFromUrl(url);

            if (htmldoc.DocumentNode != null)
            {
                var inputNodes = htmldoc.DocumentNode.SelectNodes("//input");
                if (inputNodes == null){
                    listInputIds.Add("list of input nodes is null");
                }
                else{
                    foreach (var node in inputNodes) {
                        var name = node.Attributes["name"] != null ? node.Attributes["name"].Value : "";
                        listInputIds.Add("input:" + node.OuterHtml + " name: " + name);
                    }
                }
                HelperFunctions.WriteLinesToTxtFile(listInputIds.ToArray(), url);
            }
        }

        public HtmlAgilityPack.HtmlDocument GetHtmlDocumentFromUrl(string url)
        {
            List<string> listInputIds = new List<string>();
            IWebDriver driver = new PhantomJSDriver();
            driver.Url = url;
            driver.Navigate();

            string html = driver.PageSource;
            //parse inputs and come up with submit values

            //check that the elements exist before the "sendkeys"
            //driver.FindElement(By.XPath("//input[@name='username']")).SendKeys("");
            //driver.FindElement(By.XPath("//input[@name='password']")).SendKeys("");
            //driver.FindElement(By.XPath("//input[@name='login']")).Submit();
            //get response with 
            //string pageAfterSubmit = driver.PageSource;

            //parse for errors
            driver.Dispose();

            HtmlAgilityPack.HtmlDocument htmldoc = new HtmlAgilityPack.HtmlDocument();
            htmldoc.LoadHtml(html);

            return htmldoc;
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
       
        public static void WriteLinesToTxtFile(string[] lines, string Url)
        {
            string fileLocation = "TestProgramOperations.txt";
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileLocation))
            {
                foreach (string line in lines) {
                    file.WriteLine(line);
                }
            }
        }
    }
}
