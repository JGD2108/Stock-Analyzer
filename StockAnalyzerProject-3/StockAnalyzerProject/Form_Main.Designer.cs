using System;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace StockAnalyzerProject
{
    partial class Form_Main
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


        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            ChartArea chartArea1 = new ChartArea();
            ChartArea chartArea2 = new ChartArea();
            Legend legend1 = new Legend();
            Series series1 = new Series();
            Series series2 = new Series();
            this.dateTimePicker_end = new System.Windows.Forms.DateTimePicker();
            this.label_end = new System.Windows.Forms.Label();
            this.dateTimePicker_start = new System.Windows.Forms.DateTimePicker();
            this.label_start = new System.Windows.Forms.Label();
            this.button_loadData = new System.Windows.Forms.Button();
            this.button_update = new System.Windows.Forms.Button();
            this.button_simulate = new System.Windows.Forms.Button();
            this.comboBox_patterns = new System.Windows.Forms.ComboBox();
            this.textBox_valueTimer = new System.Windows.Forms.TextBox();
            this.hScrollBar_timer = new System.Windows.Forms.HScrollBar();
            this.openFileDialog_selectCsv = new System.Windows.Forms.OpenFileDialog();
            this.tableLayoutPanel_options = new System.Windows.Forms.TableLayoutPanel();
            this.chart_PanelMain = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.timer_Simulate = new System.Windows.Forms.Timer(this.components);
            this.flowLayoutPanel_top = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel_timer = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel_options.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart_PanelMain)).BeginInit();
            this.flowLayoutPanel_top.SuspendLayout();
            this.flowLayoutPanel_timer.SuspendLayout();
            this.SuspendLayout();
            // 
            // dateTimePicker_end
            // 
            this.dateTimePicker_end.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePicker_end.Location = new System.Drawing.Point(1539, 8);
            this.dateTimePicker_end.Name = "dateTimePicker_end";
            this.dateTimePicker_end.Size = new System.Drawing.Size(200, 43);
            this.dateTimePicker_end.TabIndex = 5;
            this.dateTimePicker_end.Value = new System.DateTime(2023, 12, 30, 0, 0, 0, 0);
            // 
            // label_end
            // 
            this.label_end.AutoSize = true;
            this.label_end.Location = new System.Drawing.Point(1458, 13);
            this.label_end.Margin = new System.Windows.Forms.Padding(10, 8, 3, 3);
            this.label_end.Name = "label_end";
            this.label_end.Size = new System.Drawing.Size(68, 37);
            this.label_end.TabIndex = 4;
            this.label_end.Text = ":End";
            this.label_end.Click += new System.EventHandler(this.label_end_Click);
            // 
            // dateTimePicker_start
            // 
            this.dateTimePicker_start.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePicker_start.Location = new System.Drawing.Point(1252, 8);
            this.dateTimePicker_start.Name = "dateTimePicker_start";
            this.dateTimePicker_start.Size = new System.Drawing.Size(200, 43);
            this.dateTimePicker_start.TabIndex = 3;
            this.dateTimePicker_start.Value = new System.DateTime(2023, 1, 28, 0, 0, 0, 0);
            this.dateTimePicker_start.ValueChanged += new System.EventHandler(this.dateTimePicker_start_ValueChanged);
            // 
            // label_start
            // 
            this.label_start.AutoSize = true;
            this.label_start.Location = new System.Drawing.Point(1162, 13);
            this.label_start.Margin = new System.Windows.Forms.Padding(10, 8, 3, 3);
            this.label_start.Name = "label_start";
            this.label_start.Size = new System.Drawing.Size(77, 37);
            this.label_start.TabIndex = 3;
            this.label_start.Text = ":Start";
            this.label_start.Click += new System.EventHandler(this.label_start_Click);
            // 
            // button_loadData
            // 
            this.button_loadData.AutoSize = true;
            this.button_loadData.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button_loadData.Location = new System.Drawing.Point(1063, 8);
            this.button_loadData.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
            this.button_loadData.Name = "button_loadData";
            this.button_loadData.Size = new System.Drawing.Size(86, 47);
            this.button_loadData.TabIndex = 6;
            this.button_loadData.Text = "Load";
            this.button_loadData.UseVisualStyleBackColor = true;
            this.button_loadData.Click += new System.EventHandler(this.button_loadData_Click);
            // 
            // button_update
            // 
            this.button_update.AutoSize = true;
            this.button_update.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button_update.Location = new System.Drawing.Point(942, 8);
            this.button_update.Name = "button_update";
            this.button_update.Size = new System.Drawing.Size(115, 47);
            this.button_update.TabIndex = 7;
            this.button_update.Text = "Update";
            this.button_update.UseVisualStyleBackColor = true;
            // (Click handler wired in code-behind: this.button_update.Click += button_update_Click;)
            // 
            // button_simulate
            // 
            this.button_simulate.Location = new System.Drawing.Point(798, 8);
            this.button_simulate.Name = "button_simulate";
            this.button_simulate.Size = new System.Drawing.Size(138, 49);
            this.button_simulate.TabIndex = 8;
            this.button_simulate.Text = "Simulate";
            this.button_simulate.UseVisualStyleBackColor = true;
            this.button_simulate.Click += new System.EventHandler(this.button_simulate_Click);
            // 
            // comboBox_patterns
            // 
            this.comboBox_patterns.FormattingEnabled = true;
            this.comboBox_patterns.Items.AddRange(new object[] {
            "Doji  ",
            "Doji (Bullish)  ",
            "Doji (Bearish)  ",
            "Dragonfly Doji  ",
            "Dragonfly Doji (Bullish)  ",
            "Dragonfly Doji (Bearish)  ",
            "Gravestone Doji  ",
            "Gravestone Doji (Bullish)  ",
            "Gravestone Doji (Bearish)  ",
            "Marubozu  ",
            "Marubozu (Bullish)  ",
            "Marubozu (Bearish)  ",
            "Hammer  ",
            "Hammer (Bullish)  ",
            "Hammer (Bearish)  ",
            "Inverted Hammer  ",
            "Inverted Hammer (Bullish)  ",
            "Inverted Hammer (Bearish)  ",
            "Engulfing  ",
            "Engulfing (Bullish)  ",
            "Engulfing (Bearish)  ",
            "Harami  ",
            "Harami (Bullish)  ",
            "Harami (Bearish)"});
            this.comboBox_patterns.Location = new System.Drawing.Point(671, 8);
            this.comboBox_patterns.Name = "comboBox_patterns";
            this.comboBox_patterns.Size = new System.Drawing.Size(121, 45);
            this.comboBox_patterns.TabIndex = 9;
            this.comboBox_patterns.SelectedIndexChanged += new System.EventHandler(this.comboBox_patterns_SelectedIndexChanged);
            // 
            // textBox_valueTimer
            // 
            this.textBox_valueTimer.Location = new System.Drawing.Point(8, 8);
            this.textBox_valueTimer.Margin = new System.Windows.Forms.Padding(8, 8, 3, 3);
            this.textBox_valueTimer.Name = "textBox_valueTimer";
            this.textBox_valueTimer.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.textBox_valueTimer.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.textBox_valueTimer.Size = new System.Drawing.Size(80, 43);
            this.textBox_valueTimer.TabIndex = 10;
            // 
            // hScrollBar_timer
            // 
            this.hScrollBar_timer.LargeChange = 1;
            this.hScrollBar_timer.Location = new System.Drawing.Point(96, 10);
            this.hScrollBar_timer.Maximum = 2000;
            this.hScrollBar_timer.Minimum = 100;
            this.hScrollBar_timer.Name = "hScrollBar_timer";
            this.hScrollBar_timer.Size = new System.Drawing.Size(326, 40);
            this.hScrollBar_timer.TabIndex = 11;
            this.hScrollBar_timer.Value = 100;
            // 
            // openFileDialog_selectCsv
            // 
            this.openFileDialog_selectCsv.DefaultExt = "CSV";
            this.openFileDialog_selectCsv.Filter = "All|*.csv|Month|*-Month.csv|Week|*-Week.csv|Day|*-Day.csv";
            this.openFileDialog_selectCsv.FilterIndex = 4;
            this.openFileDialog_selectCsv.InitialDirectory = "C:\\Users\\Jdela\\Documents\\Software_system_development\\Stock_Data";
            this.openFileDialog_selectCsv.Multiselect = true;
            this.openFileDialog_selectCsv.ReadOnlyChecked = true;
            this.openFileDialog_selectCsv.RestoreDirectory = true;
            this.openFileDialog_selectCsv.ShowReadOnly = true;
            this.openFileDialog_selectCsv.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog_FileOk);
            // 
            // tableLayoutPanel_options
            // 
            this.tableLayoutPanel_options.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel_options.ColumnCount = 1;
            this.tableLayoutPanel_options.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel_options.Controls.Add(this.chart_PanelMain, 0, 0);
            this.tableLayoutPanel_options.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_options.Location = new System.Drawing.Point(0, 115);
            this.tableLayoutPanel_options.Name = "tableLayoutPanel_options";
            this.tableLayoutPanel_options.RowCount = 1;
            this.tableLayoutPanel_options.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel_options.Size = new System.Drawing.Size(1752, 768);
            this.tableLayoutPanel_options.TabIndex = 9;
            this.tableLayoutPanel_options.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
            // 
            // chart_PanelMain
            // 
            chartArea1.Name = "ChartArea_OHLC";
            chartArea2.AlignWithChartArea = "ChartArea_OHLC";
            chartArea2.Name = "ChartArea_Volume";
            this.chart_PanelMain.ChartAreas.Add(chartArea1);
            this.chart_PanelMain.ChartAreas.Add(chartArea2);
            this.chart_PanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Name = "Legend1";
            this.chart_PanelMain.Legends.Add(legend1);
            this.chart_PanelMain.Location = new System.Drawing.Point(3, 3);
            this.chart_PanelMain.Name = "chart_PanelMain";
            series1.ChartArea = "ChartArea_OHLC";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Candlestick;
            series1.CustomProperties = "PriceDownColor=Red, PriceUpColor=Green";
            series1.IsXValueIndexed = true;
            series1.Legend = "Legend1";
            series1.Name = "Series_OHLC";
            series1.XValueMember = "Date";
            series1.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Date;
            series1.YValueMembers = "High,Low,Open,Close";
            series1.YValuesPerPoint = 4;
            series2.ChartArea = "ChartArea_Volume";
            series2.IsXValueIndexed = true;
            series2.Legend = "Legend1";
            series2.Name = "Series_Volume";
            series2.XValueMember = "Date";
            series2.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Date;
            series2.YValueMembers = "Volume";
            this.chart_PanelMain.Series.Add(series1);
            this.chart_PanelMain.Series.Add(series2);
            this.chart_PanelMain.Size = new System.Drawing.Size(1746, 762);
            this.chart_PanelMain.TabIndex = 8;
            this.chart_PanelMain.Text = "chart1";
            this.chart_PanelMain.Click += new System.EventHandler(this.chart_PanelMain_Click);
            // 
            // timer_Simulate
            // 
            this.timer_Simulate.Interval = 5000;
            this.timer_Simulate.Tick += new System.EventHandler(this.timer_Simulate_Tick);
            // 
            // flowLayoutPanel_top
            // 
            this.flowLayoutPanel_top.AutoSize = true;
            this.flowLayoutPanel_top.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel_top.Controls.Add(this.dateTimePicker_end);
            this.flowLayoutPanel_top.Controls.Add(this.label_end);
            this.flowLayoutPanel_top.Controls.Add(this.dateTimePicker_start);
            this.flowLayoutPanel_top.Controls.Add(this.label_start);
            this.flowLayoutPanel_top.Controls.Add(this.button_loadData);
            this.flowLayoutPanel_top.Controls.Add(this.button_update);
            this.flowLayoutPanel_top.Controls.Add(this.button_simulate);
            this.flowLayoutPanel_top.Controls.Add(this.comboBox_patterns);
            this.flowLayoutPanel_top.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel_top.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel_top.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel_top.Name = "flowLayoutPanel_top";
            this.flowLayoutPanel_top.Padding = new System.Windows.Forms.Padding(5);
            this.flowLayoutPanel_top.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.flowLayoutPanel_top.Size = new System.Drawing.Size(1752, 65);
            this.flowLayoutPanel_top.TabIndex = 2;
            // 
            // flowLayoutPanel_timer
            // 
            this.flowLayoutPanel_timer.AutoSize = true;
            this.flowLayoutPanel_timer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel_timer.Controls.Add(this.textBox_valueTimer);
            this.flowLayoutPanel_timer.Controls.Add(this.hScrollBar_timer);
            this.flowLayoutPanel_timer.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel_timer.Location = new System.Drawing.Point(0, 65);
            this.flowLayoutPanel_timer.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel_timer.Name = "flowLayoutPanel_timer";
            this.flowLayoutPanel_timer.Padding = new System.Windows.Forms.Padding(5, 5, 5, 0);
            this.flowLayoutPanel_timer.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.flowLayoutPanel_timer.Size = new System.Drawing.Size(1752, 50);
            this.flowLayoutPanel_timer.TabIndex = 3;
            // 
            // Form_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(15F, 37F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1752, 883);
            this.Controls.Add(this.tableLayoutPanel_options);
            this.Controls.Add(this.flowLayoutPanel_timer);
            this.Controls.Add(this.flowLayoutPanel_top);
            this.Font = new System.Drawing.Font("Segoe UI", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form_Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Stock Analyzer ";
            ((System.ComponentModel.ISupportInitialize)(this.chart_PanelMain)).EndInit();
            this.tableLayoutPanel_options.ResumeLayout(false);
            this.flowLayoutPanel_top.ResumeLayout(false);
            this.flowLayoutPanel_top.PerformLayout();
            this.flowLayoutPanel_timer.ResumeLayout(false);
            this.flowLayoutPanel_timer.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label label_start;
        private System.Windows.Forms.DateTimePicker dateTimePicker_start;
        private System.Windows.Forms.Label label_end;
        private System.Windows.Forms.DateTimePicker dateTimePicker_end;
        private System.Windows.Forms.Button button_loadData;
        private System.Windows.Forms.Button button_update;
        private System.Windows.Forms.OpenFileDialog openFileDialog_selectCsv;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_options;
        private System.Windows.Forms.Button button_simulate;
        private System.Windows.Forms.Timer timer_Simulate;
        private System.Windows.Forms.ComboBox comboBox_patterns;
        private System.Windows.Forms.TextBox textBox_valueTimer;
        private System.Windows.Forms.HScrollBar hScrollBar_timer;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_top;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_timer;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart_PanelMain;
    }
}
