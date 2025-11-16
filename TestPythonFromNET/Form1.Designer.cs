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
            groupBox1 = new GroupBox();
            groupBox2 = new GroupBox();
            txtBoxLogs = new RichTextBox();
            ((System.ComponentModel.ISupportInitialize)mainFeedPixBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)liveFeedPixBox).BeginInit();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // btnRunPyScript
            // 
            btnRunPyScript.Location = new Point(35, 74);
            btnRunPyScript.Name = "btnRunPyScript";
            btnRunPyScript.Size = new Size(360, 121);
            btnRunPyScript.TabIndex = 0;
            btnRunPyScript.Text = "Run Py Script";
            btnRunPyScript.UseVisualStyleBackColor = true;
            btnRunPyScript.Click += btnRunPyScript_Click;
            // 
            // mainFeedPixBox
            // 
            mainFeedPixBox.Location = new Point(469, 40);
            mainFeedPixBox.Name = "mainFeedPixBox";
            mainFeedPixBox.Size = new Size(876, 330);
            mainFeedPixBox.TabIndex = 1;
            mainFeedPixBox.TabStop = false;
            // 
            // liveFeedPixBox
            // 
            liveFeedPixBox.Location = new Point(469, 402);
            liveFeedPixBox.Name = "liveFeedPixBox";
            liveFeedPixBox.Size = new Size(876, 350);
            liveFeedPixBox.TabIndex = 2;
            liveFeedPixBox.TabStop = false;
            // 
            // groupBox1
            // 
            groupBox1.Location = new Point(421, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(963, 762);
            groupBox1.TabIndex = 3;
            groupBox1.TabStop = false;
            groupBox1.Text = "CAM Feed";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(txtBoxLogs);
            groupBox2.Location = new Point(12, 12);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(403, 762);
            groupBox2.TabIndex = 4;
            groupBox2.TabStop = false;
            groupBox2.Text = "Controls";
            // 
            // txtBoxLogs
            // 
            txtBoxLogs.Location = new Point(23, 214);
            txtBoxLogs.Name = "txtBoxLogs";
            txtBoxLogs.Size = new Size(360, 504);
            txtBoxLogs.TabIndex = 0;
            txtBoxLogs.Text = "";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1410, 799);
            Controls.Add(liveFeedPixBox);
            Controls.Add(mainFeedPixBox);
            Controls.Add(btnRunPyScript);
            Controls.Add(groupBox1);
            Controls.Add(groupBox2);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)mainFeedPixBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)liveFeedPixBox).EndInit();
            groupBox2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Button btnRunPyScript;
        private PictureBox mainFeedPixBox;
        private PictureBox liveFeedPixBox;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private RichTextBox txtBoxLogs;
    }
}
