namespace Fanex.Caching.Tests.Mocks
{
    using System;

    public class ConnectionSettingMock : IEquatable<ConnectionSettingMock>
    {
        public ConnectionSettingMock()
        {
        }

        public ConnectionSettingMock(string property1, string property2)
        {
            Property1 = property1;
            Property2 = property2;
        }

        public string Property1 { get; set; }

        public string Property2 { get; set; }

        public override bool Equals(object obj)
        {
            ConnectionSettingMock otherObj = obj as ConnectionSettingMock;

            if (otherObj == null)
            {
                return false;
            }

            if (ReferenceEquals(this, otherObj))
            {
                return true;
            }

            return Equals(otherObj);
        }

        public bool Equals(ConnectionSettingMock other)
            => Equals(Property1, other?.Property1) && Equals(Property2, other?.Property2);

        public override int GetHashCode()
            => Property1.GetHashCode() ^ Property2.GetHashCode();
    }
}