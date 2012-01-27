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

        protected PaymentSchedule()
        {
        }

        public virtual DateTime NextRun { get; protected set; }
        public virtual Interval Interval { get; protected set; }
        public virtual decimal Amount { get; protected set; }
        public virtual string Description { get; protected set; }

        public virtual void CalculateNextRunDate()
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