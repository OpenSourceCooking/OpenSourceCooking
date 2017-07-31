using System.Web.Mvc;

namespace OpenSourceCooking
{
    sealed class FilterConfig
    {
        FilterConfig() { }
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new AIHandleErrorAttribute());
        }
    }
}
