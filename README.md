# Stock Analyzer (C# WinForms, OOP, Candlestick Pattern Detection)

This repository contains our C# stock analysis project built with Windows Forms (.NET Framework 4.8).  
The application loads OHLCV CSV data, filters by date, visualizes candlesticks and volume, runs a candle-by-candle simulation, and annotates detected candlestick patterns.

## What We Built

The project is designed to combine:

- practical C# OOP design,
- financial candlestick anatomy,
- rule-based pattern recognition,
- interactive chart visualization.

Users can:

- load one or many ticker CSV files,
- choose a date range,
- simulate chart progression one candle at a time,
- select a pattern from a dropdown,
- see annotations for matched candles/pairs after simulation completes.

## C# OOP Design

The code follows a clear object-oriented model:

- `Candlestick` (base model)
- `aSmartCandlestick` (derived, computed intelligence)
- `Recognizer` (abstract pattern recognizer contract)
- many concrete `Recognizer_*` classes (specific patterns)
- `RecognizerManager` (orchestration + cached matches)
- `Form_Home` / `Form_Main` (presentation layer and UI flow)

### 1. Base Class: `Candlestick`

`Candlestick` stores raw OHLCV data:

- `Date`
- `Open`
- `High`
- `Low`
- `Close`
- `Volume`

It supports:

- direct value constructors,
- CSV-line parsing constructor.

This keeps raw market data representation simple and reusable.

### 2. Derived Class: `aSmartCandlestick`

`aSmartCandlestick : Candlestick` extends raw data with computed behavior.

It computes:

- direction flags: `isBullish`, `isBearish`, `isNeutral`,
- anatomy metrics: `range`, `bodyRange`, `upperTailRange`, `lowerTailRange`, `topOfBody`, `bottomOfBody`,
- single-candle pattern flags (Doji family, Marubozu family, Hammer family, Inverted Hammer family).

This demonstrates inheritance + encapsulated business logic: one object contains both data and derived market interpretation.

### 3. Polymorphism Through `Recognizer`

`Recognizer` is an abstract base class with:

- pattern metadata (`Name`, `Lookback`, `Tolerance`),
- abstract `recognize(List<aSmartCandlestick> list, int index)`.

Each concrete recognizer overrides `recognize(...)` with pattern-specific rules.

Examples:

- `Recognizer_Doji`
- `Recognizer_BullishHammer`
- `Recognizer_BearishEngulfing`
- `Recognizer_Harami`

This is a textbook OOP polymorphism pattern: one common interface, many interchangeable implementations.

### 4. Pattern Coordination: `RecognizerManager`

`RecognizerManager` receives the recognizer list and:

- runs all recognizers against current smart-candle data,
- caches matched indices per recognizer,
- exposes results by recognizer index/name to the UI.

This separates pattern engine logic from UI code and keeps responsibilities clean.

## Candlestick Characteristics Used in Detection

For each candle, we derive:

- `range = High - Low`
- `bodyRange = |Close - Open|`
- `topOfBody = max(Open, Close)`
- `bottomOfBody = min(Open, Close)`
- `upperTailRange = High - topOfBody`
- `lowerTailRange = bottomOfBody - Low`

Directional interpretation:

- Bullish candle: `Close > Open`
- Bearish candle: `Open > Close`
- Neutral candle: `Open == Close`

These characteristics are the foundation for all pattern rules.

## Pattern Detection We Implemented

The app supports both single-candle and two-candle recognition.

### Single-Candle Patterns

- Doji
- Doji (Bullish)
- Doji (Bearish)
- Dragonfly Doji
- Dragonfly Doji (Bullish)
- Dragonfly Doji (Bearish)
- Gravestone Doji
- Gravestone Doji (Bullish)
- Gravestone Doji (Bearish)
- Marubozu
- Marubozu (Bullish)
- Marubozu (Bearish)
- Hammer
- Hammer (Bullish)
- Hammer (Bearish)
- Inverted Hammer
- Inverted Hammer (Bullish)
- Inverted Hammer (Bearish)

### Two-Candle Patterns

- Engulfing
- Engulfing (Bullish)
- Engulfing (Bearish)
- Harami
- Harami (Bullish)
- Harami (Bearish)

Total recognizers exposed in the UI: **24**.

## Rule Highlights (How Patterns Are Defined)

### Doji Family

- `Open` and `Close` are approximately equal within tolerance relative to candle range.
- Dragonfly Doji: open/close near high with long lower shadow.
- Gravestone Doji: open/close near low with long upper shadow.

### Marubozu Family

- strict body-dominant candles with effectively no shadows in the implemented rule,
- bullish version: opens at low and closes at high,
- bearish version: opens at high and closes at low.

### Hammer Family

- body near top of the range,
- little/no upper shadow,
- long lower shadow (greater than `2 * bodyRange`).

### Inverted Hammer Family

- body near bottom of the range,
- little/no lower shadow,
- long upper shadow (greater than `2 * bodyRange`).

### Engulfing (Two-Candle)

- Bullish Engulfing: bearish candle then bullish candle whose body fully engulfs previous body and is larger.
- Bearish Engulfing: bullish candle then bearish candle whose body fully engulfs previous body and is larger.

### Harami (Two-Candle)

- Bullish Harami: large bearish candle followed by smaller bullish candle inside previous body.
- Bearish Harami: large bullish candle followed by smaller bearish candle inside previous body.

## Application Workflow

1. User selects one or more CSV files from `Form_Home`.
2. A `Form_Main` opens for each selected ticker.
3. CSV is parsed safely (header-aware, quoted-field-safe).
4. Data is filtered by inclusive date range.
5. Candlestick and volume series are rendered.
6. Simulation reveals one candle per timer tick.
7. After simulation completes, selected pattern is annotated:
- single-candle matches: arrow marker above candle,
- two-candle matches: rectangle around candle pair.

## Repository Structure

Main solution path:

- `StockAnalyzerProject-3/StockAnalyzerProject/StockAnalyzerProject.sln`

Important files:

- `StockAnalyzerProject-3/StockAnalyzerProject/Candlestick.cs`
- `StockAnalyzerProject-3/StockAnalyzerProject/aSmartCandlestick.cs`
- `StockAnalyzerProject-3/StockAnalyzerProject/Recognizers/Recognizer.cs`
- `StockAnalyzerProject-3/StockAnalyzerProject/Recognizers/Recognizers_OneCandle.cs`
- `StockAnalyzerProject-3/StockAnalyzerProject/Recognizers/Recognizers_TwoCandle.cs`
- `StockAnalyzerProject-3/StockAnalyzerProject/Recognizers/RecognizerManager.cs`
- `StockAnalyzerProject-3/StockAnalyzerProject/Form_Home.cs`
- `StockAnalyzerProject-3/StockAnalyzerProject/Form_Main.cs`

## How To Run

1. Open `StockAnalyzerProject.sln` in Visual Studio (Windows).
2. Target framework: `.NET Framework 4.8`.
3. Build and run.
4. Use Home form to select ticker CSV files and date range.
5. Use Simulate + pattern dropdown to visualize detections.

## What This Project Demonstrates

- solid OOP in C# (inheritance, abstraction, polymorphism, separation of concerns),
- financial-domain modeling with computed candlestick anatomy,
- extensible pattern-engine design (easy to add new recognizers),
- chart-based market pattern visualization with simulation-driven UX.
