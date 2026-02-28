// Form_Main.cs - main charting and analysis form for StockAnalyzerProject

using System; // Base types, DateTime, Math, EventArgs, etc.
using System.Collections.Generic; // List<T> collections
using System.Globalization; // Culture-aware parsing/formatting helpers
using System.IO; // File and stream input
using System.Text; // StringBuilder (CSV parsing)
using System.Windows.Forms; // WinForms UI framework
using System.Drawing; // Color and drawing primitives
using System.Windows.Forms.DataVisualization.Charting; // Chart control and annotation types

namespace StockAnalyzerProject // Project namespace
{
    /// <summary>
    /// Main user interface form for loading stock OHLCV data from CSV, filtering by dates,
    /// plotting candlesticks + volume, and running candle-by-candle simulation with pattern annotations.
    /// </summary>
    public partial class Form_Main : Form
    {
        // =========================
        // DATA STORAGE FIELDS
        // =========================

        private List<aSmartCandlestick> _allCandles; // Full dataset loaded from disk (cached in memory)
        private List<aSmartCandlestick> _currentCandles; // Subset currently shown on screen (date filtered)

        // =========================
        // SIMULATION FIELDS
        // =========================

        private List<aSmartCandlestick> _simSource; // Date-filtered dataset used as the simulation "truth"
        private List<aSmartCandlestick> _simulatedCandles; // Dataset that grows 1 candle per tick during simulation
        private int _simulatedIndex; // Points to the next candle from _simSource to append into _simulatedCandles

        // =========================
        // PATTERN RECOGNITION FIELDS
        // =========================

        private RecognizerManager _recognizerManager; // Holds recognizers + caches match results against a dataset

        // =========================
        // SIMULATION STATE FLAGS
        // =========================

        private bool _simulationActive; // True while the timer is actively stepping candles
        private bool _simulationCompleted; // True once all candles in range have been shown (safe to annotate)
        private bool _toolbarLayoutComposed; // Tracks one-time composition of the top toolbar control order

        // Bounds for UI timer speed controls (milliseconds)
        private const int TIMER_MIN = 100; // Fastest allowed tick interval
        private const int TIMER_MAX = 2000; // Slowest allowed tick interval

        // Visual theme colors for a clean light dashboard style.
        private static readonly Color UiWindow = Color.FromArgb(244, 247, 252);
        private static readonly Color UiPanel = Color.FromArgb(232, 238, 247);
        private static readonly Color UiPanelSoft = Color.FromArgb(239, 244, 251);
        private static readonly Color UiText = Color.FromArgb(29, 42, 61);
        private static readonly Color UiMutedText = Color.FromArgb(95, 108, 130);
        private static readonly Color UiBorder = Color.FromArgb(189, 199, 214);
        private static readonly Color UiGrid = Color.FromArgb(222, 229, 238);
        private static readonly Color UiBull = Color.FromArgb(44, 140, 108);
        private static readonly Color UiBear = Color.FromArgb(211, 90, 84);
        private static readonly Color UiAccent = Color.FromArgb(63, 102, 176);

        /// <summary>
        /// Default constructor. Initializes UI controls, wires events safely, creates recognizers,
        /// and configures simulation timer + speed controls. Does not load data.
        /// </summary>
        public Form_Main()
        {
            InitializeComponent(); // Create the WinForms controls designed in the Designer
            ApplyVisualTheme(); // Apply control/chrome styling before data is loaded

            if (this.dateTimePicker_start != null) // Make sure the control exists before using it
                this.dateTimePicker_start.Value = DateTime.Today.AddMonths(-6); // Pick a helpful default start date

            if (this.button_update != null) // Ensure button is present
            {
                this.button_update.Click -= button_update_Click; // Prevent duplicate subscriptions if constructor runs again
                this.button_update.Click += button_update_Click; // Wire Update click to our handler
            }

            if (this.button_simulate != null) // Ensure simulate button exists
            {
                this.button_simulate.Click -= button_simulate_Click; // Remove any previous handler
                this.button_simulate.Click += button_simulate_Click; // Add the correct handler
            }

            if (this.openFileDialog_selectCsv != null) // Ensure dialog exists
            {
                this.openFileDialog_selectCsv.FileOk -= openFileDialog_FileOk; // Remove duplicates if any
                this.openFileDialog_selectCsv.FileOk += openFileDialog_FileOk; // Wire file selection handler
            }

            if (this.timer_Simulate != null) // Ensure timer exists
            {
                this.timer_Simulate.Interval = 250; // Default sim speed (ms)
                this.timer_Simulate.Stop(); // Ensure it starts stopped
                this.timer_Simulate.Tick -= timer_Simulate_Tick; // Remove duplicates in case Designer also wired it
                this.timer_Simulate.Tick += timer_Simulate_Tick; // Subscribe our Tick handler
            }

            var recognizers = BuildRecognizers(); // Create all recognizer objects we support
            _recognizerManager = new RecognizerManager(recognizers); // Create manager that can analyze and cache matches

            EnsurePatternComboBox(); // Bind the drop-down to available recognizers
            ConfigureTimerControls(); // Sync scrollbar/textbox to timer interval
            ArrangeTopControlLayout(); // Ensure toolbar/date controls are proportionate and ordered

            this.Resize -= Form_Main_Resize; // Avoid duplicate resize hook
            this.Resize += Form_Main_Resize; // Keep top controls responsive to window size
        }

        /// <summary>
        /// Overloaded constructor that builds the form and immediately loads a CSV file,
        /// applies the provided inclusive date range, and precomputes pattern matches.
        /// </summary>
        /// <param name="csvPath">Absolute or relative path to the CSV file containing OHLCV data.</param>
        /// <param name="start">Start date (inclusive) for filtering.</param>
        /// <param name="end">End date (inclusive) for filtering.</param>
        public Form_Main(string csvPath, DateTime start, DateTime end) : this()
        {
            if (string.IsNullOrWhiteSpace(csvPath) || !File.Exists(csvPath)) // Validate the path and file existence
            {
                MessageBox.Show("File not found: " + csvPath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Show user-friendly error
                return; // Stop loading since we cannot proceed
            }

            var s = start.Date; // Strip time from start date
            var e = end.Date; // Strip time from end date

            if (s > e) // If dates are reversed
            {
                var tmp = s; // Store start temporarily
                s = e; // Swap start to earlier date
                e = tmp; // Swap end to later date
            }

            this.dateTimePicker_start.Value = s; // Reflect range in UI start picker
            this.dateTimePicker_end.Value = e; // Reflect range in UI end picker

            try
            {
                _allCandles = readCandlesticksFromFile(csvPath); // Load entire CSV into memory
                this.openFileDialog_selectCsv.FileName = csvPath; // Store path so later actions know what file is loaded
                update(_allCandles, s, e); // Apply filter + normalize + display

                if (_recognizerManager != null && _currentCandles != null) // Ensure manager and data exist
                    _recognizerManager.AnalyzeAllSmart(_currentCandles); // Precompute pattern matches for current display set

                ResetSimulationStateAndAnnotations(); // Ensure simulation is fresh for the newly loaded data
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load CSV: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Show error details
            }
        }

        /// <summary>
        /// Designer-wired label click handler (kept to avoid breaking designer event wiring).
        /// No behavior is required for the application logic.
        /// </summary>
        /// <param name="sender">UI label control firing the event.</param>
        /// <param name="e">Event arguments.</param>
        private void label_end_Click(object sender, EventArgs e)
        {
            // Intentionally empty: this exists only because the designer had it wired at some point.
        }

        /// <summary>
        /// Designer-wired label click handler (kept to avoid breaking designer event wiring).
        /// No behavior is required for the application logic.
        /// </summary>
        /// <param name="sender">UI label control firing the event.</param>
        /// <param name="e">Event arguments.</param>
        private void label_start_Click(object sender, EventArgs e)
        {
            // Intentionally empty: this exists only because the designer had it wired at some point.
        }

        /// <summary>
        /// Handles Load Data button: opens the file picker dialog.
        /// Actual loading occurs in the FileOk handler after the user selects a file.
        /// </summary>
        /// <param name="sender">The Load Data button.</param>
        /// <param name="e">Click event data.</param>
        private void button_loadData_Click(object sender, EventArgs e)
        {
            openFileDialog_selectCsv.ShowDialog(); // Show the OpenFileDialog modal window
        }

        /// <summary>
        /// Handles Update button: re-filters cached candles by the selected date range,
        /// normalizes chart axis, refreshes display, and recomputes pattern matches.
        /// </summary>
        /// <param name="sender">The Update button.</param>
        /// <param name="e">Click event data.</param>
        private void button_update_Click(object sender, EventArgs e)
        {
            if (!ValidateDateRange()) return; // Stop immediately if the user selected an invalid date range

            try
            {
                if (_allCandles != null && _allCandles.Count > 0) // Confirm we have data loaded in memory
                {
                    ResetSimulationStateAndAnnotations(); // Any new date range means simulation/annotations must be restarted
                    update(); // Run filter + normalize + display using the UI picker values

                    if (_recognizerManager != null && _currentCandles != null) // Ensure data exists for recognition
                        _recognizerManager.AnalyzeAllSmart(_currentCandles); // Update recognition cache for new visible range
                }
                else
                {
                    MessageBox.Show("Load a CSV file first.", "No data", MessageBoxButtons.OK, MessageBoxIcon.Information); // Tell user there is nothing to update
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to refresh view: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Report error during update
            }
        }

        /// <summary>
        /// Handles OpenFileDialog FileOk: executes the required loading workflow:
        /// read full CSV into memory → filter to date range → normalize axis → display.
        /// </summary>
        /// <param name="sender">The OpenFileDialog control.</param>
        /// <param name="e">Cancel args; allows aborting if validation fails.</param>
        private void openFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!ValidateDateRange()) // Validate before touching data/loading
            {
                e.Cancel = true; // Block the dialog "Ok" action
                return; // Exit the handler early
            }

            try
            {
                readCandlesticksFromFile(); // Step 1: load all candles from selected CSV into _allCandles
                filterCandlesticks(); // Step 2: create _currentCandles based on the date pickers
                normalize(); // Step 3: set Y-axis min/max with padding
                displayCandlesticks(); // Step 4: bind candles to the chart

                if (_recognizerManager != null && _currentCandles != null) // Ensure manager and visible data exist
                    _recognizerManager.AnalyzeAllSmart(_currentCandles); // Precompute recognition cache for visible data

                ResetSimulationStateAndAnnotations(); // Reset simulation and clear annotations after new load
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load CSV: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Show load error
            }
        }

