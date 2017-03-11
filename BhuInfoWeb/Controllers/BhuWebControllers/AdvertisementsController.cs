﻿using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using BhuInfo.Data.Context.DataContext;
using BhuInfo.Data.Factory.BusinessFactory;
using BhuInfo.Data.Objects.Entities;
using BhuInfo.Data.Service.EmailService;
using BhuInfo.Data.Service.Encryption;
using BhuInfo.Data.Service.Enums;
using BhuInfo.Data.Service.FileUploader;

namespace BhuInfoWeb.Controllers.BhuWebControllers
{
    public class AdvertisementsController : Controller
    {
        private readonly AdvertisementDataContext _db = new AdvertisementDataContext();

        // GET: Advertisements
        public ActionResult Index()
        {
            return View(_db.Advertisements.ToList());
        }

        // GET: Advertisements/Details/5
        public ActionResult Details(string id)
        {
            var advertId = Convert.ToInt64(new Md5Ecryption().DecryptPrimaryKey(id, true));
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var advertisement = _db.Advertisements.Find(advertId);
            if (advertisement == null)
                return HttpNotFound();
            return View(advertisement);
        }

        // GET: Advertisements/Create
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        ///     This controller handles enabling and disabling an advert
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="actionType"></param>
        /// <returns></returns>
        public ActionResult EnableOrDisableAdvert(string Id, string actionType)
        {
            var advertId = Convert.ToInt64(new Md5Ecryption().DecryptPrimaryKey(Id, true));
            var advertisement = new AdvertsiementFactory().GetAdvertById((int)advertId);
            var loggedinuser = Session["bhuinfologgedinuser"] as AppUser;
            if (loggedinuser != null)
            {
                if (advertisement != null)
                {
                    if (actionType == AdvertStatus.Enable.ToString())
                        advertisement.Status = AdvertStatus.Enable.ToString();
                    if (actionType == AdvertStatus.Disable.ToString())
                        advertisement.Status = AdvertStatus.Disable.ToString();
                    advertisement.DateLastModified = DateTime.Now;
                    _db.Entry(advertisement).State = EntityState.Modified;
                    _db.SaveChanges();
                    TempData["advert"] =
                        "This advert has been" + actionType + "successfully";
                    TempData["notificationtype"] = NotificationType.Success.ToString();
                    return View("Index", _db.Advertisements.ToList());
                }
            }
            else
            {
                TempData["advert"] = "Your session has expired,Login Again!";
                TempData["notificationtype"] = NotificationType.Info.ToString();
                return RedirectToAction("Index", "Advertisements");
            }
            return View("Index", _db.Advertisements.ToList());
        }

