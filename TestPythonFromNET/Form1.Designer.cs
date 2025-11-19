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
            s = new GroupBox();
            groupBox2 = new GroupBox();
            boxRawOutAutoScroll = new CheckBox();
            label1 = new Label();
            txtBoxLogs = new RichTextBox();
            txtBoxRecentStatus = new RichTextBox();
            label2 = new Label();
            groupBox3 = new GroupBox();
            boxPyOutAutoScroll = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)mainFeedPixBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)liveFeedPixBox).BeginInit();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
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
            // s
            // 
            s.Location = new Point(421, 12);
            s.Name = "s";
            s.Size = new Size(963, 762);
            s.TabIndex = 3;
            s.TabStop = false;
            s.Text = "CAM Feed";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(boxRawOutAutoScroll);
            groupBox2.Controls.Add(label1);
            groupBox2.Controls.Add(txtBoxLogs);
            groupBox2.Location = new Point(12, 12);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(403, 762);
            groupBox2.TabIndex = 4;
            groupBox2.TabStop = false;
            groupBox2.Text = "Controls";
            // 
            // boxRawOutAutoScroll
            // 
            boxRawOutAutoScroll.AutoSize = true;
            boxRawOutAutoScroll.Location = new Point(122, 697);
            boxRawOutAutoScroll.Name = "boxRawOutAutoScroll";
            boxRawOutAutoScroll.Size = new Size(125, 29);
            boxRawOutAutoScroll.TabIndex = 2;
            boxRawOutAutoScroll.Text = "Auto Scroll";
            boxRawOutAutoScroll.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(23, 199);
            label1.Name = "label1";
            label1.Size = new Size(131, 25);
            label1.TabIndex = 1;
            label1.Text = "Raw Py Output";
            // 
            // txtBoxLogs
            // 
            txtBoxLogs.Location = new Point(23, 237);
            txtBoxLogs.Name = "txtBoxLogs";
            txtBoxLogs.Size = new Size(360, 434);
            txtBoxLogs.TabIndex = 0;
            txtBoxLogs.Text = "";
            // 
            // txtBoxRecentStatus
            // 
            txtBoxRecentStatus.Location = new Point(1415, 92);
            txtBoxRecentStatus.Name = "txtBoxRecentStatus";
            txtBoxRecentStatus.Size = new Size(360, 591);
            txtBoxRecentStatus.TabIndex = 1;
            txtBoxRecentStatus.Text = "";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(25, 37);
            label2.Name = "label2";
            label2.Size = new Size(141, 25);
            label2.TabIndex = 2;
            label2.Text = "Detection Status";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(boxPyOutAutoScroll);
            groupBox3.Controls.Add(label2);
            groupBox3.Location = new Point(1390, 12);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(409, 762);
            groupBox3.TabIndex = 5;
            groupBox3.TabStop = false;
            groupBox3.Text = "Anomaly Detection";
            // 
            // boxPyOutAutoScroll
            // 
            boxPyOutAutoScroll.AutoSize = true;
            boxPyOutAutoScroll.Location = new Point(155, 682);
            boxPyOutAutoScroll.Name = "boxPyOutAutoScroll";
            boxPyOutAutoScroll.Size = new Size(125, 29);
            boxPyOutAutoScroll.TabIndex = 3;
            boxPyOutAutoScroll.Text = "Auto Scroll";
            boxPyOutAutoScroll.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1811, 799);
            Controls.Add(txtBoxRecentStatus);
            Controls.Add(liveFeedPixBox);
            Controls.Add(mainFeedPixBox);
            Controls.Add(btnRunPyScript);
            Controls.Add(s);
            Controls.Add(groupBox2);
            Controls.Add(groupBox3);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)mainFeedPixBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)liveFeedPixBox).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button btnRunPyScript;
        private PictureBox mainFeedPixBox;
        private PictureBox liveFeedPixBox;
        private GroupBox s;
        private GroupBox groupBox2;
        private RichTextBox txtBoxLogs;
        private RichTextBox txtBoxRecentStatus;
        private Label label1;
        private Label label2;
        private GroupBox groupBox3;
        private CheckBox boxRawOutAutoScroll;
        private CheckBox boxPyOutAutoScroll;
    }
}
