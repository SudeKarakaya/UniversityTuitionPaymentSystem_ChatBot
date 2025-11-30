using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityTuitionPaymentSystem.Data;
using UniversityTuitionPaymentSystem.Models;

namespace UniversityTuitionPaymentSystem.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TuitionController : ControllerBase
    {
        private readonly UniversityDatabase _db;

        public TuitionController(UniversityDatabase db)
        {
            _db = db;
        }


        [HttpGet("query")]
        [AllowAnonymous]
        public async Task<IActionResult> QueryForMobile([FromQuery] string studentNo)
        {
            if (string.IsNullOrWhiteSpace(studentNo))
                return BadRequest("Student Number is required.");

            var student = await _db.Students
                .Include(s => s.Tuitions)
                .FirstOrDefaultAsync(s => s.StudentNo == studentNo);

            if (student == null)
                return NotFound("Student not found");

            var tuitions = student.Tuitions.Select(t => new
            {
                t.Term,
                t.TotalAmount,
                t.Balance,
                t.IsPaid
            });

            return Ok(new
            {
                StudentNo = student.StudentNo,
                Tuitions = tuitions
            });
        }


        [HttpGet("bank/query")]
        [Authorize(Roles = "Bank,Admin")]
        public Task<IActionResult> QueryForBank([FromQuery] string studentNo)
        {
            return QueryForMobile(studentNo);
        }

        [HttpPost("pay")]
        [AllowAnonymous]
        public async Task<IActionResult> PayTuition([FromBody] PayRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.StudentNo) ||
                string.IsNullOrWhiteSpace(request.Term))
            {
                return BadRequest("Student Number and Term are required.");
            }

            if (request.Amount <= 0)
                return BadRequest("Amount must be > 0");

            var tuition = await _db.Tuitions
                .Include(t => t.Student)
                .FirstOrDefaultAsync(t =>
                    t.Student.StudentNo == request.StudentNo && t.Term == request.Term);

            if (tuition == null)
                return NotFound("Tuition not found");

            var payment = new Payment
            {
                TuitionId = tuition.TuitionId,
                AmountPaid = request.Amount,
                PaymentTime = DateTime.UtcNow,
                Source = request.Source ?? "Unknown",
                Status = "Successful"
            };

            _db.Payments.Add(payment);

            tuition.Balance = Math.Max(0, tuition.Balance - request.Amount);

            await _db.SaveChangesAsync();

            return Ok(new
            {
                PaymentStatus = payment.Status,
                NewBalance = tuition.Balance
            });
        }
    }
}

public class PayRequest
{
    public string StudentNo { get; set; } = null!;
    public string Term { get; set; } = null!;
    public decimal Amount { get; set; }
    public string? Source { get; set; }
}
