using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PlatBlogs.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class AvatarValidationAttribute : ValidationAttribute
    {
        public int MaxSize { get; } = 500_000;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (!(value is IFormFile file))
                throw new InvalidOperationException("AvatarValidationAttribute must be applied to IFormFile");

            if (!isValidExtension(Path.GetExtension(file.FileName)))
                return new ValidationResult("Uploaded file must be .jpg or .jpeg or .png.");
            if (file.Length > MaxSize)
                return new ValidationResult($"Avatar size is too big. Limit is {MaxSize} bytes.");
            return ValidationResult.Success;
        }

        private bool isValidExtension(string extension)
        {
            switch (extension)
            {
            case ".jpg":
            case ".jpeg":
            case ".png":
                return true;
            default:
                return false;
            }
        }
    }
}
