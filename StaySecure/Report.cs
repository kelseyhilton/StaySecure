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
    public class Report
    {
        public Report(string url)
        {
            string sql = "insert into Reports (CreatedDate, Url) values (" + DateTime.Now + "'" + url + "')";
            //OperateOnDb(sql);
        }
        public Guid Id { get; set; }
        public string Url { get; set; }
        public int NumVulnerabilitiesDetected { get; set; }
        public List<Vulnerability> Vulnerabilities { get; set; }
        public string fileLocation { get; set; }

        private void OperateOnDb(string sqlCommandText)
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

        public void GenerateReport(string origUrl)
        {
            HelperFunctions.ClearTxtFile("");//eventually you will want to remove the empty string
            //assumptions, that the html is correct and able to be parsed
            IWebDriver browser = new PhantomJSDriver();
            //if (WebService.IsUserUrlValid(origUrl)) {

            //Test server request/response
            bool isResponseValid = WebService.IsServerResponseOK(origUrl);
            if (isResponseValid)
            {
                //get list of pre-existing keywords
                string originalPage = browser.PageSource;
                //parse for vulnerabilities
                CheckForVulnerability(originalPage);
                List<string> preexistingKeywords = CheckForExistingKeywords(originalPage);
                //create report
                InjectQuery(origUrl, browser);
            }
            //List<string> sitemapUrls = AnalyzeSiteMap(origUrl, browser);
            //int index = 0;
            //foreach(var url in sitemapUrls)
            //{
            //    if (WebService.IsUserUrlValid(url))
            //    {
            //        InjectQuery(url, browser);
            //    }
            //    index++;
            //    if (index == 3)
            //        break;
            //}
            browser.Dispose();
        }

       
        public void InjectQuery(string url, IWebDriver browser)
        {
            HelperFunctions helper = new HelperFunctions();
            List<string> listInputFieldNames = new List<string>();
            List<string> testCases = new List<string>() {
                "'",//single quote
                "''",//two single quotes
                "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",//overflow
                "trial"//valid input
             };

            browser.Url = url;
            browser.Navigate();

            string html = browser.PageSource;
            //get keywords in original html

            CheckForVulnerability(html);
            //parse inputs and come up with submit values   
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
                        if (name != "" && (node.Attributes["type"] == null || node.Attributes["type"].Value == "" ||
                            node.Attributes["type"].Value == "text" || node.Attributes["type"].Value == "password" ||
                            node.Attributes["type"].Value == "search" || node.Attributes["type"].Value == "email"))//I really want this to display if type = text
                            listInputFieldNames.Add(name);
                    }
                }
            }
            ///--------------
            if (listInputFieldNames.Count == 0)
            {
                HelperFunctions.WriteSingleLineToTxtFile("There are no visible inputs at that url.", fileLocation);
            }
            foreach (var name in listInputFieldNames)
            {
                foreach (var input in testCases)
                {
                    //check that the elements exist before the "sendkeys"
                    try
                    {
                        IWebElement element = browser.FindElement(By.Name(name));
                        if (element != null)
                        {
                            HelperFunctions.WriteSingleLineToTxtFile("Element name: " + element.GetAttribute("name"), fileLocation);
                            HelperFunctions.WriteSingleLineToTxtFile("Invalid input: " + input, "");
                            try
                            {
                                element.Clear();
                                Actions actions = new Actions(browser);

                                actions.MoveToElement(element).Click().Perform();
                                //exception:element is not selectable
                                element.SendKeys(input);
                                element.Submit();
                                string pageAfterSubmit = browser.PageSource;
                                //parse for vulnerabilities
                                CheckForVulnerability(pageAfterSubmit);
                            }
                            catch (Exception e)
                            {
                                string Id = element.GetAttribute("id");
                                if (e.HResult == -2146233088)//element not visible to phantom js, run js to expose
                                {
                                    string jsInput = input;
                                    if (input == "'")
                                    {
                                        jsInput = "&#39;";
                                    }
                                    if (input == "''")
                                    {
                                        jsInput = "/'/'";
                                    }
                                    IJavaScriptExecutor js = browser as IJavaScriptExecutor;
                                    js.ExecuteScript("document.getElementById('" + Id + "').setAttribute('value', '" + jsInput + "')", element);
                                    try
                                    {
                                        element.Submit();
                                        string pageAfterSubmit = browser.PageSource;
                                        //parse for vulnerabilities
                                        CheckForVulnerability(pageAfterSubmit);
                                    }
                                    catch (Exception exception)
                                    {
                                        HelperFunctions.WriteSingleLineToTxtFile("ERROR: " + exception, "");
                                    }
                                }
                                else
                                {
                                    HelperFunctions.WriteSingleLineToTxtFile("ERROR: " + e, "");
                                }
                            }
                            //reset to original page
                            browser.Url = url;
                            browser.Navigate();
                        }
                        else
                        {
                            HelperFunctions.WriteSingleLineToTxtFile("No input fields found.", "");
                        }
                    }
                    catch (Exception e)
                    {
                        HelperFunctions.WriteSingleLineToTxtFile("ERROR: " + e, "");
                    }
                }
                return;
            }
            //assuming a browser can be passed around, the dispose is being taken care of in the main function. that means I want to make it back there no matter what. 
            //it may be safer to create local instances.
            //driver.Dispose();
        }

        public List<string> AnalyzeSiteMap(string origUrl, IWebDriver browser)
        {
            List<string> sitemapUrls = new List<string>();
            System.Text.StringBuilder sitemapUrl = new System.Text.StringBuilder(); //come up with a different initializer
            bool validUrl = true;

            //IWebDriver browser = new PhantomJSDriver();
            //Check for valid url --> different function
            //append /sitemap.xml to .com
            if (origUrl.EndsWith(".com"))
            {
                sitemapUrl.Append(origUrl);
                sitemapUrl.Append("/sitemap.xml");
                //check if valid xml
                //if (WebService.IsUserUrlValid(origUrl))
                if (WebService.IsUserUrlValid(origUrl))
                {
                    browser.Url = sitemapUrl.ToString();
                    browser.Navigate();
                    string xml = browser.PageSource;

                    //does the server return a 404 or 500?
                    bool validXml = true;
                    //parse xml for urls
                    XmlDocument xmlDoc = new XmlDocument();
                    try
                    {
                        xmlDoc.LoadXml(xml);
                    }
                    catch { validXml = false; }

                    if (validXml)
                    {
                        HelperFunctions.WriteSingleLineToTxtFile("The sitemap for " + origUrl + "is visible to the user. This is a vulnerability.", fileLocation);
                        string xpath = "//*[contains(text(),'.com')]";//this is naive, but it has to be true...
                        var nodes = xmlDoc.SelectNodes(xpath);

                        HelperFunctions.WriteSingleLineToTxtFile("sitemap urls for " + origUrl, fileLocation);
                        foreach (XmlNode childrenNode in nodes)
                        {
                            //create list of all urls listed in site map
                            string nextUrl = childrenNode.InnerXml;
                            sitemapUrls.Add(nextUrl);
                            HelperFunctions.WriteSingleLineToTxtFile(nextUrl, fileLocation);//add url findings to report                           
                        }
                    }
                }
            }
            browser.Close();
            return sitemapUrls;
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
            foreach (var keyword in ErrorKeywords)
            {
                if (markup.IndexOf(keyword) > -1)
                {
                    //save value to database
                    potentialVulnerability.Add(keyword);
                    HelperFunctions.WriteSingleLineToTxtFile("Keyword found: " + keyword, "");
                }
            }
            foreach (var vulnerability in potentialVulnerability)
            {
                //helper.OperateOnDb("insert into vulnerabilities(reportId, description) values (1, 'keyword found: " + vulnerability + "')");//reportId 1 is a dummy variable
            }
        }

        public List<string> CheckForExistingKeywords(string html)
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
            List<string> existingKeywords = new List<string>();
            HelperFunctions.WriteSingleLineToTxtFile("List of keywords on page before input injection:", fileLocation);

            foreach (var keyword in ErrorKeywords)
            {
                if (markup.IndexOf(keyword) > -1)
                {
                    //save value to database
                    existingKeywords.Add(keyword);
                    HelperFunctions.WriteSingleLineToTxtFile(keyword, fileLocation);

                }
            }
            HelperFunctions.WriteSingleLineToTxtFile("---", fileLocation);
            return existingKeywords;
        }

    }
}
