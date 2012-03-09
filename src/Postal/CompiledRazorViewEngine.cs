using System.Web.Mvc;
using RazorEngine.Templating;

namespace Postal
{
    /// <summary>
    /// A view engine that uses the Razor engine to render a template that has previously been compiled and stored in the RazorEngine cache
    /// </summary>
    public class CompiledRazorViewEngine : IViewEngine
    {
        private readonly ITemplateService _templateService;

        public CompiledRazorViewEngine(ITemplateService templateService) 
        {
            _templateService = templateService;
        }

        public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            return new ViewEngineResult(new CompiledRazorView(partialViewName, _templateService), this);
        }

        public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            return FindPartialView(controllerContext, viewName, useCache);
        }

        public void ReleaseView(ControllerContext controllerContext, IView view)
        {
            // Nothing to do here - ResourceRazorView does not need disposing.
        }
    }
}
