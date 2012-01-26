﻿using System;

namespace Suteki.TardisBank.Model
{
    using SharpArch.Domain.DomainModel;

    public class PaymentSchedule : Entity
    {
        public PaymentSchedule(DateTime nextRun, Interval interval, decimal amount, string description)
        {
            NextRun = nextRun;
            Interval = interval;
            Amount = amount;
            Description = description;
        }

        public DateTime NextRun { get; private set; }
        public Interval Interval { get; private set; }
        public decimal Amount { get; private set; }
        public string Description { get; private set; }

        public void CalculateNextRunDate()
        {
            switch (Interval)
            {
                case Interval.Day:
                    NextRun = NextRun.AddDays(1);
                    break;
                case Interval.Week:
                    NextRun = NextRun.AddDays(7);
                    break;
                case Interval.Month:
                    NextRun = NextRun.AddMonths(1);
                    break;
            }
        }
    }
}