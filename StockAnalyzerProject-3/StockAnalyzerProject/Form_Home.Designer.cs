namespace StockAnalyzerProject
{
    partial class Form_Home
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
            this.label_startDate = new System.Windows.Forms.Label();
            this.dateTimePicker_startDate = new System.Windows.Forms.DateTimePicker();
            this.button_loadTicker = new System.Windows.Forms.Button();
            this.label_EndDate = new System.Windows.Forms.Label();
            this.dateTimePicker_endDate = new System.Windows.Forms.DateTimePicker();
            this.openFileDialog_loadTicker = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // label_startDate
            // 
            this.label_startDate.AutoSize = true;
            this.label_startDate.Location = new System.Drawing.Point(67, 123);
            this.label_startDate.Name = "label_startDate";
            this.label_startDate.Size = new System.Drawing.Size(114, 25);
            this.label_startDate.TabIndex = 0;
            this.label_startDate.Text = "Start Date:";
            this.label_startDate.Click += new System.EventHandler(this.label1_Click_1);
            // 
            // dateTimePicker_startDate
            // 
            this.dateTimePicker_startDate.Location = new System.Drawing.Point(187, 123);
            this.dateTimePicker_startDate.Name = "dateTimePicker_startDate";
            this.dateTimePicker_startDate.Size = new System.Drawing.Size(411, 31);
            this.dateTimePicker_startDate.TabIndex = 1;
            this.dateTimePicker_startDate.Value = new System.DateTime(2023, 3, 31, 0, 0, 0, 0);
            // 
            // button_loadTicker
            // 
            this.button_loadTicker.Location = new System.Drawing.Point(639, 116);
            this.button_loadTicker.Name = "button_loadTicker";
            this.button_loadTicker.Size = new System.Drawing.Size(126, 48);
            this.button_loadTicker.TabIndex = 2;
            this.button_loadTicker.Text = "Load";
            this.button_loadTicker.UseVisualStyleBackColor = true;
            this.button_loadTicker.Click += new System.EventHandler(this.button_loadTicker_event);
            // 
            // label_EndDate
            // 
            this.label_EndDate.AutoSize = true;
            this.label_EndDate.Location = new System.Drawing.Point(787, 129);
            this.label_EndDate.Name = "label_EndDate";
            this.label_EndDate.Size = new System.Drawing.Size(107, 25);
            this.label_EndDate.TabIndex = 3;
            this.label_EndDate.Text = "End Date:";
            // 
            // dateTimePicker_endDate
            // 
            this.dateTimePicker_endDate.Location = new System.Drawing.Point(900, 123);
            this.dateTimePicker_endDate.Name = "dateTimePicker_endDate";
            this.dateTimePicker_endDate.Size = new System.Drawing.Size(428, 31);
            this.dateTimePicker_endDate.TabIndex = 4;
            this.dateTimePicker_endDate.Value = new System.DateTime(2023, 8, 31, 0, 0, 0, 0);
            // 
            // openFileDialog_loadTicker
            // 
            this.openFileDialog_loadTicker.FileName = "openFileDialog_loadTicker";
            this.openFileDialog_loadTicker.Filter = "All|*.csv|Month|*-Month.csv|Week|*-Week.csv|Day|*-Day.csv";
            this.openFileDialog_loadTicker.FilterIndex = 4;
            this.openFileDialog_loadTicker.InitialDirectory = "C:\\Users\\Jdela\\OneDrive - University of South Florida\\Documentos\\Software_system_" +
    "development\\Stock_Data";
            this.openFileDialog_loadTicker.Multiselect = true;
            this.openFileDialog_loadTicker.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialogLoad_FileOk);
            // 
            // Form_Home
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1380, 241);
            this.Controls.Add(this.dateTimePicker_endDate);
            this.Controls.Add(this.label_EndDate);
            this.Controls.Add(this.button_loadTicker);
            this.Controls.Add(this.dateTimePicker_startDate);
            this.Controls.Add(this.label_startDate);
            this.Name = "Form_Home";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_startDate;
        private System.Windows.Forms.DateTimePicker dateTimePicker_startDate;
        private System.Windows.Forms.Button button_loadTicker;
        private System.Windows.Forms.Label label_EndDate;
        private System.Windows.Forms.DateTimePicker dateTimePicker_endDate;
        private System.Windows.Forms.OpenFileDialog openFileDialog_loadTicker;
    }
}