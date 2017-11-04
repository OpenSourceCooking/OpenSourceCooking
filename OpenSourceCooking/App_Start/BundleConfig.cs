using System.Web.Optimization;

namespace OpenSourceCooking
{
    sealed class BundleConfig
    {
        BundleConfig() { }
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery-ui-1.12.1.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.validate*"));
            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"));
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/tether/tether.min.js",
                "~/Scripts/umd/popper.min.js",
                "~/Scripts/bootstrap.min.js",
                "~/Scripts/respond.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/OpenSourceCooking").Include(
                "~/Scripts/isotope.pkgd.min.js",
                "~/Scripts/imagesloaded.pkgd.min.js",//Used with Isotope
                "~/Scripts/select2.min.js",
                "~/Scripts/openSourceCooking.js"
                //"~/Scripts/PhotoSwipe/photoswipe.min.js",
                //"~/Scripts/PhotoSwipe/photoswipe-ui-default.min.js"
                ));
            bundles.Add(new StyleBundle("~/Content/CssBundle").Include(
                "~/Content/tether/tether.min.css",
                "~/Content/bootstrap-grid.min.css",
                "~/Content/bootstrap-reboot.min.css",
                "~/Content/bootstrap.min.css",
                "~/Content/themes/base/jquery-ui.min.css",
                "~/Content/font-awesome.min.css",
                "~/Content/range-slider.css",
                "~/Content/css/select2.min.css",
                //"~/Content/PhotoSwipe/photoswipe.css",
                //"~/Content/PhotoSwipe/DefaultSkin/default-skin.css",
                "~/Content/Site.css"));
        }
    }
}
