using RazorEngine;
using RazorEngine.Templating;
using Xunit;
using System.IO;
using Moq;
using System.Web.Mvc;

namespace Postal
{
    public class CompiledRazorViewTests
    {
        [Fact]
        public void CompiledRazorView_Should_Render_Compiled_Template_When_TemplateService_Is_Provided()
        {
            var templateService = new TemplateService();
            templateService.Compile("@model object\r\nHello World!", "test");

            var view = new CompiledRazorView("test", templateService);
            var context = new Mock<ViewContext>();
            context.Setup(c => c.ViewData).Returns(new ViewDataDictionary(new object()));
            using (var writer = new StringWriter())
            {
                view.Render(context.Object, writer);
                var content = writer.GetStringBuilder().ToString();
                Assert.Equal("Hello World!", content);
            }
        }

        [Fact]
        public void CompiledRazorView_Should_Render_Compiled_Template_When_TemplateService_Is_Not_Provided()
        {
            Razor.Compile("@model object\r\nHello World!", "test");

            var view = new CompiledRazorView("test");
            var context = new Mock<ViewContext>();
            context.Setup(c => c.ViewData).Returns(new ViewDataDictionary(new object()));
            using (var writer = new StringWriter())
            {
                view.Render(context.Object, writer);
                var content = writer.GetStringBuilder().ToString();
                Assert.Equal("Hello World!", content);
            }
        }
    }
}
