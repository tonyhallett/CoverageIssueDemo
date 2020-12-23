using CoverageIssueDemo;
using System;
using Xunit;

namespace Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var coverMe = new CoverMe();
#if NET472
            coverMe.Method1();
#else
            coverMe.Method2();
#endif

        }
    }


}
