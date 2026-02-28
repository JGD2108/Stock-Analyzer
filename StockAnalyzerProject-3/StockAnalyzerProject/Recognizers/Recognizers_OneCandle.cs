// Recognizers_OneCandle.cs - implementations of single-candle pattern recognizers (Doji, Hammer, Marubozu, etc.) // File header describing what this source file contains
using System.Collections.Generic; // Import List<T> so we can use strongly-typed lists

namespace StockAnalyzerProject // Namespace that groups all classes belonging to this project
{
    // ========== DOJI RECOGNIZERS ==========

    /// <summary>
    /// Recognizes generic Doji candles, where Open and Close are nearly equal.
    /// A Doji usually means the market was indecisive during that period.
    /// </summary>
    public class Recognizer_Doji : Recognizer // This class specializes the generic Recognizer for the Doji pattern
    {
        /// <summary>
        /// Builds a Doji recognizer with a configurable Open/Close tolerance.
        /// </summary>
        /// <param name="tolerance">Maximum relative difference allowed between Open and Close (default 0.02 = 2%).</param>
        public Recognizer_Doji(double tolerance = 0.02) // Constructor lets the caller pick a tolerance; 2% is the default
            : base("Doji", 1, tolerance) { } // Call base Recognizer with pattern name, lookback=1 candle, and chosen tolerance

        /// <summary>
        /// Decides if the candle at the given position is a Doji.
        /// </summary>
        /// <param name="list">List of smart candlesticks to analyze.</param>
        /// <param name="index">Index of the candle we want to test.</param>
        /// <returns>True when the candle is tagged as a Doji; false otherwise.</returns>
        public override bool recognize(List<aSmartCandlestick> list, int index) // Override required by abstract base; performs the actual pattern check
        {
            if (!HasEnoughData(list, index)) return false; // If list is invalid or index is out of range, we safely report no pattern
            return list[index].isDoji; // We rely on the precomputed isDoji flag stored in the smart candlestick
        } // End of recognize for generic Doji
    }

    /// <summary>
    /// Recognizes bullish Doji candles (indecision with a slight upward bias).
    /// </summary>
    public class Recognizer_DojiBullish : Recognizer // Specialization for bullish Doji
    {
        /// <summary>
        /// Builds a bullish Doji recognizer.
        /// </summary>
        /// <param name="tolerance">Maximum allowed Open/Close difference (default 0.02 = 2%).</param>
        public Recognizer_DojiBullish(double tolerance = 0.02) // Constructor allowing the caller to adjust equality threshold
            : base("Doji (Bullish)", 1, tolerance) { } // Register this recognizer with a descriptive label and lookback=1

        /// <summary>
        /// Checks if the candle is both a Doji and classified as bullish.
        /// </summary>
        /// <param name="list">List of smart candlesticks.</param>
        /// <param name="index">Position of the candle to evaluate.</param>
        /// <returns>True only when the Doji is bullish.</returns>
        public override bool recognize(List<aSmartCandlestick> list, int index) // Override to express the bullish Doji condition
        {
            if (!HasEnoughData(list, index)) return false; // Guard against null lists or invalid indices
            var candle = list[index]; // Grab the current smart candlestick to avoid repeated indexing
            return candle.isDoji && candle.isBullish; // Pattern matches when it is marked as Doji and its direction flag is bullish
        } // End of recognize for bullish Doji
    }

    /// <summary>
    /// Recognizes bearish Doji candles (indecision with a slight downward bias).
    /// </summary>
    public class Recognizer_DojiBearish : Recognizer // Specialization for bearish Doji
    {
        /// <summary>
        /// Builds a bearish Doji recognizer.
        /// </summary>
        /// <param name="tolerance">Maximum allowed Open/Close difference (default 0.02 = 2%).</param>
        public Recognizer_DojiBearish(double tolerance = 0.02) // Constructor that allows tolerance control
            : base("Doji (Bearish)", 1, tolerance) { } // Initialize base with a descriptive name and lookback=1

