using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// Import the T4 definitions namespace.
using T4;

// Import the API namespace.
using T4.API;

namespace T4Bot
{
    public partial class MainForm : Form
    {
        // Reference to the main api host object.
        internal Host moHost;
        Market moMarket;
        Account moAccount;

        Order moFirstSellOrder;
        Order moSecondSellOrder;
        Order moThirdSellOrder;
        Order moFourthSellOrder;
        Order moFifthSellOrder;

        bool isFirstOrderFilled = false;

        decimal firstOrderFillPrice = 0;

        decimal FIRST_ORDER_PRICE = 1200;  // You can replace 1000 with the price you want for the first order, no decimal (1000=10.0)


        Order moSecondBuyOrder;
        Order moThirdBuyOrder;
        Order moFourthBuyOrder;
        Order moFifthBuyOrder;

        bool isSecondOrderRepeated = false;
        bool isThirdOrderRepeated = false;
        bool isFourthOrderRepeated = false;
        bool isFifthOrderRepeated = false;

        public MainForm()
        {
            InitializeComponent();
        }

        // Load Form
        public void MainForm_Load(object sender, EventArgs e)
        {
            // Create the api host object.
            moHost = new Host(APIServerType.Simulator, "T4Example", "112A04B0-5AAF-42F4-994E-FA7CB959C60B", Constants.FIRM, Constants.USERNAME, Constants.PASSWORD);

            // Listen for login responses.
            moHost.LoginResponse += new T4.API.Host.LoginResponseEventHandler(moHost_LoginResponse);

            Log("Logging in...");

            // Create a timer with a two hour interval (7200000 milliseconds = 2 hours)
            System.Timers.Timer timer = new System.Timers.Timer(200000); // 8 hour interval set here

            // Hook up the Elapsed event for the timer. 
            timer.Elapsed += OnTimedEvent;

            // Have the timer fire repeated events (true is the default)
            timer.AutoReset = false;

            // Start the timer
            timer.Enabled = true;
        }

        public void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            Log("Timer elapsed, signing in with new user credentials...");

           

            // Create a new host object with different credentials.
            string newUsername = "dhughes";
            string newPassword = "Temp123$";

            // Create the api host object.
            moHost = new Host(APIServerType.Simulator, "T4Example", "112A04B0-5AAF-42F4-994E-FA7CB959C60B", Constants.FIRM, newUsername, newPassword);

            // Listen for login responses.
            moHost.LoginResponse += new T4.API.Host.LoginResponseEventHandler(moHost_LoginResponse);          

            Log("Logging in with new credentials...");
        }

        // Event raised if login fails or reconnects.
        private void moHost_LoginResponse(LoginResponseEventArgs e)
        {
            foreach (Account oAccount in moHost.Accounts)
                Log(string.Format("Account: {0}, Description: {1}", oAccount.AccountNumber, oAccount.Description));

            Log(string.Format("Login Response: {0} {1}", e.Result.ToString(), e.Text));

            Log("Running strategy on: " + Constants.MARKET);

            moAccount = moHost.Accounts.FirstOrDefault(a => a.AccountNumber == "akazmar2");
            if (moAccount == null)
            {
                Log("Failed to find account with account number: akazmar2");
                return;
            }
                        // Call the method to subscribe to market data updates
            SubscribeToMarketDataUpdates();
        }

        private void SubscribeToMarketDataUpdates()
        {
            
            moAccount.Subscribe(true, arg =>
            {
                
                moAccount.OrderTrade += MoAccount_OrderTrade;

                moHost.MarketData.GetMarkets("CME_EqOp", "EX1", e2 =>
                {
                    foreach (Market oMarket in e2.Markets)
                    {
                        Log(oMarket.MarketID);
                        if (oMarket.MarketID == Constants.MARKET)
                        {
                            
                            moMarket = oMarket;
                            if (moMarket != null)
                            {
                                moMarket.MarketCheckSubscription += MoMarket_MarketCheckSubscription;
                                moMarket.MarketDepthUpdate += MoMarket_MarketDepthUpdate;
                                moMarket.DepthSubscribe(DepthBuffer.SmartTrade, DepthLevels.Normal);
                            }
                            Log(string.Format("Market: {0}, Description: {1}", oMarket.MarketID, oMarket.Description));
                        }
                    }
                });
            });
        }
        private void MoMarket_MarketCheckSubscription(MarketCheckSubscriptionEventArgs e)
        {
            e.DepthSubscribeAtLeast(DepthBuffer.SmartTrade, DepthLevels.BestOnly);
        }

