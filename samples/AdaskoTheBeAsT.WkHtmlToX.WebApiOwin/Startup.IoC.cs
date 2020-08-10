using System.Web.Http;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.BusinessLogic;
using Owin;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using SimpleInjector.Lifestyles;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiOwin
{
    public partial class Startup
    {
        private Container ConfigureIoC(IAppBuilder app, HttpConfiguration httpConfiguration)
        {
            var container = new Container();

            app.Use(async (_, next) =>
            {
                await using (AsyncScopedLifestyle.BeginScope(container))
                {
#pragma warning disable CC0031 // Check for null before calling a delegate
#pragma warning disable MA0004 // Use .ConfigureAwait(false)
                    await next();
#pragma warning restore MA0004 // Use .ConfigureAwait(false)
#pragma warning restore CC0031 // Check for null before calling a delegate
                }
            });

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            container.RegisterSingleton<IHtmlGenerator, SmallHtmlGenerator>();
            container.RegisterSingleton<IHtmlToPdfDocumentGenerator, HtmlToPdfDocumentGenerator>();
            container.RegisterSingleton<IHtmlToPdfAsyncConverter, SynchronizedPdfConverter>();
            container.RegisterWebApiControllers(httpConfiguration);

            httpConfiguration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
            return container;
        }
    }
}
