namespace StaySecure
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.urlInput = new System.Windows.Forms.TextBox();
            this.resultsDisplay = new System.Windows.Forms.RichTextBox();
            this.submitBtn = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // urlInput
            // 
            this.urlInput.Location = new System.Drawing.Point(26, 50);
            this.urlInput.Name = "urlInput";
            this.urlInput.Size = new System.Drawing.Size(550, 20);
            this.urlInput.TabIndex = 0;
            this.urlInput.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // resultsDisplay
            // 
            this.resultsDisplay.Location = new System.Drawing.Point(26, 132);
            this.resultsDisplay.Name = "resultsDisplay";
            this.resultsDisplay.ReadOnly = true;
            this.resultsDisplay.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.resultsDisplay.Size = new System.Drawing.Size(550, 450);
            this.resultsDisplay.TabIndex = 1;
            this.resultsDisplay.Text = "";
            this.resultsDisplay.TextChanged += new System.EventHandler(this.resultsDisplay_TextChanged);
            // 
            // submitBtn
            // 
            this.submitBtn.Location = new System.Drawing.Point(955, 196);
            this.submitBtn.Name = "submitBtn";
            this.submitBtn.Size = new System.Drawing.Size(142, 61);
            this.submitBtn.TabIndex = 2;
            this.submitBtn.Text = "Submit";
            this.submitBtn.UseVisualStyleBackColor = true;
            this.submitBtn.Click += new System.EventHandler(this.submitBtn_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(277, 95);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Submit";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(591, 594);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.submitBtn);
            this.Controls.Add(this.resultsDisplay);
            this.Controls.Add(this.urlInput);
            this.Margin = new System.Windows.Forms.Padding(1);
            this.Name = "Form1";
            this.Text = "StaySecure";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox urlInput;
        private System.Windows.Forms.RichTextBox resultsDisplay;
        private System.Windows.Forms.Button submitBtn;
        private System.Windows.Forms.Button button1;
    }
}

