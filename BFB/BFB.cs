//(Bot may have bugs - please report if some bug is found)

//USE THIS BOT AT YOUR OWN RISK AND USE ONLY WITH REAL MONEY IF YOU FULLY UNDERSTAND IT

//I've decided to rebuild the bot from the ground up.

//Removed limit orders since this is mainly a breakout bot, last version was flawed in that aspect and confusing for users, will create another bot apart for limit orders.

//----------------------------------------------------------------------------------------------------------------------------------------------------------------------

//This bot is useful in breakouts, handling fake breakouts and reducing risk exposure.

//Instead of risking 100 pips to make 100 pips, its better to risk 10 pips 10 times if you know the price is going to move significantly away from your entry, for example the break change of a trading session or a technical breakout.

//Characteristics

//It can handle both long and short and it displays the number of tries and total profit from all trades.
//Can work on a single side (only buy/sell on breakout)
//Can work as an OCO, this means whichever side triggers first will remain until there's profit or the numbers or meets the number of attemps, example:
//Buy Stop Executed, so the sell stop will be closed and the bot will work with buy orders only.
//Without an OCO, the sell stop order would remain, and the bot will stop until there's a profit or meets the number of attemps.
//Can work with multiple instances simultaneously.
//Can work as a martingale with parameter ("Increment Volume each # of times") example:
//Parameter is 3, so bot will double position size each time it has 3 losses in a row.
//Parameter is 1, bot will double position size after each loss.
//This feature can be disabled.
//----------------------------------------------------------------------------------------------------------------------------------------------------------------------

//Tips on how to use it

//This is not a standalone system, it's only a bot to increase your opportunities and potential profit in definite market conditions.
//Use a small stop loss.
//Know the breakout range according to your technical perspective.
//This is a bot for when market changes from choppy to trending.
//Set a realistic TP and number of attemps.
//Be careful since this is a martingale
//Breakouts or volatile scenarios may cause slippage, wide spreads, nothing can be done about it, include it in yorur trading plan.
//Safe stops in case settings are not correct

//Stop losses must not be greater than the width of the breakout band, if there's only one direction (long or short) on your settings, this won't matter.
//Ask price must be smaller than your long entry price.
//Bid price must be bigger than your short entry price.
//If there's an error while placing an order, the bot will stop and display such error on the log.

//In case there's a bug, error or suggestion, please write to: waxavi@outlook.com


