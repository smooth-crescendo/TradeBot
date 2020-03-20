﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Tinkoff.Trading.OpenApi.Network;
using Tinkoff.Trading.OpenApi.Models;
using System.Windows.Controls;
using System.Diagnostics;

namespace TradeBot
{
    /// <summary>
    /// Логика взаимодействия для RealTimeTrading.xaml
    /// </summary>
    public partial class RealTimeTrading : UserControl
    {
        private Context context;
        private MarketInstrument activeStock;

        private System.Threading.Timer candlesTimer;

        public RealTimeTrading(Context context, MarketInstrument activeStock)
        {
            InitializeComponent();

            if (activeStock == null || context == null)
                throw new ArgumentNullException();

            this.context = context;
            this.activeStock = activeStock;

            tradingChart.context = context;
            tradingChart.activeStock = activeStock;
            tradingChart.CandlesChange += TradingChart_CandlesChange;

            chartNameTextBlock.Text = activeStock.Name + " (Real-Time)";

            intervalComboBox.ItemsSource = TradingChart.intervalToMaxPeriod.Keys;
            intervalComboBox.SelectedIndex = 0;

            candlesTimer = new System.Threading.Timer((e) => CandlesTimerElapsed(),
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(5));

            DataContext = this;
        }

        // ==================================================
        // events
        // ==================================================

        private void TradingChart_CandlesChange()
        {
            foreach (var indicator in tradingChart.indicators)
            {
                indicator.UpdateState(tradingChart.candlesSpan - 1);
                if (indicator.IsBuySignal(tradingChart.candlesSpan - 1))
                    MessageBox.Show("It's time to buy the instrument");
                if (indicator.IsSellSignal(tradingChart.candlesSpan - 1))
                    MessageBox.Show("It's time to sell the instrument");
            }
        }

        private async void CandlesTimerElapsed()
        {
            //Dispatcher.Invoke(() =>
            //{
            //    if (tradingChart.candlesSeries.Items.Count > 6)
            //    {
            //        var c = tradingChart.candlesSeries.Items[6];
            //        tradingChart.candlesSeries.Items.Insert(0, new OxyPlot.Series.HighLowItem(tradingChart.candlesSeries.Items[0].X - 1,
            //            c.High, c.Low, c.Open, c.Close));
            //        tradingChart.candlesDates.Insert(0, DateTime.Now);
            //        // update indicators values
            //        tradingChart.plotView.InvalidatePlot();
            //    }
            //});
        }

        private async void intervalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CandleInterval interval = CandleInterval.Minute;
            bool intervalFound = false;
            var selectedInterval = intervalComboBox.SelectedItem.ToString();
            foreach (var k in TradingChart.intervalToMaxPeriod.Keys)
            {
                if (k.ToString() == selectedInterval)
                {
                    interval = k;
                    intervalFound = true;
                    break;
                }
            }
            if (!intervalFound)
                return;

            tradingChart.candleInterval = interval;

            await tradingChart.ResetSeries();
        }
    }
}
