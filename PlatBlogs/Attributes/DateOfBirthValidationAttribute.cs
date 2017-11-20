using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PlatBlogs.Attributes
{
    public class DateOfBirthValidationAttribute : RangeAttribute
    {
        public DateOfBirthValidationAttribute()
            : base(typeof(DateTime),
                new DateTime(1900, 1, 1).ToString("yyyy MMMM dd"),
                DateTime.Now.AddYears(-14).ToString("yyyy MMMM dd"))
        {
            this.ErrorMessage = $"Date of birth must be between {Minimum} and {Maximum}.";
        }
    }
}