        // Logging
        private delegate void LogDelegate(string what, bool unique = false);
        private void Log(string what, bool unique = false)
        {
            if (textBox1.InvokeRequired)
            {
                textBox1.BeginInvoke(new LogDelegate(Log), what, unique);
            }
            else
            {
                if (unique)
                {
                    // Check if the last line in the text box contains the new message
                    if (textBox1.Lines.Length > 0 && textBox1.Lines[textBox1.Lines.Length - 2].Contains(what))
                    {
                        return; // Return if the message already exists
                    }
                }
                // Append the new message to the text box
                textBox1.AppendText(DateTime.Now.ToString("HH:mm:ss") + " -- " + what + Environment.NewLine);
            }
        }

        // Form Exit
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        // Event raised when the the market depth is updated
        private void MoMarket_MarketDepthUpdate(MarketDepthUpdateEventArgs e)
        {
            if (!isFirstOrderFilled)
            {
                isFirstOrderFilled = true;
                firstOrderFillPrice = FIRST_ORDER_PRICE;

                // reset flags for repeat orders
                isSecondOrderRepeated = false;
                isThirdOrderRepeated = false;
                isFourthOrderRepeated = false;
                isFifthOrderRepeated = false;

                Log("First Sell Order Filled manually at price: " + FIRST_ORDER_PRICE.ToString());
            }

            if (isFirstOrderFilled)
            {
                if (moSecondSellOrder == null && !isSecondOrderRepeated && moSecondBuyOrder == null)
                {
                    decimal entryPrice = moMarket.AddPriceIncrements(Constants.SELL_SECOND, firstOrderFillPrice);
                    moSecondSellOrder = moHost.SubmitOrder(moAccount, moMarket, BuySell.Sell, PriceType.Limit, Constants.VOLUME, entryPrice);
                    Log($"Second Sell Order created - Price: {entryPrice}, Volume: {Constants.VOLUME}");
                    isSecondOrderRepeated = true;
                }
                if (moThirdSellOrder == null && !isThirdOrderRepeated && moThirdBuyOrder == null)
                {
                    decimal entryPrice = moMarket.AddPriceIncrements(Constants.SELL_THIRD, firstOrderFillPrice);
                    moThirdSellOrder = moHost.SubmitOrder(moAccount, moMarket, BuySell.Sell, PriceType.Limit, Constants.VOLUME, entryPrice);
                    Log($"Third Sell Order created - Price: {entryPrice}, Volume: {Constants.VOLUME}");
                    isThirdOrderRepeated = true;
                }
                if (moFourthSellOrder == null && !isFourthOrderRepeated && moFourthBuyOrder == null)
                {
                    decimal entryPrice = moMarket.AddPriceIncrements(Constants.SELL_FOURTH, firstOrderFillPrice);
                    moFourthSellOrder = moHost.SubmitOrder(moAccount, moMarket, BuySell.Sell, PriceType.Limit, Constants.VOLUME, entryPrice);
                    Log($"Fourth Sell Order created - Price: {entryPrice}, Volume: {Constants.VOLUME}");
                    isFourthOrderRepeated = true;
                }
                if (moFifthSellOrder == null && !isFifthOrderRepeated && moFifthBuyOrder == null)
                {
                    decimal entryPrice = moMarket.AddPriceIncrements(Constants.SELL_FIFTH, firstOrderFillPrice);
                    moFifthSellOrder = moHost.SubmitOrder(moAccount, moMarket, BuySell.Sell, PriceType.Limit, Constants.VOLUME, entryPrice);
                    Log($"Fifth Sell Order created - Price: {entryPrice}, Volume: {Constants.VOLUME}");
                    isFifthOrderRepeated = true;
                }
            }
        }

