using System.Threading.Tasks;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using System;

namespace OpenSourceCooking.Controllers.StandardControllers
{
    public class CloudFilesController : Controller
    {
        private OpenSourceCookingEntities db = new OpenSourceCookingEntities();

        /*
        // GET: CloudFiles
        public async Task<ActionResult> Index()
        {
            var cloudFiles = db.CloudFiles.Include(c => c.AspNetUser);
            return View(await cloudFiles.ToListAsync());
        }

        // GET: CloudFiles/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CloudFile cloudFile = await db.CloudFiles.FindAsync(id);
            if (cloudFile == null)
            {
                return HttpNotFound();
            }
            return View(cloudFile);
        }

        // GET: CloudFiles/Create
        public ActionResult Create()
        {
            ViewBag.CreatorId = new SelectList(db.AspNetUsers, "Id", "Email");
            return View();
        }

        // POST: CloudFiles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,FileType,Url,CreatorId")] CloudFile cloudFile)
        {
            if (ModelState.IsValid)
            {
                db.CloudFiles.Add(cloudFile);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.CreatorId = new SelectList(db.AspNetUsers, "Id", "Email", cloudFile.CreatorId);
            return View(cloudFile);
        }

        // GET: CloudFiles/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CloudFile cloudFile = await db.CloudFiles.FindAsync(id);
            if (cloudFile == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatorId = new SelectList(db.AspNetUsers, "Id", "Email", cloudFile.CreatorId);
            return View(cloudFile);
        }

        // POST: CloudFiles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,FileType,Url,CreatorId")] CloudFile cloudFile)
        {
            if (ModelState.IsValid)
            {
                db.Entry(cloudFile).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.CreatorId = new SelectList(db.AspNetUsers, "Id", "Email", cloudFile.CreatorId);
            return View(cloudFile);
        }

        // GET: CloudFiles/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CloudFile cloudFile = await db.CloudFiles.FindAsync(id);
            if (cloudFile == null)
            {
                return HttpNotFound();
            }
            return View(cloudFile);
        }

        // POST: CloudFiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            CloudFile cloudFile = await db.CloudFiles.FindAsync(id);
            db.CloudFiles.Remove(cloudFile);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        */

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public async Task<JsonResult> AjaxDeleteUnreferencedCloudFilesIfExist()
        {
            var CloudFiles = db.CloudFiles;
            foreach (CloudFile CloudFile in CloudFiles.Where(x=>x.RecipeCloudFiles.Count() == 0 && x.RecipeStepsCloudFiles.Count() == 0 && x.Ingredients.Count() == 0))
                    db.CloudFiles.Remove(CloudFile);
            await db.SaveChangesAsync();
            foreach (AzureCloudStorageWrapper.AzureBlobContainer AzureBlobContainer in Enum.GetValues(typeof(AzureCloudStorageWrapper.AzureBlobContainer)))
            {
                string AzureBlobContainerName = AzureBlobContainer.ToString();
                var CloudFilesToKeep = CloudFiles.Where(x => x.Url.Contains(AzureBlobContainerName));
                List<string> UrlsToKeep = CloudFilesToKeep.Select(x => x.Url).Union(CloudFilesToKeep.Select(x=>x.CloudFilesThumbnail.Url)).ToList();
                await AzureCloudStorageWrapper.DeleteUnreferencedCloudFilesIfExist(AzureBlobContainer, UrlsToKeep);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}
