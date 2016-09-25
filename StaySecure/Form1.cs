using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

namespace StaySecure
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            WebClient webClient = new WebClient();
            string html = webClient.DownloadString("http://www.google.com");
            HtmlAgilityPack.HtmlDocument htmldoc = new HtmlAgilityPack.HtmlDocument();
            htmldoc.LoadHtml(html);

            if(htmldoc.DocumentNode != null)
            {
                var nodes = htmldoc.DocumentNode.SelectNodes("//input");
                
            }

        }
    }
}
