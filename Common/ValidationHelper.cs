using System.ComponentModel.DataAnnotations;

namespace UserManagement.Common;

public static class ValidationHelper
{
    public static Dictionary<string, string[]> Validate(object instance)
    {
        var ctx = new ValidationContext(instance);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(instance, ctx, results, true);
        var errors = new Dictionary<string, string[]>();
        if (!isValid)
        {
            foreach (var r in results)
            {
                var key = r.MemberNames.FirstOrDefault() ?? "";
                if (!errors.ContainsKey(key)) errors[key] = Array.Empty<string>();
                errors[key] = errors[key].Concat(new[] { r.ErrorMessage ?? "Invalid value." }).ToArray();
            }
        }
        return errors;
    }
}
