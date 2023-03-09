/*  CTRADER GURU --> Indicator Template 1.0.8

    Homepage    : https://ctrader.guru/
    Telegram    : https://t.me/ctraderguru
    Twitter     : https://twitter.com/cTraderGURU/
    Facebook    : https://www.facebook.com/ctrader.guru/
    YouTube     : https://www.youtube.com/channel/UCKkgbw09Fifj65W5t5lHeCQ
    GitHub      : https://github.com/ctrader-guru

*/

using System;
using System.Threading.Tasks;
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

    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
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

        public const string VERSION = "1.0.5";

        #endregion

        #region Params

        [Parameter(NAME + " " + VERSION, Group = "Identity", DefaultValue = "https://www.google.com/search?q=ctrader+guru+amazing+price+action")]
        public string ProductInfo { get; set; }

        [Parameter("Period", Group = "Params", DefaultValue = 3, MinValue = 1)]
        public int Period { get; set; }

        [Parameter("Mode", Group = "Params", DefaultValue = Mode.Close)]
        public Mode Moden { get; set; }

        [Parameter("Enable?", Group = "Signal", DefaultValue = true)]
        public bool EnableSignal { get; set; }

        [Parameter("Color Up", Group = "Signal", DefaultValue = "DodgerBlue")]
        public Color ColorUp { get; set; }

        [Parameter("Color Dw", Group = "Signal", DefaultValue = "Red")]
        public Color ColorDw { get; set; }

        [Parameter("Enabled?", Group = "Webhook", DefaultValue = false)]
        public bool WebhookEnabled { get; set; }

        [Parameter("EndPoint", Group = "Webhook", DefaultValue = "https://api.telegram.org/bot[ YOUR TOKEN ]/sendMessage")]
        public string EndPoint { get; set; }
        public Webhook MyWebook;

        [Parameter("POST", Group = "Webhook", DefaultValue = "chat_id=[ @CHATID ]&text={0}")]
        public string PostParams { get; set; }

        [Output("APA", LineColor = "Gray", PlotType = PlotType.DiscontinuousLine, Thickness = 1)]
        public IndicatorDataSeries Result { get; set; }

        [Output("APA Up", LineColor = "DodgerBlue", PlotType = PlotType.DiscontinuousLine, Thickness = 1)]
        public IndicatorDataSeries ResultUp { get; set; }

        [Output("APA Down", LineColor = "Red", PlotType = PlotType.DiscontinuousLine, Thickness = 1)]
        public IndicatorDataSeries ResultDw { get; set; }

        [Output("APA Signal", LineColor = "Green")]
        public IndicatorDataSeries ResultSignal { get; set; }

        int LastDirection = 0;

        #endregion

        #region Indicator Events

        protected override void Initialize()
        {

            Print("{0} : {1}", NAME, VERSION);

            if (!WebhookEnabled)
                return;

            EndPoint = EndPoint.Trim();
            if (EndPoint.Length < 1)
            {

                MessageBox.Show("Wrong 'EndPoint', es. 'https://api.telegram.org/bot[ YOUR TOKEN ]/sendMessage', the service will be disabled.", "Error",MessageBoxButton.OK,MessageBoxImage.Error);
                WebhookEnabled = false;
                return;

            }

            PostParams = PostParams.Trim();
            if (PostParams.IndexOf("{0}") < 0)
            {

                MessageBox.Show("Wrong 'POST params', es. 'chat_id=[ @CHATID ]&text={0}' there must be the parameter '{0}', the service will be disabled.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                WebhookEnabled = false;
                return;

            }

            MyWebook = new Webhook(EndPoint);
            // --> ToWebhook(string.Format("{0}: APA | {1} | Activated", SymbolName, TimeFrame.ToString()), true);

        }

        public override void Calculate(int index)
        {

            ResultUp[index] = double.NaN;
            ResultDw[index] = double.NaN;
            
            if (index > Period)
            {

                double upper = (Moden == Mode.Close) ? Bars.ClosePrices.Maximum(Period) : Bars.HighPrices.Maximum(Period);
                double lower = (Moden == Mode.Close) ? Bars.ClosePrices.Minimum(Period) : Bars.LowPrices.Minimum(Period);

                if (Bars.OpenPrices[index] > Result[index - 1] && lower > Result[index - 1])
                {

                    Result[index] = lower;
                    ResultUp[index] = lower;
                    ResultUp[index-1] = Result[index-1];

                    if (EnableSignal && LastDirection != 1 ) {

                        ResultSignal[index] = Bars.OpenPrices[index];
                        ChartIcon IconUp = Chart.DrawIcon("Up" + index, ChartIconType.Circle, index, Bars.OpenPrices[index], ColorUp);
                        IconUp.IsInteractive = false;
                        IconUp.IsLocked = true;
                        ToWebhook(string.Format("{0}: APA | {1} | Buy signal", SymbolName, TimeFrame.ToString()));

                    }
                    
                    LastDirection = 1;

                }
                else if (Bars.OpenPrices[index] < Result[index - 1] && upper < Result[index - 1])
                {

                    Result[index] = upper;
                    ResultDw[index] = upper;
                    ResultDw[index-1] = Result[index-1];


                    if (EnableSignal && LastDirection != -1)
                    {

                        ResultSignal[index] = Bars.OpenPrices[index];
                        ChartIcon IconDw = Chart.DrawIcon("Dw" + index, ChartIconType.Circle, index, Bars.OpenPrices[index], ColorDw);
                        IconDw.IsInteractive = false;
                        IconDw.IsLocked = true;
                        ToWebhook(string.Format("{0}: APA | {1} | Sell signal", SymbolName, TimeFrame.ToString()));

                    }
                    LastDirection = -1;

                }
                else
                {

                    Result[index] = Result[index - 1];
                    
                    if(LastDirection == 1){

                        ResultUp[index] = Result[index];
        
                    }else if(LastDirection == -1){
            
                        ResultDw[index] = Result[index];
            
                    }
                    
                }

            }
            else
            {

                Result[index] = Bars.OpenPrices[index];

            }

        }

        private void ToWebhook(string mex, bool force = false)
        {

            bool canSendMessage = RunningMode == RunningMode.RealTime;

            if ((!IsLastBar && !force) || !canSendMessage || MyWebook == null || !WebhookEnabled || mex == null || mex.Trim().Length == 0)
                return;

            mex = mex.Trim();

            Task<Webhook.WebhookResponse> webhook_result = Task.Run(async () => await MyWebook.SendAsync(string.Format(PostParams, mex)));

            // --> We don't know which webhook the client is using, probably a json response
            // --> var Telegram = JObject.Parse(webhook_result.Result.Response);
            // --> Print(Telegram["ok"]);

        }

        #endregion

    }

}

