// Copyright 2008 Louis DeJardin - http://whereslou.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using NorthwindDemo.Models;

namespace NorthwindDemo.Controllers
{
    public class ProductsController : Controller
    {
        private readonly NorthwindRepository repository;

        public ProductsController()
            : this(new NorthwindRepository(new NorthwindDataContext()))
        {
        }

        public ProductsController(NorthwindRepository context)
        {
            repository = context;
        }

        public object Index()
        {
            return Categories();
        }

        public object Categories()
        {
            return View("Categories", repository.Categories.ToList());
        }

        public object Detail(int id)
        {
            Product product = repository.Products.SingleOrDefault(p => p.ProductID == id);
            return View(product);
        }

        public object List(string id)
        {
            Category category = repository.Categories.SingleOrDefault(c => c.CategoryName == id);

            IQueryable<Product> products = from p in repository.Products
                                           where p.CategoryID == category.CategoryID
                                           select p;

            ViewData["Title"] = "Hello World!";
            ViewData["CategoryName"] = id;

            //this.ViewEngine = new MvcContrib.NHamlViewEngine.NHamlViewFactory();
            return View("ListingByCategory", products.ToList());
        }

        public object Category(int id)
        {
            Category category = repository.Categories.SingleOrDefault(c => c.CategoryID == id);
            return View("List", category);
        }

        public object New()
        {
            var viewData = new ProductsNewViewData();

            viewData.Suppliers = repository.Suppliers.ToList();
            viewData.Categories = repository.Categories.ToList();

            return View("New", viewData);
        }

        public object Create()
        {
            var product = new Product();

            throw new NotImplementedException("Not sure what BindingHelperExtensions turned into");
            //BindingHelperExtensions.UpdateFrom(product, Request.Form);

            repository.InsertProductOnSubmit(product);
            repository.SubmitChanges();

            return RedirectToRoute(new RouteValueDictionary(new {Action = "List", ID = product.Category.CategoryName}));
        }

        public object Edit(int id)
        {
            var viewData = new ProductsEditViewData();

            Product product = repository.Products.SingleOrDefault(p => p.ProductID == id);
            viewData.Product = product;

            if (TempData.ContainsKey("ErrorMessage"))
            {
                foreach (var item in TempData)
                {
                    ViewData[item.Key] = item.Value;
                }
            }

            ViewData["CategoryID"] = new SelectList(repository.Categories.ToList(), "CategoryID", "CategoryName",
                                                    ViewData["CategoryID"] ?? product.CategoryID);
            ViewData["SupplierID"] = new SelectList(repository.Suppliers.ToList(), "SupplierID", "CompanyName",
                                                    ViewData["SupplierID"] ?? product.SupplierID);

            return View("Edit", viewData);
        }

        public object Update(int id)
        {
            Product product = repository.Products.SingleOrDefault(p => p.ProductID == id);
            if (!IsValid())
            {
                Request.Form.CopyTo(TempData);
                TempData["ErrorMessage"] = "An error occurred";
                return RedirectToAction("Edit", new {id});
            }

            throw new NotImplementedException("Not sure what BindingHelperExtensions turned into");
            //BindingHelperExtensions.UpdateFrom(product, Request.Form);
            repository.SubmitChanges();

            return RedirectToRoute(new RouteValueDictionary(new {Action = "List", ID = product.Category.CategoryName}));
        }

        private bool IsValid()
        {
            bool valid = true;

            if (!IsValidPrice(Request.Form["UnitPrice"]))
            {
                valid = false;
                SetInvalid("UnitPrice");
            }

            if (String.IsNullOrEmpty(Request.Form["ProductName"]))
            {
                valid = false;
                SetInvalid("ProductName");
            }

            return valid;
        }

        private void SetInvalid(string key)
        {
            TempData["Error:" + key] = Request.Form[key];
        }

        private bool IsValidPrice(string price)
        {
            if (String.IsNullOrEmpty(price))
                return false;

            decimal result;
            return decimal.TryParse(price, out result);
        }
    }
}