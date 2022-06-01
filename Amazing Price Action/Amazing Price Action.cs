/*  CTRADER GURU --> Indicator Template 1.0.8

    Homepage    : https://ctrader.guru/
    Telegram    : https://t.me/ctraderguru
    Twitter     : https://twitter.com/cTraderGURU/
    Facebook    : https://www.facebook.com/ctrader.guru/
    YouTube     : https://www.youtube.com/channel/UCKkgbw09Fifj65W5t5lHeCQ
    GitHub      : https://github.com/ctrader-guru

*/

using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{

    #region Extensions

    public static class SymbolExtensions
    {

        public static double DigitsToPips(this Symbol MySymbol, double Pips)
        {

            return Math.Round(Pips / MySymbol.PipSize, 2);

        }

        public static double PipsToDigits(this Symbol MySymbol, double Pips)
        {

            return Math.Round(Pips * MySymbol.PipSize, MySymbol.Digits);

        }

    }

    public static class BarsExtensions
    {

        public static int GetIndexByDate(this Bars MyBars, DateTime MyTime)
        {

            for (int i = MyBars.ClosePrices.Count - 1; i >= 0; i--)
            {

                if (MyTime == MyBars.OpenTimes[i])
                    return i;

            }

            return -1;

        }

    }

    #endregion

    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class AmazingPriceAction : Indicator
    {

        #region Enums

        public enum Mode
        {

            Close,
            HighLow

        }

        #endregion

        #region Identity

        public const string NAME = "Amazing Price Action";

        public const string VERSION = "1.0.2";

        #endregion

        #region Params

        [Parameter(NAME + " " + VERSION, Group = "Identity", DefaultValue = "https://www.google.com/search?q=ctrader+guru+amazing+price+action")]
        public string ProductInfo { get; set; }

        [Parameter("Period", Group = "Params", DefaultValue = 20, MinValue = 1)]
        public int Period { get; set; }

        [Parameter("Mode", Group = "Params", DefaultValue = Mode.Close)]
        public Mode Moden { get; set; }

        [Output("APA", LineColor = "DodgerBlue")]
        public IndicatorDataSeries Result { get; set; }

        #endregion

        #region Indicator Events

        protected override void Initialize()
        {

            Print("{0} : {1}", NAME, VERSION);

        }

        public override void Calculate(int index)
        {

            if (index > Period)
            {

                double upper = (Moden == Mode.Close) ? Bars.ClosePrices.Maximum(Period) : Bars.HighPrices.Maximum(Period);
                double lower = (Moden == Mode.Close) ? Bars.ClosePrices.Minimum(Period) : Bars.LowPrices.Minimum(Period);

                if (Bars.OpenPrices[index] > Result[index - 1] && lower > Result[index - 1])
                {

                    Result[index] = lower;

                }
                else if (Bars.OpenPrices[index] < Result[index - 1] && upper < Result[index - 1])
                {

                    Result[index] = upper;

                }
                else
                {

                    Result[index] = Result[index - 1];

                }

            }
            else
            {

                Result[index] = Bars.OpenPrices[index];

            }

        }

        #endregion

    }

}