        /// <summary>
        /// Checks if the candle is both a Doji and classified as bearish.
        /// </summary>
        /// <param name="list">List of smart candlesticks.</param>
        /// <param name="index">Index of the candle to test.</param>
        /// <returns>True only when the Doji is bearish.</returns>
        public override bool recognize(List<aSmartCandlestick> list, int index) // Override to represent bearish Doji detection
        {
            if (!HasEnoughData(list, index)) return false; // Make sure we have a valid candle to inspect
            var candle = list[index]; // Retrieve the current smart candlestick
            return candle.isDoji && candle.isBearish; // Match when Doji flag is set and the direction flag is bearish
        } // End of recognize for bearish Doji
    }

    // ========== DRAGONFLY DOJI RECOGNIZERS ==========

    /// <summary>
    /// Recognizes Dragonfly Doji candles (long lower shadow, open/close at the top).
    /// </summary>
    public class Recognizer_DragonflyDoji : Recognizer // Recognizer for the Dragonfly Doji subtype
    {
        /// <summary>
        /// Builds a Dragonfly Doji recognizer.
        /// </summary>
        /// <param name="tolerance">Tolerance used by the underlying computations (default 0.02 = 2%).</param>
        public Recognizer_DragonflyDoji(double tolerance = 0.02) // Constructor exposes tolerance so it can be tweaked if needed
            : base("Dragonfly Doji", 1, tolerance) { } // Initialize base recognizer with label and single-candle lookback

        /// <summary>
        /// Checks whether the current candle is tagged as a Dragonfly Doji.
        /// </summary>
        /// <param name="list">List of smart candlesticks to analyze.</param>
        /// <param name="index">Index of the candidate candle.</param>
        /// <returns>True when isDragonflyDoji is set on the candle.</returns>
        public override bool recognize(List<aSmartCandlestick> list, int index) // Override implementing Dragonfly Doji detection
        {
            if (!HasEnoughData(list, index)) return false; // If there is no valid candle at this index, return false
            return list[index].isDragonflyDoji; // Use the precomputed Dragonfly Doji flag on the smart candlestick
        } // End of recognize for Dragonfly Doji
    }

    /// <summary>
    /// Recognizes bullish Dragonfly Doji candles.
    /// </summary>
    public class Recognizer_DragonflyDojiBullish : Recognizer // Recognizer for bullish Dragonfly Doji
    {
        /// <summary>
        /// Builds a bullish Dragonfly Doji recognizer.
        /// </summary>
        /// <param name="tolerance">Tolerance used by the underlying computations (default 0.02 = 2%).</param>
        public Recognizer_DragonflyDojiBullish(double tolerance = 0.02) // Constructor that allows passing a custom tolerance
            : base("Dragonfly Doji (Bullish)", 1, tolerance) { } // Initialize base with descriptive name and lookback=1

        /// <summary>
        /// Checks if the candle is a Dragonfly Doji with a bullish direction.
        /// </summary>
        /// <param name="list">List of smart candlesticks.</param>
        /// <param name="index">Position of the candle we are checking.</param>
        /// <returns>True only when the candle is both Dragonfly Doji and bullish.</returns>
        public override bool recognize(List<aSmartCandlestick> list, int index) // Override that joins Doji subtype and direction
        {
            if (!HasEnoughData(list, index)) return false; // Validate that we have enough data around this index
            var candle = list[index]; // Access the current smart candlestick once
            return candle.isDragonflyDoji && candle.isBullish; // Match when Dragonfly Doji flag and bullish flag are both true
        } // End of recognize for bullish Dragonfly Doji
    }

    /// <summary>
    /// Recognizes bearish Dragonfly Doji candles.
    /// </summary>
    public class Recognizer_DragonflyDojiBearish : Recognizer // Recognizer for bearish Dragonfly Doji
    {
        /// <summary>
        /// Builds a bearish Dragonfly Doji recognizer.
        /// </summary>
        /// <param name="tolerance">Tolerance used by the underlying computations (default 0.02 = 2%).</param>
        public Recognizer_DragonflyDojiBearish(double tolerance = 0.02) // Constructor to set up the recognizer
            : base("Dragonfly Doji (Bearish)", 1, tolerance) { } // Call base with pattern label and single-candle lookback

