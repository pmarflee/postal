namespace Postal
{
// ReSharper disable InconsistentNaming
    public interface ICSSInliner
// ReSharper restore InconsistentNaming
    {
        string Inline(string viewName, string viewOutput);
    }
}
