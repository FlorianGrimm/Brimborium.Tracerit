namespace Brimborium.Tracerit.Test.DataAccessor;

/// <summary>
/// Unit tests for ITracorData implementations and related data accessor functionality.
/// </summary>
public class ValueTracorDataTests {

    [Test]
    public async Task ValueTracorData_ShouldProvideValueProperty() {
        // Arrange
        var testValue = "test string";
        var tracorData = new ValueTracorData<string>(testValue);

        // Act & Assert
        await Assert.That(tracorData.GetListPropertyName()).Contains(
            TracorConstants.TracorDataPropertyNameValue
            );
        await Assert.That(tracorData.TryGetPropertyValue(
            TracorConstants.TracorDataPropertyNameValue,
            out var propertyValue)).IsTrue();
        await Assert.That(propertyValue).IsEqualTo(testValue);
        await Assert.That(tracorData["Value"]).IsEqualTo(testValue);
    }

    [Test]
    public async Task ValueTracorData_ShouldReturnOriginalValue() {
        // Arrange
        var testValue = 42;
        var tracorData = new ValueTracorData<int>(testValue);

        // Act & Assert
        await Assert.That(tracorData.TryGetOriginalValue(out var originalValue)).IsTrue();
        await Assert.That(originalValue).IsEqualTo(testValue);
    }

    [Test]
    public async Task ValueTracorData_ShouldReturnNullForUnknownProperty() {
        // Arrange
        var tracorData = new ValueTracorData<string>("test");

        // Act & Assert
        await Assert.That(tracorData.TryGetPropertyValue("UnknownProperty", out var propertyValue)).IsFalse();
        await Assert.That(propertyValue).IsNull();
        await Assert.That(tracorData["UnknownProperty"]).IsNull();
    }

    [Test]
    public async Task SystemUriTracorDataAccessor_ShouldProvideUriProperties() {
        // Arrange
        var uri = new Uri("https://example.com/path?query=value");
        var accessor = new SystemUriTracorDataAccessor();

        // Act
        var propertyNames = accessor.GetListPropertyNameTyped(uri);

        // Assert
        await Assert.That(propertyNames).Contains("Value");
        await Assert.That(propertyNames).Contains("ToString");
        await Assert.That(propertyNames).Contains("Host");
        await Assert.That(propertyNames).Contains("PathAndQuery");
    }

    [Test]
    public async Task SystemUriTracorDataAccessor_ShouldReturnCorrectPropertyValues() {
        // Arrange
        var uri = new Uri("https://example.com/path?query=value");
        var accessor = new SystemUriTracorDataAccessor();

        // Act & Assert
        await Assert.That(accessor.TryGetPropertyValueTyped(uri, "Value", out var valueProperty)).IsTrue();
        await Assert.That(valueProperty).IsEqualTo(uri);

        await Assert.That(accessor.TryGetPropertyValueTyped(uri, "Host", out var hostProperty)).IsTrue();
        await Assert.That(hostProperty).IsEqualTo("example.com");

        await Assert.That(accessor.TryGetPropertyValueTyped(uri, "PathAndQuery", out var pathProperty)).IsTrue();
        await Assert.That(pathProperty).IsEqualTo("/path?query=value");

        await Assert.That(accessor.TryGetPropertyValueTyped(uri, "ToString", out var toStringProperty)).IsTrue();
        await Assert.That(toStringProperty).IsEqualTo(uri.ToString());
    }

    [Test]
    public async Task SystemUriTracorDataAccessor_ShouldReturnFalseForUnknownProperty() {
        // Arrange
        var uri = new Uri("https://example.com");
        var accessor = new SystemUriTracorDataAccessor();

        // Act & Assert
        await Assert.That(accessor.TryGetPropertyValueTyped(uri, "UnknownProperty", out var propertyValue)).IsFalse();
        await Assert.That(propertyValue).IsNull();
    }

    [Test]
    public async Task TracorDataAccessorFactory_ShouldCreateTracorDataForCorrectType() {
        // Arrange
        var uri = new Uri("https://example.com");
        var accessor = new SystemUriTracorDataAccessor();
        var factory = new BoundAccessorTracorDataFactory<Uri>(accessor, new(0));

        // Act & Assert
        await Assert.That(factory.TryGetData(uri, out var tracorData)).IsTrue();
        await Assert.That(tracorData).IsNotNull();

        await Assert.That(factory.TryGetDataTyped(uri, out var typedTracorData)).IsTrue();
        await Assert.That(typedTracorData).IsNotNull();
    }

    [Test]
    public async Task TracorDataAccessorFactory_ShouldReturnFalseForIncorrectType() {
        // Arrange
        var accessor = new SystemUriTracorDataAccessor();
        var factory = new BoundAccessorTracorDataFactory<Uri>(accessor, new(0));

        // Act & Assert
        await Assert.That(factory.TryGetData("not a uri", out var tracorData)).IsFalse();
        await Assert.That(tracorData).IsNull();
    }

    [Test]
    public async Task ValueAccessorFactory_ShouldCreateValueTracorData() {
        // Arrange
        var factory = new ValueAccessorFactory<string>(new(0));
        var testValue = "test string";

        // Act & Assert
        await Assert.That(factory.TryGetData(testValue, out var tracorData)).IsTrue();
        await Assert.That(tracorData).IsNotNull();
        await Assert.That(tracorData).IsTypeOf<ValueTracorData<string>>();

        await Assert.That(factory.TryGetDataTyped(testValue, out var typedTracorData)).IsTrue();
        await Assert.That(typedTracorData).IsNotNull();
    }

    [Test]
    public async Task NullTypeData_ShouldProvideEmptyProperties() {
        // Arrange
        var nullData = new NullTypeData();

        // Act & Assert
        await Assert.That(nullData.GetListPropertyName()).IsEmpty();
        await Assert.That(nullData.TryGetPropertyValue("AnyProperty", out var propertyValue)).IsFalse();
        await Assert.That(propertyValue).IsNull();
        await Assert.That(nullData["AnyProperty"]).IsNull();
    }

    [Test]
    public async Task ITracorDataExtension_TryGetPropertyValue_ShouldCastCorrectly() {
        // Arrange
        var tracorData = new ValueTracorData<int>(42);

        // Act & Assert
        await Assert.That(tracorData.TryGetPropertyValue<int>(
            TracorConstants.TracorDataPropertyNameValue,
            out var intValue)).IsTrue();
        await Assert.That(intValue).IsEqualTo(42);

        await Assert.That(tracorData.TryGetPropertyValue<string>(
            TracorConstants.TracorDataPropertyNameValue,
            out var stringValue)).IsFalse();
        await Assert.That(stringValue).IsNull();
    }
}
