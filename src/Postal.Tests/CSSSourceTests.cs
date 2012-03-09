using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BoneSoft.CSS;
using Xunit;

namespace Postal
{
// ReSharper disable InconsistentNaming
    public class CSSSourceTests
// ReSharper restore InconsistentNaming
    {
        [Fact]
        public void GivenACSSDocument_WhenCreatingAnInstanceOfCSSSource_RulesetsShouldBePopulatedFromTheDocument()
        {
            const string css = @"
                #myDiv { font-family: Arial, Helvetica, sans-serif; }
                div { font-family: Times New Roman, sans-serif; }";
            var cssDocument = new CSSParser().ParseText(css);

            var cssSource = new CSSSource(cssDocument);

            Assert.Equal(cssDocument.RuleSets, cssSource.Rulesets);
        }

        [Fact]
        public void GivenAFilePath_WhenCreatingAnInstanceOfCSSSource_RuleSetsShouldBeCreatedFromTheFileContents()
        {
            var filename = Path.Combine(Path.GetTempPath(), "test.css");
            const string css = @"
                #myDiv { font-family: Arial, Helvetica, sans-serif; }
                div { font-family: Times New Roman, sans-serif; }";
            File.WriteAllText(filename, css);
            try
            {
                var cssSource = new CSSSource(filename);

                Assert.Equal(cssSource.Rulesets.Count(), 2);
                Assert.Equal(cssSource.Rulesets.First().Selectors[0].ToString(), "#myDiv");
                Assert.Equal(cssSource.Rulesets.First().Declarations[0].ToString(), "font-family: Arial, Helvetica, sans-serif");
                Assert.Equal(cssSource.Rulesets.Last().Selectors[0].ToString(), "div");
                Assert.Equal(cssSource.Rulesets.Last().Declarations[0].ToString(), "font-family: Times New Roman, sans-serif");
            }
            finally
            {
                File.Delete(filename);
            }
        }

        [Fact]
        public void GivenAnAssemblyAndResourceName_WhenCreatingAnInstanceOfCSSSource_RulesetsShouldBeCreatedFromTheResourceContents()
        {
            var cssSource = new CSSSource(Assembly.GetExecutingAssembly(), "Postal.Resources.Test.css");

            Assert.Equal(cssSource.Rulesets.Count(), 2);
            Assert.Equal(cssSource.Rulesets.First().Selectors[0].ToString(), "#myDiv");
            Assert.Equal(cssSource.Rulesets.First().Declarations[0].ToString(), "font-family: Arial, Helvetica, sans-serif");
            Assert.Equal(cssSource.Rulesets.Last().Selectors[0].ToString(), "div");
            Assert.Equal(cssSource.Rulesets.Last().Declarations[0].ToString(), "font-family: Times New Roman, sans-serif");
        }

        [Fact]
        public void GivenACSSSource_WhenMergingAnotherCSSSource_AUnionOfRulesInBothSourcesShouldBeReturnedWhenRulesDoNotConflict()
        {
            const string css1 = @"#myDiv { font-family: Arial, Helvetica, sans-serif; }";
            var cssDocument1 = new CSSParser().ParseText(css1);
            var cssSource1 = new CSSSource(cssDocument1);

            const string css2 = @"div { font-family: Arial, Helvetica, sans-serif; }";
            var cssDocument2 = new CSSParser().ParseText(css2);
            var cssSource2 = new CSSSource(cssDocument2);

            var cssSource3 = cssSource1.Merge(cssSource2);

            Assert.Equal(cssSource3.Rulesets.Count(), 2);
            Assert.Equal(cssSource3.Rulesets.First().Selectors[0].ToString(), "#myDiv");
            Assert.Equal(cssSource3.Rulesets.First().Declarations[0].ToString(), "font-family: Arial, Helvetica, sans-serif");
            Assert.Equal(cssSource3.Rulesets.Last().Selectors[0].ToString(), "div");
            Assert.Equal(cssSource3.Rulesets.Last().Declarations[0].ToString(), "font-family: Arial, Helvetica, sans-serif");
        }

        [Fact]
        public void GivenACSSSource_WhenMergingAnotherCssSource_RulesInTheOriginalCSSSourceShouldHavePrecedence()
        {
            const string css1 = @"
                #myDiv { font-family: Arial, Helvetica, sans-serif; }
                div { font-family: Times New Roman, sans-serif; }";
            var cssDocument1 = new CSSParser().ParseText(css1);
            var cssSource1 = new CSSSource(cssDocument1);

            const string css2 = @"
                #myDiv { font-family: Times New Roman, sans-serif; }
                div { font-family: Arial, Helvetica, sans-serif; }";
            var cssDocument2 = new CSSParser().ParseText(css2);
            var cssSource2 = new CSSSource(cssDocument2);

            var cssSource3 = cssSource1.Merge(cssSource2);

            Assert.Equal(cssSource3.Rulesets.Count(), 2);
            Assert.Equal(cssSource3.Rulesets.First().Selectors[0].ToString(), "#myDiv");
            Assert.Equal(cssSource3.Rulesets.First().Declarations[0].ToString(), "font-family: Arial, Helvetica, sans-serif");
            Assert.Equal(cssSource3.Rulesets.Last().Selectors[0].ToString(), "div");
            Assert.Equal(cssSource3.Rulesets.Last().Declarations[0].ToString(), "font-family: Times New Roman, sans-serif");
        }

        [Fact]
        public void GivenACSSSource_WhenMergingAnotherCssSource_ShouldThrowAnArgumentNullExceptionIfTheOtherCSSSourceIsNull()
        {
            var cssSource = new CSSSource(new CSSParser().ParseText("{}"));

            Assert.Throws(typeof (ArgumentNullException), () => cssSource.Merge(null));
        }
    }
}
