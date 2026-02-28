// Recognizers_TwoCandle.cs - logic and recognizers for patterns that compare two candles (Engulfing, Harami) // File header documenting that this file focuses on two-candle patterns
using System.Collections.Generic; // Import List<T> to work with collections of smart candlesticks

namespace StockAnalyzerProject // Namespace shared by all project classes
{
    /// <summary>
    /// Helper class that holds reusable logic for two-candle candlestick patterns,
    /// such as Engulfing and Harami. This class does not store state; it only
    /// provides static methods that evaluate a pair of candles.
    /// </summary>
    public static class TwoCandleLogic // Static class because we only need pure functions, not instances
    {
        /// <summary>
        /// Determines whether the given pair of candles forms a Bullish Engulfing pattern.
        /// The previous candle must be bearish, the current one bullish, and the current
        /// candle's body must wrap completely around the previous body.
        /// </summary>
        /// <param name="prev">Earlier candle in the sequence (index - 1).</param>
        /// <param name="curr">Later candle in the sequence (index).</param>
        /// <returns>True when the pattern matches a Bullish Engulfing.</returns>
        public static bool BullishEngulfing(aSmartCandlestick prev, aSmartCandlestick curr) // Static method that checks bullish engulfing conditions
        {
            if (!prev.isBearish || !curr.isBullish) return false; // If the directions are not bearish then bullish, pattern cannot be bullish engulfing

            return curr.bottomOfBody <= prev.bottomOfBody && // Current body must start at or below the previous bottom
                   curr.topOfBody >= prev.topOfBody && // Current body must end at or above the previous top
                   curr.bodyRange > prev.bodyRange;      // Current body must be strictly larger to truly engulf the previous one
        } // End of BullishEngulfing

        /// <summary>
        /// Determines whether the given pair of candles forms a Bearish Engulfing pattern.
        /// The previous candle must be bullish, the current one bearish, and the current
        /// body must fully contain the previous body.
        /// </summary>
        /// <param name="prev">Earlier candle in the sequence (index - 1).</param>
        /// <param name="curr">Later candle in the sequence (index).</param>
        /// <returns>True when the pattern matches a Bearish Engulfing.</returns>
        public static bool BearishEngulfing(aSmartCandlestick prev, aSmartCandlestick curr) // Static method that checks bearish engulfing conditions
        {
            if (!prev.isBullish || !curr.isBearish) return false; // Without bullish then bearish order, we cannot have bearish engulfing

            return curr.bottomOfBody <= prev.bottomOfBody && // Current body must reach at least as low as the previous bottom
                   curr.topOfBody >= prev.topOfBody && // Current body must reach at least as high as the previous top
                   curr.bodyRange > prev.bodyRange;      // Current body must be longer than the previous to engulf it
        } // End of BearishEngulfing

        /// <summary>
        /// Determines whether the given pair of candles forms a Bullish Harami pattern.
        /// The previous candle is a large bearish candle, and the current candle is
        /// a smaller bullish one whose body fits inside the previous body.
        /// </summary>
        /// <param name="prev">Earlier (larger) candle.</param>
        /// <param name="curr">Later (smaller) candle.</param>
        /// <returns>True when the pattern matches a Bullish Harami.</returns>
        public static bool BullishHarami(aSmartCandlestick prev, aSmartCandlestick curr) // Static method implementing bullish Harami rules
        {
            if (!prev.isBearish || !curr.isBullish) return false; // Pattern requires a bearish candle followed by a bullish one

            return curr.bottomOfBody >= prev.bottomOfBody && // Current body must start inside or above previous body bottom
                   curr.topOfBody <= prev.topOfBody && // Current body must end inside or below previous body top
                   curr.bodyRange < prev.bodyRange;      // Current body must be smaller to look "inside" the previous one
        } // End of BullishHarami

        /// <summary>
        /// Determines whether the given pair of candles forms a Bearish Harami pattern.
        /// The previous candle is a large bullish candle, and the current candle is
        /// a smaller bearish one whose body fits inside the previous body.
        /// </summary>
        /// <param name="prev">Earlier (larger) candle.</param>
        /// <param name="curr">Later (smaller) candle.</param>
        /// <returns>True when the pattern matches a Bearish Harami.</returns>
        public static bool BearishHarami(aSmartCandlestick prev, aSmartCandlestick curr) // Static method implementing bearish Harami rules
        {
            if (!prev.isBullish || !curr.isBearish) return false; // Pattern requires a bullish candle followed by a bearish one

            return curr.bottomOfBody >= prev.bottomOfBody && // Current body must begin at or above the previous body bottom
                   curr.topOfBody <= prev.topOfBody && // Current body must end at or below the previous body top
                   curr.bodyRange < prev.bodyRange;      // Current body must be smaller than the previous body
        } // End of BearishHarami