        /// <summary>
        /// Validates that the Start date is not after the End date (inclusive range).
        /// </summary>
        /// <returns>True when start &lt;= end; false if the range is invalid.</returns>
        private bool ValidateDateRange()
        {
            var start = dateTimePicker_start.Value.Date; // Get start date ignoring time
            var end = dateTimePicker_end.Value.Date; // Get end date ignoring time

            if (start > end) // Check whether user inverted the range
            {
                MessageBox.Show("Start date must be earlier than or equal to End date.", "Invalid Date Range", MessageBoxButtons.OK, MessageBoxIcon.Warning); // Inform user
                return false; // Report invalid range to caller
            }

            return true; // Report valid range to caller
        }

        /// <summary>
        /// Reads a CSV file from disk and converts each valid row into a smart candlestick object.
        /// The output list is sorted chronologically by date before being returned.
        /// </summary>
        /// <param name="csvPath">Path to the CSV file.</param>
        /// <returns>Chronologically sorted list of parsed aSmartCandlestick objects.</returns>
        private List<aSmartCandlestick> readCandlesticksFromFile(string csvPath)
        {
            var result = new List<aSmartCandlestick>(); // Create list that will hold parsed candles

            using (var reader = new StreamReader(csvPath, Encoding.UTF8)) // Open CSV stream using UTF-8 text
            {
                if (reader.EndOfStream) return result; // If there is no content, return empty list

                var headerLine = reader.ReadLine(); // Read header line
                if (string.IsNullOrWhiteSpace(headerLine)) return result; // If header is missing, return empty list

                var headerFields = ParseCsvLine(headerLine); // Split header into tokens safely
                for (int i = 0; i < headerFields.Count; i++) // Walk each header cell
                    headerFields[i] = CleanField(headerFields[i]); // Normalize header cell text (trim, unquote, unescape)

                int idxDate = FindIndex(headerFields, "Date"); // Locate Date column
                int idxOpen = FindIndex(headerFields, "Open"); // Locate Open column
                int idxHigh = FindIndex(headerFields, "High"); // Locate High column
                int idxLow = FindIndex(headerFields, "Low"); // Locate Low column
                int idxClose = FindIndex(headerFields, "Close"); // Locate Close column
                int idxVolume = FindIndex(headerFields, "Volume"); // Locate Volume column

                if (idxDate < 0 || idxOpen < 0 || idxHigh < 0 || idxLow < 0 || idxClose < 0 || idxVolume < 0) // Ensure all required headers exist
                    throw new InvalidOperationException("CSV missing required headers: Date, Open, High, Low, Close, Volume."); // Hard fail because mapping would be unsafe

                while (!reader.EndOfStream) // Process each remaining data row
                {
                    var line = reader.ReadLine(); // Read next row
                    if (string.IsNullOrEmpty(line)) continue; // Skip blank lines

                    var fields = ParseCsvLine(line); // Split row using CSV-safe parsing
                    if (fields.Count == 0) continue; // If nothing parsed, skip row

                    var dateRaw = CleanField(fields[idxDate]); // Get Date cell text
                    if (!DateTime.TryParse(dateRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt)) continue; // Skip invalid dates

                    var d = dt.Date; // Convert to date-only

                    if (!double.TryParse(CleanField(fields[idxOpen]), NumberStyles.Any, CultureInfo.InvariantCulture, out double open)) continue; // Parse Open
                    if (!double.TryParse(CleanField(fields[idxHigh]), NumberStyles.Any, CultureInfo.InvariantCulture, out double high)) continue; // Parse High
                    if (!double.TryParse(CleanField(fields[idxLow]), NumberStyles.Any, CultureInfo.InvariantCulture, out double low)) continue; // Parse Low
                    if (!double.TryParse(CleanField(fields[idxClose]), NumberStyles.Any, CultureInfo.InvariantCulture, out double close)) continue; // Parse Close

                    var volStr = CleanField(fields[idxVolume]); // Get Volume as string
                    long volume; // Storage for parsed volume value

                    if (volStr.IndexOf('.') >= 0) // Some datasets store volume as "12345.0" instead of integer
                    {
                        if (!long.TryParse(volStr.Split('.')[0], NumberStyles.Any, CultureInfo.InvariantCulture, out volume)) continue; // Parse integer part only
                    }
                    else
                    {
                        if (!long.TryParse(volStr, NumberStyles.Any, CultureInfo.InvariantCulture, out volume)) continue; // Parse directly
                    }

                    if (high < low) continue; // Reject logically invalid candles where High is below Low

                    var sc = new aSmartCandlestick(d, open, high, low, close, volume); // Construct smart candlestick (it computes derived properties)
                    result.Add(sc); // Store the parsed candle
                }
            }

            result.Sort((a, b) => a.Date.CompareTo(b.Date)); // Ensure chronological ordering for plotting
            return result; // Return parsed and sorted candle list
        }

        /// <summary>
        /// Reads the CSV file currently selected in the OpenFileDialog and stores results in _allCandles.
        /// </summary>
        private void readCandlesticksFromFile()
        {
            var path = openFileDialog_selectCsv.FileName; // Get selected filename from dialog
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) // Validate that the file exists
                throw new FileNotFoundException("Selected file not found."); // Stop execution with clear error

            _allCandles = readCandlesticksFromFile(path); // Load and cache full dataset in memory
        }

