namespace UniversityTuitionPaymentSystem.Models
{
    public class Student
    {
        public int StudentId { get; set; }
        public string StudentNo { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;

        public ICollection<Tuition> Tuitions { get; set; } = new List<Tuition>();

    }
}
