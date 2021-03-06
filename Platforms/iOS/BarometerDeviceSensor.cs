﻿using CoreMotion;
using Foundation;
using Plugin.DeviceSensors.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.DeviceSensors.Platforms.iOS
{
    public class BarometerDeviceSensor :  IDeviceSensor<double>
    {
        bool isActive = false;
        double _lastReading = 0;
        DateTime _lastReadingTime;
        int _readingInterval;
        bool firstRead = false;

        static readonly Lazy<CMAltimeter> altimeter = new Lazy<CMAltimeter>(() => new CMAltimeter());

        public bool IsSupported => CMAltimeter.IsRelativeAltitudeAvailable;

        public bool IsActive { get { return isActive; } }

        public double LastReading
        {
            get
            {
                return _lastReading;
            }
        }

        public int ReadingInterval
        {
            get
            {
                return _readingInterval;
            }
            set
            {
                _readingInterval = value;
                _lastReadingTime = DateTime.Now;
            }
        }

        public event EventHandler<DeviceSensorReadingEventArgs<double>> OnReadingChanged;

        public void StartReading(int reportingInterval = -1)
        {
            firstRead = true;
            altimeter.Value.StartRelativeAltitudeUpdates(NSOperationQueue.CurrentQueue, OnBarometerChanged);
            if (reportingInterval >= 0)
            {
                _readingInterval = reportingInterval;
            }
            isActive = true;
        }

        public void StopReading()
        {
            firstRead = false;
            isActive = false;
            altimeter.Value.StopRelativeAltitudeUpdates();
        }

        void OnBarometerChanged(CMAltitudeData data, NSError error)
        {
            var currentReadingTime = DateTime.Now;

            if (firstRead || (currentReadingTime - _lastReadingTime).TotalMilliseconds >= ReadingInterval)
            {
                _lastReading = data.Pressure.DoubleValue;
                OnReadingChanged?.Invoke(this, new DeviceSensorReadingEventArgs<double>(_lastReading));
                firstRead = false;
            }
        }
    }
}
