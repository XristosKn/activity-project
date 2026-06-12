using ActivityProjectApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ActivityProjectApp.Data
{
    public class FakeAnnouncementRepository
    {
        private readonly List<Announcement> _announcements = new List<Announcement>();

        private int _nextId = 1;

        public FakeAnnouncementRepository()
        {
            SeedAnnouncements();
        }

        private void SeedAnnouncements()
        {
            AddAnnouncement(new Announcement
            {
                Title = "New equipment available",
                Text = "New training equipment has been added for this activity.",
                ItemType = AnnouncementItemType.Event,
                ItemId = 1,
                AnnouncementDate = DateTime.Now,
                Gallery = string.Empty,
                IsActive = true
            });

            AddAnnouncement(new Announcement
            {
                Title = "Schedule update",
                Text = "The course schedule has been updated for the upcoming week.",
                ItemType = AnnouncementItemType.Course,
                ItemId = 1,
                AnnouncementDate = DateTime.Now,
                Gallery = string.Empty,
                IsActive = true
            });
        }

        public void AddAnnouncement(Announcement announcement)
        {
            announcement.Id = _nextId;
            _nextId++;

            _announcements.Add(announcement);
        }

        public List<Announcement> GetAllAnnouncements()
        {
            return _announcements.ToList();
        }

        public List<Announcement> GetActiveAnnouncements()
        {
            return _announcements
                .Where(announcement => announcement.IsActive)
                .OrderByDescending(announcement => announcement.AnnouncementDate)
                .ToList();
        }

        public List<Announcement> GetAnnouncementsByItem(
            int itemId,
            AnnouncementItemType itemType)
        {
            return _announcements
                .Where(announcement =>
                    announcement.ItemId == itemId &&
                    announcement.ItemType == itemType &&
                    announcement.IsActive)
                .OrderByDescending(announcement => announcement.AnnouncementDate)
                .ToList();
        }

        public List<Announcement> GetAnnouncementsByItemType(
            AnnouncementItemType itemType)
        {
            return _announcements
                .Where(announcement =>
                    announcement.ItemType == itemType &&
                    announcement.IsActive)
                .OrderByDescending(announcement => announcement.AnnouncementDate)
                .ToList();
        }

        public void DeactivateAnnouncement(int announcementId)
        {
            Announcement? announcement = _announcements
                .FirstOrDefault(item => item.Id == announcementId);

            if (announcement == null)
            {
                return;
            }

            announcement.IsActive = false;
        }

        public void ActivateAnnouncement(int announcementId)
        {
            Announcement? announcement = _announcements
                .FirstOrDefault(item => item.Id == announcementId);

            if (announcement == null)
            {
                return;
            }

            announcement.IsActive = true;
        }
    }
}