        /// <summary>
        /// Filters a provided candle list by an inclusive date range and returns a new list.
        /// </summary>
        /// <param name="unfilteredList">Full list to filter.</param>
        /// <param name="startDate">Start date inclusive.</param>
        /// <param name="endDate">End date inclusive.</param>
        /// <returns>New list containing only candles whose Date falls inside the inclusive interval.</returns>
        private List<aSmartCandlestick> filterCandlesticks(List<aSmartCandlestick> unfilteredList, DateTime startDate, DateTime endDate)
        {
            if (unfilteredList == null) return new List<aSmartCandlestick>(); // Return empty if input is null
            if (startDate > endDate) return new List<aSmartCandlestick>(); // Return empty if range is inverted
            return unfilteredList.FindAll(c => c.Date >= startDate && c.Date <= endDate); // Keep only candles in-range
        }

        /// <summary>
        /// Filters the master dataset _allCandles based on current UI date pickers and stores into _currentCandles.
        /// </summary>
        private void filterCandlesticks()
        {
            var start = dateTimePicker_start.Value.Date; // Read UI start date
            var end = dateTimePicker_end.Value.Date; // Read UI end date
            _currentCandles = filterCandlesticks(_allCandles, start, end); // Compute filtered subset for display
        }

        /// <summary>
        /// Computes a padded Y-axis range based on candle Low/High values.
        /// Padding is applied as 2% of the visible price span.
        /// </summary>
        /// <param name="candles">Candles to inspect for min/max bounds.</param>
        /// <returns>(minY, maxY) bounds with padding; returns (NaN, NaN) if input is null/empty.</returns>
        private (double minY, double maxY) normalize(List<aSmartCandlestick> candles)
        {
            if (candles == null || candles.Count == 0) return (double.NaN, double.NaN); // No data => no meaningful axis

            double minLow = double.MaxValue; // Start min tracker high so first Low will replace it
            double maxHigh = double.MinValue; // Start max tracker low so first High will replace it

            for (int i = 0; i < candles.Count; i++) // Loop through all visible candles
            {
                var c = candles[i]; // Grab current candle object
                if (c.Low < minLow) minLow = c.Low; // Update global minimum
                if (c.High > maxHigh) maxHigh = c.High; // Update global maximum
            }

            double range = Math.Max(1e-9, maxHigh - minLow); // Avoid zero-range (prevents degenerate axis)
            double pad = range * 0.02; // Compute 2% padding
            return (minLow - pad, maxHigh + pad); // Return padded lower and upper bounds
        }

        /// <summary>
        /// Applies the normalized Y-axis range based on the current visible candles (_currentCandles)
        /// to the OHLC chart area.
        /// </summary>
        private void normalize()
        {
            if (_currentCandles == null || _currentCandles.Count == 0) return; // Nothing to scale if no candles are visible

            var area = chart_PanelMain.ChartAreas["ChartArea_OHLC"]; // Get reference to the OHLC chart area
            var range = normalize(_currentCandles); // Compute axis bounds with padding

            if (double.IsNaN(range.minY) || double.IsNaN(range.maxY)) return; // Abort if bounds are invalid (NaN)

            area.AxisY.IsStartedFromZero = false; // Use computed bounds instead of forcing zero baseline
            area.AxisY.Minimum = range.minY; // Set Y min
            area.AxisY.Maximum = range.maxY; // Set Y max
            area.AxisY.LabelStyle.Format = "0.00"; // Format tick labels as 2 decimals

            chart_PanelMain.Invalidate(); // Request chart redraw
        }

        /// <summary>
        /// Builds a two-line chart title using the loaded file name and the current UI date range.
        /// </summary>
        /// <returns>Display title string to be assigned to chart titles.</returns>
        private string BuildChartTitle()
        {
            var start = dateTimePicker_start.Value.Date; // Read start from UI
            var end = dateTimePicker_end.Value.Date; // Read end from UI
            var path = openFileDialog_selectCsv.FileName; // Get loaded CSV path (if any)

            var name = string.IsNullOrEmpty(path) ? "Stock" : Path.GetFileNameWithoutExtension(path); // Use "Stock" fallback if file name missing
            var top = name; // First title line: symbol/period encoded in file name
            var bottom = start.ToString("M/d/yyyy", CultureInfo.InvariantCulture) + " – " + end.ToString("M/d/yyyy", CultureInfo.InvariantCulture); // Second line: date interval

            return top + Environment.NewLine + Environment.NewLine + bottom; // Combine lines with spacing
        }

        /// <summary>
        /// Applies presentation-level chart configuration after binding:
        /// removes legend, sets title, and indexes X-values for daily data so missing days do not create gaps.
        /// </summary>
        private void ApplyChartPresentation()
        {
            if (chart_PanelMain == null || chart_PanelMain.ChartAreas.Count < 2) return; // Require chart and both areas

            var path = openFileDialog_selectCsv.FileName; // Get the file currently loaded
            var fileNoExt = string.IsNullOrEmpty(path) ? string.Empty : Path.GetFileNameWithoutExtension(path); // Derive base file name
            bool isDaily = fileNoExt.EndsWith("-Day", StringComparison.OrdinalIgnoreCase); // Infer daily periodicity by naming convention

            var sOhlc = chart_PanelMain.Series["Series_OHLC"]; // Candlestick series reference
            var sVol = chart_PanelMain.Series["Series_Volume"]; // Volume series reference
            var areaOhlc = chart_PanelMain.ChartAreas["ChartArea_OHLC"]; // Main price area
            var areaVol = chart_PanelMain.ChartAreas["ChartArea_Volume"]; // Volume area

            sOhlc.IsXValueIndexed = isDaily; // If daily, treat X values as indexed to eliminate weekend gaps
            sVol.IsXValueIndexed = isDaily; // Keep volume aligned with candle positions
            sOhlc.CustomProperties = "PriceDownColor=" + ColorTranslator.ToHtml(UiBear) + ", PriceUpColor=" + ColorTranslator.ToHtml(UiBull); // Use softer candle colors
            sOhlc.BorderWidth = 1; // Keep candle outlines thin and crisp
            sVol.Color = Color.FromArgb(170, UiAccent); // Semi-opaque volume bars
            sVol.BorderWidth = 0; // Avoid visual noise on volume bars

            chart_PanelMain.Palette = ChartColorPalette.None; // Use explicit series colors
            chart_PanelMain.BackColor = UiWindow; // Blend chart into form background
            chart_PanelMain.BorderlineColor = UiBorder; // Subtle frame
            chart_PanelMain.BorderlineDashStyle = ChartDashStyle.Solid;
            chart_PanelMain.BorderlineWidth = 1;
            chart_PanelMain.AntiAliasing = AntiAliasingStyles.All; // Smooth lines and shapes
            chart_PanelMain.TextAntiAliasingQuality = TextAntiAliasingQuality.High;

            areaOhlc.BackColor = Color.White;
            areaOhlc.BackSecondaryColor = Color.FromArgb(247, 250, 255);
            areaOhlc.BackGradientStyle = GradientStyle.TopBottom;
            areaOhlc.BorderColor = UiBorder;
            areaOhlc.BorderWidth = 1;
            areaOhlc.Position.Auto = false; // Use explicit split between price and volume
            areaOhlc.Position.X = 4f;
            areaOhlc.Position.Y = 8f;
            areaOhlc.Position.Width = 92f;
            areaOhlc.Position.Height = 62f;

            areaOhlc.AxisX.LabelStyle.Enabled = false; // Show date labels only on the volume pane
            areaOhlc.AxisX.MajorGrid.Enabled = false;
            areaOhlc.AxisX.LineColor = UiBorder;
            areaOhlc.AxisX.LabelStyle.ForeColor = UiMutedText;
            areaOhlc.AxisY.LabelStyle.ForeColor = UiMutedText;
            areaOhlc.AxisY.LineColor = UiBorder;
            areaOhlc.AxisY.MajorGrid.Enabled = true;
            areaOhlc.AxisY.MajorGrid.LineColor = UiGrid;
            areaOhlc.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            areaVol.AlignWithChartArea = areaOhlc.Name;
            areaVol.BackColor = Color.White;
            areaVol.BackSecondaryColor = Color.FromArgb(247, 250, 255);
            areaVol.BackGradientStyle = GradientStyle.TopBottom;
            areaVol.BorderColor = UiBorder;
            areaVol.BorderWidth = 1;
            areaVol.Position.Auto = false;
            areaVol.Position.X = 4f;
            areaVol.Position.Y = 73f;
            areaVol.Position.Width = 92f;
            areaVol.Position.Height = 19f;

            areaVol.AxisX.MajorGrid.Enabled = false;
            areaVol.AxisX.LineColor = UiBorder;
            areaVol.AxisX.LabelStyle.ForeColor = UiMutedText;
            areaVol.AxisY.IsStartedFromZero = true;
            areaVol.AxisY.MajorGrid.Enabled = false;
            areaVol.AxisY.LabelStyle.Enabled = false;
            areaVol.AxisY.LineColor = UiBorder;

            if (chart_PanelMain.Legends.Count > 0) // If a legend exists
                chart_PanelMain.Legends.Clear(); // Remove it to avoid wasting chart space

            chart_PanelMain.Titles.Clear(); // Remove existing titles before adding a new one

            var titleText = BuildChartTitle(); // Build the combined title text
            var title = new Title(); // Create a new chart title object
            title.Text = titleText; // Set title text
            title.Alignment = ContentAlignment.TopCenter; // Center title horizontally
            title.ForeColor = UiText;
            title.Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold);
            title.Docking = Docking.Top;