        /// <summary>
        /// Checks if the candle is a Dragonfly Doji with a bearish direction.
        /// </summary>
        /// <param name="list">List of smart candlesticks.</param>
        /// <param name="index">Index of the candle to test.</param>
        /// <returns>True when Dragonfly Doji and bearish flags are both set.</returns>
        public override bool recognize(List<aSmartCandlestick> list, int index) // Override for bearish Dragonfly Doji detection
        {
            if (!HasEnoughData(list, index)) return false; // Ensure the call is made with a valid index and candle list
            var candle = list[index]; // Retrieve the smart candlestick we want to inspect
            return candle.isDragonflyDoji && candle.isBearish; // Match only when it is Dragonfly Doji and directional bias is bearish
        } // End of recognize for bearish Dragonfly Doji
    }

    // ========== GRAVESTONE DOJI RECOGNIZERS ==========

    /// <summary>
    /// Recognizes Gravestone Doji candles (long upper shadow, open/close at the bottom).
    /// </summary>
    public class Recognizer_GravestoneDoji : Recognizer // Recognizer for the Gravestone Doji subtype
    {
        /// <summary>
        /// Builds a Gravestone Doji recognizer.
        /// </summary>
        /// <param name="tolerance">Tolerance used by underlying equality checks (default 0.02 = 2%).</param>
        public Recognizer_GravestoneDoji(double tolerance = 0.02) // Constructor allowing tolerance adjustment
            : base("Gravestone Doji", 1, tolerance) { } // Initialize base with name and lookback=1 for single-candle pattern

        /// <summary>
        /// Checks whether the current candle is tagged as a Gravestone Doji.
        /// </summary>
        /// <param name="list">List of smart candlesticks.</param>
        /// <param name="index">Index of the candidate candle.</param>
        /// <returns>True when isGravestoneDoji is true.</returns>
        public override bool recognize(List<aSmartCandlestick> list, int index) // Override implementing Gravestone Doji detection
        {
            if (!HasEnoughData(list, index)) return false; // Guard to avoid indexing invalid or missing data
            return list[index].isGravestoneDoji; // Return the Gravestone Doji flag from the smart candlestick
        } // End of recognize for Gravestone Doji
    }

    /// <summary>
    /// Recognizes bullish Gravestone Doji candles.
    /// </summary>
    public class Recognizer_GravestoneDojiBullish : Recognizer // Recognizer for bullish Gravestone Doji
    {
        /// <summary>
        /// Builds a bullish Gravestone Doji recognizer.
        /// </summary>
        /// <param name="tolerance">Tolerance used by underlying equality checks (default 0.02 = 2%).</param>
        public Recognizer_GravestoneDojiBullish(double tolerance = 0.02) // Constructor that takes an optional tolerance
            : base("Gravestone Doji (Bullish)", 1, tolerance) { } // Initialize the base type with label and single-candle lookback

        /// <summary>
        /// Checks if the candle is both a Gravestone Doji and bullish.
        /// </summary>
        /// <param name="list">List of smart candlesticks.</param>
        /// <param name="index">Position of the candle we are analyzing.</param>
        /// <returns>True when it is a Gravestone Doji with bullish direction.</returns>
        public override bool recognize(List<aSmartCandlestick> list, int index) // Override for bullish Gravestone Doji detection
        {
            if (!HasEnoughData(list, index)) return false; // Ensure we only index valid entries
            var candle = list[index]; // Retrieve the candle to inspect
            return candle.isGravestoneDoji && candle.isBullish; // True when Gravestone Doji flag and bullish flag are both active
        } // End of recognize for bullish Gravestone Doji
    }

    /// <summary>
    /// Recognizes bearish Gravestone Doji candles.
    /// </summary>
    public class Recognizer_GravestoneDojiBearish : Recognizer // Recognizer for bearish Gravestone Doji
    {
        /// <summary>
        /// Builds a bearish Gravestone Doji recognizer.
        /// </summary>
        /// <param name="tolerance">Tolerance used by underlying equality checks (default 0.02 = 2%).</param>
        public Recognizer_GravestoneDojiBearish(double tolerance = 0.02) // Constructor with optional tolerance
            : base("Gravestone Doji (Bearish)", 1, tolerance) { } // Initialize base with descriptive name and lookback=1

