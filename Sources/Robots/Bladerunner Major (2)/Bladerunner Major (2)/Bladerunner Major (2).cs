﻿// HMA Fast / Slow, RSI <20 >80, MACD Rising / Falling, Candlestick Tendency, Sinewave Support / Resistance

using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{

    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class BladerunnerMajor : Robot
    {

        // general params

        [Parameter()]
        public TimeFrame GlobalTimeFrame { get; set; }

        [Parameter(DefaultValue = 10000, Step = 1000, MinValue = 1)]
        public int Volume { get; set; }

        [Parameter("Minimum Global Candle Size", DefaultValue = 0, MinValue = 0)]
        public int MinimumGlobalCandleSize { get; set; }

        [Parameter(DefaultValue = true)]
        public bool EnableStopLoss { get; set; }

        [Parameter(DefaultValue = 100, MinValue = 1, MaxValue = 1000, Step = 1)]
        public double StopLoss { get; set; }

        [Parameter(DefaultValue = false)]
        public bool EnableBreakEven { get; set; }

        [Parameter(DefaultValue = 10, MinValue = 1, MaxValue = 400, Step = 1)]
        public double BreakEvenPips { get; set; }

        [Parameter(DefaultValue = 20, MinValue = 1, MaxValue = 400, Step = 1)]
        public double BreakEvenGain { get; set; }

        [Parameter(DefaultValue = false)]
        public bool EnableTrailingStop { get; set; }

        [Parameter(DefaultValue = 10, MinValue = 0, MaxValue = 1000, Step = 1)]
        public double TrailingStop { get; set; }

        [Parameter(DefaultValue = 10, MinValue = 0, MaxValue = 1000, Step = 1)]
        public double TrailingStart { get; set; }

        [Parameter(DefaultValue = true)]
        public bool EnableTakeProfit { get; set; }

        [Parameter(DefaultValue = 30, MinValue = 1, MaxValue = 1000, Step = 1)]
        public int TakeProfit { get; set; }

        [Parameter(DefaultValue = true)]
        public bool EnterOnSyncSignalOnly { get; set; }

        [Parameter(DefaultValue = false)]
        public bool ExitOnOppositeSignal { get; set; }

        //[Parameter("HMA Slow Period", DefaultValue = 31, MinValue = 2, MaxValue = 150, Step = 1)]
        //public int SlowPeriod { get; set; }

        [Parameter("Data Source")]
        public DataSeries Price { get; set; }

        [Parameter("Exp Fast Periods", DefaultValue = 5, MinValue = 1, MaxValue = 550, Step = 1)]
        public int FastPeriods { get; set; }

        [Parameter("Period", DefaultValue = 9, MinValue = 1, MaxValue = 100, Step = 1)]
        public int Period { get; set; }

        [Parameter("Long Cycle", DefaultValue = 26, MinValue = 1, MaxValue = 100, Step = 1)]
        public int LongCycle { get; set; }

        [Parameter("Short Cycle", DefaultValue = 12, MinValue = 1, MaxValue = 100, Step = 1)]
        public int ShortCycle { get; set; }

        [Parameter("Source")]
        public DataSeries Source { get; set; }

        [Parameter("ADX Period", DefaultValue = 14, MinValue = 1, MaxValue = 100, Step = 1)]
        public int interval { get; set; }

        [Parameter("ADX Trend Strength", DefaultValue = 20, MinValue = 10, MaxValue = 30, Step = 1)]
        public int trend { get; set; }


        private MacdHistogram _macd;
        private ExponentialMovingAverage _emaFast;
        private string label;

        private ADXR _adx;

        private const int indexOffset = 0;
        private int index;
        private CandlestickTendencyII tendency;

        public bool globalTendencyWasLong;
        public bool globalTendencyWasShort;
        public bool localTendencyWasLong;
        public bool localTendencyWasShort;

        public Position currentPosition
        {
            get { return Positions.Find(label); }
        }
        public bool inPosition
        {
            get { return currentPosition != null; }
        }
        public bool inShortPosition
        {
            get { return currentPosition != null && currentPosition.TradeType == TradeType.Sell; }
        }
        public bool inLongPosition
        {
            get { return currentPosition != null && currentPosition.TradeType == TradeType.Buy; }
        }

        public bool globalTendencyIsLong
        {
            get { return tendency.GlobalTrendSignal[index] > 0; }
        }
        public bool localTendencyIsLong
        {
            get { return tendency.LocalTrendSignal[index] > 0; }
        }
        public bool globalTendencyIsShort
        {
            get { return tendency.GlobalTrendSignal[index] < 0; }
        }
        public bool localTendencyIsShort
        {
            get { return tendency.LocalTrendSignal[index] < 0; }
        }

        public bool longSignal
        {
            get { return adxrtrend && macdlong && emalong && localTendencyIsLong && globalTendencyIsLong; }
        }
        public bool shortSignal
        {
            get { return adxrtrend && macdshort && emashort && localTendencyIsShort && globalTendencyIsShort; }
        }

        public bool closeSignal
        {
            get { return inPosition ? ((currentPosition.TradeType == TradeType.Sell) ? longSignal : shortSignal) : false; }
        }

        public bool macdlong
        {
            get { return _macd.Histogram.LastValue < 0.0 && _macd.Signal.IsRising(); }
        }

        public bool macdshort
        {
            get { return _macd.Histogram.LastValue > 0.0 && _macd.Signal.IsFalling(); }
        }

        //public bool emalong
        //{
        //get { return MarketSeries.Open.LastValue > _emaFast.Result.LastValue && _emaFast.Result.IsRising(); }
        //}
        //public bool emashort
        //{
        //get { return MarketSeries.Open.LastValue < _emaFast.Result.LastValue && _emaFast.Result.IsFalling(); }
        //}
        public bool emalong
        {
            get { return MarketSeries.Open.LastValue > _emaFast.Result.LastValue && _emaFast.Result.IsRising() && MarketSeries.Close.Last(1) > MarketSeries.Close.Last(2); }
        }
        public bool emashort
        {
            get { return MarketSeries.Open.LastValue < _emaFast.Result.LastValue && _emaFast.Result.IsFalling() && MarketSeries.Close.Last(1) < MarketSeries.Close.Last(2); }
        }

        public bool adxrlong
        {
            get { return _adx.diminus.Last(1) > _adx.diplus.Last(1) && _adx.diminus.Last(0) <= _adx.diplus.Last(0); }
        }
        public bool adxrshort
        {
            get { return _adx.diminus.Last(1) < _adx.diplus.Last(1) && _adx.diminus.Last(0) >= _adx.diplus.Last(0); }
        }
        public bool adxrtrend
        {
            get { return _adx.adxr[index] >= trend; }
        }
        public bool adxrrising
        {
            get { return _adx.adxr.IsRising(); }
        }

        protected override void OnStart()
        {

            label = "Bladerunner Major II" + Symbol.Code + " " + TimeFrame.ToString() + " / " + GlobalTimeFrame.ToString();
            tendency = Indicators.GetIndicator<CandlestickTendencyII>(GlobalTimeFrame, MinimumGlobalCandleSize);
            index = MarketSeries.Close.Count - 1;
            _macd = Indicators.MacdHistogram(LongCycle, ShortCycle, Period);
            _emaFast = Indicators.ExponentialMovingAverage(Price, FastPeriods);
            _adx = Indicators.GetIndicator<ADXR>(Source, interval);

            Positions.Opened += PositionsOnOpened;
            Positions.Closed += PositionsOnClosed;
        }

        protected void UpdateTrailingStops()
        {

            if (!EnableTrailingStop)
                return;

            var positions = Positions.FindAll(label);
            if (positions == null)
                return;

            foreach (var position in positions)
            {
                if (position.Pips >= TrailingStart)
                {
                    if (position.TradeType == TradeType.Buy)
                    {
                        var newStopLoss = Symbol.Bid - TrailingStop * Symbol.PipSize;
                        if (position.StopLoss < newStopLoss)
                            ModifyPosition(position, newStopLoss, null);
                    }
                    else if (position.TradeType == TradeType.Sell)
                    {
                        var newStopLoss = Symbol.Ask + TrailingStop * Symbol.PipSize;
                        if (position.StopLoss > newStopLoss)
                            ModifyPosition(position, newStopLoss, null);
                    }
                }
            }
        }

        protected void MoveToBreakEven()
        {

            if (!EnableBreakEven)
                return;

            var positions = Positions.FindAll(label);
            if (positions == null)
                return;

            foreach (var position in positions)
            {
                if (position.Pips >= BreakEvenPips)
                {
                    if (position.TradeType == TradeType.Buy)
                    {
                        var newStopLoss = Symbol.Bid - BreakEvenGain * Symbol.PipSize;
                        if (position.StopLoss < newStopLoss)
                            ModifyPosition(position, newStopLoss, null);
                    }
                    else if (position.TradeType == TradeType.Sell)
                    {
                        var newStopLoss = Symbol.Ask + BreakEvenGain * Symbol.PipSize;
                        if (position.StopLoss > newStopLoss)
                            ModifyPosition(position, newStopLoss, null);
                    }
                }
            }
        }

        protected TradeResult EnterInPosition(TradeType direction)
        {

            if (!EnableStopLoss && EnableTakeProfit)
                return ExecuteMarketOrder(direction, Symbol, Volume, label, null, TakeProfit);

            if (!EnableStopLoss && !EnableTakeProfit)
                return ExecuteMarketOrder(direction, Symbol, Volume, label, null, null);

            if (EnableStopLoss && !EnableTakeProfit)
                return ExecuteMarketOrder(direction, Symbol, Volume, label, StopLoss, null);

            return ExecuteMarketOrder(direction, Symbol, Volume, label, StopLoss, TakeProfit);
        }

        protected override void OnTick()
        {

            index = MarketSeries.Close.Count - 1;
            UpdateTrailingStops();
            MoveToBreakEven();

        }

        protected override void OnBar()
        {


            index = MarketSeries.Close.Count - 2;

            if (ExitOnOppositeSignal && closeSignal)
                ClosePosition(currentPosition);

            //if (closeSignal)
            //ClosePosition(currentPosition);

            if (!inPosition)
            {

                if (EnterOnSyncSignalOnly)
                {

                    if (localTendencyWasShort && globalTendencyWasShort && localTendencyIsLong && globalTendencyIsLong && longSignal)
                    {
                        EnterInPosition(TradeType.Buy);
                    }
                    else if (localTendencyWasLong && globalTendencyWasLong && localTendencyIsShort && globalTendencyIsShort && shortSignal)
                    {
                        EnterInPosition(TradeType.Sell);
                    }

                }
                else
                {

                    if (shortSignal)
                    {
                        EnterInPosition(TradeType.Sell);
                    }
                    else if (longSignal)
                    {
                        EnterInPosition(TradeType.Buy);
                    }
                }
            }

            localTendencyWasLong = localTendencyIsLong;
            localTendencyWasShort = localTendencyIsShort;
            globalTendencyWasLong = globalTendencyIsLong;
            globalTendencyWasShort = globalTendencyIsShort;
        }

        private void PositionsOnOpened(PositionOpenedEventArgs obj)
        {
            Position openedPosition = obj.Position;
            if (openedPosition.Label != label)
                return;

            Print("position opened at {0}", openedPosition.EntryPrice);
        }

        private void PositionsOnClosed(PositionClosedEventArgs obj)
        {
            Position closedPosition = obj.Position;
            if (closedPosition.Label != label)
                return;

            Print("position closed with {0} gross profit", closedPosition.GrossProfit);
        }

        //protected override double GetFitness(GetFitnessArgs args)
        //{
        //maximize count of winning trades and minimize count of losing trades
        //return (Math.Pow(args.NetProfit, 3) * Math.Pow(args.WinningTrades, 2) * TakeProfit) / (Math.Pow(args.MaxEquityDrawdownPercentages, 2) * Math.Pow(StopLoss, 2) * args.MaxEquityDrawdown);
        //return (args.NetProfit * args.WinningTrades) / (args.MaxEquityDrawdownPercentages * StopLoss);
        //}

        protected override void OnStop()
        {
        }



    }
}
