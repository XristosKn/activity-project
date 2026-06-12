using ActivityProjectApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ActivityProjectApp.Data
{
    public class FakeEnrollmentRepository
    {
        private readonly List<Enrollment> _enrollments = new List<Enrollment>();

        private int _nextId = 1;

        public List<Enrollment> GetAllEnrollments()
        {
            return _enrollments.ToList();
        }

        public List<Enrollment> GetEnrollmentsByCustomerId(int customerId)
        {
            return _enrollments
                .Where(enrollment => enrollment.CustomerId == customerId)
                .ToList();
        }

        public bool IsCustomerEnrolled(int customerId, int itemId, EnrollmentItemType itemType)
        {
            return _enrollments.Any(enrollment =>
                enrollment.CustomerId == customerId &&
                enrollment.ItemId == itemId &&
                enrollment.ItemType == itemType &&
                enrollment.Status != EnrollmentStatus.Cancelled);
        }

        public Enrollment? GetEnrollment(int customerId, int itemId, EnrollmentItemType itemType)
        {
            return _enrollments.FirstOrDefault(enrollment =>
                enrollment.CustomerId == customerId &&
                enrollment.ItemId == itemId &&
                enrollment.ItemType == itemType &&
                enrollment.Status != EnrollmentStatus.Cancelled);
        }

        public Enrollment CreateEnrollment(int customerId, int itemId, EnrollmentItemType itemType)
        {
            Enrollment? existingEnrollment = GetEnrollment(customerId, itemId, itemType);

            if (existingEnrollment != null)
            {
                return existingEnrollment;
            }

            Enrollment enrollment = new Enrollment
            {
                Id = _nextId,
                CustomerId = customerId,
                ItemId = itemId,
                ItemType = itemType,
                Status = EnrollmentStatus.PendingPayment,
                EnrollmentDate = DateTime.Now
            };

            _enrollments.Add(enrollment);

            _nextId++;

            return enrollment;
        }

        public void MarkAsConfirmed(int enrollmentId)
        {
            Enrollment? enrollment = _enrollments
                .FirstOrDefault(item => item.Id == enrollmentId);

            if (enrollment == null)
            {
                return;
            }

            enrollment.Status = EnrollmentStatus.Confirmed;
        }

        public void CancelEnrollment(int enrollmentId)
        {
            Enrollment? enrollment = _enrollments
                .FirstOrDefault(item => item.Id == enrollmentId);

            if (enrollment == null)
            {
                return;
            }

            enrollment.Status = EnrollmentStatus.Cancelled;
        }
            }
}