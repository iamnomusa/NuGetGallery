﻿using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;

namespace NuGetGallery
{
    public static class TestUtility
    {
        // We only need this method because testing URL generation is a pain.
        // Alternatively, we could write our own service for generating URLs.
        public static Mock<HttpContextBase> SetupHttpContextMockForUrlGeneration(Mock<HttpContextBase> httpContext, Controller controller)
        {
            httpContext.Setup(c => c.Request.Url).Returns(new Uri("https://example.org/"));
            httpContext.Setup(c => c.Request.ApplicationPath).Returns("/");
            httpContext.Setup(c => c.Response.ApplyAppPathModifier(It.IsAny<string>())).Returns<string>(s => s);
            var requestContext = new RequestContext(httpContext.Object, new RouteData());
            var controllerContext = new ControllerContext(requestContext, controller);
            controller.ControllerContext = controllerContext;
            var routeCollection = new RouteCollection();
            routeCollection.MapRoute("catch-all", "{*catchall}");
            controller.Url = new UrlHelper(requestContext, routeCollection);
            return httpContext;
        }

        public static void SetupUrlHelper(Controller controller, Mock<HttpContextBase> mockHttpContext)
        {
            var routes = new RouteCollection();
            Routes.RegisterRoutes(routes);
            controller.Url = new UrlHelper(new RequestContext(mockHttpContext.Object, new RouteData()), routes);
        }

        public static UrlHelper MockUrlHelper()
        {
            var mockHttpContext = new Mock<HttpContextBase>(MockBehavior.Strict);
            var mockHttpRequest = new Mock<HttpRequestBase>(MockBehavior.Strict);
            var mockHttpResponse = new Mock<HttpResponseBase>(MockBehavior.Strict);
            mockHttpContext.Setup(httpContext => httpContext.Request).Returns(mockHttpRequest.Object);
            mockHttpContext.Setup(httpContext => httpContext.Response).Returns(mockHttpResponse.Object);
            mockHttpRequest.Setup(httpRequest => httpRequest.Url).Returns(new Uri("http://unittest.nuget.org/"));
            mockHttpRequest.Setup(httpRequest => httpRequest.ApplicationPath).Returns("http://unittest.nuget.org/");
            mockHttpRequest.Setup(httpRequest => httpRequest.ServerVariables).Returns(new NameValueCollection());

            string value = null;
            Action<string> saveValue = x =>
            {
                value = x;
            };
            Func<String> restoreValue = () => value;
            mockHttpResponse.Setup(httpResponse => httpResponse.ApplyAppPathModifier(It.IsAny<string>()))
                            .Callback(saveValue).Returns(restoreValue);
            var requestContext = new RequestContext(mockHttpContext.Object, new RouteData());
            var routes = new RouteCollection();
            Routes.RegisterRoutes(routes);
            return new UrlHelper(requestContext, routes);
        }

        public static T GetAnonymousPropertyValue<T>(Object source, string propertyName)
        {
            var property = source.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public);
            if (property == null)
            {
                return default(T);
            }
            return (T)property.GetValue(source, null);
        }
    }
}