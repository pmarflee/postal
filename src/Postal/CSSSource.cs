using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BoneSoft.CSS;

namespace Postal
{
    /// <summary>
    /// The source for a CSS document
    /// </summary>
    public class CSSSource
    {
        private readonly IEnumerable<RuleSet>  _rulesets;
        public IEnumerable<RuleSet> Rulesets
        {
            get { return _rulesets; }
        }

        public CSSSource(CSSDocument cssDocument)
        {
            if (cssDocument == null) throw new ArgumentNullException("cssDocument");

            _rulesets = cssDocument.RuleSets;
        }

        public CSSSource(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");

            _rulesets = ParseCSS(parser => parser.ParseFile(fileName));
        }

        public CSSSource(Assembly assembly, string resourceName)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            if (string.IsNullOrEmpty(resourceName)) throw new ArgumentNullException("resourceName");

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                _rulesets = ParseCSS(parser => parser.ParseStream(stream));
            }
        }

        private CSSSource(IEnumerable<RuleSet> ruleSets)
        {
            if (ruleSets == null) throw new ArgumentNullException("ruleSets");

            _rulesets = ruleSets;
        }

        private static IEnumerable<RuleSet> ParseCSS(Func<CSSParser, CSSDocument> getDocument)
        {
            var parser = new CSSParser();
            var cssDocument = getDocument(parser);

            if (parser.Errors.Any())
                throw new Exception(
                    string.Format("CSS stylesheet has errors: {0}",
                                  parser.Errors.Aggregate((acc, error) => acc + error + " ")));

            return cssDocument.RuleSets;
        }

        /// <summary>
        /// Merges the rulesets of the current CSS source with those of another CSS source.
        /// Rules in the current CSS source take precedence over those in the other CSS source.
        /// </summary>
        /// <param name="other">The other CSS source to merge with the current one</param>
        /// <returns>A new CSS source containing the union of the rulsets in the current and other CSS sources</returns>
        public CSSSource Merge(CSSSource other)
        {
            if (other == null) throw new ArgumentNullException("other");
            if (other.Rulesets == null) throw new ArgumentNullException("other", "Rulesets cannot be null");

            return new CSSSource(_rulesets.Union(other._rulesets, new RuleSetEqualityComparer()));
        }

        /// <summary>
        /// Writes inline CSS styles to the provided view content using the current rulesets
        /// </summary>
        /// <param name="viewContent">The view content to write inline CSS to</param>
        /// <returns>The view content updated with inline CSS styles</returns>
        public string InlineCss(string viewContent)
        {
            if (viewContent == null) throw new ArgumentNullException("viewContent");

            return new CSSInliner().InlineCSS(viewContent, _rulesets);
        }

        private class RuleSetEqualityComparer : IEqualityComparer<RuleSet>
        {
            public bool Equals(RuleSet x, RuleSet y)
            {
                // 2 rulesets are considered equal if:
                // 1. Both ruleset references point to the same object OR
                // 2. Both rulesets contain the same declarations 

                if (ReferenceEquals(x, y)) return true;

                if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;

                if (x.Selectors.Count != y.Selectors.Count) return false;

                return x.Selectors
                    .Zip(y.Selectors, (x1, y1) => new { x = x1, y = y1 })
                    .All(pair => pair.x.ToString().Equals(pair.y.ToString(), StringComparison.CurrentCultureIgnoreCase));
            }

            public int GetHashCode(RuleSet ruleSet)
            {
                if (ReferenceEquals(ruleSet, null)) return 0;

                return ruleSet.Selectors.Aggregate(0, (hash, selector) => hash ^ selector.ToString().GetHashCode());
            }
        }

    }
}
