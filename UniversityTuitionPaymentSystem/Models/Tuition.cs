namespace UniversityTuitionPaymentSystem.Models
{
    public class Tuition
    {
        public int TuitionId { get; set; }
        public int StudentId { get; set; }
        public string Term { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public decimal Balance { get; set; }

        public bool IsPaid => Balance <= 0;

        public Student? Student { get; set; }
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    }
}
