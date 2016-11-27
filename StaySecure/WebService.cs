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
    public class WebService
    {
        public static bool IsServerResponseOK(string url)
        {
            DirectoryInfo directory = new DirectoryInfo(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\TestProgramOperations.txt")));
            string fileLocation = directory.ToString();

            bool isValid = Uri.IsWellFormedUriString(url, UriKind.Absolute);
            if (isValid)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                try
                {
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        isValid = false;
                        HelperFunctions.AddToErrorLog("Server returned Bad Request for: " + url);
                    }
                    else if (response.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        isValid = false;
                        HelperFunctions.AddToErrorLog("Potential vulnerability at " + url + ". Returned Internal Server Error");
                    }

                    response.Close();
                }
                catch (Exception e)
                {
                    HelperFunctions.WriteSingleLineToTxtFile("Please make sure you are connected to the internet.");
                    HelperFunctions.AddToErrorLog("ERROR: " + e);

                    isValid = false;
                    return isValid;
                }
            }

            return isValid;
        }

        public static IWebDriver CreateBrowserInstance()
        {
            IWebDriver browser = new PhantomJSDriver();
            return browser;
        }

        public static void CloseBrowserInstance(IWebDriver browser)
        {
            browser.Dispose();
        }

        public static bool IsUserUrlValid(string url)
        {
            bool isValid = Uri.IsWellFormedUriString(url, UriKind.Absolute);
            if (isValid)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                try
                {
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        isValid = false;
                        HelperFunctions.AddToErrorLog("Server returned Bad Request for: " + url);
                    }
                    else if (response.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        HelperFunctions.AddToErrorLog("Potential vulnerability at " + url + ". Returned Internal Server Error");
                    }
                    response.Close();
                }
                catch (Exception e)
                {
                    HelperFunctions.WriteSingleLineToTxtFile("Please make sure you are connected to the internet.");

                    HelperFunctions.AddToErrorLog("[Poor Internet Connection] Error: " + e);
                    isValid = false;
                    return isValid;
                }

            }
            else {
                HelperFunctions.WriteSingleLineToTxtFile("That url is not valid. Please try again.");
                HelperFunctions.WriteSingleLineToTxtFile("[Invalid Url]");
            }
            return isValid;
        }

    }
}
