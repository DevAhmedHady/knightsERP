using Core.DomainExceptions;

namespace Core
{
    internal static class Consts
    {
        public static class ValidationRoles
        {
            public static void IsNotNullOrEmpty(string propName, object value)
            {
                if (value != null && string.IsNullOrEmpty(value.ToString()))
                    throw new ValidationException(propName, $"{propName}: Value Can't be Null or Empty");
            }

            public static void IsNotNullOrEmpty<T>(string propName, IEnumerable<T> value)
            {
                if (value == null || !value.Any())
                    throw new ValidationException(propName, $"{propName}: Collection Can't be Null or Empty");
            }

            public static void IsGreaterThan(string propName, int value, int threshold)
            {
                if (value <= threshold)
                    throw new ValidationException(propName, $"{propName}: Value Must be Greater Than {threshold}");
            }

            public static void IsGreaterThan(string propName, decimal value, decimal threshold)
            {
                if (value <= threshold)
                    throw new ValidationException(propName, $"{propName}: Value Must be Greater Than {threshold}");
            }

            public static void IsValidEmail(string propName, string email)
            {
                var pattern = @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$";
                var regex = new System.Text.RegularExpressions.Regex(pattern);

                if (string.IsNullOrEmpty(email) || !regex.IsMatch(email))
                    throw new ValidationException(propName, $"{propName}: Invalid Email Format");
            }

            public static void IsValidPassword(string propName, string password)
            {
                var pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
                var regex = new System.Text.RegularExpressions.Regex(pattern);
                if (string.IsNullOrEmpty(password) || !regex.IsMatch(password))
                    throw new ValidationException(propName, $"{propName}: Password Must Be At Least 8 Characters Long");
            }   
        }
    }
}
