namespace Fanex.Caching.Tests
{
    using System;
    using Ploeh.AutoFixture;
    using Ploeh.AutoFixture.AutoNSubstitute;
    using Ploeh.AutoFixture.Xunit2;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class AutoNSubstituteDataAttribute : AutoDataAttribute
    {
        public AutoNSubstituteDataAttribute()
            : base(new Fixture().Customize(new AutoNSubstituteCustomization()))
        {
        }
    }
}