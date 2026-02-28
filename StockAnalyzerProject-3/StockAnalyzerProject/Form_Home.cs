// Form_Home.cs - initial form where user picks CSV files and date range
using System;               // Brings in core types like EventArgs and DateTime
using System.ComponentModel; // Provides CancelEventArgs used by FileOk handler
using System.IO;            // Used for Path helpers (file name without extension)
using System.Windows.Forms; // Windows Forms UI framework types (Form, OpenFileDialog, MessageBox)
using System.Drawing;       // Colors and fonts for UI styling

namespace StockAnalyzerProject // Groups related types for this application
{
    /// <summary>
    /// Home (input) form that lets the user pick one or more stock CSV files
    /// and a date range; opens a chart form per selected file.
    /// </summary>
    public partial class Form_Home : Form // Inherits from Form to participate in WinForms lifecycle
    {
        private string fileName;     // Holds the last-selected file path (convenience field)
        private string[] fileNames;  // Holds all selected file paths when MultiSelect is used
        private static readonly Color UiWindow = Color.FromArgb(244, 247, 252); // App background
        private static readonly Color UiText = Color.FromArgb(29, 42, 61); // Main text tone
        private static readonly Color UiAccent = Color.FromArgb(63, 102, 176); // Primary action color

        /// <summary>
        /// Initializes the Home form and its designer-defined controls.
        /// </summary>
        public Form_Home()
        {
            InitializeComponent(); // Creates and lays out controls added via the Designer
            ApplyVisualTheme(); // Apply cohesive styling for the home screen
            this.Resize += Form_Home_Resize; // Keep proportional layout when window size changes
            this.Shown += Form_Home_Shown; // Perform one layout pass once dimensions are final
        }

        /// <summary>
        /// Applies a cleaner, cohesive look to the home screen controls.
        /// </summary>
        private void ApplyVisualTheme()
        {
            this.BackColor = UiWindow;
            this.ForeColor = UiText;
            this.Text = "Stock Analyzer - Home";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(920, 300);
            this.Font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);

            if (label_startDate != null) label_startDate.ForeColor = UiText;
            if (label_EndDate != null) label_EndDate.ForeColor = UiText;

            if (button_loadTicker != null)
            {
                button_loadTicker.Text = "Open CSV";
                button_loadTicker.FlatStyle = FlatStyle.Flat;
                button_loadTicker.FlatAppearance.BorderSize = 0;
                button_loadTicker.BackColor = UiAccent;
                button_loadTicker.ForeColor = Color.White;
                button_loadTicker.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold, GraphicsUnit.Point);
            }