        /// <summary>
        /// Checks if the candle is a Gravestone Doji and bearish.
        /// </summary>
        /// <param name="list">List of smart candlesticks.</param>
        /// <param name="index">Index of the candle to test.</param>
        /// <returns>True when Gravestone Doji and bearish flags are set.</returns>
        public override bool recognize(List<aSmartCandlestick> list, int index) // Override for bearish Gravestone Doji detection
        {
            if (!HasEnoughData(list, index)) return false; // Avoid accessing the list when it is not ready or index is invalid
            var candle = list[index]; // Access the specific smart candlestick we are interested in
            return candle.isGravestoneDoji && candle.isBearish; // True only when both the Gravestone Doji and bearish flags are true
        } // End of recognize for bearish Gravestone Doji
    }

    // ========== MARUBOZU RECOGNIZERS ==========

    /// <summary>
    /// Recognizes Marubozu candles (full body with almost no shadows).
    /// </summary>
    public class Recognizer_Marubozu : Recognizer // Recognizer for generic Marubozu candles
    {
        /// <summary>
        /// Builds a Marubozu recognizer.
        /// </summary>
        /// <param name="tolerance">Tolerance used in shadow/body comparisons (default 0.0 = strict).</param>
        public Recognizer_Marubozu(double tolerance = 0.0) // Constructor lets the caller decide how strict "no shadow" should be
            : base("Marubozu", 1, tolerance) { } // Register this pattern with name and lookback=1 candle

        /// <summary>
        /// Checks if the candle is marked as a Marubozu.
        /// </summary>
        /// <param name="list">List of smart candlesticks.</param>
        /// <param name="index">Index of the candidate candle.</param>
        /// <returns>True when the candle is a Marubozu.</returns>
        public override bool recognize(List<aSmartCandlestick> list, int index) // Override implementing Marubozu detection
        {
            if (!HasEnoughData(list, index)) return false; // Ensure the list and index are safe to use
            return list[index].isMarubozu; // Use the Marubozu flag that aSmartCandlestick computed earlier
        } // End of recognize for Marubozu
    }

    /// <summary>
    /// Recognizes bullish Marubozu candles (solid upward move).
    /// </summary>
    public class Recognizer_BullishMarubozu : Recognizer // Recognizer for bullish Marubozu candles
    {
        /// <summary>
        /// Builds a bullish Marubozu recognizer.
        /// </summary>
        /// <param name="tolerance">Tolerance used in shadow/body comparisons (default 0.0 = strict).</param>
        public Recognizer_BullishMarubozu(double tolerance = 0.0) // Constructor with optional tolerance
            : base("Marubozu (Bullish)", 1, tolerance) { } // Initialize base with bullish label and lookback=1

        /// <summary>
        /// Checks if the candle is a bullish Marubozu.
        /// </summary>
        /// <param name="list">List of smart candlesticks.</param>
        /// <param name="index">Index of the candle to test.</param>
        /// <returns>True when the bullish Marubozu flag is set.</returns>
        public override bool recognize(List<aSmartCandlestick> list, int index) // Override for bullish Marubozu detection
        {
            if (!HasEnoughData(list, index)) return false; // Guard for invalid data
            return list[index].isMarubozuBullish; // Check the bullish Marubozu flag on the candle
        } // End of recognize for bullish Marubozu
    }

    /// <summary>
    /// Recognizes bearish Marubozu candles (solid downward move).
    /// </summary>
    public class Recognizer_BearishMarubozu : Recognizer // Recognizer for bearish Marubozu candles
    {
        /// <summary>
        /// Builds a bearish Marubozu recognizer.
        /// </summary>
        /// <param name="tolerance">Tolerance used in shadow/body comparisons (default 0.0 = strict).</param>
        public Recognizer_BearishMarubozu(double tolerance = 0.0) // Constructor that lets the caller decide tolerance
            : base("Marubozu (Bearish)", 1, tolerance) { } // Initialize base with bearish label and single-candle lookback

