using Grand.Domain.Cms;
using Grand.Core.Plugins;
using Grand.Services.Cms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grand.Services.Tests.Cms
{
    [TestClass()]
    public class WidgetExtensionsTests
    {
        private Mock<IWidgetPlugin> _widgetPlugin;
        private WidgetSettings _settings;

        [TestInitialize()]
        public void Init()
        {
            _widgetPlugin = new Mock<IWidgetPlugin>();
            _settings = new WidgetSettings();
        }

        [TestMethod()]
        public void IsWidgetActive_NullSettings_ThrowException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => _widgetPlugin.Object.IsWidgetActive(null), "widgetSettings");
        }

        [TestMethod()]
        public void IsWidgetActive_NullActiveSystemNamesSettings_ReturnFalse()
        {
            _settings.ActiveWidgetSystemNames = null;
            Assert.IsFalse(_widgetPlugin.Object.IsWidgetActive(_settings));
        }

        [TestMethod()]
        public void IsWidgetActive_SettingsContainWidgetSystemName_ReturnTrue()
        {
            var descriptor = new PluginDescriptor();
            descriptor.SystemName = "Sample system name";
            _widgetPlugin.Setup(c => c.PluginDescriptor).Returns(descriptor);
            _settings.ActiveWidgetSystemNames = new List<string>() { "system name", "Sample system name" };
            Assert.IsTrue(_widgetPlugin.Object.IsWidgetActive(_settings));
        }

        [TestMethod()]
        public void IsWidgetActive_SettingsNotContainWidgetSystemName_ReturnFalse()
        {
            var descriptor = new PluginDescriptor();
            descriptor.SystemName = "Sample system name";
            _widgetPlugin.Setup(c => c.PluginDescriptor).Returns(descriptor);
            _settings.ActiveWidgetSystemNames = new List<string>() { "system name", "..." };
            Assert.IsFalse(_widgetPlugin.Object.IsWidgetActive(_settings));
        }
    }
}