        /// <summary>
        /// Checks if the pair of candles forms any Engulfing pattern,
        /// either bullish or bearish.
        /// </summary>
        /// <param name="prev">Earlier candle to compare.</param>
        /// <param name="curr">Later candle to compare.</param>
        /// <returns>True when either BullishEngulfing or BearishEngulfing is true.</returns>
        public static bool Engulfing(aSmartCandlestick prev, aSmartCandlestick curr) // Static method that aggregates both engulfing variants
        {
            return BullishEngulfing(prev, curr) || // If it matches bullish engulfing, pattern is detected
                   BearishEngulfing(prev, curr);   // Or if it matches bearish engulfing, pattern is also detected
        } // End of Engulfing

        /// <summary>
        /// Checks if the pair of candles forms any Harami pattern,
        /// either bullish or bearish.
        /// </summary>
        /// <param name="prev">Earlier candle to compare.</param>
        /// <param name="curr">Later candle to compare.</param>
        /// <returns>True when either BullishHarami or BearishHarami is true.</returns>
        public static bool Harami(aSmartCandlestick prev, aSmartCandlestick curr) // Static method that aggregates both Harami variants
        {
            return BullishHarami(prev, curr) || // True if the pair satisfies bullish Harami conditions
                   BearishHarami(prev, curr);   // Or if it satisfies bearish Harami conditions
        } // End of Harami
    }

    // ========== ENGULFING RECOGNIZERS ==========

    /// <summary>
    /// Recognizer that detects any Engulfing pattern (bullish or bearish).
    /// This class uses TwoCandleLogic.Engulfing to evaluate each position.
    /// </summary>
    public class Recognizer_Engulfing : Recognizer // Recognizer subclass for generic Engulfing patterns
    {
        /// <summary>
        /// Builds a generic Engulfing recognizer.
        /// The lookback is fixed to 2 because the pattern uses the current and previous candles.
        /// </summary>
        public Recognizer_Engulfing() // Constructor with no parameters; pattern name and lookback are fixed
            : base("Engulfing", 2) { } // Initialize base with pattern label "Engulfing" and lookback of 2 candles

        /// <summary>
        /// Checks if there is an Engulfing pattern (of either type) at the given index.
        /// </summary>
        /// <param name="list">List of smart candlesticks.</param>
        /// <param name="index">Index of the second candle in the two-candle pattern.</param>
        /// <returns>True when the pair (index - 1, index) forms an Engulfing pattern.</returns>
        public override bool recognize(List<aSmartCandlestick> list, int index) // Override that implements Engulfing detection at a given index
        {
            if (!HasEnoughData(list, index)) return false; // Ensure we have at least 2 candles and a valid list before checking

            var prev = list[index - 1]; // Previous candle is at index - 1
            var curr = list[index];     // Current candle is at index

            return TwoCandleLogic.Engulfing(prev, curr); // Delegate the actual pattern evaluation to the helper logic
        } // End of recognize for generic Engulfing
    }

    /// <summary>
    /// Recognizer that detects specifically Bullish Engulfing patterns.
    /// </summary>
    public class Recognizer_BullishEngulfing : Recognizer // Recognizer subclass for Bullish Engulfing only
    {
        /// <summary>
        /// Builds a Bullish Engulfing recognizer.
        /// The lookback is 2 because we compare the current candle with the previous one.
        /// </summary>
        public Recognizer_BullishEngulfing() // Empty constructor that configures the base recognizer
            : base("Engulfing (Bullish)", 2) { } // Register pattern name and lookback needed to support this pattern

        /// <summary>
        /// Checks if there is a Bullish Engulfing pattern at the given index.
        /// </summary>
        /// <param name="list">List of smart candlesticks.</param>
        /// <param name="index">Index of the second candle in the pattern.</param>
        /// <returns>True when the previous candle and current candle form a Bullish Engulfing pattern.</returns>
        public override bool recognize(List<aSmartCandlestick> list, int index) // Override for Bullish Engulfing detection
        {
            if (!HasEnoughData(list, index)) return false; // Verify that we have the previous candle available

            var prev = list[index - 1]; // Retrieve the earlier candle in the pair
            var curr = list[index];     // Retrieve the later candle in the pair

            return TwoCandleLogic.BullishEngulfing(prev, curr); // Use the helper method dedicated to Bullish Engulfing logic
        } // End of recognize for Bullish Engulfing
    }

    /// <summary>
    /// Recognizer that detects specifically Bearish Engulfing patterns.
    /// </summary>
    public class Recognizer_BearishEngulfing : Recognizer // Recognizer subclass for Bearish Engulfing only
    {
        /// <summary>
        /// Builds a Bearish Engulfing recognizer.
        /// </summary>
        public Recognizer_BearishEngulfing() // Constructor that wires this recognizer's name and lookback
            : base("Engulfing (Bearish)", 2) { } // Register pattern name and required lookback of 2 candles

