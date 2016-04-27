namespace Fanex.Caching.Tests.Abstraction
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Mocks;
    using Ploeh.AutoFixture;
    using Xunit;

    public class ObjectMapperTests
    {
        private Fixture _fixture;

        public ObjectMapperTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void ConvertTo_HashtableIsNull_ThrowArgumentNullException()
        {
            // Arrange.
            Hashtable data = null;

            // Act and assert.
            Assert.Throws<ArgumentNullException>(
                () => ObjectMapper.ConvertTo<ConnectionSettingMock>(
                    data,
                    _fixture.Create<Dictionary<string, Func<object, object>>>()));
        }

        [Fact]
        public void ConvertTo_HashtableDoesNotContainNeededData_ReturnEmptyObject()
        {
            // Arrange.
            var data = new Hashtable
            {
                { "prop1", "value1" },
                { "prop2", "value2" }
            };

            // Act.
            var actualObject = ObjectMapper.ConvertTo<ConnectionSettingMock>(
                    data,
                    _fixture.Create<Dictionary<string, Func<object, object>>>());

            // Assert.
            Assert.Equal(actualObject, new ConnectionSettingMock());
        }

        [Fact]
        public void ConvertTo_HashtableDoesNotContainAllData_DefaultValuesForMissingProperties()
        {
            // Arrange.
            var data = new Hashtable
            {
                { "Property1", "value1" },
                { "prop2", "value2" }
            };

            // Act.
            var actualObject = ObjectMapper.ConvertTo<ConnectionSettingMock>(
                    data,
                    _fixture.Create<Dictionary<string, Func<object, object>>>());

            // Assert.
            Assert.Null(actualObject.Property2);
        }

        [Fact]
        public void ConvertTo_HashtableContainAllData_ReturnExpectedObject()
        {
            // Arrange.
            var data = new Hashtable
            {
                { "Property1", "value1" },
                { "Property2", "value2" }
            };

            // Act.
            var actualObject = ObjectMapper.ConvertTo<ConnectionSettingMock>(
                    data,
                    _fixture.Create<Dictionary<string, Func<object, object>>>());

            // Assert.
            var expectedObject = new ConnectionSettingMock("value1", "value2");

            Assert.Equal(actualObject, expectedObject);
        }

        [Fact]
        public void ConvertTo_PropertiesMaperIsNotEmpty_CallMapperWithAssociateProperty()
        {
            // Arrange.
            var data = new Hashtable
            {
                { "Property1", "value1" },
                { "Property2", "value2" }
            };
            var propertiesMapper = new Dictionary<string, Func<object, object>>();

            propertiesMapper.Add("Property2", configuredValue => "Updated value2");

            // Act.
            var actualObject = ObjectMapper.ConvertTo<ConnectionSettingMock>(data, propertiesMapper);

            // Assert.
            var expectedObject = new ConnectionSettingMock("value1", "Updated value2");

            Assert.Equal(actualObject, expectedObject);
        }
    }
}