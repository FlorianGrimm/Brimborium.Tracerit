namespace Brimborium.Tracerit;

/// <summary>
/// Service interface for converting trace data between different formats and property lists.
/// </summary>
public interface ITracorDataConvertService {
        /// <summary>
        /// Converts a value to private trace data with the specified callee identifier.
        /// </summary>
        /// <typeparam name="T">The type of value to convert.</typeparam>
        /// <param name="callee">The tracor identifier for the callee.</param>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted trace data.</returns>
        ITracorData ConvertPrivate<T>(TracorIdentifier callee, T value);

        /// <summary>
        /// Converts a value to public trace data with the specified callee identifier.
        /// </summary>
        /// <typeparam name="T">The type of value to convert.</typeparam>
        /// <param name="callee">The tracor identifier for the callee.</param>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted trace data.</returns>
        ITracorData ConvertPublic<T>(TracorIdentifier callee, T value);

        /// <summary>
        /// Gets the converter for converting objects of the specified type to a property list.
        /// </summary>
        /// <param name="typeValue">The type to get a converter for.</param>
        /// <returns>The converter if available; otherwise, null.</returns>
        ITracorConvertObjectToListProperty? GetTracorConvertObjectToListProperty(Type typeValue);

        /// <summary>
        /// Gets the strongly-typed converter for converting values to a property list.
        /// </summary>
        /// <typeparam name="T">The type of value to convert.</typeparam>
        /// <returns>The converter if available; otherwise, null.</returns>
        ITracorConvertValueToListProperty<T>? GetConverterValueListProperty<T>();

        /// <summary>
        /// Converts an object to a list of trace data properties.
        /// </summary>
        /// <param name="isPublic">True if properties should be public; otherwise, false.</param>
        /// <param name="levelWatchDog">The recursion depth limit to prevent infinite loops.</param>
        /// <param name="name">The property name prefix.</param>
        /// <param name="value">The object to convert.</param>
        /// <param name="listProperty">The target list to add properties to.</param>
        void ConvertObjectToListProperty(
                bool isPublic,
                int levelWatchDog,
                string name,
                object? value,
                List<TracorDataProperty> listProperty);

        /// <summary>
        /// Converts a strongly-typed value to a list of trace data properties.
        /// </summary>
        /// <typeparam name="T">The type of value to convert.</typeparam>
        /// <param name="isPublic">True if properties should be public; otherwise, false.</param>
        /// <param name="levelWatchDog">The recursion depth limit to prevent infinite loops.</param>
        /// <param name="name">The property name prefix.</param>
        /// <param name="value">The value to convert.</param>
        /// <param name="listProperty">The target list to add properties to.</param>
        void ConvertValueToListProperty<T>(
            bool isPublic,
            int levelWatchDog,
            string name,
            T value,
            List<TracorDataProperty> listProperty);
}
