﻿using System;
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
            //uncomment for production: hides phantom console popup
            //var driverService = PhantomJSDriverService.CreateDefaultService();
            //driverService.HideCommandPromptWindow = true;
            //IWebDriver browser = new PhantomJSDriver(driverService);

            //browser for debugging. In production uncomment the browser and service above.
            IWebDriver browser = new PhantomJSDriver();
            if (!WebService.IsUserUrlValid(origUrl))
            {
                browser.Dispose();
                return;
            }
            //Test server request/response
            bool isResponseValid = WebService.IsServerResponseOK(origUrl);
            if (isResponseValid)
            {
                HelperFunctions.WriteSingleLineToTxtFile("Date: " + DateTime.Now.ToShortDateString());
                HelperFunctions.WriteSingleLineToTxtFile("Url: " + origUrl);
                HelperFunctions.WriteSingleLineToTxtFile("");

                HelperFunctions.AddToErrorLog("Date: " + DateTime.Now.ToShortDateString());
                HelperFunctions.AddToErrorLog("Url: " + origUrl);
                HelperFunctions.AddToErrorLog("[Start]");
                //get list of pre-existing keywords
                string originalPage = browser.PageSource;
                //parse for vulnerabilities
                CheckForVulnerability(originalPage);
                List<string> preexistingKeywords = CheckForExistingKeywords(originalPage, false);
                //create report
                InjectQuery(origUrl, browser);
                BypassAuthentication(origUrl, browser);
                HelperFunctions.WriteSingleLineToTxtFile("Potential vulnerabilities determined by your scan are listed above. If there are any threats labeled as [High] your application data" +
                    " has been compromised and your site needs immediate attention. Threats labeled as [Medium] indicate that your site is exposing too much information about its construction and is" +
                    " at risk for a clever attack. Click the View Details button for more information about the scan and its findings.");
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
                            if (node.Attributes["type"] == null)
                            {
                                Input input = new Input(name, "");
                                listInputFieldNames.Add(input);
                            }
                            else if (node.Attributes["type"].Value == "")
                            {
                                Input input = new Input(name, "");
                                listInputFieldNames.Add(input);
                            }
                            else if (node.Attributes["type"].Value == "text")
                            {
                                Input input = new Input(name, "text");
                                listInputFieldNames.Add(input);
                            }
                            else if (node.Attributes["type"].Value == "password")
                            {
                                Input input = new Input(name, "password");
                                listInputFieldNames.Add(input);
                            }

                            else if (node.Attributes["type"].Value == "search")
                            {
                                Input input = new Input(name, "search");
                                listInputFieldNames.Add(input);
                            }
                            else if (node.Attributes["type"].Value == "email")
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
                HelperFunctions.AddToErrorLog("There are no visible inputs at " + url);
            }
            else {
                HelperFunctions.WriteSingleLineToTxtFile("Inputs on page:");
                HelperFunctions.AddToErrorLog("Inputs on page:");
                foreach (var name in listInputFieldNames)
                {
                    HelperFunctions.WriteSingleLineToTxtFile("Element Name: " + name.Name + ", Input type: " + name.Type);
                    HelperFunctions.AddToErrorLog("Element Name: " + name.Name + ", Input type: " + name.Type);
                }
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
                            HelperFunctions.AddToErrorLog("");
                            HelperFunctions.AddToErrorLog("Element name: " + element.GetAttribute("name"));
                            HelperFunctions.AddToErrorLog("Input: " + input.Name + " (" + input.Type + ")");
                            try
                            {
                                element.Clear();
                                Actions actions = new Actions(browser);
                                actions.MoveToElement(element).Click().Perform();
                                //depending on the visibility of the input field, this will throw an exception:element is not selectable, it is handled in
                                //handled in the catch statement
                                element.SendKeys(input.Name);
                                element.Submit();
                                string pageAfterSubmit = browser.PageSource;
                                if (input.Type == "valid")
                                {
                                    resultsFromValidInput.AddRange(CheckForExistingKeywords(pageAfterSubmit, true));
                                }
                                else
                                {
                                    if (pageAfterSubmit.IndexOf("Server Error") > -1)
                                    {
                                        HelperFunctions.AddToErrorLog("Server Error [Medium]");
                                    }
                                    //parse for vulnerabilities
                                    CheckForVulnerability(pageAfterSubmit);
                                }
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
                                    js.ExecuteScript("document.getElementsByName('" + name.Name + "')[0].setAttribute('value', '" + jsInput + "')", element);

                                    try
                                    {
                                        element.Submit();
                                        string pageAfterSubmit = browser.PageSource;
                                        //parse for vulnerabilities
                                        if (pageAfterSubmit.IndexOf("Server Error") > -1)
                                        {
                                            HelperFunctions.AddToErrorLog("Server Error [Medium]");
                                        }
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
                            HelperFunctions.AddToErrorLog("No input fields found.");
                        }
                    }
                    catch (Exception e)
                    {
                        HelperFunctions.AddToErrorLog("ERROR: " + e);
                    }
                }
            }
            return;
        }
        public void BypassAuthentication(string url, IWebDriver browser)
        {
            HelperFunctions.WriteSingleLineToTxtFile("");
            HelperFunctions.AddToErrorLog("");
            HelperFunctions.AddToErrorLog("[Description: Insert test case into every field on page at the same time]");
            HelperFunctions helper = new HelperFunctions();
            List<Input> listInputFieldNames = new List<Input>();

            browser.Url = url;
            browser.Navigate();

            string html = browser.PageSource;

            if (html.IndexOf("Server Error") > -1)
            {
                HelperFunctions.WriteSingleLineToTxtFile("Server Error [Medium]");
            }
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
                            if (node.Attributes["type"] == null)
                            {
                                Input input = new Input(name, "");
                                listInputFieldNames.Add(input);
                            }
                            else if (node.Attributes["type"].Value == "")
                            {
                                Input input = new Input(name, "");
                                listInputFieldNames.Add(input);
                            }
                            else if (node.Attributes["type"].Value == "text")
                            {
                                Input input = new Input(name, "text");
                                listInputFieldNames.Add(input);
                            }
                            else if (node.Attributes["type"].Value == "password")
                            {
                                Input input = new Input(name, "password");
                                listInputFieldNames.Add(input);
                            }

                            else if (node.Attributes["type"].Value == "search")
                            {
                                Input input = new Input(name, "search");
                                listInputFieldNames.Add(input);
                            }
                            else if (node.Attributes["type"].Value == "email")
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
                HelperFunctions.AddToErrorLog("There are no visible inputs at that url.");
            }

            foreach (var input in testCases)
            {
                browser.Url = url;
                browser.Navigate();
                //check that the elements exist before the "sendkeys"
                try
                {
                    List<IWebElement> elements = new List<IWebElement>();
                    foreach (var item in listInputFieldNames)
                    {
                        IWebElement element = browser.FindElement(By.Name(item.Name));
                        if (element != null)
                            elements.Add(element);
                    }

                    if (elements.Count > 0)
                    {
                        try
                        {
                            foreach (var formElement in elements)
                            {
                                formElement.Clear();
                                Actions actions = new Actions(browser);
                                actions.MoveToElement(formElement).Click().Perform();
                                //depending on the visibility of the input field, this will throw an exception:element is not selectable, it is handled in
                                //handled in the catch statement
                                formElement.SendKeys(input.Name);
                            }
                            elements[0].Submit();
                            string pageAfterSubmit = browser.PageSource;
                            if (input.Type == "valid")
                            {
                                resultsFromValidInput.AddRange(CheckForExistingKeywords(pageAfterSubmit, true));
                            }
                            else
                            {
                                //parse for vulnerabilities
                                HelperFunctions.AddToErrorLog("testcase: " + input.Name);
                                

                                var uri = new Uri(url);
                                string OriginalPath = uri.GetLeftPart(UriPartial.Path);

                                uri = new Uri(browser.Url);
                                string NavigatedToPath = uri.GetLeftPart(UriPartial.Path);
                                if (OriginalPath != NavigatedToPath)
                                {
                                    HelperFunctions.WriteSingleLineToTxtFile("testcase: " + input.Name);
                                    HelperFunctions.AddToErrorLog("Bypassed Authentication");
                                    HelperFunctions.WriteSingleLineToTxtFile("Bypassed Authentication [High]");
                                    HelperFunctions.WriteSingleLineToTxtFile("");
                                }
                                if (pageAfterSubmit.IndexOf("Server Error") > -1)
                                {
                                    HelperFunctions.WriteSingleLineToTxtFile("testcase: " + input.Name);
                                    HelperFunctions.WriteSingleLineToTxtFile("Server Error [Medium]");
                                    HelperFunctions.WriteSingleLineToTxtFile("");
                                }
                                CheckForVulnerability(pageAfterSubmit);
                            }
                        }
                        catch (Exception e)
                        {
                            HelperFunctions.AddToErrorLog("ERROR: " + e);
                        }
                    }
                }
                catch (Exception e)
                {
                    HelperFunctions.AddToErrorLog("ERROR: " + e);
                }
            }
        }

        public void CheckForVulnerability(string html)
        {
            HelperFunctions helper = new HelperFunctions();

            var markup = html;

            foreach (var keyword in ErrorKeywords)
            {
                //only mark the keyword as suspicious if it does not show up in the same list as a valid input
                if (markup.IndexOf(keyword) > -1 && !resultsFromValidInput.Contains(keyword))
                {
                    HelperFunctions.AddToErrorLog("Keyword found: " + keyword);
                }
            }
        }


        public List<string> CheckForExistingKeywords(string html, bool isTest)
        {
            HelperFunctions helper = new HelperFunctions();

            var markup = html;
            List<string> existingKeywords = new List<string>();
            if (!isTest)
            {
                HelperFunctions.AddToErrorLog("List of keywords on page before input injection:");
            }
            else { HelperFunctions.AddToErrorLog("List of keywords on page after valid input"); }

            foreach (var keyword in ErrorKeywords)
            {
                if (markup.IndexOf(keyword) > -1)
                {
                    //save value to database
                    existingKeywords.Add(keyword);
                    HelperFunctions.AddToErrorLog(keyword);
                }
            }
            HelperFunctions.AddToErrorLog("---");
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
               new Input("input'%20or%201=1", "invalid"), //obfuscated boolean based bypass authentication
               new Input("input'%20or%201=1--", "invalid"),
               new Input("&apos;%20or%201=1", "invalid"),//obfuscated hex-encoded boolean based
               new Input("&apos;%20or%201=1--", "invalid"),
               new Input("substring(@@version,1,1) = 5", "invalid")//information phishing
             };
        List<string> resultsFromValidInput = new List<string>() { "" };
    }
}
