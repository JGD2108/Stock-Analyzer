using System;
using System.Globalization;

namespace StockAnalyzerProject
{
    /// <summary>
    /// Represents a single candlestick (OHLCV) data point for one time period.
    /// This is the basic data structure that holds raw price and volume information
    /// without any computed properties or pattern detection.
    /// </summary>
    public class Candlestick
    {
        /// <summary>
        /// Gets or sets the date/time when this candlestick period occurred.
        /// </summary>
        public DateTime Date { get; set; }
        
        /// <summary>
        /// Gets or sets the opening price (first trade in the period).
        /// </summary>
        public double Open { get; set; }
        
        /// <summary>
        /// Gets or sets the highest price reached during the period.
        /// </summary>
        public double High { get; set; }
        
        /// <summary>
        /// Gets or sets the lowest price reached during the period.
        /// </summary>
        public double Low { get; set; }
        
        /// <summary>
        /// Gets or sets the closing price (last trade in the period).
        /// </summary>
        public double Close { get; set; }
        
        /// <summary>
        /// Gets or sets the total trading volume during the period.
        /// </summary>
        public long Volume { get; set; }

        /// <summary>
        /// Creates a new candlestick with default values (all zeros).
        /// This parameterless constructor is needed so derived classes can call base().
        /// </summary>
        public Candlestick() { }

        /// <summary>
        /// Creates a new candlestick with specific OHLCV values.
        /// This is the main constructor used when building candlesticks from parsed CSV data.
        /// </summary>
        /// <param name="date">The trading date/time for this candlestick.</param>
        /// <param name="open">Opening price.</param>
        /// <param name="high">Highest price.</param>
        /// <param name="low">Lowest price.</param>
        /// <param name="close">Closing price.</param>
        /// <param name="volume">Trading volume.</param>
        public Candlestick(DateTime date, double open, double high, double low, double close, long volume)
        {
            // Just store all the provided values in the properties
            Date = date;           // Store the provided date in the Date property
            Open = open;           // Store the opening price
            High = high;           // Store the highest price
            Low = low;             // Store the lowest price
            Close = close;         // Store the closing price
            Volume = volume;       // Store the total traded volume
        }

        /// <summary>
        /// Creates a candlestick by parsing a comma-separated string.
        /// Expected format: "Date,Open,High,Low,Close,Volume"
        /// This constructor is useful for quick testing or loading from simple text files.
        /// </summary>
        /// <param name="dataLine">A CSV line with 6 comma-separated values.</param>
        /// <exception cref="ArgumentException">Thrown if the data line is null or empty.</exception>
        /// <exception cref="FormatException">Thrown if the line doesn't have 6 values or values can't be parsed.</exception>
        public Candlestick(string dataLine)
        {
            // Make sure we actually got some data
            if (string.IsNullOrWhiteSpace(dataLine))
                throw new ArgumentException("Data line is null/empty.", nameof(dataLine));

            // Split the line on commas to get individual fields
            var parts = dataLine.Split(','); // Split the CSV line into individual tokens by comma
            
            // Verify we have all 6 expected fields
            if (parts.Length < 6)
                throw new FormatException("Expected at least 6 comma-separated values: Date,Open,High,Low,Close,Volume"); // Guard against malformed lines

            // Parse each field in order, trimming whitespace first
            // Using InvariantCulture ensures we parse numbers consistently regardless of regional settings
            Date = DateTime.Parse(parts[0].Trim(), CultureInfo.InvariantCulture); // Parse date using invariant culture
            Open = double.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);   // Parse open price
            High = double.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);   // Parse high price
            Low = double.Parse(parts[3].Trim(), CultureInfo.InvariantCulture);    // Parse low price
            Close = double.Parse(parts[4].Trim(), CultureInfo.InvariantCulture);  // Parse close price

            // Volume needs special handling because sometimes CSV files have decimal volumes by mistake
            var volToken = parts[5].Trim();                                       // Read raw volume token and trim whitespace
            
            // If there's a decimal point in the volume, just take the part before it
            if (volToken.IndexOf('.') >= 0)
                volToken = volToken.Split('.')[0]; // If volume has decimals, keep only the integer part
            
            // Now parse it as a long integer
            Volume = long.Parse(volToken, CultureInfo.InvariantCulture);          // Parse volume as a long integer
        }
    }
}