        /// <summary>
        /// Checks if there is a Bearish Engulfing pattern at the given index.
        /// </summary>
        /// <param name="list">List of smart candlesticks.</param>
        /// <param name="index">Index of the second candle in the pattern.</param>
        /// <returns>True when the previous candle and current candle form a Bearish Engulfing pattern.</returns>
        public override bool recognize(List<aSmartCandlestick> list, int index) // Override for Bearish Engulfing detection
        {
            if (!HasEnoughData(list, index)) return false; // Make sure we have a valid current index and a previous candle

            var prev = list[index - 1]; // Fetch the candle at index - 1
            var curr = list[index];     // Fetch the candle at index

            return TwoCandleLogic.BearishEngulfing(prev, curr); // Call the helper dedicated to Bearish Engulfing rules
        } // End of recognize for Bearish Engulfing
    }

    // ========== HARAMI RECOGNIZERS ==========

    /// <summary>
    /// Recognizer that detects any Harami pattern (bullish or bearish).
    /// A Harami is a two-candle pattern where the second candle's body
    /// fits inside the first candle's body.
    /// </summary>
    public class Recognizer_Harami : Recognizer // Recognizer subclass for generic Harami patterns
    {
        /// <summary>
        /// Builds a Harami recognizer with a two-candle lookback.
        /// </summary>
        public Recognizer_Harami() // Constructor that configures the base recognizer
            : base("Harami", 2) { } // Register pattern name "Harami" and lookback of 2 candles

        /// <summary>
        /// Checks if there is a Harami pattern (of either type) at the given index.
        /// </summary>
        /// <param name="list">List of smart candlesticks.</param>
        /// <param name="index">Index of the second candle in the two-candle pattern.</param>
        /// <returns>True when the pair forms either a Bullish or Bearish Harami.</returns>
        public override bool recognize(List<aSmartCandlestick> list, int index) // Override implementing generic Harami detection
        {
            if (!HasEnoughData(list, index)) return false; // Ensure we have at least 2 valid candles before checking

            var prev = list[index - 1]; // Earlier, larger candle in the Harami pattern
            var curr = list[index];     // Later, smaller candle that sits inside the first

            return TwoCandleLogic.Harami(prev, curr); // Delegate to helper that checks both bullish and bearish Harami conditions
        } // End of recognize for generic Harami
    }

    /// <summary>
    /// Recognizer that detects specifically Bullish Harami patterns.
    /// </summary>
    public class Recognizer_BullishHarami : Recognizer // Recognizer subclass for Bullish Harami only
    {
        /// <summary>
        /// Builds a Bullish Harami recognizer.
        /// </summary>
        public Recognizer_BullishHarami() // Constructor that sets pattern name and lookback
            : base("Harami (Bullish)", 2) { } // Register pattern as "Harami (Bullish)" with lookback of 2 candles

        /// <summary>
        /// Checks if there is a Bullish Harami pattern at the given index.
        /// </summary>
        /// <param name="list">List of smart candlesticks.</param>
        /// <param name="index">Index of the second candle in the pattern.</param>
        /// <returns>True when the pair forms a Bullish Harami pattern.</returns>
        public override bool recognize(List<aSmartCandlestick> list, int index) // Override for Bullish Harami detection
        {
            if (!HasEnoughData(list, index)) return false; // Validate that list and index allow us to look one candle back

            var prev = list[index - 1]; // Previous candle in the series
            var curr = list[index];     // Current candle to complete the Harami pair

            return TwoCandleLogic.BullishHarami(prev, curr); // Use the helper that specifically checks Bullish Harami
        } // End of recognize for Bullish Harami
    }

    /// <summary>
    /// Recognizer that detects specifically Bearish Harami patterns.
    /// </summary>
    public class Recognizer_BearishHarami : Recognizer // Recognizer subclass for Bearish Harami only
    {
        /// <summary>
        /// Builds a Bearish Harami recognizer.
        /// </summary>
        public Recognizer_BearishHarami() // Constructor that wires the base recognizer
            : base("Harami (Bearish)", 2) { } // Register pattern name "Harami (Bearish)" with lookback of 2 candles

        /// <summary>
        /// Checks if there is a Bearish Harami pattern at the given index.
        /// </summary>
        /// <param name="list">List of smart candlesticks.</param>
        /// <param name="index">Index of the second candle in the pattern.</param>
        /// <returns>True when the pair forms a Bearish Harami pattern.</returns>
        public override bool recognize(List<aSmartCandlestick> list, int index) // Override for Bearish Harami detection
        {
            if (!HasEnoughData(list, index)) return false; // Guard to avoid reading invalid candles

            var prev = list[index - 1]; // Candle at index - 1, expected to be the larger one
            var curr = list[index];     // Candle at index, expected to be smaller and inside previous body

            return TwoCandleLogic.BearishHarami(prev, curr); // Call helper that encapsulates Bearish Harami rules
        } // End of recognize for Bearish Harami
    }
} // End of namespace StockAnalyzerProject
