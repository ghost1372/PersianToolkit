﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PersianToolkit
{
    public class HolidayHelper
    {
        public static DateTime GetDate(DependencyObject obj)
        {
            return (DateTime)obj.GetValue(DateProperty);
        }

        public static void SetDate(DependencyObject obj, DateTime value)
        {
            obj.SetValue(DateProperty, value);
        }

        public static readonly DependencyProperty DateProperty =
        DependencyProperty.RegisterAttached("Date", typeof(DateTime), typeof(HolidayHelper), new PropertyMetadata { PropertyChangedCallback = DatePropertyChanged });

        private async static void DatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var date = GetDate(d);
            SetIsHoliday(d, await CheckIsHoliday(date));
        }

        private static async Task<bool> CheckIsHoliday(DateTime date)
        {
            bool result = false;
            await Task.Run(() => {
                PersianCalendar pc = new PersianCalendar();

                //Todo: every year must be checked
                // -1 is HijriAdjustment for fixing right day
                var hijriNow = IslamicDateUtils.GregorianToIslamicDay(date.Year, date.Month, date.Day - 1);
                JObject oo = JObject.Parse(Properties.Resources.events);

                var getPersianEvents = oo["Persian Calendar"].Where(x => x != null && x.SelectToken("day").ToString() == pc.GetDayOfMonth(date).ToString() &&
                x.SelectToken("month").ToString() == pc.GetMonth(date).ToString() && x.SelectToken("type").ToString() == "Iran")
                    .Select(m => (string)m.SelectToken("holiday")).ToArray();

                var getHijriEvents = oo["Hijri Calendar"].Where(x => x != null && x.SelectToken("day").ToString() == hijriNow.Day.ToString() &&
                x.SelectToken("month").ToString() == hijriNow.Month.ToString() && x.SelectToken("type").ToString() == "Islamic Iran")
                    .Select(m => (string)m.SelectToken("holiday")).ToArray();


                if (string.Join(", ", getPersianEvents).Contains("True") || string.Join(", ", getHijriEvents).Contains("True"))
                    result = true;
                else
                    result = false;
            });
            return result;
        }

        private static readonly DependencyPropertyKey IsHolidayPropertyKey =
        DependencyProperty.RegisterAttachedReadOnly("IsHoliday", typeof(bool), typeof(HolidayHelper), new PropertyMetadata());

        public static readonly DependencyProperty IsHolidayProperty = IsHolidayPropertyKey.DependencyProperty;

        public static bool GetIsHoliday(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsHolidayProperty);
        }

        private static void SetIsHoliday(DependencyObject obj, bool value)
        {
            obj.SetValue(IsHolidayPropertyKey, value);
        }
    }
}
