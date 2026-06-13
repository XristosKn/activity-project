using ActivityProjectApp.Database;
using ActivityProjectApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace ActivityProjectApp.Data
{
    public class DatabaseAnnouncementRepository
    {
        public void AddAnnouncement(Announcement announcement)
        {
            using AppDbContext dbContext = new AppDbContext();

            dbContext.Announcements.Add(announcement);
            dbContext.SaveChanges();
        }

        public void AddAnnouncement(Announcement announcement, List<AnnouncementTarget> targets)
        {
            using AppDbContext dbContext = new AppDbContext();

            dbContext.Announcements.Add(announcement);
            dbContext.SaveChanges();

            foreach (AnnouncementTarget target in targets)
            {
                target.AnnouncementId = announcement.Id;
                dbContext.AnnouncementTargets.Add(target);
            }

            dbContext.SaveChanges();
        }

        public List<Announcement> GetAllAnnouncements()
        {
            using AppDbContext dbContext = new AppDbContext();

            return dbContext.Announcements
                .ToList();
        }

        public List<Announcement> GetActiveAnnouncements()
        {
            using AppDbContext dbContext = new AppDbContext();

            return dbContext.Announcements
                .Where(announcement => announcement.IsActive)
                .OrderByDescending(announcement => announcement.AnnouncementDate)
                .ToList();
        }

        public List<AnnouncementTarget> GetTargetsByAnnouncementId(int announcementId)
        {
            using AppDbContext dbContext = new AppDbContext();

            return dbContext.AnnouncementTargets
                .Where(target => target.AnnouncementId == announcementId)
                .ToList();
        }

        public List<Announcement> GetAnnouncementsByItem(int itemId, AnnouncementItemType itemType)
        {
            using AppDbContext dbContext = new AppDbContext();

            List<int> announcementIds = dbContext.AnnouncementTargets
                .Where(target =>
                    target.ItemId == itemId &&
                    target.ItemType == itemType)
                .Select(target => target.AnnouncementId)
                .ToList();

            return dbContext.Announcements
                .Where(announcement =>
                    announcementIds.Contains(announcement.Id) &&
                    announcement.IsActive)
                .OrderByDescending(announcement => announcement.AnnouncementDate)
                .ToList();
        }

        public List<Announcement> GetAnnouncementsByItemType(AnnouncementItemType itemType)
        {
            using AppDbContext dbContext = new AppDbContext();

            List<int> announcementIds = dbContext.AnnouncementTargets
                .Where(target => target.ItemType == itemType)
                .Select(target => target.AnnouncementId)
                .Distinct()
                .ToList();

            return dbContext.Announcements
                .Where(announcement =>
                    announcementIds.Contains(announcement.Id) &&
                    announcement.IsActive)
                .OrderByDescending(announcement => announcement.AnnouncementDate)
                .ToList();
        }

        public void DeactivateAnnouncement(int announcementId)
        {
            using AppDbContext dbContext = new AppDbContext();

            Announcement? announcement = dbContext.Announcements
                .FirstOrDefault(item => item.Id == announcementId);

            if (announcement == null)
            {
                return;
            }

            announcement.IsActive = false;
            dbContext.SaveChanges();
        }

        public void ActivateAnnouncement(int announcementId)
        {
            using AppDbContext dbContext = new AppDbContext();

            Announcement? announcement = dbContext.Announcements
                .FirstOrDefault(item => item.Id == announcementId);

            if (announcement == null)
            {
                return;
            }

            announcement.IsActive = true;
            dbContext.SaveChanges();
        }
        public Announcement? GetAnnouncementById(int announcementId)
        {
            using AppDbContext dbContext = new AppDbContext();

            return dbContext.Announcements
                .FirstOrDefault(announcement => announcement.Id == announcementId);
        }

        public void UpdateAnnouncement(Announcement updatedAnnouncement, List<AnnouncementTarget> updatedTargets)
        {
            using AppDbContext dbContext = new AppDbContext();

            Announcement? existingAnnouncement = dbContext.Announcements
                .FirstOrDefault(announcement => announcement.Id == updatedAnnouncement.Id);

            if (existingAnnouncement == null)
            {
                return;
            }

            existingAnnouncement.Title = updatedAnnouncement.Title;
            existingAnnouncement.Text = updatedAnnouncement.Text;
            existingAnnouncement.AnnouncementDate = updatedAnnouncement.AnnouncementDate;
            existingAnnouncement.Gallery = updatedAnnouncement.Gallery;
            existingAnnouncement.IsActive = updatedAnnouncement.IsActive;
            existingAnnouncement.ServiceProviderId = updatedAnnouncement.ServiceProviderId;

            List<AnnouncementTarget> existingTargets = dbContext.AnnouncementTargets
                .Where(target => target.AnnouncementId == updatedAnnouncement.Id)
                .ToList();

            dbContext.AnnouncementTargets.RemoveRange(existingTargets);

            foreach (AnnouncementTarget target in updatedTargets)
            {
                target.AnnouncementId = updatedAnnouncement.Id;
                dbContext.AnnouncementTargets.Add(target);
            }

            dbContext.SaveChanges();
        }

        public void DeleteAnnouncement(int announcementId)
        {
            using AppDbContext dbContext = new AppDbContext();

            Announcement? existingAnnouncement = dbContext.Announcements
                .FirstOrDefault(announcement => announcement.Id == announcementId);

            if (existingAnnouncement == null)
            {
                return;
            }

            List<AnnouncementTarget> existingTargets = dbContext.AnnouncementTargets
                .Where(target => target.AnnouncementId == announcementId)
                .ToList();

            dbContext.AnnouncementTargets.RemoveRange(existingTargets);
            dbContext.Announcements.Remove(existingAnnouncement);

            dbContext.SaveChanges();
        }
    }
}