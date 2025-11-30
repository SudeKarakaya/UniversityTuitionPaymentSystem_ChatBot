namespace UniversityTuitionPaymentSystem.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int TuitionId { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime PaymentTime { get; set; }
        public string Source { get; set; } = null!;
        public string Status { get; set; } = null!;

        public Tuition? Tuition { get; set; }
    }
}