        // Event raised when the order has received a fill.
        private void MoAccount_OrderTrade(OrderTradeEventArgs e)
        {
            foreach (Trade oTrade in e.Trades)
                Log(string.Format("OrderTrade: {0}, Fill: {1}@{2}, PossibleResend: {3}",
                    e.Order.UniqueID, oTrade.Volume, oTrade.Price, e.PossibleResend));

            if (e.Order.Status == OrderStatus.Finished)
            {
                if (e.Order == moFirstSellOrder)
                {
                    isFirstOrderFilled = true;
                    firstOrderFillPrice = e.Trades.Last().Price;
                    Log("First Sell Order Filled.");

                    // reset flags for repeat orders
                    isSecondOrderRepeated = false;
                    isThirdOrderRepeated = false;
                    isFourthOrderRepeated = false;
                    isFifthOrderRepeated = false;
                }
                else if (e.Order == moSecondSellOrder)
                {
                    Log("Second Sell Order Filled.");
                    moSecondSellOrder = null; // Add this line
                    decimal profitTargetPrice = moMarket.AddPriceIncrements(Constants.PT2, e.Trades.Last().Price);
                    moSecondBuyOrder = moHost.SubmitOrder(moAccount, moMarket, BuySell.Buy, PriceType.Limit, Constants.VOLUME, profitTargetPrice);
                    Log($"Second Buy Order created - Price: {profitTargetPrice}, Volume: {Constants.VOLUME}");
                    isSecondOrderRepeated = false;
                }
                else if (e.Order == moThirdSellOrder)
                {
                    Log("Third Sell Order Filled.");
                    moThirdSellOrder = null; // Add this line
                    decimal profitTargetPrice = moMarket.AddPriceIncrements(Constants.PT3, e.Trades.Last().Price);
                    moThirdBuyOrder = moHost.SubmitOrder(moAccount, moMarket, BuySell.Buy, PriceType.Limit, Constants.VOLUME, profitTargetPrice);
                    Log($"Third Buy Order created - Price: {profitTargetPrice}, Volume: {Constants.VOLUME}");
                    isThirdOrderRepeated = false;
                }
                else if (e.Order == moFourthSellOrder)
                {
                    Log("Fourth Sell Order Filled.");
                    moFourthSellOrder = null; // Add this line
                    decimal profitTargetPrice = moMarket.AddPriceIncrements(Constants.PT4, e.Trades.Last().Price);
                    moFourthBuyOrder = moHost.SubmitOrder(moAccount, moMarket, BuySell.Buy, PriceType.Limit, Constants.VOLUME, profitTargetPrice);
                    Log($"Fourth Buy Order created - Price: {profitTargetPrice}, Volume: {Constants.VOLUME}");
                    isFourthOrderRepeated = false;
                }
                else if (e.Order == moFifthSellOrder)
                {
                    Log("Fifth Sell Order Filled.");
                    moFifthSellOrder = null; // Add this line
                    decimal profitTargetPrice = moMarket.AddPriceIncrements(Constants.PT5, e.Trades.Last().Price);
                    moFifthBuyOrder = moHost.SubmitOrder(moAccount, moMarket, BuySell.Buy, PriceType.Limit, Constants.VOLUME, profitTargetPrice);
                    Log($"Fifth Buy Order created - Price: {profitTargetPrice}, Volume: {Constants.VOLUME}");
                    isFifthOrderRepeated = false;
                }
                else if (e.Order == moSecondBuyOrder)
                {
                    Log("Second Buy Order Filled.");
                    moSecondBuyOrder = null;
                    isSecondOrderRepeated = false;
                }
                else if (e.Order == moThirdBuyOrder)
                {
                    Log("Third Buy Order Filled.");
                    moThirdBuyOrder = null;
                    isThirdOrderRepeated = false;
                }
                else if (e.Order == moFourthBuyOrder)
                {
                    Log("Fourth Buy Order Filled.");
                    moFourthBuyOrder = null;
                    isFourthOrderRepeated = false;
                }
                else if (e.Order == moFifthBuyOrder)
                {
                    Log("Fifth Buy Order Filled.");
                    moFifthBuyOrder = null;
                    isFifthOrderRepeated = false;
                }
            }
        }
    }
}