        /// <summary>
        /// Checks if the candle is a bearish Marubozu.
        /// </summary>
        /// <param name="list">List of smart candlesticks.</param>
        /// <param name="index">Index of the candle we are analyzing.</param>
        /// <returns>True when the bearish Marubozu flag is set.</returns>
        public override bool recognize(List<aSmartCandlestick> list, int index) // Override for bearish Marubozu detection
        {
            if (!HasEnoughData(list, index)) return false; // Ensure list and index are valid before reading
            return list[index].isMarubozuBearish; // Use the bearish Marubozu indicator to decide the match
        } // End of recognize for bearish Marubozu
    }

    // ========== HAMMER RECOGNIZERS ==========

    /// <summary>
    /// Recognizes generic Hammer candles (small body with a long lower shadow).
    /// </summary>
    public class Recognizer_Hammer : Recognizer // Recognizer for the Hammer pattern
    {
        /// <summary>
        /// Builds a Hammer recognizer.
        /// </summary>
        /// <param name="tolerance">Tolerance for tail/body ratio used inside aSmartCandlestick (default 0.0).</param>
        public Recognizer_Hammer(double tolerance = 0.0) // Constructor exposing optional tolerance
            : base("Hammer", 1, tolerance) { } // Register base pattern name with lookback=1 candle

        /// <summary>
        /// Checks whether the candle is marked as a Hammer.
        /// </summary>
        /// <param name="list">List of smart candlesticks.</param>
        /// <param name="index">Index of the candidate candle.</param>
        /// <returns>True when the candle is flagged as a Hammer.</returns>
        public override bool recognize(List<aSmartCandlestick> list, int index) // Override implementing generic Hammer detection
        {
            if (!HasEnoughData(list, index)) return false; // Validate that we can safely read from the list
            return list[index].isHammer; // Return the Hammer flag from the smart candlestick
        } // End of recognize for Hammer
    }

    /// <summary>
    /// Recognizes bullish Hammer candles (Hammer that closes above its open).
    /// </summary>
    public class Recognizer_BullishHammer : Recognizer // Recognizer for bullish Hammers
    {
        /// <summary>
        /// Builds a bullish Hammer recognizer.
        /// </summary>
        /// <param name="tolerance">Tolerance for tail/body ratio used inside aSmartCandlestick (default 0.0).</param>
        public Recognizer_BullishHammer(double tolerance = 0.0) // Constructor for bullish Hammer recognizer
            : base("Hammer (Bullish)", 1, tolerance) { } // Initialize base with descriptive name and single-candle lookback

        /// <summary>
        /// Checks if the candle is a Hammer with bullish direction.
        /// </summary>
        /// <param name="list">List of smart candlesticks.</param>
        /// <param name="index">Index of the candle being analyzed.</param>
        /// <returns>True when the candle is both a Hammer and bullish.</returns>
        public override bool recognize(List<aSmartCandlestick> list, int index) // Override for bullish Hammer detection
        {
            if (!HasEnoughData(list, index)) return false; // Ensure data is valid before accessing it
            return list[index].isHammerBullish; // True when the bullish Hammer flag is set
        } // End of recognize for bullish Hammer
    }

    /// <summary>
    /// Recognizes bearish Hammer candles (Hammer that closes below its open).
    /// </summary>
    public class Recognizer_BearishHammer : Recognizer // Recognizer for bearish Hammers
    {
        /// <summary>
        /// Builds a bearish Hammer recognizer.
        /// </summary>
        /// <param name="tolerance">Tolerance for tail/body ratio used inside aSmartCandlestick (default 0.0).</param>
        public Recognizer_BearishHammer(double tolerance = 0.0) // Constructor for bearish Hammer recognizer
            : base("Hammer (Bearish)", 1, tolerance) { } // Initialize base with descriptive name and single-candle lookback

        /// <summary>
        /// Checks if the candle is a Hammer with bearish direction.
        /// </summary>
        /// <param name="list">List of smart candlesticks.</param>
        /// <param name="index">Index of the candle to examine.</param>
        /// <returns>True when the candle is a bearish Hammer.</returns>
        public override bool recognize(List<aSmartCandlestick> list, int index) // Override for bearish Hammer detection
        {
            if (!HasEnoughData(list, index)) return false; // Check that we are not reading outside the list
            return list[index].isHammerBearish; // Use the bearish Hammer flag to determine pattern match
        } // End of recognize for bearish Hammer
    }

