// Recognizer.cs - abstract base class used by all pattern recognizers
using System.Collections.Generic; // For List<T>

namespace StockAnalyzerProject
{
    /// <summary>
    /// Abstract base class for all candlestick pattern recognizers.
    /// Each derived recognizer implements its own logic to detect a specific pattern
    /// (like Doji, Hammer, Engulfing, etc.). This class provides the common structure
    /// and metadata that all recognizers need.
    /// </summary>
    public abstract class Recognizer
    {
        /// <summary>
        /// Creates a new recognizer with a display name, lookback period, and tolerance.
        /// </summary>
        /// <param name="name">The human-readable name for this pattern (e.g., "Doji", "Hammer").</param>
        /// <param name="lookback">How many candlesticks this pattern needs to examine (1 for single-candle patterns, 2 for two-candle patterns, etc.).</param>
        /// <param name="tolerance">Percentage tolerance for equality checks (e.g., 0.02 for 2%). Default is 0.02 (2%).</param>
        protected Recognizer(string name, int lookback = 1, double tolerance = 0.02)
        {
            // Store the pattern name for display in the UI
            Name = name;
            
            // Make sure lookback is at least 1 (can't have a pattern that uses zero candles)
            Lookback = lookback < 1 ? 1 : lookback;
            
            // Store tolerance, ensuring it's non-negative
            Tolerance = tolerance < 0 ? 0 : tolerance;
        }

        /// <summary>
        /// Gets the display name of this pattern (e.g., "Doji", "Hammer", "Bullish Engulfing").
        /// This is what shows up in the ComboBox in the UI.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Gets the number of candlesticks this pattern needs to examine.
        /// Single-candle patterns (Doji, Hammer) use 1, two-candle patterns (Engulfing, Harami) use 2.
        /// </summary>
        public int Lookback { get; }
        
        /// <summary>
        /// Gets the tolerance percentage used for "approximately equal" comparisons.
        /// For example, 0.02 means 2% tolerance. A Doji with 2% tolerance would accept
        /// Open and Close values that differ by up to 2% of the candle's range.
        /// </summary>
        public double Tolerance { get; }

        /// <summary>
        /// Determines whether this pattern occurs at the specified index in the candlestick list.
        /// This is the core method that each derived recognizer must implement with its specific
        /// pattern detection logic.
        /// </summary>
        /// <param name="list">The complete list of smart candlesticks to analyze.</param>
        /// <param name="index">The index position to check for this pattern.</param>
        /// <returns>True if the pattern is found at this index, false otherwise.</returns>
        public abstract bool recognize(List<aSmartCandlestick> list, int index);

        /// <summary>
        /// Helper method for derived classes to verify there's enough data to check the pattern.
        /// For example, a two-candle pattern at index 0 can't work because there's no previous candle.
        /// </summary>
        /// <param name="list">The candlestick list to check.</param>
        /// <param name="index">The index where we want to check the pattern.</param>
        /// <returns>True if there's enough data to check this pattern at this index.</returns>
        protected bool HasEnoughData(List<aSmartCandlestick> list, int index)
        {
            // Need a valid list
            // Need the index to be at least (Lookback - 1) so we can look back far enough
            // Need the index to be within the list bounds
            return list != null && index >= (Lookback - 1) && index < list.Count; // Ensure list exists and index has enough prior candles and is within bounds
        }
        
        /// <summary>
        /// Checks if two price values are approximately equal within the pattern's tolerance.
        /// Uses the candlestick's range as the basis for percentage calculation.
        /// </summary>
        /// <param name="value1">First price value to compare.</param>
        /// <param name="value2">Second price value to compare.</param>
        /// <param name="candle">The candlestick providing the range context.</param>
        /// <returns>True if the values are within tolerance of each other.</returns>
        protected bool AreApproximatelyEqual(double value1, double value2, aSmartCandlestick candle)
        {
            // If the candle has no range (high == low), use exact equality
            if (candle.range <= 0)
                return value1 == value2;
            
            // Calculate the maximum allowed difference based on tolerance
            double maxDiff = candle.range * Tolerance;
            
            // Check if the absolute difference is within tolerance
            return System.Math.Abs(value1 - value2) <= maxDiff;
        }

        /// <summary>
        /// Returns the pattern name when this object is converted to a string.
        /// This makes debugging easier and ensures the ComboBox displays the right text.
        /// </summary>
        /// <returns>The pattern name.</returns>
        public override string ToString() => Name; // Return pattern name so UI and debugging show the recognizer label
    }
}