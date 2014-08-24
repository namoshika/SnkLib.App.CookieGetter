using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    using SunokoLibrary.Application;
    using SunokoLibrary.Application.Browsers;

    [TestClass]
    public class GetterManagerTests
    {
        [TestMethod]
        public void GoogleChromeTest()
        {
            var manager = new GoogleChromeBrowserManager();
            var getters = manager.CreateCookieImporters();
            CheckGetters(getters, true);
        }
        [TestMethod]
        public void InternetExplorerTest()
        {
            var manager = new IEBrowserManager();
            var getters = manager.CreateCookieImporters();
            CheckGetters(getters.OfType<IECookieGetter>(), true);
        }
        [TestMethod]
        public void InternetExplorerTest_ProtectedMode()
        {
            var manager = new IEBrowserManager();
            var getters = manager.CreateCookieImporters();
            CheckGetters(getters.OfType<IEPMCookieGetter>(), true);
        }
        [TestMethod]
        public void FirefoxTest()
        {
            var manager = new FirefoxBrowserManager();
            var getters = manager.CreateCookieImporters();
            CheckGetters(getters, true);
        }
        [TestMethod]
        public void Sleipnir5BlinkTest()
        {
            var manager = new Sleipnir5BlinkBrowserManager();
            var getters = manager.CreateCookieImporters();
            CheckGetters(getters, true);
        }
        [TestMethod]
        public void Lunascape6GeckoTest()
        {
            var manager = new LunascapeGeckoBrowserManager();
            var getters = manager.CreateCookieImporters();
            CheckGetters(getters, true);
        }
        [TestMethod]
        public void Lunascape6WebkitTest()
        {
            var manager = new LunascapeWebkitBrowserManager();
            var getters = manager.CreateCookieImporters();
            CheckGetters(getters, true);
        }
        [TestMethod]
        public void OperaWebkitBlinkTest()
        {
            var manager = new OperaWebkitBrowserManager();
            var getters = manager.CreateCookieImporters();
            CheckGetters(getters, true);
        }
        [TestMethod]
        public void AvailableAllBrowserTest()
        {
            var getters = CookieGetters.CreateInstances(true)
                .Where(getter => getter is IECookieGetter == false).ToArray();
            CheckGetters(getters, true);
        }
        [TestMethod]
        public void NotAvailableAllBrowserTest()
        {
            var getters = CookieGetters.CreateInstances(false)
                .Where(getter => getter.IsAvailable == false).ToArray();
            CheckGetters(getters, false);
        }

        void CheckGetters(IEnumerable<ICookieImporter> getters, bool expectedIsAvailable)
        {
            foreach (var item in getters)
            {
                var cookies = new CookieContainer();
                var url = new Uri("http://nicovideo.jp/");
                item.GetCookies(url, cookies);
                if (expectedIsAvailable)
                {
                    Assert.IsTrue(item.IsAvailable);
                    Assert.IsNotNull(item.Config.BrowserName);
                    Assert.IsNotNull(item.Config.ProfileName);
                    Assert.IsNotNull(item.Config.CookiePath);
                    Assert.IsNotNull(cookies.GetCookies(url)["user_session"]);
                }
                else
                {
                    Assert.IsFalse(item.IsAvailable);
                    Assert.IsNotNull(item.Config.BrowserName);
                    Assert.IsNotNull(item.Config.ProfileName);
                    Assert.IsNull(cookies.GetCookies(url)["user_session"]);
                }
            }
        }
    }
}
