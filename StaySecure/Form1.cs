﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace StaySecure
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();         
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void submitBtn_Click(object sender, EventArgs e)
        {
            //TODO: add warning for incorrect input
            string url = urlInput.Text;
            if (url == null || url == ""){
                url = "https://google.com/";//use google as the default web page. Also use google if there is an exception and the entered page cannot be used
            }

            Report report = new Report(url);
            report.GenerateReport(url);

            //write results summary to gui
            
            DirectoryInfo directory = new DirectoryInfo(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\TestProgramOperations.txt")));
            string fileLocation = directory.ToString();

            if (System.IO.File.Exists(fileLocation))
            {
                resultsDisplay.LoadFile(fileLocation, RichTextBoxStreamType.PlainText);
            }
        }

        private void resultsDisplay_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //TODO: add warning for incorrect input
            string url = urlInput.Text;
            if (url == null || url == "")
            {
                url = "https://google.com/";//use google as the default web page. Also use google if there is an exception and the entered page cannot be used
            }
            Cursor.Current = Cursors.WaitCursor;
            Report report = new Report(url);
            report.GenerateReport(url);

            //write results summary to gui
            Cursor.Current = Cursors.Default;
            DirectoryInfo directory = new DirectoryInfo(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\TestProgramOperations.txt")));
            string fileLocation = directory.ToString();

            if (System.IO.File.Exists(fileLocation))
            {
                resultsDisplay.LoadFile(fileLocation, RichTextBoxStreamType.PlainText);
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            DirectoryInfo directory = new DirectoryInfo(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\ErrorLog.txt")));
            string fileLocation = directory.ToString();

            if (System.IO.File.Exists(fileLocation))
            {
                Process.Start("notepad", fileLocation);
            }

           //else file does not exist warning. 
        }
    }
}
