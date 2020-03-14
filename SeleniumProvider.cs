using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Camellia_Management_System
{
    /// @author Yevgeniy Cherdantsev
    /// @date 14.03.2020 11:42:17
    /// @version 1.0
    /// <summary>
    /// INPUT
    /// </summary>
    public class SeleniumProvider
    {
        private Dictionary<IWebDriver, Boolean> _webDrivers = new Dictionary<IWebDriver, bool>();
        private object _lock = new object();

        public SeleniumProvider(int numberOfBrowsers, string pathToWebDriver = @".\")
        {
            for (var i = 0; i < numberOfBrowsers; i++)
            {
                var webDriver = new ChromeDriver(pathToWebDriver);
                webDriver.Url = "https://egov.kz/";
                _webDrivers.Add(webDriver, false);
            }
        }

        public IWebDriver GetDriver()
        {
            lock (_lock)
            {
                while (_webDrivers.ContainsValue(false))
                {
                }

                var webDriver = _webDrivers.FirstOrDefault(x => x.Value == false).Key;
                _webDrivers[webDriver] = false;
                return webDriver;
            }
        }


        public void ReleaseDriver(IWebDriver webDriver)
        {
            webDriver.Url = "https://egov.kz/";
            _webDrivers[webDriver] = false;
        }
    }
}