    // ========== INVERTED HAMMER RECOGNIZERS ==========

    /// <summary>
    /// Recognizes generic Inverted Hammer candles (small body with long upper shadow).
    /// </summary>
    public class Recognizer_InvertedHammer : Recognizer // Recognizer for the Inverted Hammer pattern
    {
        /// <summary>
        /// Builds an Inverted Hammer recognizer.
        /// </summary>
        /// <param name="tolerance">Tolerance for tail/body ratio used inside aSmartCandlestick (default 0.0).</param>
        public Recognizer_InvertedHammer(double tolerance = 0.0) // Constructor with optional tolerance parameter
            : base("Inverted Hammer", 1, tolerance) { } // Register pattern label and single-candle lookback with base class

        /// <summary>
        /// Checks whether the candle is marked as an Inverted Hammer.
        /// </summary>
        /// <param name="list">List of smart candlesticks.</param>
        /// <param name="index">Index of the candidate candle.</param>
        /// <returns>True when the Inverted Hammer flag is set.</returns>
        public override bool recognize(List<aSmartCandlestick> list, int index) // Override implementing generic Inverted Hammer detection
        {
            if (!HasEnoughData(list, index)) return false; // Guard to avoid accessing invalid indices
            return list[index].isInvertedHammer; // Use the Inverted Hammer flag on the smart candlestick
        } // End of recognize for Inverted Hammer
    }

    /// <summary>
    /// Recognizes bullish Inverted Hammer candles.
    /// </summary>
    public class Recognizer_BullishInvertedHammer : Recognizer // Recognizer for bullish Inverted Hammers
    {
        /// <summary>
        /// Builds a bullish Inverted Hammer recognizer.
        /// </summary>
        /// <param name="tolerance">Tolerance for tail/body ratio used inside aSmartCandlestick (default 0.0).</param>
        public Recognizer_BullishInvertedHammer(double tolerance = 0.0) // Constructor to set up bullish Inverted Hammer recognizer
            : base("Inverted Hammer (Bullish)", 1, tolerance) { } // Initialize base with descriptive label and single-candle lookback

        /// <summary>
        /// Checks whether the candle is an Inverted Hammer and bullish.
        /// </summary>
        /// <param name="list">List of smart candlesticks.</param>
        /// <param name="index">Index of the candle we are checking.</param>
        /// <returns>True when Inverted Hammer and bullish flags are true.</returns>
        public override bool recognize(List<aSmartCandlestick> list, int index) // Override for bullish Inverted Hammer detection
        {
            if (!HasEnoughData(list, index)) return false; // Validate list and index before looking at the candle
            return list[index].isInvertedHammerBullish; // True when the bullish Inverted Hammer flag is active
        } // End of recognize for bullish Inverted Hammer
    }

    /// <summary>
    /// Recognizes bearish Inverted Hammer candles.
    /// </summary>
    public class Recognizer_BearishInvertedHammer : Recognizer // Recognizer for bearish Inverted Hammers
    {
        /// <summary>
        /// Builds a bearish Inverted Hammer recognizer.
        /// </summary>
        /// <param name="tolerance">Tolerance for tail/body ratio used inside aSmartCandlestick (default 0.0).</param>
        public Recognizer_BearishInvertedHammer(double tolerance = 0.0) // Constructor for bearish Inverted Hammer recognizer
            : base("Inverted Hammer (Bearish)", 1, tolerance) { } // Initialize base with descriptive label and single-candle lookback

        /// <summary>
        /// Checks whether the candle is an Inverted Hammer and bearish.
        /// </summary>
        /// <param name="list">List of smart candlesticks.</param>
        /// <param name="index">Index of the candle to analyze.</param>
        /// <returns>True when Inverted Hammer and bearish flags are true.</returns>
        public override bool recognize(List<aSmartCandlestick> list, int index) // Override for bearish Inverted Hammer detection
        {
            if (!HasEnoughData(list, index)) return false; // Guard against invalid list or index usage
            return list[index].isInvertedHammerBearish; // True when the bearish Inverted Hammer flag is set
        } // End of recognize for bearish Inverted Hammer
    }
} // End of namespace StockAnalyzerProject