            chart_PanelMain.Titles.Add(title); // Add title to chart
        }

        /// <summary>
        /// Applies a cohesive style to form chrome and controls so the dashboard looks intentional and readable.
        /// </summary>
        private void ApplyVisualTheme()
        {
            this.BackColor = UiWindow;
            this.ForeColor = UiText;
            this.Text = "Stock Analyzer";
            this.Font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);

            if (flowLayoutPanel_top != null)
            {
                flowLayoutPanel_top.BackColor = UiPanel;
                flowLayoutPanel_top.Padding = new Padding(10, 8, 10, 8);
            }

            if (flowLayoutPanel_timer != null)
            {
                flowLayoutPanel_timer.BackColor = UiPanelSoft;
                flowLayoutPanel_timer.Padding = new Padding(10, 6, 10, 0);
            }

            if (tableLayoutPanel_options != null)
                tableLayoutPanel_options.BackColor = UiWindow;

            if (label_start != null)
            {
                label_start.ForeColor = UiText;
                label_start.Text = "Start:";
            }

            if (label_end != null)
            {
                label_end.ForeColor = UiText;
                label_end.Text = "End:";
            }

            if (dateTimePicker_start != null)
                dateTimePicker_start.CalendarMonthBackground = Color.White;

            if (dateTimePicker_end != null)
                dateTimePicker_end.CalendarMonthBackground = Color.White;

            StyleActionButton(button_loadData, UiAccent, Color.White);
            StyleActionButton(button_update, Color.FromArgb(67, 91, 122), Color.White);
            StyleActionButton(button_simulate, Color.FromArgb(233, 169, 80), Color.FromArgb(40, 34, 20));
            if (button_loadData != null) button_loadData.Text = "Load CSV";

            if (comboBox_patterns != null)
            {
                comboBox_patterns.BackColor = Color.White;
                comboBox_patterns.ForeColor = UiText;
                comboBox_patterns.FlatStyle = FlatStyle.Flat;
                comboBox_patterns.Width = 190;
            }

            if (textBox_valueTimer != null)
            {
                textBox_valueTimer.BackColor = Color.White;
                textBox_valueTimer.ForeColor = UiText;
                textBox_valueTimer.BorderStyle = BorderStyle.FixedSingle;
                textBox_valueTimer.TextAlign = HorizontalAlignment.Center;
            }

            ArrangeTopControlLayout();
        }

        /// <summary>
        /// Normalizes button styling across the dashboard.
        /// </summary>
        private void StyleActionButton(Button button, Color backColor, Color foreColor)
        {
            if (button == null) return;

            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold, GraphicsUnit.Point);
            button.Padding = new Padding(10, 6, 10, 6);
            button.AutoSize = false;
            button.Size = new Size(110, 40);
            button.Margin = new Padding(6, 6, 6, 6);
        }

        /// <summary>
        /// Composes and sizes top panels so control positions remain clean and predictable.
        /// </summary>
        private void ArrangeTopControlLayout()
        {
            if (flowLayoutPanel_top == null || flowLayoutPanel_timer == null) return;

            flowLayoutPanel_top.SuspendLayout();
            flowLayoutPanel_top.AutoSize = false;
            flowLayoutPanel_top.Height = 66;
            flowLayoutPanel_top.FlowDirection = FlowDirection.LeftToRight;
            flowLayoutPanel_top.RightToLeft = RightToLeft.No;
            flowLayoutPanel_top.WrapContents = false;
            flowLayoutPanel_top.AutoScroll = true;
            flowLayoutPanel_top.Padding = new Padding(10, 8, 10, 8);

            if (!_toolbarLayoutComposed)
            {
                flowLayoutPanel_top.Controls.Clear();
                if (comboBox_patterns != null) flowLayoutPanel_top.Controls.Add(comboBox_patterns);
                if (button_simulate != null) flowLayoutPanel_top.Controls.Add(button_simulate);
                if (button_update != null) flowLayoutPanel_top.Controls.Add(button_update);
                if (button_loadData != null) flowLayoutPanel_top.Controls.Add(button_loadData);
                if (label_start != null) flowLayoutPanel_top.Controls.Add(label_start);
                if (dateTimePicker_start != null) flowLayoutPanel_top.Controls.Add(dateTimePicker_start);
                if (label_end != null) flowLayoutPanel_top.Controls.Add(label_end);
                if (dateTimePicker_end != null) flowLayoutPanel_top.Controls.Add(dateTimePicker_end);
                _toolbarLayoutComposed = true;
            }

            if (comboBox_patterns != null)
            {
                comboBox_patterns.Width = this.ClientSize.Width < 1280 ? 190 : 240;
                comboBox_patterns.Margin = new Padding(6, 6, 10, 6);
            }

            if (label_start != null)
            {
                label_start.Text = "Start:";
                label_start.AutoSize = true;
                label_start.Margin = new Padding(14, 10, 6, 6);
            }

            if (dateTimePicker_start != null)
            {
                dateTimePicker_start.Format = DateTimePickerFormat.Short;
                dateTimePicker_start.Width = 132;
                dateTimePicker_start.Margin = new Padding(0, 6, 10, 6);
            }

            if (label_end != null)
            {
                label_end.Text = "End:";
                label_end.AutoSize = true;
                label_end.Margin = new Padding(10, 10, 6, 6);
            }

            if (dateTimePicker_end != null)
            {
                dateTimePicker_end.Format = DateTimePickerFormat.Short;
                dateTimePicker_end.Width = 132;
                dateTimePicker_end.Margin = new Padding(0, 6, 6, 6);
            }

            flowLayoutPanel_top.ResumeLayout();

            flowLayoutPanel_timer.SuspendLayout();
            flowLayoutPanel_timer.AutoSize = false;
            flowLayoutPanel_timer.Height = 54;
            flowLayoutPanel_timer.FlowDirection = FlowDirection.LeftToRight;
            flowLayoutPanel_timer.RightToLeft = RightToLeft.No;
            flowLayoutPanel_timer.WrapContents = false;
            flowLayoutPanel_timer.AutoScroll = true;
            flowLayoutPanel_timer.Padding = new Padding(10, 6, 10, 0);

            if (textBox_valueTimer != null)
            {
                textBox_valueTimer.Width = 72;
                textBox_valueTimer.Margin = new Padding(8, 6, 10, 6);
            }

            if (hScrollBar_timer != null)
            {
                int width = Math.Max(260, Math.Min(560, this.ClientSize.Width / 3));
                hScrollBar_timer.Width = width;
                hScrollBar_timer.Margin = new Padding(0, 11, 10, 6);
            }

            flowLayoutPanel_timer.ResumeLayout();
        }

        /// <summary>
        /// Keeps top panel dimensions and control spacing correct after window resize.
        /// </summary>
        private void Form_Main_Resize(object sender, EventArgs e)
        {
            ArrangeTopControlLayout();
        }

        /// <summary>
        /// Chooses an annotation color based on recognizer semantic meaning.
        /// </summary>
        private Color ResolveAnnotationColor(Recognizer recognizer, aSmartCandlestick candle)
        {
            if (recognizer == null) return UiAccent;

            var name = recognizer.Name ?? string.Empty;

            if (name.IndexOf("Bullish", StringComparison.OrdinalIgnoreCase) >= 0) return UiBull;
            if (name.IndexOf("Bearish", StringComparison.OrdinalIgnoreCase) >= 0) return UiBear;

            if (candle != null)
            {
                if (candle.isBullish) return UiBull;
                if (candle.isBearish) return UiBear;
            }

            return UiAccent;
        }

        /// <summary>
        /// Displays a candle list on the chart using data binding. Clears annotations each time a new set is bound.
        /// The skipRecognition flag is kept for future flexibility; recognition is currently handled after simulation.
        /// </summary>
        /// <param name="candles">Candles to display (null treated as empty).</param>
        /// <param name="skipRecognition">Currently not used for recognition logic; kept for extension.</param>
        /// <returns>Number of candles displayed (points bound).</returns>
        private int displayCandlesticks(List<aSmartCandlestick> candles, bool skipRecognition)
        {
            if (chart_PanelMain != null) // Ensure chart exists
                chart_PanelMain.Annotations.Clear(); // Remove old pattern markers when rebinding

            if (candles == null) candles = new List<aSmartCandlestick>(); // Treat null as empty dataset

            if (candles.Count == 0) // Handle empty dataset case
            {
                chart_PanelMain.DataSource = null; // Clear binding
                chart_PanelMain.Series["Series_OHLC"].Points.Clear(); // Clear OHLC points
                chart_PanelMain.Series["Series_Volume"].Points.Clear(); // Clear Volume points
                chart_PanelMain.Titles.Clear(); // Clear chart title
                chart_PanelMain.Invalidate(); // Refresh chart visuals
                return 0; // No candles displayed
            }

            chart_PanelMain.DataSource = candles; // Assign the binding source to the chart
            ApplyChartPresentation(); // Apply formatting rules (title, legend removal, x indexing)

            var sOhlc = chart_PanelMain.Series["Series_OHLC"]; // Grab OHLC series
            sOhlc.XValueMember = "Date"; // Map X values to candle Date
            sOhlc.YValueMembers = "High,Low,Open,Close"; // Map Y values for candlestick format

            var sVol = chart_PanelMain.Series["Series_Volume"]; // Grab volume series
            sVol.XValueMember = "Date"; // Map X values to Date
            sVol.YValueMembers = "Volume"; // Map Y values to Volume

            chart_PanelMain.DataBind(); // Execute binding (creates chart points)

            _currentCandles = candles; // Store visible candles for later simulation/annotation logic
            return candles.Count; // Return displayed count
        }

        /// <summary>
        /// Convenience overload that displays candles using default flags.
        /// </summary>
        /// <param name="candles">Candles to display.</param>
        /// <returns>Number of candles displayed.</returns>
        private int displayCandlesticks(List<aSmartCandlestick> candles)
        {
            return displayCandlesticks(candles, skipRecognition: false); // Call full version with a default argument value
        }

        /// <summary>
        /// Displays the currently filtered list (_currentCandles) on the chart.
        /// </summary>
        private void displayCandlesticks()
        {
            displayCandlesticks(_currentCandles); // Render the currently filtered candle set
        }

        /// <summary>
        /// Runs the full refresh pipeline using an explicit source list and explicit date bounds:
        /// filter → normalize axis → display.
        /// </summary>
        /// <param name="source">Unfiltered candle source.</param>
        /// <param name="start">Start date inclusive.</param>
        /// <param name="end">End date inclusive.</param>
        /// <returns>Number of candles displayed for the selected range.</returns>
        private int update(List<aSmartCandlestick> source, DateTime start, DateTime end)
        {
            var filtered = filterCandlesticks(source, start, end); // Compute filtered subset for the requested range
            _currentCandles = filtered; // Store filtered candles as the current visible set
            normalize(); // Resize Y-axis around the visible candles
            return displayCandlesticks(filtered); // Bind filtered candles to chart and return count
        }

        /// <summary>
        /// Resets simulation internal state and clears any chart annotations.
        /// This should be called whenever underlying data changes (file or date range).
        /// </summary>
        private void ResetSimulationStateAndAnnotations()
        {
            if (this.timer_Simulate != null) // Ensure timer exists
                this.timer_Simulate.Stop(); // Stop simulation if it is running

            _simSource = null; // Clear simulation source dataset
            _simulatedCandles = null; // Clear progressive list
            _simulatedIndex = 0; // Reset next index
            _simulationActive = false; // Mark simulation inactive
            _simulationCompleted = false; // Mark simulation not completed

            if (chart_PanelMain != null) // Ensure chart exists
            {
                chart_PanelMain.Annotations.Clear(); // Remove any arrows/rectangles
                chart_PanelMain.Invalidate(); // Redraw chart without annotations
            }

            if (this.button_simulate != null) // Ensure simulate button exists
                this.button_simulate.Text = "Simulate"; // Reset label to initial state
        }

        /// <summary>
        /// Runs the refresh pipeline using the UI date pickers and cached full dataset:
        /// filter → normalize axis → display.
        /// </summary>
        private void update()
        {
            var start = dateTimePicker_start.Value.Date; // Read UI start date
            var end = dateTimePicker_end.Value.Date; // Read UI end date
            update(_allCandles, start, end); // Run explicit update pipeline using cached dataset
        }

        /// <summary>
        /// Parses a CSV line into a list of fields while supporting quoted values and escaped quotes.
        /// </summary>
        /// <param name="line">Raw CSV line.</param>
        /// <returns>List of field strings in the order they appear.</returns>
        private static List<string> ParseCsvLine(string line)
        {
            var result = new List<string>(); // Collected output fields
            if (line == null) return result; // Null input produces empty output

            var sb = new StringBuilder(); // Builds the current field
            bool inQuotes = false; // Tracks quoting state

            for (int i = 0; i < line.Length; i++) // Scan each character in line
            {
                char c = line[i]; // Current character

                if (c == '"') // If we see a quote character
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"') // If inside quotes and next char is also a quote => escaped quote
                    {
                        sb.Append('"'); // Append a literal quote to output
                        i++; // Skip over the second quote
                    }
                    else
                    {
                        inQuotes = !inQuotes; // Flip quoting state when encountering a quote boundary
                    }
                }
                else if (c == ',' && !inQuotes) // Comma splits fields only when not in quotes
                {
                    result.Add(sb.ToString()); // Commit current field string
                    sb.Clear(); // Reset buffer for next field
                }
                else
                {
                    sb.Append(c); // Normal character → accumulate into current field
                }
            }

            result.Add(sb.ToString()); // Add last field after loop ends
            return result; // Return list of parsed fields
        }

        /// <summary>
        /// Finds the index of a target header name (case-insensitive) inside a header list.
        /// </summary>
        /// <param name="headers">List of header field names.</param>
        /// <param name="target">Target header to locate.</param>
        /// <returns>Index of header if found; otherwise -1.</returns>
        private static int FindIndex(List<string> headers, string target)
        {
            for (int i = 0; i < headers.Count; i++) // Scan header fields
            {
                if (string.Equals(headers[i], target, StringComparison.OrdinalIgnoreCase)) // Compare ignoring case
                    return i; // Return the index on first match
            }

            return -1; // Return -1 if no match exists
        }

        /// <summary>
        /// Cleans a CSV field by trimming whitespace, removing wrapping quotes, and unescaping doubled quotes.
        /// </summary>
        /// <param name="s">Raw field string extracted from CSV.</param>
        /// <returns>Cleaned/normalized field text.</returns>
        private static string CleanField(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty; // Normalize null/empty to empty string

            s = s.Trim(); // Remove surrounding whitespace

            if (s.Length >= 2 && s[0] == '"' && s[s.Length - 1] == '"') // If it is wrapped in quotes
                s = s.Substring(1, s.Length - 2); // Remove the outer quote characters

            s = s.Replace("\"\"", "\""); // Convert doubled quotes ("") into a single literal quote

            if (s.Length >= 2 && s[0] == '"' && s[s.Length - 1] == '"') // If still wrapped (defensive)
                s = s.Substring(1, s.Length - 2); // Remove outer quotes again

            return s; // Provide cleaned field
        }

        /// <summary>
        /// Designer-wired start date change handler. We do not auto-update in this project,
        /// because updates are performed explicitly using the Update button.
        /// </summary>
        /// <param name="sender">Start DateTimePicker.</param>
        /// <param name="e">Event args.</param>
        private void dateTimePicker_start_ValueChanged(object sender, EventArgs e)
        {
            // Intentionally empty: user must click Update for changes to take effect.
        }

        /// <summary>
        /// Designer-wired end date change handler. We keep it for designer wiring stability
        /// but do not auto-refresh from this event.
        /// </summary>
        /// <param name="sender">End DateTimePicker.</param>
        /// <param name="e">Event args.</param>
        private void dateTimePicker_end_ValueChanged(object sender, EventArgs e)
        {
            // Intentionally empty: user must click Update for changes to take effect.
        }

        /// <summary>
        /// Designer-wired paint handler kept for compatibility; we do not custom paint this panel.
        /// </summary>
        /// <param name="sender">Panel raising the paint event.</param>
        /// <param name="e">Paint event args.</param>
        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {
            // No custom drawing: chart control handles its own rendering.
        }

        /// <summary>
        /// Designer-wired paint handler kept for compatibility; no custom drawing is performed.
        /// </summary>
        /// <param name="sender">Panel raising the paint event.</param>
        /// <param name="e">Paint event args.</param>
        private void splitContainer_main_Panel2_Paint(object sender, PaintEventArgs e)
        {
            // No custom drawing implemented for this panel.
        }

        /// <summary>
        /// Handles the Simulate button: initializes simulation if needed, pauses if running,
        /// or resumes if paused. At completion, it triggers pattern annotation.
        /// </summary>
        /// <param name="sender">Simulate button.</param>
        /// <param name="e">Click event args.</param>
        private void button_simulate_Click(object sender, EventArgs e)
        {
            if (_allCandles == null || _allCandles.Count == 0) // Ensure data has been loaded
            {
                MessageBox.Show("Load a CSV file first.", "No data", MessageBoxButtons.OK, MessageBoxIcon.Information); // Prompt user to load data
                return; // Exit because we cannot simulate without data
            }

            if (!ValidateDateRange()) return; // Enforce that the selected range is logically valid

            if (this.timer_Simulate != null && this.timer_Simulate.Enabled) // If simulation is currently running
            {
                this.timer_Simulate.Stop(); // Stop timer to pause simulation
                _simulationActive = false; // Update internal state flag
                this.button_simulate.Text = "Resume"; // Tell user that next click resumes
                return; // Exit after pausing
            }

            if (_simSource == null || _simulatedCandles == null || _simulatedIndex >= _simSource.Count) // If not initialized or needs restart
            {
                var start = dateTimePicker_start.Value.Date; // Read start date from UI
                var end = dateTimePicker_end.Value.Date; // Read end date from UI

                _simSource = filterCandlesticks(_allCandles, start, end); // Build source list for simulation using date filter

                if (_recognizerManager != null) // Ensure manager exists
                    _recognizerManager.AnalyzeAllSmart(_simSource); // Precompute matches for this simulation dataset

                if (_simSource == null || _simSource.Count == 0) // If selected range produced no rows
                {
                    MessageBox.Show("No data in the selected date range.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); // Inform user
                    return; // Stop since nothing can be simulated
                }

                _simulatedCandles = new List<aSmartCandlestick>(); // Start with an empty list to display gradually
                _simulatedIndex = 0; // Start pointer at first candle
                _simulationActive = true; // Mark active state
                _simulationCompleted = false; // Mark not completed

                var area = chart_PanelMain.ChartAreas["ChartArea_OHLC"]; // Get OHLC chart area for axis control
                var fullRange = normalize(_simSource); // Compute axis bounds once (keeps axis stable across ticks)

                if (!double.IsNaN(fullRange.minY) && !double.IsNaN(fullRange.maxY)) // If range returned valid numeric bounds
                {
                    area.AxisY.IsStartedFromZero = false; // Use computed bounds instead of forcing 0
                    area.AxisY.Minimum = fullRange.minY; // Apply min
                    area.AxisY.Maximum = fullRange.maxY; // Apply max
                    area.AxisY.LabelStyle.Format = "0.00"; // Use 2 decimal places
                }

                displayCandlesticks(_simulatedCandles); // Clear chart (shows nothing) as simulation starting point
            }

            if (this.timer_Simulate != null) // Ensure timer exists
            {
                this.button_simulate.Text = "Pause"; // Set label to Pause while running
                _simulationActive = true; // Update state flag
                this.timer_Simulate.Start(); // Start ticking simulation
            }
        }

        /// <summary>
        /// Timer tick handler: adds exactly one candle to the visible list, redraws,
        /// and finalizes (annotates) once all candles are shown.
        /// </summary>
        /// <param name="sender">Timer instance.</param>
        /// <param name="e">Tick event args.</param>
        private void timer_Simulate_Tick(object sender, EventArgs e)
        {
            if (this.timer_Simulate != null) // Ensure timer exists
                this.timer_Simulate.Stop(); // Stop immediately to avoid overlapping tick re-entry

            try
            {
                if (_simSource == null || _simulatedCandles == null) // If data is not ready
                {
                    _simulationActive = false; // Mark not active
                    if (this.timer_Simulate != null) this.timer_Simulate.Stop(); // Ensure timer off
                    this.button_simulate.Text = "Simulate"; // Reset UI label
                    return; // Exit because simulation state is invalid
                }

                if (_simulatedIndex >= _simSource.Count) // If we already finished
                {
                    _simulationActive = false; // Mark inactive
                    _simulationCompleted = true; // Mark completed

                    if (_recognizerManager != null) // Ensure manager exists
                    {
                        _currentCandles = _simulatedCandles; // Sync "current" dataset to simulated dataset
                        AnalyzeAndAnnotate(); // Draw annotations for selected pattern
                    }

                    if (this.timer_Simulate != null) this.timer_Simulate.Stop(); // Ensure timer is off
                    this.button_simulate.Text = "Restart"; // Let user restart if desired
                    return; // Exit after finalization
                }

                _simulatedCandles.Add(_simSource[_simulatedIndex]); // Add one new candle to the display list
                _simulatedIndex++; // Move pointer to the next candle

                displayCandlesticks(_simulatedCandles, skipRecognition: true); // Redraw chart with partial list (no annotations yet)

                if (_simulatedIndex >= _simSource.Count) // If we just added the last candle
                {
                    _simulationActive = false; // Stop running
                    _simulationCompleted = true; // Mark completed

                    if (_recognizerManager != null) // Ensure manager exists
                    {
                        _currentCandles = _simulatedCandles; // Sync visible data
                        AnalyzeAndAnnotate(); // Add annotations after completion
                    }

                    this.button_simulate.Text = "Restart"; // Update simulate button text
                    return; // Exit handler after finishing
                }
            }
            finally
            {
                if (_simulationActive && this.timer_Simulate != null) // Only restart timer if simulation is still active
                    this.timer_Simulate.Start(); // Continue stepping candles
            }
        }

        /// <summary>
        /// Creates and returns the complete list of pattern recognizers supported by the application.
        /// </summary>
        /// <returns>List of Recognizer instances configured with their tolerances.</returns>
        private List<Recognizer> BuildRecognizers()
        {
            return new List<Recognizer> // Return a new list instance
            {
                new Recognizer_Doji(0.02), // Detect Doji candles with small body tolerance
                new Recognizer_DojiBullish(0.02), // Detect bullish Doji variant
                new Recognizer_DojiBearish(0.02), // Detect bearish Doji variant
                new Recognizer_DragonflyDoji(0.02), // Detect dragonfly Doji
                new Recognizer_DragonflyDojiBullish(0.02), // Detect bullish dragonfly Doji
                new Recognizer_DragonflyDojiBearish(0.02), // Detect bearish dragonfly Doji
                new Recognizer_GravestoneDoji(0.02), // Detect gravestone Doji
                new Recognizer_GravestoneDojiBullish(0.02), // Detect bullish gravestone Doji
                new Recognizer_GravestoneDojiBearish(0.02), // Detect bearish gravestone Doji
                new Recognizer_Marubozu(0.0), // Detect Marubozu (no body tolerance)
                new Recognizer_BullishMarubozu(0.0), // Detect bullish Marubozu
                new Recognizer_BearishMarubozu(0.0), // Detect bearish Marubozu
                new Recognizer_Hammer(0.01), // Detect hammer with small tolerance
                new Recognizer_BullishHammer(0.01), // Detect bullish hammer
                new Recognizer_BearishHammer(0.01), // Detect bearish hammer
                new Recognizer_InvertedHammer(0.01), // Detect inverted hammer
                new Recognizer_BullishInvertedHammer(0.01), // Detect bullish inverted hammer
                new Recognizer_BearishInvertedHammer(0.01), // Detect bearish inverted hammer
                new Recognizer_Engulfing(), // Detect generic engulfing
                new Recognizer_BullishEngulfing(), // Detect bullish engulfing
                new Recognizer_BearishEngulfing(), // Detect bearish engulfing
                new Recognizer_Harami(), // Detect generic harami
                new Recognizer_BullishHarami(), // Detect bullish harami
                new Recognizer_BearishHarami(), // Detect bearish harami
            };
        }

        /// <summary>
        /// Prepares the patterns ComboBox: binds it to recognizers and sets UI behavior
        /// so users must choose from supported patterns (no free text).
        /// </summary>
        private void EnsurePatternComboBox()
        {
            if (comboBox_patterns == null || _recognizerManager == null) return; // Quit if UI or manager is missing

            comboBox_patterns.DropDownStyle = ComboBoxStyle.DropDownList; // Force selection from list only
            comboBox_patterns.DisplayMember = "Name"; // Display recognizer Name in UI list
            comboBox_patterns.DataSource = _recognizerManager.GetRecognizers(); // Bind recognizer list as the item source

            try
            {
                int maxWidth = 0; // Track the widest item label
                using (var g = comboBox_patterns.CreateGraphics()) // Graphics context for text measurement
                {
                    foreach (var item in comboBox_patterns.Items) // Measure each item
                    {
                        var text = item?.ToString() ?? string.Empty; // Convert item to display text safely
                        var size = TextRenderer.MeasureText(g, text, comboBox_patterns.Font); // Measure pixel width
                        if (size.Width > maxWidth) maxWidth = size.Width; // Update maximum width
                    }
                }

                int padding = 24; // Add extra spacing so text is not clipped
                comboBox_patterns.DropDownWidth = Math.Max(comboBox_patterns.Width, maxWidth + padding); // Apply width
                comboBox_patterns.MaxDropDownItems = 12; // Limit how many items are shown without scrolling
                comboBox_patterns.IntegralHeight = false; // Let WinForms choose a smooth dropdown height
            }
            catch
            {
                // Sizing is best-effort: if measurement fails, default behavior is still fine.
            }

            comboBox_patterns.SelectedIndexChanged -= comboBox_patterns_SelectedIndexChanged; // Avoid attaching twice
            comboBox_patterns.SelectedIndexChanged += comboBox_patterns_SelectedIndexChanged; // Attach selection handler

            if (comboBox_patterns.Items.Count > 0 && comboBox_patterns.SelectedIndex < 0) // If list has items but nothing selected
                comboBox_patterns.SelectedIndex = 0; // Select first recognizer by default
        }

        /// <summary>
        /// Handles ComboBox pattern selection changes.
        /// If (and only if) the simulation has already completed, this redraws annotations
        /// for the newly selected pattern without forcing the user to re-simulate.
        /// </summary>
        /// <param name="sender">ComboBox control.</param>
        /// <param name="e">Event args.</param>
        private void comboBox_patterns_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_simulationCompleted) return; // Do not show markers until simulation completes (per project requirement)
            if (_simulationActive) return; // Ignore changes while the timer is actively stepping candles
            AnalyzeAndAnnotate(); // Recompute and redraw markers for the newly selected pattern
        }

        /// <summary>
        /// Draws chart annotations for the currently selected pattern using cached match indices,
        /// but only after the simulation has reached the end of the selected range.
        /// </summary>
        private void AnalyzeAndAnnotate()
        {
            if (!_simulationCompleted) return; // Do not annotate until simulation shows the full dataset
            if (chart_PanelMain == null || chart_PanelMain.ChartAreas.Count == 0) return; // Do nothing if chart not ready

            chart_PanelMain.Annotations.Clear(); // Remove any previous markers
            if (comboBox_patterns == null) return; // Cannot annotate without a selected pattern

            int recIndex = comboBox_patterns.SelectedIndex; // Get selected recognizer index
            if (recIndex < 0) return; // Quit if nothing selected

            var rec = comboBox_patterns.SelectedItem as Recognizer; // Get selected recognizer instance
            if (rec == null || _currentCandles == null || _currentCandles.Count == 0 || _recognizerManager == null) return; // Require recognizer + data + manager

            var area = chart_PanelMain.ChartAreas["ChartArea_OHLC"]; // Get chart area
            var sOhlc = chart_PanelMain.Series["Series_OHLC"]; // Get OHLC series
            bool isIndexed = sOhlc.IsXValueIndexed; // Determine whether X is index-based (daily mode)

            var indices = _recognizerManager.GetMatchesByIndex(recIndex); // Retrieve match index list for the selected pattern
            if (indices == null || indices.Count == 0) // If nothing matched
            {
                chart_PanelMain.Invalidate(); // Refresh chart anyway
                return; // Exit because there is nothing to draw
            }

            int n = _currentCandles.Count; // Visible candle count (used for scaling spacing)
            const double nMin = 50.0; // Low end for scaling
            const double nMax = 1000.0; // High end for scaling

            double t = (n - nMin) / (nMax - nMin); // Normalize density factor
            if (t < 0) t = 0; // Clamp to 0
            if (t > 1) t = 1; // Clamp to 1

            double basePaddingPercent = 0.15 + t * (0.20 - 0.15); // Increase padding slightly for denser charts
            double axisMin = area.AxisY.Minimum; // Read axis min
            double axisMax = area.AxisY.Maximum; // Read axis max
            double axisSpan = axisMax - axisMin; // Compute span
            double margin = axisSpan * 0.001; // Tiny margin to keep shapes inside axis
            double minArrowDelta = axisSpan * 0.10; // Ensure arrows are visible when candles are tiny

            foreach (int i in indices) // Process every match index
            {
                if (i < 0 || i >= _currentCandles.Count) continue; // Skip invalid indices
                var sc = _recognizerManager.GetSmart(i); // Get computed candle at that index
                if (sc == null) continue; // Skip if manager returned null
                if (_currentCandles[i].Date != sc.Date) continue; // Safety check: ensure cache aligns with display data
                if (i >= sOhlc.Points.Count) continue; // Skip if chart does not have that point

                var chartPoint = sOhlc.Points[i]; // Get point instance used by chart

                if (rec.Lookback == 2) // Two-candle patterns get a rectangle around both candles
                {
                    int firstIdx = i - 1; // First candle index in the pair
                    if (firstIdx >= 0 && firstIdx < sOhlc.Points.Count) // Ensure prior candle exists on chart
                    {
                        var scPrev = _recognizerManager.GetSmart(firstIdx); // Get previous candle data
                        var prevPoint = sOhlc.Points[firstIdx]; // Get previous chart point
                        if (scPrev != null && prevPoint != null) // Ensure both exist
                        {
                            var matchColor = ResolveAnnotationColor(rec, sc); // Color-code the shape by signal context
                            double x1 = isIndexed ? (firstIdx + 1.0) : prevPoint.XValue; // Compute X for first candle
                            double x2 = isIndexed ? (i + 1.0) : chartPoint.XValue; // Compute X for second candle

                            double xLeft; // Rectangle left coordinate
                            double xWidth; // Rectangle width

                            if (isIndexed) // When indexed, candle width is 1 unit
                            {
                                double left = Math.Min(x1, x2) - 0.5; // Extend half unit left
                                double right = Math.Max(x1, x2) + 0.5; // Extend half unit right
                                xLeft = left; // Save left edge
                                xWidth = Math.Max(1e-6, right - left); // Save width (avoid 0)
                            }
                            else // When not indexed, use delta between X values
                            {
                                double dx = Math.Abs(x2 - x1); // Compute X distance
                                if (dx < 1e-9) dx = 1.0; // Avoid zero
                                double half = 0.5 * dx; // Expand around candles
                                xLeft = Math.Min(x1, x2) - half; // Set left edge
                                xWidth = Math.Max(1e-6, dx + 2.0 * half); // Set width
                            }

                            double yLow = Math.Min(scPrev.Low, sc.Low); // Rectangle should start at min of lows
                            double yHigh = Math.Max(scPrev.High, sc.High); // Rectangle top at max of highs
                            double y = Math.Max(axisMin, yLow - margin); // Clamp rectangle bottom inside chart
                            double h = Math.Min(axisMax, yHigh + margin) - y; // Clamp height inside chart

                            var rect = new RectangleAnnotation // Create rectangle annotation object
                            {
                                AxisX = area.AxisX, // Anchor X to chart axis
                                AxisY = area.AxisY, // Anchor Y to chart axis
                                IsSizeAlwaysRelative = false, // Use axis units rather than percentages
                                ClipToChartArea = area.Name, // Clip to visible plot region
                                LineColor = matchColor, // Border color
                                LineWidth = 2, // Border thickness
                                BackColor = Color.FromArgb(24, matchColor), // Soft fill for visibility without blocking candles
                                X = xLeft, // Set left coordinate
                                Y = y, // Set bottom coordinate
                                Width = xWidth, // Set width
                                Height = Math.Max(1e-9, h) // Set height (avoid zero)
                            };

                            chart_PanelMain.Annotations.Add(rect); // Add rectangle to chart annotation collection
                        }
                    }

                    continue; // Skip arrow drawing for two-candle patterns
                }

                double candleHigh = sc.High; // High price of the matched candle
                double candleRange = Math.Max(sc.range, 1e-9); // Range of the candle (protect against zero)
                double offsetFromCandle = Math.Max(candleRange * basePaddingPercent, minArrowDelta); // Choose arrow offset (scaled)
                double targetTopY = candleHigh + offsetFromCandle; // Desired top Y for arrow
                double arrowTopY = Math.Max(axisMin + margin, Math.Min(axisMax - margin, targetTopY)); // Clamp arrow start within axis
                double arrowHeight = candleHigh - arrowTopY; // Arrow height down to candle

                double xCand = isIndexed ? (i + 1.0) : chartPoint.XValue; // X coordinate used to place arrow above candle
                var arrowColor = ResolveAnnotationColor(rec, sc); // Color-code arrow by bullish/bearish context

                var line = new LineAnnotation // Create arrow annotation
                {
                    AxisX = area.AxisX, // Anchor X to axis
                    AxisY = area.AxisY, // Anchor Y to axis
                    IsSizeAlwaysRelative = false, // Use axis units
                    ClipToChartArea = area.Name, // Clip within chart area
                    LineColor = arrowColor, // Arrow color
                    LineWidth = 2, // Arrow thickness
                    StartCap = LineAnchorCapStyle.None, // No start cap
                    EndCap = LineAnchorCapStyle.Arrow, // Arrow head at end
                    X = xCand, // X position
                    Y = arrowTopY, // Start Y position
                    Width = 0, // Vertical line
                    Height = arrowHeight // Height downwards to candle
                };

                chart_PanelMain.Annotations.Add(line); // Add arrow annotation to chart
            }

            chart_PanelMain.Invalidate(); // Ask chart to repaint so new annotations appear
        }

        /// <summary>
        /// Configures the scrollbar and textbox used to control simulation speed.
        /// Ensures valid range, synchronized display, and event wiring to keep both controls consistent.
        /// </summary>
        private void ConfigureTimerControls()
        {
            if (hScrollBar_timer == null || textBox_valueTimer == null) return; // Require both controls to exist

            hScrollBar_timer.Minimum = TIMER_MIN; // Apply minimum speed value

            // Note: WinForms ScrollBar Maximum includes LargeChange behavior; setting Maximum like this ensures TIMER_MAX is reachable.
            // Helpful reference: Microsoft docs for ScrollBar.Maximum property.
            // https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.scrollbar.maximum
            hScrollBar_timer.Maximum = TIMER_MAX + hScrollBar_timer.LargeChange - 1; // Adjust max so user can select TIMER_MAX

            int initial = timer_Simulate != null ? timer_Simulate.Interval : TIMER_MIN; // Read current timer interval or default
            initial = Math.Max(TIMER_MIN, Math.Min(TIMER_MAX, initial)); // Clamp to allowed range

            hScrollBar_timer.Value = initial; // Set scrollbar to initial interval
            textBox_valueTimer.Text = initial.ToString(CultureInfo.InvariantCulture); // Show same interval in textbox

            hScrollBar_timer.ValueChanged -= hScrollBar_timer_ValueChanged; // Remove previous wiring to avoid duplicates
            hScrollBar_timer.ValueChanged += hScrollBar_timer_ValueChanged; // Subscribe to scrollbar changes

            textBox_valueTimer.TextChanged -= textBox_valueTimer_TextChanged; // Remove previous wiring to avoid duplicates
            textBox_valueTimer.TextChanged += textBox_valueTimer_TextChanged; // Subscribe to textbox changes
        }

        /// <summary>
        /// Scrollbar handler: updates timer interval and syncs the textbox display accordingly.
        /// </summary>
        /// <param name="sender">Scrollbar control.</param>
        /// <param name="e">Event args.</param>
        private void hScrollBar_timer_ValueChanged(object sender, EventArgs e)
        {
            int v = Math.Max(TIMER_MIN, Math.Min(TIMER_MAX, hScrollBar_timer.Value)); // Clamp value from control into valid range

            if (timer_Simulate != null) timer_Simulate.Interval = v; // Apply new interval to the simulation timer

            var s = v.ToString(CultureInfo.InvariantCulture); // Convert to consistent numeric string
            if (textBox_valueTimer.Text != s) textBox_valueTimer.Text = s; // Sync textbox only if it differs (prevents loops)
        }

        /// <summary>
        /// Textbox handler: parses typed interval and updates scrollbar (which also updates the timer).
        /// </summary>
        /// <param name="sender">Textbox control.</param>
        /// <param name="e">Event args.</param>
        private void textBox_valueTimer_TextChanged(object sender, EventArgs e)
        {
            if (!int.TryParse(textBox_valueTimer.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int val)) // Validate integer input
                return; // Ignore invalid input (non-numeric)

            val = Math.Max(TIMER_MIN, Math.Min(TIMER_MAX, val)); // Clamp to valid range
            if (hScrollBar_timer.Value != val) hScrollBar_timer.Value = val; // Sync scrollbar (its handler will update timer)
        }

        /// <summary>
        /// Designer-wired chart click handler kept for compatibility; currently unused by project logic.
        /// </summary>
        /// <param name="sender">Chart control.</param>
        /// <param name="e">Event args.</param>
        private void chart_PanelMain_Click(object sender, EventArgs e)
        {
            // No click actions implemented; chart is view-only in this version.
        }
    }
}
