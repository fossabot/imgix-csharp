﻿using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Imgix.Tests
{
    [TestFixture]
    public class UrlBuilderTest
    {
        private String SignKey = "aaAAbbBB11223344";

        [Test]
        public void UrlBuilderBuildsBasicUrlHttp()
        {
            var test = new UrlBuilder("domain.imgix.net", includeLibraryParam: false);

            Assert.AreEqual(test.BuildUrl("gaiman.jpg"), "https://domain.imgix.net/gaiman.jpg");
        }

        [Test]
        public void UrlBuilderBuildsBasicUrlHttps()
        {
            var test = new UrlBuilder("domain.imgix.net", useHttps: true, includeLibraryParam: false);

            Assert.AreEqual(test.BuildUrl("gaiman.jpg"), "https://domain.imgix.net/gaiman.jpg");
        }

        [Test]
        public void UrlBuilderBuildsQueryStringUrlHttp()
        {
            var test = new UrlBuilder("domain.imgix.net", includeLibraryParam: false);

            var parameters = new Dictionary<String, String>();
            parameters["w"] = "500";
            parameters["blur"] = "100";

            Assert.AreEqual(test.BuildUrl("gaiman.jpg", parameters), "https://domain.imgix.net/gaiman.jpg?w=500&blur=100");
        }

        [Test]
        public void UrlBuilderBuildsQueryStringUrlHttps()
        {
            var test = new UrlBuilder("domain.imgix.net", useHttps: true, includeLibraryParam: false);

            var parameters = new Dictionary<String, String>();
            parameters["w"] = "500";
            parameters["blur"] = "100";

            Assert.AreEqual(test.BuildUrl("gaiman.jpg", parameters), "https://domain.imgix.net/gaiman.jpg?w=500&blur=100");
        }

        [Test]
        public void UrlBuilderSignsParameterlessRequests()
        {
            var test = new UrlBuilder("domain.imgix.net", signKey: SignKey, includeLibraryParam: false);

            Assert.AreEqual(test.BuildUrl("gaiman.jpg"), "https://domain.imgix.net/gaiman.jpg?s=db6110637ad768e4b1d503cb96e6439a");
        }

        [Test]
        public void UrlBuilderSignsParameteredRequests()
        {
            var test = new UrlBuilder("domain.imgix.net", signKey: SignKey, includeLibraryParam: false);

            var parameters = new Dictionary<String, String>();
            parameters.Add("w", "500");
            parameters.Add("h", "1000");

            Assert.AreEqual(test.BuildUrl("gaiman.jpg", parameters), "https://domain.imgix.net/gaiman.jpg?w=500&h=1000&s=fc4afbc39b6741560717142aeada876c");
        }

        [Test]
        public void UrlBuilderSignsNestedPaths()
        {
            var test = new UrlBuilder("domain.imgix.net", signKey: SignKey, includeLibraryParam: false);

            Assert.AreEqual(test.BuildUrl("test/gaiman.jpg"), "https://domain.imgix.net/test/gaiman.jpg?s=51033c27726f19c0f8229a1ed2dc8523");
        }

        [Test]
        public void UrlBuilderEscapesParamKeys()
        {
            var test = new UrlBuilder("demo.imgix.net", includeLibraryParam: false);

            var parameters = new Dictionary<String, String>();
            parameters["hello world"] = "interesting";

            Assert.AreEqual("https://demo.imgix.net/demo.png?hello%20world=interesting", test.BuildUrl("demo.png", parameters));
        }

        [Test]
        public void UrlBuilderEscapesParamValues()
        {
            var test = new UrlBuilder("demo.imgix.net", includeLibraryParam: false);

            var parameters = new Dictionary<String, String>();
            parameters["hello_world"] = "/foo\"> <script>alert(\"hacked\")</script><";

            Assert.AreEqual("https://demo.imgix.net/demo.png?hello_world=%2Ffoo%22%3E%20%3Cscript%3Ealert(%22hacked%22)%3C%2Fscript%3E%3C", test.BuildUrl("demo.png", parameters));
        }

        [Test]
        public void UrlBuilderBase64EncodesBase64ParamVariants()
        {
            var test = new UrlBuilder("demo.imgix.net", includeLibraryParam: false);

            var parameters = new Dictionary<String, String>();
            parameters["txt64"] = "I cannøt belîév∑ it wors! \ud83d\ude31";

            Assert.AreEqual("https://demo.imgix.net/~text?txt64=SSBjYW5uw7h0IGJlbMOuw6l24oiRIGl0IHdvcu-jv3MhIPCfmLE", test.BuildUrl("~text", parameters));
        }

        [Test]
        public void UrlBuilderGeneratesIxLibParam()
        {
            var test = new UrlBuilder("demo.imgix.net", includeLibraryParam: true);
            Assert.AreEqual(String.Format("https://demo.imgix.net/demo.png?ixlib=csharp-{0}", typeof(UrlBuilder).Assembly.GetName().Version), test.BuildUrl("demo.png"));
        }
    }
}
