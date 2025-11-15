namespace TestPythonFromNET
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnRunPyScript = new Button();
            mainFeedPixBox = new PictureBox();
            liveFeedPixBox = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)mainFeedPixBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)liveFeedPixBox).BeginInit();
            SuspendLayout();
            // 
            // btnRunPyScript
            // 
            btnRunPyScript.Location = new Point(40, 169);
            btnRunPyScript.Name = "btnRunPyScript";
            btnRunPyScript.Size = new Size(243, 172);
            btnRunPyScript.TabIndex = 0;
            btnRunPyScript.Text = "Run Py Script";
            btnRunPyScript.UseVisualStyleBackColor = true;
            btnRunPyScript.Click += btnRunPyScript_Click;
            // 
            // mainFeedPixBox
            // 
            mainFeedPixBox.Location = new Point(335, 40);
            mainFeedPixBox.Name = "mainFeedPixBox";
            mainFeedPixBox.Size = new Size(839, 330);
            mainFeedPixBox.TabIndex = 1;
            mainFeedPixBox.TabStop = false;
            // 
            // liveFeedPixBox
            // 
            liveFeedPixBox.Location = new Point(347, 408);
            liveFeedPixBox.Name = "liveFeedPixBox";
            liveFeedPixBox.Size = new Size(827, 350);
            liveFeedPixBox.TabIndex = 2;
            liveFeedPixBox.TabStop = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1243, 799);
            Controls.Add(liveFeedPixBox);
            Controls.Add(mainFeedPixBox);
            Controls.Add(btnRunPyScript);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)mainFeedPixBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)liveFeedPixBox).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button btnRunPyScript;
        private PictureBox mainFeedPixBox;
        private PictureBox liveFeedPixBox;
    }
}
