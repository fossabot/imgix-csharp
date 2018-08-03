﻿using System;
using System.Linq;
using Imgix;
using NUnit.Framework;

namespace Imgix_Tests
{
    [TestFixture]
    public class UrlBuilderTest
    {
        private String SignKey = "aaAAbbBB11223344";

        [Test]
        public void UrlBuilderBuildsBasicUrlHttp()
        {
            var test = new UrlBuilder("domain.imgix.net");

            Assert.AreEqual(test.BuildUrl("gaiman.jpg"), "http://domain.imgix.net/gaiman.jpg");
        }

        [Test]
        public void UrlBuilderBuildsBasicUrlHttps()
        {
            var test = new UrlBuilder("domain.imgix.net", useHttps: true);

            Assert.AreEqual(test.BuildUrl("gaiman.jpg"), "https://domain.imgix.net/gaiman.jpg");
        }

        [Test]
        public void UrlBuilderBuildsQueryStringUrlHttp()
        {
            var test = new UrlBuilder("domain.imgix.net");

            test.Parameters["w"] = "500";
            test.Parameters["blur"] = "100";

            Assert.AreEqual(test.BuildUrl("gaiman.jpg"), "http://domain.imgix.net/gaiman.jpg?w=500&blur=100");
        }

        [Test]
        public void UrlBuilderBuildsQueryStringUrlHttps()
        {
            var test = new UrlBuilder("domain.imgix.net", useHttps: true);

            test.Parameters["w"] = "500";
            test.Parameters["blur"] = "100";

            Assert.AreEqual(test.BuildUrl("gaiman.jpg"), "https://domain.imgix.net/gaiman.jpg?w=500&blur=100");
        }

        [Test]
        public void UrlBuilderUsesCRCShardingBydefault()
        {
            var test = new UrlBuilder("domain.imgix.net", useHttps: true);

            Assert.AreEqual(test.ShardStrategy, UrlBuilder.ShardStrategyType.CRC);
        }

        [Test]
        public void UrlBuilderSignsParameterlessRequests()
        {
            var test = new UrlBuilder("domain.imgix.net")
            {
                SignKey = SignKey
            };

            Assert.AreEqual(test.BuildUrl("gaiman.jpg"), "http://domain.imgix.net/gaiman.jpg?s=db6110637ad768e4b1d503cb96e6439a");
        }

        [Test]
        public void UrlBuilderSignsParameteredRequests()
        {
            var test = new UrlBuilder("domain.imgix.net")
            {
                SignKey = SignKey
            };

            test.Parameters.Add("w", "500");
            test.Parameters.Add("h", "1000");

            Assert.AreEqual(test.BuildUrl("gaiman.jpg"), "http://domain.imgix.net/gaiman.jpg?w=500&h=1000&s=fc4afbc39b6741560717142aeada876c");
        }

        [Test]
        public void UrlBuilderSignsNestedPaths()
        {
            var test = new UrlBuilder("domain.imgix.net")
            {
                SignKey = SignKey
            };

            Assert.AreEqual(test.BuildUrl("test/gaiman.jpg"), "http://domain.imgix.net/test/gaiman.jpg?s=51033c27726f19c0f8229a1ed2dc8523");
        }

        [Test]
        public void UrlBuilderWithMultipleDomainCyclesThroughDomains()
        {
            var domains = new[] {"domain.imgix.net", "domain2.imgix.net", "domain3.imgix.net"};

            var test = new UrlBuilder(domains)
            {
                ShardStrategy = UrlBuilder.ShardStrategyType.CYCLE
            };

            Assert.AreEqual(test.BuildUrl("gaiman.jpg"), "http://domain.imgix.net/gaiman.jpg");
            Assert.AreEqual(test.BuildUrl("gaiman.jpg"), "http://domain2.imgix.net/gaiman.jpg");
            Assert.AreEqual(test.BuildUrl("gaiman.jpg"), "http://domain3.imgix.net/gaiman.jpg");
            Assert.AreEqual(test.BuildUrl("gaiman.jpg"), "http://domain.imgix.net/gaiman.jpg");
        }

        [Test]
        public void UrlBuilderWithMultipleDomainsPicksTheFirstWhenNoShardTypeSelected()
        {
            var domains = new[] { "domain.imgix.net", "domain2.imgix.net", "domain3.imgix.net" };

            var test = new UrlBuilder(domains)
            {
                ShardStrategy = null
            };

            Assert.AreEqual(test.BuildUrl("gaiman.jpg"), "http://domain.imgix.net/gaiman.jpg");
        }

        [Test]
        public void UrlBuilderWithMultipleDomainsSelectsServerByCRC()
        {
            var domains = new[] { "domain.imgix.net", "domain2.imgix.net", "domain3.imgix.net" };
            var crcs = new [] { "test1.png", "test2.png", "test3.png" }.Select(i => Convert.ToInt32(new Crc32().ComputeCrcHash(i) % domains.Length)).ToArray();

            var test = new UrlBuilder(domains)
            {
                ShardStrategy = UrlBuilder.ShardStrategyType.CRC
            };

            Assert.AreEqual(test.BuildUrl("test1.png"), String.Format("http://{0}/test1.png", domains[crcs[0]]));
            Assert.AreEqual(test.BuildUrl("test2.png"), String.Format("http://{0}/test2.png", domains[crcs[1]]));
            Assert.AreEqual(test.BuildUrl("test3.png"), String.Format("http://{0}/test3.png", domains[crcs[2]]));
        }

        [Test]
        public void UrlBuilderEscapesParamKeys()
        {
            var test = new UrlBuilder("demo.imgix.net");

            test.Parameters["hello world"] = "interesting";

            Assert.AreEqual("http://demo.imgix.net/demo.png?hello%20world=interesting", test.BuildUrl("demo.png"));
        }

        [Test]
        public void UrlBuilderEscapesParamValues()
        {
            var test = new UrlBuilder("demo.imgix.net");

            test.Parameters["hello_world"] = "/foo\"> <script>alert(\"hacked\")</script><";

            Assert.AreEqual("http://demo.imgix.net/demo.png?hello_world=%2Ffoo%22%3E%20%3Cscript%3Ealert(%22hacked%22)%3C%2Fscript%3E%3C", test.BuildUrl("demo.png"));
        }

        [Test]
        public void UrlBuilderBase64EncodesBase64ParamVariants()
        {
            var test = new UrlBuilder("demo.imgix.net");

            test.Parameters["txt64"] = "I cannøt belîév∑ it wors! \ud83d\ude31";

            Assert.AreEqual("http://demo.imgix.net/~text?txt64=SSBjYW5uw7h0IGJlbMOuw6l24oiRIGl0IHdvcu-jv3MhIPCfmLE", test.BuildUrl("~text"));
        }
    }
}
