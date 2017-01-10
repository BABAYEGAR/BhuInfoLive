﻿using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using BhuInfo.Data.Context.DataContext;
using BhuInfo.Data.Factory.BusinessFactory;
using BhuInfo.Data.Objects.Entities;
using BhuInfo.Data.Service.EmailService;
using BhuInfo.Data.Service.Encryption;
using BhuInfo.Data.Service.Enums;

namespace BhuInfoWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly NewsDataContext _db = new NewsDataContext();
        private readonly ContactUsDataContext _dbc = new ContactUsDataContext();
        private readonly EventDataContext _dbd = new EventDataContext();
        private readonly NewsStatusDataContext _dbe = new NewsStatusDataContext();

        public ActionResult Index()
        {
            var news = new NewsDataFactory().GetTopNthMostRecentNews(5);
            return View(news);
        }
        public ActionResult Dashboard()
        {
            var news = new NewsDataFactory().GetTopNthMostRecentNews(5);
            return View(news);
        }

        public ActionResult About()
        {
            var generalNews =
                new NewsDataFactory().GetTopFiveMostRecentNewsByCategory(NewsCategoryEnum.General.ToString());
            return View(generalNews);
        }

        public ActionResult Contact()
        {
            return View();
        }
        public ActionResult AllPastNews()
        {
            var allNews = _db.News.ToList();
            return View(allNews);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ContactUs([Bind(Include = "SenderName,Message,Email")] ContactUs contact,
            FormCollection collectedValues)
        {
            if (ModelState.IsValid)
            {
                var sendername = collectedValues["SenderName"];
                var message = collectedValues["message"];
                var email = collectedValues["Email"];
                contact.DateCreated = DateTime.Now;
                _dbc.Contact.Add(contact);
                _dbc.SaveChanges();
                new MailerDaemon().ContactUs(sendername, message, email);
                return RedirectToAction("Contact", "Home");
            }
            return RedirectToAction("Contact", "Home");
        }

        public ActionResult ViewNewsDetails(string Id)
        {
            var newsId = Convert.ToInt64(new Md5Ecryption().DecryptPrimaryKey(Id,true));
            if (ModelState.IsValid)
            {
                var news = new NewsDataFactory().GetNewsById(Convert.ToInt64(newsId));
                var newsUpdate = _db.News.Find(newsId);
                newsUpdate.NewsView = newsUpdate.NewsView + 1;
                _db.Entry(newsUpdate).State = EntityState.Modified;
                _db.SaveChanges();
                return View("ViewNewsDetails", news);
            }
            var newsToRedirect = _db.News.Find(newsId);
            return View(newsToRedirect);
        }

        [HttpPost]
        public ActionResult LikeOrDislikeANews(long Id, string actionType)
        {
            var loggedinuser = Session["bhuinfologgedinuser"] as AppUser;
            bool checkLike = false;
            bool checkDislike = false;
            NewsStatus status = new NewsStatus();
            if (ModelState.IsValid)
            {
                var allNewsStatus = _dbe.NewsStatuses.ToList();
                if (loggedinuser != null)
                {
                    var checkLikeNewsStatus =
                        allNewsStatus.SingleOrDefault(
                            n =>
                                n.LoggedInUserId == loggedinuser.AppUserId &&
                                n.Status == NewsActionType.Like.ToString() && n.NewsId == Id);
                    var checkDisLikeNewsStatus =
                        allNewsStatus.SingleOrDefault(
                            n =>
                                n.LoggedInUserId == loggedinuser.AppUserId &&
                                n.Status == NewsActionType.Dislike.ToString() && n.NewsId == Id);

                    var newsModel = new NewsDataFactory().GetNewsById(Id);
                    var news = _db.News.Find(Id);
                    //compare for like status
                    if (actionType == NewsActionType.Like.ToString() && checkLikeNewsStatus != null)
                    {


                    }
                    if (actionType == NewsActionType.Like.ToString() && (checkLikeNewsStatus == null && checkDisLikeNewsStatus == null))
                    {
                        news.Likes = news.Likes + 1;
                        status.NewsId = news.NewsId;
                        status.LoggedInUserId = loggedinuser.AppUserId;
                        status.Status = NewsActionType.Like.ToString();

                    }
                    if (actionType == NewsActionType.Dislike.ToString() && checkLikeNewsStatus != null)
                    {
                        news.Dislikes = news.Dislikes + 1;
                        news.Likes = news.Likes - 1;
                        status.NewsId = news.NewsId;
                        status.LoggedInUserId = loggedinuser.AppUserId;
                        status.Status = NewsActionType.Dislike.ToString();
                        checkDislike = true;
                    }
                    if (actionType == NewsActionType.Dislike.ToString() && (checkLikeNewsStatus == null && checkDisLikeNewsStatus == null))
                    {
                        news.Dislikes = news.Dislikes + 1;
                        status.NewsId = news.NewsId;
                        status.LoggedInUserId = loggedinuser.AppUserId;
                        status.Status = NewsActionType.Dislike.ToString();
                    }


                    //compare for dislike status
                    if (actionType == NewsActionType.Like.ToString() && checkDisLikeNewsStatus != null)
                    {
                        news.Dislikes = news.Dislikes - 1;
                        news.Likes = news.Likes + 1;
                        status.NewsId = news.NewsId;
                        status.LoggedInUserId = loggedinuser.AppUserId;
                        status.Status = NewsActionType.Like.ToString();
                        checkLike = true;

                    }
                    if (actionType == NewsActionType.Dislike.ToString() && checkDisLikeNewsStatus != null)
                    {
                    }
                    if (checkDislike)
                    {
                        _dbe.NewsStatuses.Remove(checkLikeNewsStatus);
                        _dbe.SaveChanges();
                    }
                    if (checkLike)
                    {
                        _dbe.NewsStatuses.Remove(checkDisLikeNewsStatus);
                        _dbe.SaveChanges();
                    }

                    _db.Entry(news).State = EntityState.Modified;
                    _db.SaveChanges();
                    _dbe.NewsStatuses.Add(status);
                    _dbe.SaveChanges();
                    return PartialView("_LikeOrDislikePartial", newsModel);
                }
            }
            var newsToRedirect = _db.News.Find(Id);
            return View("ViewNewsDetails", newsToRedirect);
        }

        [HttpGet]
        public ActionResult ReloadPartialView(string Id)
        {
            var newsId = Convert.ToInt64(new Md5Ecryption().DecryptPrimaryKey(Id, true));
            var newsModel = new NewsDataFactory().GetNewsById(newsId);
            return PartialView("SubComment", newsModel);
        }
        [HttpGet]
        public ActionResult ReloadCompleteView(string Id)
        {
            var newsId = Convert.ToInt64(new Md5Ecryption().DecryptPrimaryKey(Id, true));
            var newsModel = new NewsDataFactory().GetNewsById(newsId);
            return PartialView("Comment", newsModel);
        }
        [HttpGet]
        public ActionResult ReloadLikeDislikeInfo(string Id)
        {
            var newsId = Convert.ToInt64(new Md5Ecryption().DecryptPrimaryKey(Id, true));
            var newsModel = new NewsDataFactory().GetNewsById(newsId);
            return PartialView("_LikeDislikeInfo", newsModel);
        }
        [HttpGet]
        public ActionResult ReloadLikeDislikePartial(string Id)
        {
            var newsId = Convert.ToInt64(new Md5Ecryption().DecryptPrimaryKey(Id, true));
            var newsModel = new NewsDataFactory().GetNewsById(newsId);
            return PartialView("_LikeOrDislikePartial", newsModel);
        }

        public ActionResult SportsNews()
        {
            var news = new NewsDataFactory().GetTopNthMostRecentNews(5);
            return View("Sports", news);
        }

        public ActionResult FashionNews()
        {
            var news = new NewsDataFactory().GetTopNthMostRecentNews(5);
            return View("Fashion", news);
        }

        public ActionResult GeneralNews()
        {
            var news = new NewsDataFactory().GetTopNthMostRecentNews(5);
            return View("General", news);
        }

        public ActionResult UpcomingEvent()
        {
            var events = _dbd.Events.ToList();
            return View("UpcomingEvent", events);
        }

        public ActionResult SrcTeam()
        {
            return View("SrcTeam");
        }
    }
}