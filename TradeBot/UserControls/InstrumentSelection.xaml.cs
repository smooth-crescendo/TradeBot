﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;

namespace TradeBot
{
    /// <summary>
    ///     Логика взаимодействия для StockSelection.xaml
    /// </summary>
    public partial class InstrumentSelection : UserControl
    {
        readonly Context context;
        readonly TabItem parent;
        List<string> instrumentsLabels;
        MarketInstrumentList instruments;

        public InstrumentSelection(Context context, TabItem parent)
        {
            InitializeComponent();

            this.context = context;
            this.parent = parent;

            Dispatcher.InvokeAsync(async () =>
            {
                instruments = await context.MarketEtfsAsync();
                instrumentsLabels = instruments.Instruments.ConvertAll(v => $"{v.Ticker} ({v.Name})");
                TickerComboBox.ItemsSource = instrumentsLabels;
            });
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            InstrumentErrorTextBlock.Text = string.Empty;
            try
            {
                var activeInstrument =
                    instruments.Instruments[instrumentsLabels.FindIndex(v => v == TickerComboBox.Text)];

                parent.Header = activeInstrument.Name;

                if (RealTimeRadioButton.IsChecked == true)
                {
                    parent.Header += " (Real-Time)";
                    parent.Content = new RealTimeTrading(context, activeInstrument);
                }
                else if (TestingRadioButton.IsChecked == true)
                {
                    parent.Header += " (Testing)";
                    parent.Content = new TestingTrading(context, activeInstrument);
                }
            }
            catch (Exception)
            {
                InstrumentErrorTextBlock.Text = "* Pick an instrument first";
                TickerComboBox.IsDropDownOpen = true;
            }
        }

        void TickerComboBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TickerComboBox.ItemsSource == null)
                return;
            
            var tb = (TextBox) e.OriginalSource;
            if (tb.SelectionStart != 0)
                TickerComboBox.SelectedItem = null;

            if (TickerComboBox.SelectedItem != null) return;
            var cv = (CollectionView) CollectionViewSource.GetDefaultView(TickerComboBox.ItemsSource);
            cv.Filter = s =>
                ((string) s).IndexOf(TickerComboBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;

            TickerComboBox.IsDropDownOpen = cv.Count < 100;
            tb.SelectionLength = 0;
            tb.SelectionStart = tb.Text.Length;
        }

        void TickerComboBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            TickerComboBox.IsDropDownOpen = ((CollectionView)CollectionViewSource.GetDefaultView(TickerComboBox.ItemsSource)).Count < 100;
        }
    }
}