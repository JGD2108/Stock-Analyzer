// RecognizerManager.cs - coordinates running recognizers across smart candlesticks and caches matches
using System; // Core types like ArgumentNullException
using System.Collections.Generic; // List and Dictionary collections
using System.Diagnostics; // For debug output and Conditional attribute

namespace StockAnalyzerProject
{
    /// <summary>
    /// Manages all pattern recognizers and coordinates pattern detection across candlestick data.
    /// Responsible for running recognizers against already-prepared smart candlesticks,
    /// caching matches, and exposing results to the UI.
    /// </summary>
    public class RecognizerManager
    {
        // Holds all the pattern recognizers we want to use (Doji, Hammer, Engulfing, etc.)
        private readonly List<Recognizer> _recognizers;

        // NEW: parallel list-of-lists.
        // For recognizer at index k in _recognizers, the matches are stored in _matchesPerRecognizer[k].
        private readonly List<List<int>> _matchesPerRecognizer;

        // Cached smart candlesticks after conversion - stores computed anatomy and pattern flags
        // Note: conversion/creation of aSmartCandlestick is performed by the caller (Form_Main).
        private List<aSmartCandlestick> _smartCandles;

        /// <summary>
        /// Creates a new manager with a collection of recognizers to use for pattern detection.
        /// </summary>
        /// <param name="recognizers">The set of pattern recognizers to manage (cannot be null).</param>
        /// <exception cref="ArgumentNullException">Thrown if recognizers is null.</exception>
        public RecognizerManager(IEnumerable<Recognizer> recognizers)
        {
            if (recognizers == null) throw new ArgumentNullException(nameof(recognizers)); // Ensure caller provided a non-null recognizer collection

            _recognizers = new List<Recognizer>(recognizers); // Copy recognizers into a concrete list for indexing
            _matchesPerRecognizer = new List<List<int>>(_recognizers.Count); // Create parallel list for match indices
            for (int i = 0; i < _recognizers.Count; i++)
            {
                _matchesPerRecognizer.Add(new List<int>()); // Initialize an empty hit list for recognizer i
            }

            _smartCandles = new List<aSmartCandlestick>(); // Start with an empty candle list until AnalyzeAllSmart is called
        }

        /// <summary>
        /// Analyze a list of pre-built smart candlesticks and run all recognizers.
        /// Form_Main is responsible for creating aSmartCandlestick instances and calling computeProperties().
        /// </summary>
        /// <param name="smartCandles">Pre-computed smart candlesticks with properties already calculated.</param>
        public void AnalyzeAllSmart(List<aSmartCandlestick> smartCandles)
        {
            // Store the provided smart candles, or use empty list if null was passed
            _smartCandles = smartCandles ?? new List<aSmartCandlestick>(); // assign provided list or empty fallback

            // Clear previous results
            for (int i = 0; i < _matchesPerRecognizer.Count; i++)
            {
                _matchesPerRecognizer[i].Clear(); // clear cached matches for recognizer i
            }

            // Run pattern detection
            RunRecognizers(); // populate _matchesPerRecognizer based on current _smartCandles
        }

        /// <summary>
        /// Runs all registered recognizers against the current smart candlestick data
        /// and stores the matching indices for each pattern in the parallel list-of-lists.
        /// </summary>
        /// <summary>
        /// Runs all registered recognizers against the current smart candlestick data
        /// and stores the matching indices for each pattern in the parallel list-of-lists.
        /// </summary>
        private void RunRecognizers()
        {
            for (int r = 0; r < _recognizers.Count; r++)
            {
                var rec = _recognizers[r];           // recognizer to run
                var hits = _matchesPerRecognizer[r]; // match list for this recognizer

                hits.Clear(); // clear previous matches

                // Earliest index that has enough prior candles for this recognizer
                int startIndex = rec.Lookback - 1;

                for (int i = startIndex; i < _smartCandles.Count; i++)
                {
                    if (rec.recognize(_smartCandles, i))
                    {
                        hits.Add(i); // record pattern occurrence
                    }
                }
            }
        }


        /// <summary>
        /// Retrieves the list of candlestick indices where the recognizer at the given index matched.
        /// Index in this list matches the index in the recognizers list (prof requirement).
        /// </summary>
        /// <param name="recognizerIndex">Index of recognizer in the manager's recognizer list.</param>
        /// <returns>A read-only list of indices where the pattern occurs.</returns>
        public IReadOnlyList<int> GetMatchesByIndex(int recognizerIndex)
        {
            if (recognizerIndex < 0 || recognizerIndex >= _matchesPerRecognizer.Count)
                return Array.Empty<int>(); // out-of-range index

            return _matchesPerRecognizer[recognizerIndex].AsReadOnly();
        }


