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
            this.SuspendLayout();
            // 
            // urlInput
            // 
            this.urlInput.Location = new System.Drawing.Point(64, 113);
            this.urlInput.Multiline = true;
            this.urlInput.Name = "urlInput";
            this.urlInput.Size = new System.Drawing.Size(1033, 50);
            this.urlInput.TabIndex = 0;
            this.urlInput.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // resultsDisplay
            // 
            this.resultsDisplay.Location = new System.Drawing.Point(50, 311);
            this.resultsDisplay.Name = "resultsDisplay";
            this.resultsDisplay.Size = new System.Drawing.Size(1047, 949);
            this.resultsDisplay.TabIndex = 1;
            this.resultsDisplay.Text = "";
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
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1149, 1313);
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
    }
}

