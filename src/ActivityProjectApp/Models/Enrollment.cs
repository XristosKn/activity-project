using System;

namespace ActivityProjectApp.Models
{
    public enum EnrollmentItemType
    {
        Event,
        Course
    }

    public enum EnrollmentStatus
    {
        PendingPayment,
        Paid,
        Cancelled
    }

    public class Enrollment
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public int ItemId { get; set; }

        public EnrollmentItemType ItemType { get; set; }

        public EnrollmentStatus Status { get; set; }

        public DateTime EnrollmentDate { get; set; }
    }
}