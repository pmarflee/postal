using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using BoneSoft.CSS;
using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;

namespace Postal
{
    /// <summary>
    /// Rewrites the CSS rules defined in a set of external rulesets to inline CSS in a given HTML document
    /// </summary>
    public class CSSInliner
    {          
        /// <summary>
        /// Rewrites the CSS rules defined in a set of external rulesets to inline CSS in HTML content
        /// </summary>
        /// <param name="content">The HTML content to inline CSS for</param>
        /// <param name="ruleSets">A sequence of rulesets that define the CSS rules to inline</param>
        /// <returns>A copy of the HTML content with all CSS inlined</returns>
        public string InlineCSS(string content, IEnumerable<RuleSet> ruleSets)
        {            
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(content);

            foreach (var info in GetMatchedNodes(htmlDocument, ruleSets)) 
                UpdateInlineStyleForElement(info.Item1, info.Item2);

            RemoveAttributesWithName(htmlDocument.DocumentNode, "class", "id");

            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
                htmlDocument.Save(sw);

            return sb.ToString();
        }

        /// <summary>
        /// Gets the nodes in the HTML document that most specifically match the CSS rulesets, using CSS specificity weightings.  
        /// </summary>
        /// <param name="document">The HTML document to search</param>
        /// <param name="ruleSets">A sequence of rulesets to search the document with</param>
        /// <returns></returns>
        public IEnumerable<Tuple<HtmlNode, Declaration>> GetMatchedNodes(HtmlDocument document, IEnumerable<RuleSet> ruleSets)
        {
            return from ruleSetSelector in ruleSets.Select((r, i) => new { ruleSet = r, index = i })
                   let specificity = ruleSetSelector.ruleSet.CalculateSpecificity()
                   from selector in ruleSetSelector.ruleSet.Selectors
                   from element in document.DocumentNode.QuerySelectorAll(selector.ToString())
                   from declaration in ruleSetSelector.ruleSet.Declarations
                   where declaration.Important || !HasDeclarationDefinedInline(declaration, element.GetAttributeValue("style", String.Empty))
                   select new { element, declaration, specificity, ruleSetSelector.index }
                       into matched
                       group matched by new { matched.element, declarationName = matched.declaration.Name }
                           into groupMatched
                           let mostSpecific = (
                                from gm in groupMatched 
                                orderby gm.declaration.Important descending, gm.specificity descending, gm.index descending 
                                select gm).First()
                           select Tuple.Create(groupMatched.Key.element, mostSpecific.declaration);
        }

        /// <summary>
        /// Updates the inline styles for a given HTML node
        /// </summary>
        /// <param name="element">The HTML node to update inline styles for</param>
        /// <param name="declaration">The CSS declaration containing the styles to update the HTML node with</param>
        public static void UpdateInlineStyleForElement(HtmlNode element, Declaration declaration)
        {
            var style = element.GetAttributeValue("style", String.Empty);

            if (!HasDeclarationDefinedInline(declaration, style))
            {
                style += String.Format("{0}{1};", style == String.Empty ? String.Empty : " ", declaration);
            }
            else
            {
                var regex = new Regex(string.Format(@"(?<={0}:\s)(.*?)(?=;)", declaration.Name));
                style = regex.Replace(style, declaration.Expression.ToString());
            }

            element.SetAttributeValue("style", style);
        }

        private static bool HasDeclarationDefinedInline(Declaration declaration, string style)
        {
            var styleMatcher = new Regex(String.Format(@"(?:^|;)\s*{0}", declaration.Name));
            return styleMatcher.IsMatch(style);
        }

        private static void RemoveAttributesWithName(HtmlNode document, params string[] attributes)
        {
            foreach (var attribute in attributes)
            {
                var elements = document.QuerySelectorAll(string.Format("[{0}]", attribute)).ToList();
                foreach (var element in elements)
                {
                    element.Attributes.Remove(attribute);
                }
            }
        }
    }
}