using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using System.Collections.Generic;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class BruteForceBreakoutBotv2 : Robot
    {
        [Parameter("Stop loss(pips)", DefaultValue = 5, MinValue = 1)]
        public int _SL { get; set; }
        [Parameter("Take Profit(pips)", DefaultValue = 100, MinValue = 0)]
        public double _TP { get; set; }
        //--
        [Parameter("Max Tries", DefaultValue = 20, MinValue = 2)]
        public double _MaxTries { get; set; }
        [Parameter("Go Long", DefaultValue = true)]
        public bool _GoLong { get; set; }
        [Parameter("Long Start Level", DefaultValue = 1.0)]
        public double _LongPrice { get; set; }
        //--
        [Parameter("Go Short", DefaultValue = true)]
        public bool _GoShort { get; set; }
        [Parameter("Short Start Level", DefaultValue = 1.0)]
        public double _ShortPrice { get; set; }
        [Parameter("Position Size", DefaultValue = 10000)]
        public int _Volume { get; set; }
        [Parameter("Increment Position Size x number of times", DefaultValue = false)]
        public bool _IPS { get; set; }
        //Increment Position Size
        [Parameter("Increment Volume Each # of try", DefaultValue = 4)]
        public int _TryN { get; set; }
        [Parameter("OCO Mode", DefaultValue = false)]
        public bool _OCOMode { get; set; }
        [Parameter("Close Orders/Positions on Bot Stop", DefaultValue = true)]
        public bool _CloseOnStop { get; set; }

        string _Label;
        double _ValueAtRisk;

        private void CancellAllMatchingOrders(IEnumerable<PendingOrder> _pendings)
        {
            if (_pendings.Count() == 0)
                return;

            foreach (var pen in _pendings)
                CancelPendingOrder(pen);
        }


        private void CloseAllMatchingPositions(IEnumerable<Position> _positions)
        {
            if (_positions.Count() == 0)
                return;

            foreach (var pos in _positions)
                ClosePosition(pos);
        }

        private TradeResult PlaceOrder(TradeType _tt, long _volume, double _price)
        {
            TradeResult _TR = PlaceStopOrder(_tt, Symbol, Symbol.NormalizeVolume(_volume), _price, _Label, _SL, _TP);
            if (_TR.IsSuccessful)
            {
                Print("Order Placed.");
                return _TR;
            }
            else
            {
                Print(_TR.Error);
                Stop();
                return _TR;
            }
        }

        private double BandWidthPips
        {
            get { return Math.Round(Math.Abs(_LongPrice - _ShortPrice) / Symbol.PipSize, 2); }
        }

        private IEnumerable<PendingOrder> _Pendings
        {
            get { return PendingOrders.Where(item => item.Label == _Label); }
        }

        private IEnumerable<Position> _Positions
        {
            get { return Positions.Where(item => item.Label == _Label); }
        }

        private IEnumerable<HistoricalTrade> _History
        {
            get { return History.Where(item => item.Label == _Label); }
        }

        private void PositionsOnOpened(PositionOpenedEventArgs args)
        {
            if (args.Position.Label != _Label)
                return;

            if (_OCOMode)
                CancellAllMatchingOrders(_Pendings);
        }

        private void PositionsOnClosed(PositionClosedEventArgs args)
        {
            if (args.Position.Label != _Label)
                return;

            if (args.Position.NetProfit > 0 || _History.Count() > _MaxTries - 1)
                Stop();
            else
            {
                int result;
                Math.DivRem(_History.Count(), _TryN, out result);
                if (result == 0 && _IPS)
                {
                    foreach (var pen in _Pendings)
                    {
                        CancelPendingOrder(pen);
                        PlaceOrder(pen.TradeType, pen.Volume * 2, pen.TargetPrice);
                    }

                    PlaceOrder(args.Position.TradeType, args.Position.Volume * 2, args.Position.EntryPrice);
                }
                else
                {
                    PlaceOrder(args.Position.TradeType, args.Position.Volume, args.Position.EntryPrice);
                }
            }
        }


        protected override void OnStart()
        {
            Positions.Opened += PositionsOnOpened;
            Positions.Closed += PositionsOnClosed;

            _Label = Symbol.Code + TimeFrame.ToString() + Server.Time.Ticks.ToString();

            if (Symbol.Ask > _LongPrice && _GoLong)
            {
                Print("Wrong Settings: Ask > Price Target.");
                Stop();
            }

            if (Symbol.Bid < _ShortPrice && _GoShort)
            {
                Print("Wrong Settings: Bid < Price Target.");
                Stop();
            }

            if (_GoLong && _GoShort)
            {
                if (BandWidthPips < _SL)
                {
                    Print("Wrong Settings: SL ({0}) must not exceed band width ({1}) in OCO Mode.", _SL, BandWidthPips);
                    Stop();
                }
            }

            if (_GoShort)
            {
                if (Symbol.Bid > _ShortPrice)
                    PlaceOrder(TradeType.Sell, _Volume, _ShortPrice);
            }

            if (_GoLong)
            {
                if (Symbol.Ask < _LongPrice)
                    PlaceOrder(TradeType.Buy, _Volume, _LongPrice);

            }

            //Print theoretical max loss
            int _counter = 0;
            int _result = 0;
            double _pipsrisked = _SL;
            double _totalpipsrisked = 0;

            while (_counter < _MaxTries)
            {
                Math.DivRem(_counter, _TryN, out _result);
                if (_result == 0 && _IPS && _counter != 0)
                {
                    _pipsrisked *= 2;
                    _totalpipsrisked += _pipsrisked;
                }
                else
                {
                    _totalpipsrisked += _pipsrisked;
                }
                _counter++;
            }

            _ValueAtRisk = Symbol.PipValue * _Volume * _totalpipsrisked;
            var _pnl = _History.Sum(item => item.NetProfit) + _Positions.Sum(item => item.NetProfit);
            ChartObjects.DrawText("Text", String.Format("Current PnL: {0} | Number of Tries: {1} | Theoretical Risk: {2}", _pnl, _History.Count() + _Positions.Count(), _ValueAtRisk), StaticPosition.TopRight, Colors.Orange);
        }

        protected override void OnTick()
        {
            var _pnl = _History.Sum(item => item.NetProfit) + _Positions.Sum(item => item.NetProfit);
            ChartObjects.DrawText("Text", String.Format("Current PnL: {0} | Number of Tries: {1} | Theoretical Risk: {2}", _pnl, _History.Count() + _Positions.Count(), _ValueAtRisk.ToString() + Account.Currency.ToString()), StaticPosition.TopRight, Colors.Orange);
        }


        protected override void OnStop()
        {
            if (_CloseOnStop)
            {
                CloseAllMatchingPositions(_Positions);

                CancellAllMatchingOrders(_Pendings);
            }
        }
    }
}
