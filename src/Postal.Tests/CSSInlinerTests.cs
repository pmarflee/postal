using System.Linq;
using BoneSoft.CSS;
using HtmlAgilityPack;
using Xunit;

namespace Postal
{
// ReSharper disable InconsistentNaming
   public class CSSInlinerTests
// ReSharper restore InconsistentNaming
   {
        [Fact]
        public void GetNodesAndDeclarations_Should_Pick_Id_Rule_Over_Element_Rule()
        {
            var html = @"
                <html>
                    <head>
                        <title>test</title>
                    </head>
                    <body>
                        <div id=""myDiv"">
                        </div>
                    </body>
                </html>";
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var css = @"
                #myDiv { font-family: Arial, Helvetica, sans-serif; }
                div { font-family: Times New Roman, sans-serif; }";
            var ruleSets = new CSSParser().ParseText(css).RuleSets;

            var cssRewriter = new CSSInliner();
            var nodesAndDeclarations = cssRewriter.GetMatchedNodes(document, ruleSets).ToList();

            Assert.Equal(nodesAndDeclarations.Count(), 1);
            Assert.Equal(nodesAndDeclarations.First().Item1.Id, "myDiv");
            Assert.Equal(nodesAndDeclarations.First().Item2.Expression.ToString(), "Arial, Helvetica, sans-serif");
        }

        [Fact]
        public void GetNodesAndDeclarations_Should_Pick_Class_Rule_Over_Element_Rule()
        {
            var html = @"
                <html>
                    <head>
                        <title>test</title>
                    </head>
                    <body>
                        <div class=""myDiv"">
                        </div>
                    </body>
                </html>";
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var css = @"
                .myDiv { font-family: Arial, Helvetica, sans-serif; }
                div { font-family: Times New Roman, sans-serif; }";
            var ruleSets = new CSSParser().ParseText(css).RuleSets;

            var cssRewriter = new CSSInliner();
            var nodesAndDeclarations = cssRewriter.GetMatchedNodes(document, ruleSets).ToList();

            Assert.Equal(nodesAndDeclarations.Count(), 1);
            Assert.Equal(nodesAndDeclarations.First().Item1.Attributes["class"].Value, "myDiv");
            Assert.Equal(nodesAndDeclarations.First().Item2.Expression.ToString(), "Arial, Helvetica, sans-serif");
        }

        [Fact]
        public void GetNodesAndDeclarations_Important_Declaration_Should_Override_Specificity()
        {
            var html = @"
                <html>
                    <head>
                        <title>test</title>
                    </head>
                    <body>
                        <div id=""myDiv"">
                        </div>
                    </body>
                </html>";
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var css = @"
                #myDiv { font-family: Arial, Helvetica, sans-serif; }
                div { font-family: Times New Roman !important; }";
            var ruleSets = new CSSParser().ParseText(css).RuleSets;

            var cssRewriter = new CSSInliner();
            var nodesAndDeclarations = cssRewriter.GetMatchedNodes(document, ruleSets).ToList();

            Assert.Equal(nodesAndDeclarations.Count(), 1);
            Assert.Equal(nodesAndDeclarations.First().Item1.Id, "myDiv");
            Assert.Equal(nodesAndDeclarations.First().Item2.Expression.ToString(), "Times New Roman");
        }

        [Fact]
        public void GetNodesAndDeclarations_NodeAndDeclaration_Should_Not_Be_Returned_When_Node_Has_Declaration_Defined_Inline()
        {
            var html = @"
                <html>
                    <head>
                        <title>test</title>
                    </head>
                    <body>
                        <div id=""myDiv"" style=""font-family: Comic Sans MS;"">
                        </div>
                    </body>
                </html>";
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var css = @"
                #myDiv { font-family: Arial, Helvetica, sans-serif; }
                div { font-family: Times New Roman; }";
            var ruleSets = new CSSParser().ParseText(css).RuleSets;

            var cssRewriter = new CSSInliner();
            var nodesAndDeclarations = cssRewriter.GetMatchedNodes(document, ruleSets).ToList();

            Assert.Empty(nodesAndDeclarations);
        }

        [Fact]
        public void GetNodesAndDeclarations_Last_RuleSet_Has_Priority_When_Specificity_Is_Equal()
        {
            var html = @"
                <html>
                    <head>
                        <title>test</title>
                    </head>
                    <body>
                        <div id=""myDiv"">
                        </div>
                    </body>
                </html>";
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var css = @"
                #myDiv { font-family: Arial, Helvetica, sans-serif; }
                #myDiv { font-family: Times New Roman; }";
            var ruleSets = new CSSParser().ParseText(css).RuleSets;

            var cssRewriter = new CSSInliner();
            var nodesAndDeclarations = cssRewriter.GetMatchedNodes(document, ruleSets).ToList();

            Assert.Equal(nodesAndDeclarations.Count(), 1);
            Assert.Equal(nodesAndDeclarations.First().Item2.Expression.ToString(), "Times New Roman");
        }

        [Fact]
        public void GetNodesAndDeclarations_Important_Declarations_Have_Priority_Over_Declarations_Defined_Inline()
        {
            var html = @"
                <html>
                    <head>
                        <title>test</title>
                    </head>
                    <body>
                        <div id=""myDiv"" style=""font-family: Comic Sans MS;"">
                        </div>
                    </body>
                </html>";
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var css = @"#myDiv { font-family: Arial, Helvetica, sans-serif !important; }";
            var ruleSets = new CSSParser().ParseText(css).RuleSets;

            var cssRewriter = new CSSInliner();
            var nodesAndDeclarations = cssRewriter.GetMatchedNodes(document, ruleSets).ToList();

            Assert.Equal(nodesAndDeclarations.Count(), 1);
            Assert.Equal(nodesAndDeclarations.First().Item2.Expression.ToString(), "Arial, Helvetica, sans-serif");
        }

        [Fact]
        public void UpdateInlineStyleForElement_Should_Append_New_Declaration()
        {
            var css = @"#myDiv { font-family: Arial, Helvetica, sans-serif; }";
            var declaration = new CSSParser().ParseText(css).RuleSets.First().Declarations.First();
            var node = HtmlNode.CreateNode(@"<div style=""font-weight: bold; font-height: 14px;""/>");

            CSSInliner.UpdateInlineStyleForElement(node, declaration);

            Assert.Equal(node.Attributes["style"].Value, "font-weight: bold; font-height: 14px; font-family: Arial, Helvetica, sans-serif;");
        }

        [Fact]
        public void UpdateInlineStyleForElement_Should_Update_Existing_Declaration()
        {
            var css = @"#myDiv { font-family: Arial, Helvetica, sans-serif; }";
            var declaration = new CSSParser().ParseText(css).RuleSets.First().Declarations.First();
            var node = HtmlNode.CreateNode(@"<div style=""font-weight: bold; font-family: Comic Sans MS; font-height: 14px;""/>");

            CSSInliner.UpdateInlineStyleForElement(node, declaration);

            Assert.Equal(node.Attributes["style"].Value, "font-weight: bold; font-family: Arial, Helvetica, sans-serif; font-height: 14px;");
        }

        [Fact]
        public void UpdateInlineStyleForElement_Should_Update_Color_And_BackgroundColor()
        {
            var css = @"#myDiv { background-color: white; color: black; }";
            var declarations = new CSSParser().ParseText(css).RuleSets.SelectMany(x => x.Declarations);
            var node = HtmlNode.CreateNode(@"<div />");

            foreach (var declaration in declarations)
            {
                CSSInliner.UpdateInlineStyleForElement(node, declaration);
            }

            Assert.Equal(node.Attributes["style"].Value, "background-color: white; color: black;");
        }
    }
}
