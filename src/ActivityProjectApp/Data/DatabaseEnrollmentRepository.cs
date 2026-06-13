using ActivityProjectApp.Database;
using ActivityProjectApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ActivityProjectApp.Data
{
    public class DatabaseEnrollmentRepository
    {
        public List<Enrollment> GetAllEnrollments()
        {
            using AppDbContext dbContext = new AppDbContext();

            return dbContext.Enrollments
                .ToList();
        }

        public List<Enrollment> GetEnrollmentsByCustomerId(int customerId)
        {
            using AppDbContext dbContext = new AppDbContext();

            return dbContext.Enrollments
                .Where(enrollment => enrollment.CustomerId == customerId)
                .ToList();
        }

        public bool IsCustomerEnrolled(int customerId, int itemId, EnrollmentItemType itemType)
        {
            using AppDbContext dbContext = new AppDbContext();

            return dbContext.Enrollments
                .Any(enrollment =>
                    enrollment.CustomerId == customerId &&
                    enrollment.ItemId == itemId &&
                    enrollment.ItemType == itemType &&
                    enrollment.Status != EnrollmentStatus.Cancelled);
        }

        public Enrollment? GetEnrollment(int customerId, int itemId, EnrollmentItemType itemType)
        {
            using AppDbContext dbContext = new AppDbContext();

            return dbContext.Enrollments
                .FirstOrDefault(enrollment =>
                    enrollment.CustomerId == customerId &&
                    enrollment.ItemId == itemId &&
                    enrollment.ItemType == itemType &&
                    enrollment.Status != EnrollmentStatus.Cancelled);
        }

        public Enrollment CreateEnrollment(int customerId, int itemId, EnrollmentItemType itemType)
        {
            using AppDbContext dbContext = new AppDbContext();

            Enrollment enrollment = new Enrollment
            {
                CustomerId = customerId,
                ItemId = itemId,
                ItemType = itemType,
                EnrollmentDate = DateTime.Now,
                Status = EnrollmentStatus.PendingPayment
            };

            dbContext.Enrollments.Add(enrollment);
            dbContext.SaveChanges();

            return enrollment;
        }

        public void MarkAsConfirmed(int enrollmentId)
        {
            using AppDbContext dbContext = new AppDbContext();

            Enrollment? enrollment = dbContext.Enrollments
                .FirstOrDefault(item => item.Id == enrollmentId);

            if (enrollment == null)
            {
                return;
            }

            enrollment.Status = EnrollmentStatus.Confirmed;
            dbContext.SaveChanges();
        }

        public void CancelEnrollment(int enrollmentId)
        {
            using AppDbContext dbContext = new AppDbContext();

            Enrollment? enrollment = dbContext.Enrollments
                .FirstOrDefault(item => item.Id == enrollmentId);

            if (enrollment == null)
            {
                return;
            }

            enrollment.Status = EnrollmentStatus.Cancelled;
            dbContext.SaveChanges();
        }
    }
}