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
            // for now... assumption: the user enters a valid url
            //url = FixUserAssumptions(url);

            DirectoryInfo directory = new DirectoryInfo(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\TestProgramOperations.txt")));
            string fileLocation = directory.ToString();

            Uri uriResult;
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
                        HelperFunctions.WriteSingleLineToTxtFile("Server returned Bad Request for: " + url, fileLocation);
                    }
                    else if (response.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        isValid = false;
                        HelperFunctions.WriteSingleLineToTxtFile("Potential vulnerability at " + url + ". Returned Internal Server Error", fileLocation);
                    }
                    //else
                    //{
                    //    isValid = false;
                    //    HelperFunctions.WriteSingleLineToTxtFile("The server response is not valid.", fileLocation);
                    //}

                    response.Close();
                }
                catch (Exception e)
                {
                    HelperFunctions.WriteSingleLineToTxtFile("Please make sure you are connected to the internet.", fileLocation);
                    HelperFunctions.WriteSingleLineToTxtFile("ERROR: " + e, fileLocation);

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
            // for now... assumption: the user enters a valid url
            //url = FixUserAssumptions(url);

            DirectoryInfo directory = new DirectoryInfo(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\TestProgramOperations.txt")));
            string fileLocation = directory.ToString();

            Uri uriResult;
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
                        HelperFunctions.WriteSingleLineToTxtFile("Server returned Bad Request for: " + url, fileLocation);
                    }
                    else if (response.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        HelperFunctions.WriteSingleLineToTxtFile("Potential vulnerability at " + url + ". Returned Internal Server Error", fileLocation);
                    }
                    response.Close();
                }
                catch (Exception e)
                {
                    HelperFunctions.WriteSingleLineToTxtFile("Please make sure you are connected to the internet.", fileLocation);
                    isValid = false;
                    return isValid;
                }

            }
            else { HelperFunctions.WriteSingleLineToTxtFile("That url is not valid.", fileLocation); }
            return isValid;
        }

    }
}