        // POST: Advertisements/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AdvertCompanyName,Email")] Advertisement advertisement,
            FormCollection collectedValues)
        {
            var loggedinuser = Session["bhuinfologgedinuser"] as AppUser;
            if (ModelState.IsValid)
                if (loggedinuser != null)
                {
                    var advertType = typeof(AdvertType).GetEnumName(int.Parse(collectedValues["AdvertType"]));
                    if (new AdvertsiementFactory().CheckIfAdvertTypeExist(advertType, AdvertStatus.Enable.ToString(),
                            collectedValues["Email"]) == false)
                    {
                        advertisement.AccessCode = Membership.GeneratePassword(5, 0);
                        advertisement.AdvertType = advertType;
                        advertisement.Status = AdvertStatus.Enable.ToString();
                        advertisement.DateCreated = DateTime.Now;
                        advertisement.AdvertText = "";
                        advertisement.Title = "";
                        advertisement.DateLastModified = DateTime.Now;
                        advertisement.CreatedById = loggedinuser.AppUserId;
                        advertisement.LastModifiedById = loggedinuser.AppUserId;
                        _db.Advertisements.Add(advertisement);
                        _db.SaveChanges();
                        new MailerDaemon().SuscribeAdvert(advertisement);
                        TempData["advert"] =
                            "This advert has been created successfully";
                        TempData["notificationtype"] = NotificationType.Success.ToString();
                    }
                    else
                    {
                        TempData["advert"] =
                            "This advert cannot be created because the vacancy has been occupied by another advert";
                        TempData["notificationtype"] = NotificationType.Danger.ToString();
                        return View(advertisement);
                    }
                }
                else
                {
                    TempData["login"] = "Your session has expired,Login Again!";
                    TempData["notificationtype"] = NotificationType.Info.ToString();
                    return RedirectToAction("Index", "Home");
                }


            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult VerifyAccessCode()
        {
            return View();
        }

        [HttpPost]
        public ActionResult VerifyAccessCode(FormCollection collectedValues)
        {
            if (ModelState.IsValid)
            {
                var accessCode = collectedValues["AccessCode"];
                var advertExist = new AdvertsiementFactory().CheckIfAccessCodeExist(accessCode);
                if (advertExist != null)
                    return View("CreateAdvert", advertExist);
                TempData["advert"] = "This access code is invalid!Check it and try again";
                TempData["notificationtype"] = NotificationType.Danger.ToString();
            }
            return View();
        }

        // POST: Advertisements/CreateAdvert/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpGet]
        public ActionResult CreateAdvert()
        {
            return View("CreateAdvert");
        }

        // POST: Advertisements/CreateAdvert/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult CreateAdvert(FormCollection collectedValues)
        {
            var advertImage = Request.Files["Image"];
            var title = collectedValues["Title"];
            var companyName = collectedValues["AdvertCompanyName"];
            var advertId = long.Parse(collectedValues["Id"]);
            var status = collectedValues["Status"];
            var dateCreated = collectedValues["Created"];
            var dateModified = collectedValues["LastModified"];
            var createdBy = long.Parse(collectedValues["CreatedBy"]);
            var modifiedBy = long.Parse(collectedValues["ModifiedBy"]);
            var email = collectedValues["Email"];
            var type = collectedValues["Type"];
            var code = collectedValues["AccessCode"];
            var content = collectedValues["AdvertText"];
            var advertisement = new AdvertsiementFactory().GetAdvertById((int) advertId);
            if (ModelState.IsValid)
                if (advertisement != null)
                {
                    advertisement.AccessCode = code;
                    advertisement.AdvertType = type;
                    advertisement.CreatedById = createdBy;
                    advertisement.LastModifiedById = modifiedBy;
                    advertisement.Email = email;
                    advertisement.Status = status;
                    advertisement.DateCreated = Convert.ToDateTime(dateCreated);
                    advertisement.DateLastModified = Convert.ToDateTime(dateModified);
                    advertisement.AdvertImage = new FileUploader().UploadFile(advertImage, UploadType.Advert);
                    advertisement.Title = title;
                    advertisement.AdvertCompanyName = companyName;
                    advertisement.AdvertText = content;
                    _db.Entry(advertisement).State = EntityState.Modified;
                    _db.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
            return View(advertisement);
        }

        // GET: Advertisements/Edit/5
        public ActionResult Edit(string id)
        {
            var advertId = Convert.ToInt64(new Md5Ecryption().DecryptPrimaryKey(id, true));
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var advertisement = _db.Advertisements.Find(advertId);
            if (advertisement == null)
                return HttpNotFound();
            var advertType = new SelectList(typeof(AdvertType).GetEnumNames());
            ViewBag.advertType = advertType;
            return View(advertisement);
        }

        // POST: Advertisements/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FormCollection collectedValues)
        {
            var loggedinuser = Session["bhuinfologgedinuser"] as AppUser;

            HttpPostedFileBase advertImage = null;
            if (Request.Files["Image"] != null)
            {
                advertImage = Request.Files["Image"];
            }
            var Id = collectedValues["Id"];
            var advertId = Convert.ToInt64(new Md5Ecryption().DecryptPrimaryKey(Id, true));
            var companyName = collectedValues["AdvertCompanyName"];
            var advertisement = new AdvertsiementFactory().GetAdvertById((int)advertId);
            if (ModelState.IsValid)
                if (loggedinuser != null)
                {
                    if (advertImage != null && advertImage.FileName != "")
                    {
                        advertisement.AdvertImage = new FileUploader().UploadFile(advertImage, UploadType.Advert);
                    }
                    advertisement.AdvertCompanyName = companyName;
                    advertisement.DateLastModified = DateTime.Now;
                    advertisement.LastModifiedById = loggedinuser.AppUserId;
                    advertisement.AdvertType = collectedValues["AdvertType"];
                    _db.Entry(advertisement).State = EntityState.Modified;
                    _db.SaveChanges();
                    TempData["advert"] = "This advert has been modified successfully!";
                    TempData["notificationtype"] = NotificationType.Success.ToString();
                    return RedirectToAction("Index");
                }
                else
                {
                    
                    TempData["login"] = "Your session has expired,Login Again!";
                    TempData["notificationtype"] = NotificationType.Info.ToString();
                    return RedirectToAction("Login", "Account");
                }
            return View(advertisement);
        }

        // GET: Advertisements/Delete/5
        public ActionResult Delete(string id)
        {
            var advertId = Convert.ToInt64(new Md5Ecryption().DecryptPrimaryKey(id, true));
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var advertisement = _db.Advertisements.Find(advertId);
            if (advertisement == null)
                return HttpNotFound();
            return View(advertisement);
        }

        // POST: Advertisements/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var advertId = Convert.ToInt64(new Md5Ecryption().DecryptPrimaryKey(id, true));
            var advertisement = _db.Advertisements.Find(advertId);
            _db.Advertisements.Remove(advertisement);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _db.Dispose();
            base.Dispose(disposing);
        }
    }
}