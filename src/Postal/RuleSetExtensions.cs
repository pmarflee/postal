using System;
using System.Linq;
using BoneSoft.CSS;

namespace Postal
{
    public static class RuleSetExtensions
    {
        private static readonly string[] PseudoElements = new[] {"before", "after", "first-line", "first-letter"};

        private static readonly Func<RuleSet, int> SpecificityMemoizer = Memoizer.Memoize(
            (RuleSet r) => r.Selectors
                               .SelectMany(selector => selector.SimpleSelectors)
                               .Sum(simpleSelector => Calculate(simpleSelector))
            );

        /// <summary>
        /// Calculates the specificity for a given CSS ruleset.
        /// Information on CSS specificity rules can be found here: http://coding.smashingmagazine.com/2007/07/27/css-specificity-things-you-should-know/
        /// </summary>
        /// <param name="ruleSet">The CSS ruleset to calculate the specificity of</param>
        /// <returns>The specificity of the CSS ruleset</returns>
        public static int CalculateSpecificity(this RuleSet ruleSet)
        {
            return SpecificityMemoizer(ruleSet);
        }

        private static int Calculate(SimpleSelector simpleSelector)
        {
            var total = 0;

            if (simpleSelector.Child != null) total += Calculate(simpleSelector.Child);

            if (!string.IsNullOrEmpty(simpleSelector.ID)) total += 100;

            if (simpleSelector.Attribute != null) total += 10;

            if (!string.IsNullOrEmpty(simpleSelector.Class)) total += 10;

            if (!string.IsNullOrEmpty(simpleSelector.Pseudo))
                total += PseudoElements.Contains(simpleSelector.Pseudo.ToLower()) ? 1 : 10;

            if (!string.IsNullOrEmpty(simpleSelector.ElementName) && simpleSelector.ElementName != "*") total += 1;

            return total;
        }
    }
}