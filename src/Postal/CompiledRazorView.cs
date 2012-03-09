using System;
using System.IO;
using System.Web.Mvc;
using RazorEngine;
using RazorEngine.Templating;

namespace Postal
{
    public class CompiledRazorView : IView
    {
        private readonly Func<object, ITemplate> _templateResolver;

        public CompiledRazorView(string templateName)
        {
            _templateResolver = model => Razor.Resolve(templateName, model);
        }

        public CompiledRazorView(string templateName, ITemplateService templateService)
        {
            _templateResolver = model => templateService.Resolve(templateName, model);
        }

        public void Render(ViewContext viewContext, TextWriter writer)
        {
            var template = _templateResolver(viewContext.ViewData.Model);

            var content = template.Run(new ExecuteContext());

            writer.Write(content);
            writer.Flush();
        }
    }
}
