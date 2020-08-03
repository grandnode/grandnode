using Grand.Domain.Cms;
using Grand.Core.Plugins;
using Grand.Services.Cms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grand.Services.Tests.Cms
{
    [TestClass()]
    public class WidgetServiceTests
    {
        private Mock<IPluginFinder> _finder;
        private WidgetSettings _settings;
        private IWidgetService _widgedService;

        [TestInitialize()]
        public void Init()
        {
            _finder = new Mock<IPluginFinder>();
            _settings = new WidgetSettings();
            _finder.Setup(f => f.GetPlugins<IWidgetPlugin>(It.IsAny<LoadPluginsMode>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(GetFakeListOfPlugins());
            _widgedService = new WidgetService(_finder.Object,_settings);
        }

        [TestMethod()]
        public void LoadActiveWidgets_SettingsNotContainsSystemName_ReturnEmptyList()
        {
            _settings.ActiveWidgetSystemNames = new List<string>();
            var result = _widgedService.LoadActiveWidgets();
            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod()]
        public void LoadActiveWidgets_SettingsContainsSystemName_ReturnList()
        {
            _settings.ActiveWidgetSystemNames = new List<string>() { "name0", "name1" };
            var result = _widgedService.LoadActiveWidgets();
            Assert.IsTrue(result.Count == _settings.ActiveWidgetSystemNames.Count);
        }

        [TestMethod()]
        public void LoadActiveWidgetsByWidgetZone_EmptyWidgetZone_ReturnEmptyList()
        {
            var result = _widgedService.LoadActiveWidgetsByWidgetZone("");
            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod()]
        public void LoadActiveWidgetsByWidgetZone()
        {
            _settings.ActiveWidgetSystemNames = new List<string>() { "name0", "name1" };
            var result = _widgedService.LoadActiveWidgetsByWidgetZone("widgetZone1");
            Assert.IsTrue(result.Count != 0);
        }

        [TestMethod()]
        public void LoadWidgetBySystemName_ReturnPlugin()
        {
            var descriptorMock = new Mock<PluginDescriptor>();
            descriptorMock.Setup(c => c.Instance<IWidgetPlugin>(It.IsAny<IServiceProvider>())).Returns(() => new Mock<IWidgetPlugin>().Object);
            _finder.Setup(f => f.GetPluginDescriptorBySystemName<IWidgetPlugin>(It.IsAny<string>(), It.IsAny<LoadPluginsMode>()))
                .Returns(() => descriptorMock.Object);
            var result = _widgedService.LoadWidgetBySystemName("name");
            Assert.IsNotNull(result);
        }

        private List<IWidgetPlugin> GetFakeListOfPlugins()
        {
            var result = new List<IWidgetPlugin>();
            foreach(var i in Enumerable.Range(0, 5))
            {
                var mock = new Mock<IWidgetPlugin>();
                mock.Setup(c => c.PluginDescriptor).Returns(new PluginDescriptor() { SystemName = $"name{i}" });
                mock.Setup(c => c.GetWidgetZones()).Returns(() => new List<string> { $"widgetZone{i}"});
                result.Add(mock.Object);
            }
            return result;
        }
    }
}