            ArrangeHomeLayout();
        }

        /// <summary>
        /// Keeps the Home controls balanced and proportional across window sizes.
        /// </summary>
        private void ArrangeHomeLayout()
        {
            if (label_startDate == null || label_EndDate == null || dateTimePicker_startDate == null || dateTimePicker_endDate == null || button_loadTicker == null)
                return;

            int padX = 34;
            int padY = 34;
            int labelW = 95;
            int gap = 12;
            int rowGap = 20;
            int buttonW = 140;
            int buttonH = 40;
            int rowH = Math.Max(dateTimePicker_startDate.Height, dateTimePicker_endDate.Height);

            int left = padX;
            int right = Math.Max(left + 300, this.ClientSize.Width - padX);
            int inputLeft = left + labelW + gap;
            int y1 = padY;
            int y2 = y1 + rowH + rowGap;

            int rowInputWidth = Math.Max(220, right - inputLeft);
            int buttonX = right - buttonW;
            int row2PickerWidth = buttonX - gap - inputLeft;
            bool placeButtonBelow = row2PickerWidth < 220;

            if (placeButtonBelow)
                row2PickerWidth = rowInputWidth;

            label_startDate.Text = "Start Date:";
            label_EndDate.Text = "End Date:";

            label_startDate.Location = new Point(left, y1 + 6);
            label_startDate.Size = new Size(labelW, rowH);

            dateTimePicker_startDate.Location = new Point(inputLeft, y1);
            dateTimePicker_startDate.Size = new Size(rowInputWidth, rowH);

            label_EndDate.Location = new Point(left, y2 + 6);
            label_EndDate.Size = new Size(labelW, rowH);

            dateTimePicker_endDate.Location = new Point(inputLeft, y2);
            dateTimePicker_endDate.Size = new Size(row2PickerWidth, rowH);

            button_loadTicker.Size = new Size(buttonW, buttonH);
            button_loadTicker.Location = placeButtonBelow
                ? new Point(inputLeft, y2 + rowH + rowGap - 2)
                : new Point(buttonX, y2 - 2);
        }

        private void Form_Home_Shown(object sender, EventArgs e) => ArrangeHomeLayout(); // First-pass final layout
        private void Form_Home_Resize(object sender, EventArgs e) => ArrangeHomeLayout(); // Resize-aware layout

        /// <summary>
        /// Placeholder click handler for a label on the form (not used).
        /// </summary>
        /// <param name="sender">The label control that was clicked.</param>
        /// <param name="e">Event data (unused).</param>
        private void label1_Click(object sender, EventArgs e)
        {
            // Intentionally left blank: label click has no behavior
        }

        /// <summary>
        /// Second placeholder click handler for a label on the form (not used).
        /// </summary>
        /// <param name="sender">The label control that was clicked.</param>
        /// <param name="e">Event data (unused).</param>
        private void label1_Click_1(object sender, EventArgs e)
        {
            // Intentionally left blank: label click has no behavior
        }

        /// <summary>
        /// Runs when the user confirms file selection in the OpenFileDialog.
        /// Creates and shows one chart form per selected CSV path using the current date range.
        /// </summary>
        /// <param name="sender">The OpenFileDialog that raised the event.</param>
        /// <param name="e">Cancelable event args (unused here).</param>
        private void openFileDialogLoad_FileOk(object sender, CancelEventArgs e)
        {
            fileNames = openFileDialog_loadTicker.FileNames;               // Read all selected file paths (supports MultiSelect)
            if (fileNames == null || fileNames.Length == 0) return;        // Abort if nothing was selected
            fileName = openFileDialog_loadTicker.FileName;                 // Record the first/active selected file path

            var start = dateTimePicker_startDate.Value.Date;               // Get the chosen start date (date-only)
            var end = dateTimePicker_endDate.Value.Date;                   // Get the chosen end date (date-only)

            foreach (var path in fileNames)                                // Loop through each selected CSV file
            {
                var frm = new Form_Main(path, start, end);                 // Create a new chart form for this file and date range
                frm.StartPosition = FormStartPosition.CenterScreen;      // Center the child form on screen
                frm.MinimumSize = new System.Drawing.Size(800, 600);    // Set a minimum size for usability
                frm.Size = new System.Drawing.Size(1000, 800);          // Set an initial size for the form
                frm.Text = "Stock Analyzer - " + Path.GetFileNameWithoutExtension(path); // Set window title to include stock name
                frm.Show();                                                // Show the chart form modelessly
            }
        }

        /// <summary>
        /// Handles the Load Ticker button click. Validates date range and opens the file picker.
        /// </summary>
        /// <param name="sender">The Load button that was clicked.</param>
        /// <param name="e">Event data (unused).</param>
        private void button_loadTicker_event(object sender, EventArgs e)
        {
            var startDate = dateTimePicker_startDate.Value;                // Read current start date/time from the picker
            var endDate = dateTimePicker_endDate.Value;                    // Read current end date/time from the picker
            if (startDate >= endDate)                                      // Guard against an invalid (inverted) date range
            {
                MessageBox.Show(                                           // Inform the user about the invalid range
                    "Start Date must be earlier than End Date.",           // Message text
                    "Invalid Date Range",                                  // Dialog caption
                    MessageBoxButtons.OK,                                  // OK-only button
                    MessageBoxIcon.Error);                                 // Error icon to draw attention
                return;                                                    // Stop here; do not open the file dialog
            }

            openFileDialog_loadTicker.ShowDialog(this);                    // Open the file picker (modal) over this form
        }
    }
}
