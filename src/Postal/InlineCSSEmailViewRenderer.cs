using System;
using System.Web.Mvc;

namespace Postal
{
    /// <summary>
    /// Renders <see cref="Email"/> view's into raw strings using the MVC ViewEngine infrastructure.
    /// Updates the view content with inline styles from the CSS Source provided
    /// </summary>
    public class InlineCSSEmailViewRenderer : EmailViewRenderer
    {
        private readonly CSSSource _cssSource;

        public InlineCSSEmailViewRenderer(ViewEngineCollection viewEngines, Uri url, CSSSource cssSource) : base(viewEngines, url)
        {
            if (cssSource == null) throw new ArgumentNullException("cssSource");
            if (cssSource.Rulesets == null) throw new ArgumentNullException("cssSource", "Rulesets cannot be null");

            _cssSource = cssSource;
        }

        public override string Render(Email email, string viewName = null)
        {
            var viewContent = base.Render(email, viewName);

            return _cssSource.InlineCss(viewContent);
        }
    }
}