        /// <summary>
        /// Retrieves the list of candlestick indices where a specific pattern was found,
        /// looked up by recognizer Name (kept for compatibility).
        /// </summary>
        /// <param name="patternName">The name of the pattern to look up (e.g., "Doji", "Hammer").</param>
        /// <returns>A list of indices where the pattern occurs, or an empty list if not found.</returns>
        public List<int> GetMatches(string patternName)
        {
            if (string.IsNullOrEmpty(patternName))
                return new List<int>(); // Guard: empty pattern name yields empty result

            int idx = _recognizers.FindIndex(r =>
                string.Equals(r.Name, patternName, StringComparison.OrdinalIgnoreCase)); // find recognizer index by name

            if (idx < 0 || idx >= _matchesPerRecognizer.Count)
                return new List<int>(); // Guard: pattern name not found or index out of range

            // Return a copy so callers can't mutate internal list
            return new List<int>(_matchesPerRecognizer[idx]); // defensive copy of internal hits list
        }

        /// <summary>
        /// Gets the complete list of all recognizers managed by this instance.
        /// </summary>
        /// <returns>The list of all pattern recognizers.</returns>
        /// <summary>
        /// Gets the complete list of all recognizers managed by this instance.
        /// </summary>
        /// <returns>A read-only list of pattern recognizers.</returns>
        public IReadOnlyList<Recognizer> GetRecognizers() => _recognizers.AsReadOnly(); // Return internal recognizer list for binding

        /// <summary>
        /// Retrieves a specific smart candlestick by its index in the analyzed data.
        /// </summary>
        /// <param name="index">The zero-based index of the candlestick to retrieve.</param>
        /// <returns>The smart candlestick at that position, or null if index is out of range.</returns>
        public aSmartCandlestick GetSmart(int index)
        {
            if (_smartCandles == null) return null; // No candles analyzed yet
            if (index < 0 || index >= _smartCandles.Count) return null; // Invalid index guard
            return _smartCandles[index]; // Return the smart candlestick at that index
        }

        /// <summary>
        /// Debug-only method that dumps all candlestick data with computed properties to the output window.
        /// Only runs in DEBUG builds.
        /// </summary>
        [Conditional("DEBUG")]
        public void DumpAllCandlesticks()
        {
            if (_smartCandles == null)
            {
                Debug.WriteLine("========== NO SMART CANDLESTICKS TO DUMP =========="); // Log when there is no data to dump
                return; // Exit early
            }

            Debug.WriteLine("========== ALL CANDLESTICKS WITH COMPUTED PROPERTIES =========="); // Header for debug dump
            Debug.WriteLine($"Total candles: {_smartCandles.Count}"); // Log total number of candles
            Debug.WriteLine(""); // Blank line for readability

            for (int i = 0; i < _smartCandles.Count; i++) // Loop through all smart candles and print details
            {
                var sc = _smartCandles[i]; // current smart candlestick
                Debug.WriteLine($"[{i}] {sc.Date:yyyy-MM-dd}"); // index and date
                Debug.WriteLine($"    OHLC: O={sc.Open:F2} H={sc.High:F2} L={sc.Low:F2} C={sc.Close:F2} V={sc.Volume}"); // OHLCV
                Debug.WriteLine($"    Anatomy: range={sc.range:F4} body={sc.bodyRange:F4} upperTail={sc.upperTailRange:F4} lowerTail={sc.lowerTailRange:F4}"); // anatomy metrics
                Debug.WriteLine($"             topBody={sc.topOfBody:F2} bottomBody={sc.bottomOfBody:F2}"); // body extents
                Debug.WriteLine($"    Direction: Bullish={sc.isBullish} Bearish={sc.isBearish} Neutral={sc.isNeutral}"); // direction flags
                Debug.WriteLine($"    Patterns: Doji={sc.isDoji} DragonflyDoji={sc.isDragonflyDoji} GravestoneDoji={sc.isGravestoneDoji}"); // single-candle pattern flags
                Debug.WriteLine($"              Marubozu={sc.isMarubozu} (Bull={sc.isMarubozuBullish} Bear={sc.isMarubozuBearish})"); // marubozu flags
                Debug.WriteLine($"              Hammer={sc.isHammer} (Bull={sc.isHammerBullish} Bear={sc.isHammerBearish})"); // hammer flags
                Debug.WriteLine($"              InvHammer={sc.isInvertedHammer} (Bull={sc.isInvertedHammerBullish} Bear={sc.isInvertedHammerBearish})"); // inverted hammer flags
                Debug.WriteLine(""); // blank separator
            }

            Debug.WriteLine("========== END CANDLESTICK DUMP =========="); // footer for debug dump
        }
    }
}
