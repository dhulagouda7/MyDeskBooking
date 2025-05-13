using System;
using System.ComponentModel.DataAnnotations;

namespace BookMyDesk.Models.Validation
{
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime date)
            {
                if (date.Date < DateTime.Today)
                {
                    return new ValidationResult(ErrorMessage ?? "Date must be in the future");
                }
            }
            return ValidationResult.Success;
        }
    }

    public class BusinessHoursAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is TimeSpan time)
            {
                var minTime = new TimeSpan(8, 0, 0); // 8 AM
                var maxTime = new TimeSpan(18, 0, 0); // 6 PM

                if (time < minTime || time > maxTime)
                {
                    return new ValidationResult(ErrorMessage ?? "Time must be between 8 AM and 6 PM");
                }
            }
            return ValidationResult.Success;
        }
    }

    public class EndTimeAfterStartTimeAttribute : ValidationAttribute
    {
        private readonly string _startTimePropertyName;

        public EndTimeAfterStartTimeAttribute(string startTimePropertyName)
        {
            _startTimePropertyName = startTimePropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(_startTimePropertyName);
            if (property == null)
            {
                return new ValidationResult($"Unknown property: {_startTimePropertyName}");
            }

            var startTime = (TimeSpan)property.GetValue(validationContext.ObjectInstance);
            var endTime = (TimeSpan)value;

            if (endTime <= startTime)
            {
                return new ValidationResult(ErrorMessage ?? "End time must be after start time");
            }

            return ValidationResult.Success;
        }
    }

    public class MaxFutureBookingAttribute : ValidationAttribute
    {
        private readonly int _maxMonths;

        public MaxFutureBookingAttribute(int maxMonths)
        {
            _maxMonths = maxMonths;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime date)
            {
                if (date.Date > DateTime.Today.AddMonths(_maxMonths))
                {
                    return new ValidationResult(ErrorMessage ?? $"Cannot book more than {_maxMonths} months in advance");
                }
            }
            return ValidationResult.Success;
        }
    }
}
