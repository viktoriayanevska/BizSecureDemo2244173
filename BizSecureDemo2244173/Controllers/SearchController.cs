using BizSecureDemo.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BizSecureDemo.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        private readonly AppDbContext _db;

        public SearchController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Results(string keyword)
        {
            // УЯЗВИМ ВАРИАНТ - остави го закоментиран за демонстрация
            // var vulnerableSql = $"SELECT * FROM Orders WHERE Title LIKE '%{keyword}%'";
            // var vulnerableResults = await _db.Orders
            //     .FromSqlRaw(vulnerableSql)
            //     .ToListAsync();
            // return View(vulnerableResults);

            // ЗАЩИТЕН ВАРИАНТ - параметризирана заявка
            var sql = "SELECT * FROM Orders WHERE Title LIKE @keyword";
            var param = new SqlParameter("@keyword", $"%{keyword}%");

            var results = await _db.Orders
                .FromSqlRaw(sql, param)
                .ToListAsync();

            return View(results);
        }
    }
}