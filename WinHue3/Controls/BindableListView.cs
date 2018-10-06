﻿using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace WinHue3.Controls
{
    public class BindableListView : ListView
    {
        public BindableListView()
        {
            SelectionChanged += BindableListView_SelectionChangedEvent;
        }

        void BindableListView_SelectionChangedEvent(object sender, SelectionChangedEventArgs e)
        {
            IList listselected = (IList) Activator.CreateInstance(SelectedItemsList.GetType());
            foreach (var i in SelectedItems)
            {
                listselected.Add(i);
            }
            SelectedItemsList = listselected;
        }

        public IList SelectedItemsList
        {
            get => (IList)GetValue(SelectedItemsListProperty);
            set => SetValue(SelectedItemsListProperty, value);
        }

        public static readonly DependencyProperty SelectedItemsListProperty =
        DependencyProperty.Register("SelectedItemsList", typeof(IList), typeof(BindableListView), new FrameworkPropertyMetadata(null,FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,RaisePropertyChanged));

        private static void RaisePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetCurrentValue(SelectedItemsListProperty, e.NewValue);
            BindableListView blv = d as BindableListView; 
            blv?.OnPropertyModified(e);
        }

        private void OnPropertyModified(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) return;
            IList listsel = (IList)e.NewValue;
            foreach (var i in listsel)
            {
                SelectedItems.Add(i);
            }
        }

    }
}
