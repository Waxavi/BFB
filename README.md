# BFB
Brute Force Breakout Bot

(Bot may have bugs - please report if some bug is found)

USE THIS BOT AT YOUR OWN RISK AND USE ONLY WITH REAL MONEY IF YOU FULLY UNDERSTAND IT

I've decided to rebuild the bot from the ground up.

Removed limit orders since this is mainly a breakout bot, last version was flawed in that aspect and confusing for users, will create another bot apart for limit orders.

----------------------------------------------------------------------------------------------------------------------------------------------------------------------

This bot is useful in breakouts, handling fake breakouts and reducing risk exposure.

Instead of risking 100 pips to make 100 pips, its better to risk 10 pips 10 times if you know the price is going to move significantly away from your entry, for example the break change of a trading session or a technical breakout.

Characteristics

It can handle both long and short and it displays the number of tries and total profit from all trades.
Can work on a single side (only buy/sell on breakout)
Can work as an OCO, this means whichever side triggers first will remain until there's profit or the numbers or meets the number of attemps, example:
Buy Stop Executed, so the sell stop will be closed and the bot will work with buy orders only.
Without an OCO, the sell stop order would remain, and the bot will stop until there's a profit or meets the number of attemps.
Can work with multiple instances simultaneously.
Can work as a martingale with parameter ("Increment Volume each # of times") example:
Parameter is 3, so bot will double position size each time it has 3 losses in a row.
Parameter is 1, bot will double position size after each loss.
This feature can be disabled.
----------------------------------------------------------------------------------------------------------------------------------------------------------------------

Tips on how to use it

This is not a standalone system, it's only a bot to increase your opportunities and potential profit in definite market conditions.
Use a small stop loss.
Know the breakout range according to your technical perspective.
This is a bot for when market changes from choppy to trending.
Set a realistic TP and number of attemps.
Be careful since this is a martingale
Breakouts or volatile scenarios may cause slippage, wide spreads, nothing can be done about it, include it in yorur trading plan.
Safe stops in case settings are not correct

Stop losses must not be greater than the width of the breakout band, if there's only one direction (long or short) on your settings, this won't matter.
Ask price must be smaller than your long entry price.
Bid price must be bigger than your short entry price.
If there's an error while placing an order, the bot will stop and display such error on the log.

In case there's a bug, error or suggestion, please write to: waxavi@outlook.com
