using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using UniversityTuitionPaymentSystem.Data;
using UniversityTuitionPaymentSystem.Models;

namespace UniversityTuitionPaymentSystem.Controllers
{
    [ApiController]
    [Route("api/v1/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UniversityDatabase _db;

        public AdminController(UniversityDatabase db)
        {
            _db = db;
        }


        [HttpPost("addTuition")]
        public async Task<IActionResult> AddTuition([FromBody] AddTuitionRequest request)
        {
            var student = await _db.Students.FirstOrDefaultAsync(s => s.StudentNo == request.StudentNo);
            if (student == null)
                return NotFound("Student not found");

            var tuition = new Tuition
            {
                StudentId = student.StudentId,
                Term = request.Term,
                TotalAmount = request.TotalAmount,
                Balance = request.TotalAmount
            };

            _db.Tuitions.Add(tuition);
            await _db.SaveChangesAsync();

            return Ok(new { TransactionStatus = "Added", TuitionId = tuition.TuitionId });
        }

      
        [HttpPost("addTuitionBatch")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddTuitionBatch(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("CSV file is required");

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<BatchRecord>().ToList();
            int added = 0;

            foreach (var rec in records)
            {
                var student = await _db.Students.FirstOrDefaultAsync(s => s.StudentNo == rec.StudentNo);
                if (student == null) continue;

                var tuition = new Tuition
                {
                    StudentId = student.StudentId,
                    Term = rec.Term,
                    TotalAmount = rec.TotalAmount,
                    Balance = rec.TotalAmount
                };

                _db.Tuitions.Add(tuition);
                added++;
            }

            await _db.SaveChangesAsync();

            return Ok(new { TransactionStatus = "BatchProcessed", AddedCount = added });
        }

        
        [HttpGet("unpaid")]
        public async Task<IActionResult> GetUnpaid([FromQuery] string term, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest("term is required");

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var query = _db.Tuitions
                .Include(t => t.Student)
                .Where(t => t.Term == term && t.Balance > 0);

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(t => t.Student.StudentNo)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    t.TuitionId,
                    t.Student.StudentNo,
                    StudentName = t.Student.FirstName + " " + t.Student.LastName,
                    t.Term,
                    t.Balance
                })
                .ToListAsync();

            return Ok(new
            {
                Total = total,
                Page = page,
                PageSize = pageSize,
                Items = items
            });
        }
    }

    public class AddTuitionRequest
    {
        public string StudentNo { get; set; } = null!;
        public string Term { get; set; } = null!;
        public decimal TotalAmount { get; set; }
    }

    public class BatchRecord
    {
        public string StudentNo { get; set; } = null!;
        public string Term { get; set; } = null!;
        public decimal TotalAmount { get; set; }
    }
}
