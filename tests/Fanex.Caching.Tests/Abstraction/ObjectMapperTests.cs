namespace Fanex.Caching.Tests.Abstraction
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Ploeh.AutoFixture;
    using Xunit;
    using Xunit.Extensions;
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

        [Theory, MemberData(nameof(SampleData))]
        public void ConvertTo(string param1, int param2)
        {

        }

        public static IEnumerable<object[]> SampleData
        {
            get
            {
                // Or this could read from a file. :)
                return new[]
                {
                    new object[] { "xUnit", 1 },
                    new object[] { "is fun", 2 },
                    new object[] { "to test with", 3 }
                };
            }
        }
    }
}