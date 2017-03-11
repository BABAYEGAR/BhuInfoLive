using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BhuInfo.Data.Context.DataContext;
using BhuInfo.Data.Objects.Entities;
using BhuInfo.Data.Service.Enums;

namespace BhuInfoWeb.Controllers.BhuWebControllers
{
    public class VideosController : Controller
    {
        private readonly VideoDataContext db = new VideoDataContext();

        // GET: Videos
        public ActionResult Index()
        {
            return View(db.Videos.ToList());
        }

        // GET: Videos/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var video = db.Videos.Find(id);
            if (video == null)
                return HttpNotFound();
            return View(video);
        }

        // GET: Videos/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Videos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "VideoId,Name,Link")] Video video)
        {
            var loggedinuser = Session["bhuinfologgedinuser"] as AppUser;
            if (ModelState.IsValid)
                if (loggedinuser != null)
                {
                    video.DateCreated = DateTime.Now;
                    video.DateLastModified = DateTime.Now;
                    video.CreatedById = loggedinuser.AppUserId;
                    video.LastModifiedById = loggedinuser.AppUserId;
                    db.Videos.Add(video);
                    db.SaveChanges();
                    TempData["video"] = "You have successfully added a new video!";
                    TempData["notificationtype"] = NotificationType.Success.ToString();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["login"] = "Your session has expired,Login Again!";
                    TempData["notificationtype"] = NotificationType.Info.ToString();
                    return RedirectToAction("Index");
                }

            return View(video);
        }

        // GET: Videos/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var video = db.Videos.Find(id);
            if (video == null)
                return HttpNotFound();
            return View(video);
        }

        // POST: Videos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "VideoId,Name,Link,CreatedById,DateCreated")] Video video)
        {
            var loggedinuser = Session["bhuinfologgedinuser"] as AppUser;
            if (ModelState.IsValid)
                if (loggedinuser != null)
                {
                    video.DateLastModified = DateTime.Now;
                    video.LastModifiedById = loggedinuser.AppUserId;
                    db.Entry(video).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["video"] = "You have successfully edited the video!";
                    TempData["notificationtype"] = NotificationType.Success.ToString();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["login"] = "Your session has expired,Login Again!";
                    TempData["notificationtype"] = NotificationType.Info.ToString();
                    return RedirectToAction("Index");
                }
            return View(video);
        }

        // GET: Videos/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var video = db.Videos.Find(id);
            if (video == null)
                return HttpNotFound();
            return View(video);
        }

        // POST: Videos/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            var video = db.Videos.Find(id);
            db.Videos.Remove(video);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}