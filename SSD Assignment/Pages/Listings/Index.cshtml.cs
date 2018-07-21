﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SSD_Assignment.Models;
using SSD_Assignment.Data;

namespace SSD_Assignment.Pages.Listings
{
    public class IndexModel : PageModel
    {
        private readonly SSD_Assignment.Data.ApplicationDbContext _context;

        public IndexModel(SSD_Assignment.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Listing> Listing { get;set; }

        public async Task OnGetAsync(string searchString)
        {
            var listings = from l in _context.Listing
                           select l;

            if(!String.IsNullOrEmpty(searchString))
    {
                listings = listings.Where(s => s.Title.Contains(searchString));
            }

            Listing = await listings.ToListAsync();
        }
    }
}
