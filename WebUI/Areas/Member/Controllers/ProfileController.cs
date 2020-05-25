﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Business.Abstract;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebUI.Areas.Admin.Models;
using WebUI.Areas.Member.Models;

namespace WebUI.Areas.Member.Controllers
{
    [Area("Member")]
    [Authorize(Roles="Member")]
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private IBookService _bookService;
        private readonly IBookAppUserService _bookAppUserService;

        public ProfileController(UserManager<AppUser> userManager, IBookService bookService, IBookAppUserService bookAppUserService)
        {
            _userManager = userManager;
            _bookService = bookService;
            _bookAppUserService = bookAppUserService;
        }

        public  async Task<IActionResult> Index()
        {
            var books = _bookService.GetList();
           
            TempData["Active"] = "profile";
            TempData["sa"] =TempData["deneme"];
            var appUser = await _userManager.FindByNameAsync(User.Identity.Name);
            Models.Member member = new Models.Member();
            member.Id = appUser.Id;
            member.FirstName = appUser.FirstName;
            member.LastName = appUser.LastName;
            member.Email = appUser.Email;
            member.UserName = appUser.UserName;
            member.Description = appUser.Description;
            member.Picture = appUser.ProfileImageFile;
            CustomModel model = new CustomModel();
            model.Member = member;

            List<Review> reviews = new List<Review>();
            var userId = appUser.Id;
            var userbooks = _bookAppUserService.GetByAppUserId(userId);
            foreach (var book in userbooks)
            {
                if (book.Reviews.Count > 0)
                {
                    reviews.AddRange(book.Reviews);
                }
            }

            ViewBag.Comment= reviews;
          
            return View(model);
        }


    


        [HttpPost]

        public async Task<IActionResult> Index(CustomModel model)
        {
            if (ModelState.IsValid)
            {

                var updatedUser = _userManager.Users.FirstOrDefault(I => I.Id == model.Member.Id);


                if (model.MyFile != null)
                {
                    string uzanti = Path.GetExtension(model.MyFile.FileName);
                    string photoName = Guid.NewGuid() + uzanti;
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/" + photoName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await model.MyFile.CopyToAsync(stream);
                    }

                    updatedUser.ProfileImageFile = photoName;
                }

                updatedUser.Id = model.Member.Id;
                updatedUser.UserName = model.Member.UserName;
                updatedUser.FirstName = model.Member.FirstName;
                updatedUser.LastName = model.Member.LastName;
                updatedUser.Email = model.Member.Email;
                updatedUser.Description = model.Member.Description;
                var result = await _userManager.UpdateAsync(updatedUser);
                if (result.Succeeded)
                {
                    TempData["username"] = updatedUser.UserName;
                    TempData["deneme"] = "Güncelleme işleminiz başarı ile gerçekleşti.";
                    return RedirectToAction("Index");
                }
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
            }

            return View(model);
        }
    }   
}