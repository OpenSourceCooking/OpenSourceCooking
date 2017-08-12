using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using OpenSourceCooking.Models;

namespace OpenSourceCooking.Controllers.StandardControllers
{
    public class IngredientsController : Controller
    {
        private OpenSourceCookingEntities db = new OpenSourceCookingEntities();

        protected override void Dispose(bool disposing)
        {
            if (disposing)            
                db.Dispose();            
            base.Dispose(disposing);
        }


        public JsonResult AjaxGetIngredients(int pageIndex, int pageSize)
        {
            List<IngredientDataTransferObject> Ingredients = (from i in db.Ingredients
                                               orderby i.IngredientName ascending
                                               select new IngredientDataTransferObject()
                                               {
                                                   IngredientName = i.IngredientName,
                                                   ImageUrl = i.CloudFile != null ? i.CloudFile.Url : "",
                                                   CreatorId = i.CreatorId,
                                                   CreateDateUtc = i.CreateDateUtc
                                               }).Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return Json(Ingredients, JsonRequestBehavior.AllowGet);
        }        
        [HttpPost]
        public JsonResult AjaxIngredientsAutoComplete(string prefix)
        {
            //Using Anonymous here because the JQuery UI need the key/value named "label" & "val"
            var aIngredients = (db.Ingredients.Where(x => x.IngredientName.Contains(prefix)).OrderBy(x => x.IngredientName).Select(x => new
            {
                label = x.IngredientName,
                val = x.IngredientName
            })).Take(6).ToList();
            return Json(aIngredients, JsonRequestBehavior.AllowGet);
        }
    }
}
