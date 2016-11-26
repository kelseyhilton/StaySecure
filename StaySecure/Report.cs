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
        public string Url { get; set; }

        public void GenerateReport(string origUrl)
        {
            HelperFunctions.ClearTxtFiles();
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
            List<Input> listInputFieldNames = new List<Input>();
            
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
                        if (name != "")
                        {
                            if (node.Attributes["type"] == null || node.Attributes["type"].Value == "") {
                                Input input = new Input(name, "");
                                listInputFieldNames.Add(input);
                            }
                            if (node.Attributes["type"].Value == "text")
                            {
                                Input input = new Input(name, "text");
                                listInputFieldNames.Add(input);
                            }
                            if (node.Attributes["type"].Value == "password")
                            {
                                Input input = new Input(name, "password");
                                listInputFieldNames.Add(input);
                            }

                            if (node.Attributes["type"].Value == "search")
                            {
                                Input input = new Input(name, "search");
                                listInputFieldNames.Add(input);
                            }
                            if (node.Attributes["type"].Value == "email")
                            {
                                Input input = new Input(name, "email");
                                listInputFieldNames.Add(input);
                            }
                           
                        }
                    }
                }
            }
            ///--------------
            if (listInputFieldNames.Count == 0)
            {
                HelperFunctions.WriteSingleLineToTxtFile("There are no visible inputs at that url.");
            }
            foreach (var name in listInputFieldNames)
            {
                foreach (var input in testCases)
                {
                    //check that the elements exist before the "sendkeys"
                    try
                    {
                        IWebElement element = browser.FindElement(By.Name(name.Name));
                        if (element != null)
                        {
                            HelperFunctions.WriteSingleLineToTxtFile("");
                            HelperFunctions.WriteSingleLineToTxtFile("Element name: " + element.GetAttribute("name"));
                            HelperFunctions.WriteSingleLineToTxtFile("Input: " + input.Name);
                            try
                            {
                                element.Clear();
                                Actions actions = new Actions(browser);
                                actions.MoveToElement(element).Click().Perform();
                                
                                element.SendKeys(input.Name);//depending on the visibility of the input field, this will throw an exception:element is not selectable
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
                                    string jsInput = input.Name;
                                    if (input.Name == "'")
                                    {
                                        jsInput = "&#39;";
                                    }
                                    if (input.Name == "''")
                                    {
                                        jsInput = "/'/'";
                                    }
                                    IJavaScriptExecutor js = browser as IJavaScriptExecutor;
                                    //js.ExecuteScript("document.getElementById('" + Id + "').setAttribute('value', '" + jsInput + "')", element);
                                    js.ExecuteScript("document.getElementsByName('" + name.Name + "')[0].setAttribute('value', '" + jsInput + "')", element);

                                    try {
                                        element.Submit();
                                        string pageAfterSubmit = browser.PageSource;
                                        //parse for vulnerabilities
                                        CheckForVulnerability(pageAfterSubmit);
                                    }
                                    catch (Exception exception)
                                    {
                                        HelperFunctions.AddToErrorLog("ERROR: " + exception);
                                    }
                                }
                                else
                                {
                                    HelperFunctions.AddToErrorLog("ERROR: " + e);
                                }
                            }
                            //reset to original page
                            browser.Url = url;
                            browser.Navigate();
                        }
                        else
                        {
                            HelperFunctions.WriteSingleLineToTxtFile("No input fields found.");
                        }
                    }
                    catch (Exception e)
                    {
                        HelperFunctions.AddToErrorLog("ERROR: " + e);
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
                    HelperFunctions.WriteSingleLineToTxtFile("Keyword found: " + keyword);
                }
            }
        }

        public List<string> CheckForExistingKeywords(string html)
        {
            HelperFunctions helper = new HelperFunctions();
           
            var markup = html;
            List<string> existingKeywords = new List<string>();
            HelperFunctions.WriteSingleLineToTxtFile("List of keywords on page before input injection:");

            foreach (var keyword in ErrorKeywords)
            {
                if (markup.IndexOf(keyword) > -1)
                {
                    //save value to database
                    existingKeywords.Add(keyword);
                    HelperFunctions.WriteSingleLineToTxtFile(keyword);

                }
            }
            HelperFunctions.WriteSingleLineToTxtFile("---");
            return existingKeywords;
        }

        List<string> ErrorKeywords = new List<string>() {
                "error", "incorrect syntax", "server", "privilege",
                "identity", "select", "insert","update", "datetime",
                "Arithmetic overflow", "statement", "column", "conversion failed",
                "cast", "convert", "fails", "runtime", "null", "sql", "iis", "microsoft"
            };
        
        List<Input> testCases = new List<Input>() {
               new Input( "validTest", "valid"),//simple string, valid for text input
               new Input( "validTest@email.com", "validEmail"),//simple string, valid for text input
               new Input( "'", "invalid"),//single quote
               new Input("''", "invalid"),//two single quotes
               new Input("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "invalid"),//overflow
               new Input(";", "invalid"),//syntax error
               new Input("--", "invalid"), //comment
               new Input("input' or 1 = 1", "invalid"), //boolean based bypass authentication
               new Input("input' or 1 = 1--", "invalid"),
               new Input("&apos; or 1 = 1", "invalid"),//hex-encoded boolean based
               new Input("&apos; or 1 = 1--", "invalid"),
               new Input("substring(@@version,1,1) = 5", "invalid")//information phishing
             };

    }
}
