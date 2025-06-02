using DataAccessLayer.Abstract;
using DataAccessLayer.Context;
using DataAccessLayer.Repositories;
using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.EntityFrameWork
{
    public class EfProductDal : GenericRepository<Product>, IProductDal
    {
        private readonly BookContext _context;

        public EfProductDal(BookContext context) : base(context)
        {
            _context = context;
        }

        public List<Product> GetProductWithCategory()
        {
            var values = _context.Products.Include(x => x.Category).ToList();
            return values;
        }
    }
}
