using System.Reflection;

namespace XLSXPipeline.Actions
{
    /// <summary>
    /// Attribute to mark properties that should have date/time placeholders automatically replaced
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ReplacePlaceholdersAttribute : Attribute
    {
    }

    /// <summary>
    /// Extension methods for processing placeholder replacement on objects
    /// </summary>
    public static class PlaceholderProcessor
    {
        /// <summary>
        /// Processes all properties marked with [ReplacePlaceholders] attribute on the given object
        /// </summary>
        /// <param name="obj">The object to process</param>
        public static void ProcessPlaceholders(object obj)
        {
            if (obj == null) return;

            var type = obj.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                // Check if property has the ReplacePlaceholders attribute
                if (!property.IsDefined(typeof(ReplacePlaceholdersAttribute), false))
                    continue;

                // Only process string properties
                if (property.PropertyType != typeof(string))
                    continue;

                // Check if property is writable
                if (!property.CanWrite)
                    continue;

                // Get current value
                var currentValue = property.GetValue(obj) as string;

                // Replace placeholders if value exists
                if (!string.IsNullOrEmpty(currentValue))
                {
                    var newValue = Helpers.ReplaceDateTimePlaceholders(currentValue);
                    property.SetValue(obj, newValue);
                }
            }
        }
    }
}
