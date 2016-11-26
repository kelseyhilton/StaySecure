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
        public Report(string url) { }
        public Guid Id { get; set; }
        public string Url { get; set; }
        public int NumVulnerabilitiesDetected { get; set; }
        public List<Vulnerability> Vulnerabilities { get; set; }
        public string fileLocation { get; set; }

        public void GenerateReport(string origUrl)
        {
            HelperFunctions.ClearTxtFile("");
            //assumptions, that the html is correct and able to be parsed
            IWebDriver browser = new PhantomJSDriver();
            if (!WebService.IsUserUrlValid(origUrl)) {
                browser.Dispose(); 
                return;
            }
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
            browser.Dispose();
        }

       
        public void InjectQuery(string url, IWebDriver browser)
        {
            HelperFunctions helper = new HelperFunctions();
            List<string> listInputFieldNames = new List<string>();
            
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
                            HelperFunctions.WriteSingleLineToTxtFile("" , fileLocation);
                            HelperFunctions.WriteSingleLineToTxtFile("Element name: " + element.GetAttribute("name"), fileLocation);
                            HelperFunctions.WriteSingleLineToTxtFile("Invalid input: " + input, "");
                            try
                            {
                                element.Clear();
                                Actions actions = new Actions(browser);
                                actions.MoveToElement(element).Click().Perform();
                                
                                element.SendKeys(input);//depending on the visibility of the input field, this will throw an exception:element is not selectable
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
                                    //js.ExecuteScript("document.getElementById('" + Id + "').setAttribute('value', '" + jsInput + "')", element);
                                    js.ExecuteScript("document.getElementsByName('" + name + "')[0].setAttribute('value', '" + jsInput + "')", element);

                                    try {
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
        }

        public void CheckForVulnerability(string html)
        {
            HelperFunctions helper = new HelperFunctions();
           
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
        }

        public List<string> CheckForExistingKeywords(string html)
        {
            HelperFunctions helper = new HelperFunctions();
           
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

        List<string> ErrorKeywords = new List<string>() {
                "error", "incorrect syntax", "server", "privilege",
                "identity", "select", "insert","update", "datetime",
                "Arithmetic overflow", "statement", "column", "conversion failed",
                "cast", "convert", "fails", "runtime", "null", "sql", "iis", "microsoft"
            };
        List<string> validTestCases = new List<string>() {"valid"};
        List<string> testCases = new List<string>() {
                "'",//single quote
                "''",//two single quotes
                "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",//overflow
                ";",//syntax error
                "--", //comment
                "input' or 1 = 1", //boolean based bypass authentication
                "input' or 1 = 1--",
                "&apos; or 1 = 1",//hex-encoded boolean based
                "&apos; or 1 = 1--",
                "substring(@@version,1,1) = 5"//information phishing
             };

    }
}
