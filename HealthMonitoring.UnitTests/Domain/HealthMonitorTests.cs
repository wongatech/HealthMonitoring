using System;
using System.Linq;
using System.Threading;
using HealthMonitoring.Configuration;
using HealthMonitoring.UnitTests.Helpers;
using Moq;
using Xunit;

namespace HealthMonitoring.UnitTests.Domain
{
    public class HealthMonitorTests
    {
        private readonly TestableHealthMonitor _testableHealthMonitor;
        private readonly EndpointRegistry _endpointRegistry;

        public HealthMonitorTests()
        {
            _testableHealthMonitor = new TestableHealthMonitor();
            _endpointRegistry = new EndpointRegistry(new HealthMonitorRegistry(new[] { _testableHealthMonitor }), new Mock<IConfigurationStore>().Object);
        }

        [Fact]
        public void Monitor_should_start_checking_the_health_of_endpoints_until_disposed()
        {
            var endpoint1 = _endpointRegistry.RegisterOrUpdate(_testableHealthMonitor.Name, "address1", "group", "name");
            _endpointRegistry.RegisterOrUpdate(_testableHealthMonitor.Name, "address2", "group", "name");
            _testableHealthMonitor.StartWatch();

            using (new HealthMonitor(_endpointRegistry, TimeSpan.FromMilliseconds(250)))
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(350));
                _endpointRegistry.RegisterOrUpdate(_testableHealthMonitor.Name, "address3", "group", "name");
                Thread.Sleep(TimeSpan.FromMilliseconds(250));
                _endpointRegistry.TryUnregisterById(endpoint1);
                Thread.Sleep(TimeSpan.FromMilliseconds(200));
            }
            Thread.Sleep(TimeSpan.FromMilliseconds(200));
            var a1 = _testableHealthMonitor.Calls.Where(c => c.Item1 == "address1").ToArray();
            var a2 = _testableHealthMonitor.Calls.Where(c => c.Item1 == "address2").ToArray();
            var a3 = _testableHealthMonitor.Calls.Where(c => c.Item1 == "address3").ToArray();

            Assert.Equal(3, a1.Length);
            Assert.Equal(4, a2.Length);
            Assert.Equal(2, a3.Length);
        }
    }
}