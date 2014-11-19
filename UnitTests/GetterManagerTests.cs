using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    using SunokoLibrary.Application;
    using SunokoLibrary.Application.Browsers;

    [TestClass]
    public class GetterManagerTests
    {
        [TestMethod]
        public async Task GoogleChromeTest()
        {
            var manager = new GoogleChromeBrowserManager();
            var getters = manager.GetCookieImporters();
            await CheckGetters(getters, true);
        }
        [TestMethod]
        public async Task InternetExplorerTest()
        {
            var manager = new IEBrowserManager();
            var getters = manager.GetCookieImporters();
            await CheckGetters(getters.OfType<IECookieGetter>(), true);
        }
        [TestMethod]
        public async Task InternetExplorerTest_ProtectedMode()
        {
            var manager = new IEBrowserManager();
            var getters = manager.GetCookieImporters();
            await CheckGetters(getters.OfType<IEPMCookieGetter>(), true);
        }
        [TestMethod]
        public async Task FirefoxTest()
        {
            var manager = new FirefoxBrowserManager();
            var getters = manager.GetCookieImporters();
            await CheckGetters(getters, true);
        }
        [TestMethod]
        public async Task SleipnirTest()
        {
            var manager = new SleipnirBrowserManager();
            var getters = manager.GetCookieImporters();
            await CheckGetters(getters, true);
        }
        [TestMethod]
        public async Task LunascapeTest()
        {
            var manager = new LunascapeBrowserManager();
            var getters = manager.GetCookieImporters();
            await CheckGetters(getters, true);
        }
        [TestMethod]
        public async Task OperaWebkitBlinkTest()
        {
            var manager = new OperaWebkitBrowserManager();
            var getters = manager.GetCookieImporters();
            await CheckGetters(getters, true);
        }
        [TestMethod]
        public async Task AvailableAllBrowserTest()
        {
            var getters = (await CookieGetters.GetInstancesAsync(true))
                .Where(getter => getter is IECookieGetter == false).ToArray();
            await CheckGetters(getters, true);
        }
        [TestMethod]
        public async Task NotAvailableAllBrowserTest()
        {
            var getters = (await CookieGetters.GetInstancesAsync(false))
                .Where(getter => getter.IsAvailable == false).ToArray();
            await CheckGetters(getters, false);
        }

        async Task CheckGetters(IEnumerable<ICookieImporter> getters, bool expectedIsAvailable)
        {
            foreach (var item in getters)
            {
                var cookies = new CookieContainer();
                var url = new Uri("http://nicovideo.jp/");
                await item.GetCookiesAsync(url, cookies);
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
