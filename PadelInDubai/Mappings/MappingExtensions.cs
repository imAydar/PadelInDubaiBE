using PadelInDubai.DAL.Entities;
using PadelInDubai.Models;
using PadelInDubai.Models.Dtos;

namespace PadelInDubai.Mappings
{
    public static class MappingExtensions
    {
        private const int _gamesId = 10759477;
        private const int _trainsId = 10761747;

        public static EventData ToModel(this DAL.Entities.Event dalEvent)
        {
            if (dalEvent == null)
                return null;

            return new EventData
            {
                Id = dalEvent.Id,
                ServiceId = dalEvent.ServiceId,
                CompanyId = dalEvent.CompanyId,
                StaffId = dalEvent.StaffId,
                Date = dalEvent.Date,
                Timestamp = dalEvent.Timestamp,
                Length = dalEvent.Length,
                Capacity = dalEvent.Capacity,
                Comment = dalEvent.Comment,
                RecordsCount = dalEvent.RecordsCount,
                Staff = dalEvent.Staff.ToModel(),
                Service = dalEvent.Service.ToModel()
            };
        }

        public static DAL.Entities.Event ToEntity(this EventData modelEvent)
        {
            if (modelEvent == null)
                return null;

            return new DAL.Entities.Event
            {
                Id = modelEvent.Id,
                ServiceId = modelEvent.ServiceId,
                CompanyId = modelEvent.CompanyId,
                StaffId = modelEvent.StaffId,
                Date = modelEvent.Date,
                Timestamp = modelEvent.Timestamp,
                Length = modelEvent.Length,
                Capacity = modelEvent.Capacity,
                Comment = modelEvent.Comment,
                RecordsCount = modelEvent.RecordsCount,
                Staff = modelEvent.Staff.ToEntity(),
                Service = modelEvent.Service.ToEntity(),
            };
        }

        public static Models.StaffData ToModel(this DAL.Entities.Staff dalStaff)
        {
            if (dalStaff == null)
                return null;

            return new Models.StaffData
            {
                Id = dalStaff.Id,
                Name = dalStaff.Name,
                CompanyId = dalStaff.CompanyId,
                Specialization = dalStaff.Specialization,
                Rating = dalStaff.Rating,
                Avatar = dalStaff.Avatar,
                AvatarBig = dalStaff.AvatarBig
            };
        }

        public static DAL.Entities.Staff ToEntity(this Models.StaffData modelStaff)
        {
            if (modelStaff == null)
                return null;

            return new DAL.Entities.Staff
            {
                Id = modelStaff.Id,
                Name = modelStaff.Name,
                CompanyId = modelStaff.CompanyId,
                Specialization = modelStaff.Specialization,
                Rating = modelStaff.Rating,
                Avatar = modelStaff.Avatar,
                AvatarBig = modelStaff.AvatarBig
            };
        }

        public static Models.ServiceData ToModel(this DAL.Entities.Service dalService)
        {
            if (dalService == null)
                return null;

            return new Models.ServiceData
            {
                Id = dalService.Id,
                Title = dalService.Title,
                ImageUrl = dalService.ImageUrl,
                CategoryId = dalService.CategoryId,
                SalonServiceId = dalService.SalonServiceId,
                Comment = dalService.Comment,
                PriceMin = dalService.PriceMin,
                PriceMax = dalService.PriceMax,
                Category = dalService.Category.ToModel()
            };
        }

        public static DAL.Entities.Service ToEntity(this Models.ServiceData modelService)
        {
            if (modelService == null)
                return null;

            return new DAL.Entities.Service
            {
                Id = modelService.Id,
                Title = modelService.Title,
                ImageUrl = modelService.ImageUrl,
                CategoryId = modelService.CategoryId,
                SalonServiceId = modelService.SalonServiceId,
                Comment = modelService.Comment,
                PriceMin = modelService.PriceMin,
                PriceMax = modelService.PriceMax,
                Category = modelService.Category.ToEntity()
            };
        }

        public static Models.CategoryData ToModel(this DAL.Entities.Category dalCategory)
        {
            if (dalCategory == null)
                return null;

            return new Models.CategoryData
            {
                Id = dalCategory.Id,
                Title = dalCategory.Title,
                CategoryId = dalCategory.CategoryId,
                IsCategory = dalCategory.IsCategory,
                SalonServiceId = dalCategory.SalonServiceId
            };
        }

        public static DAL.Entities.Category ToEntity(this Models.CategoryData modelCategory)
        {
            if (modelCategory == null)
                return null;

            return new DAL.Entities.Category
            {
                Id = modelCategory.Id,
                Title = modelCategory.Title,
                CategoryId = modelCategory.CategoryId,
                IsCategory = modelCategory.IsCategory,
                SalonServiceId = modelCategory.SalonServiceId
            };
        }



        public static Record ToEntity(this WebhookRequest request)
        {
            if (request?.Body?.Data == null)
                return null;

            var record = request.Body.Data.ToEntity();
            // Optionally map WebhookBody.Status to the DAL Record.Status property.
            record.Status = request.Body.Status;
            return record;
        }

        public static Record ToEntity(this RecordData data)
        {
            if (data == null)
                return null;

            return new Record
            {
                Id = data.Id,
                CompanyId = data.CompanyId,
                StaffId = data.StaffId,
                Services = data.Services?.Select(s => s.ToEntity()).ToList(),
                Staff = data.Staff?.ToEntity(),
                Client = data.Client?.ToEntity(data.Comment),
                ClientsCount = data.ClientsCount,
                Date = data.Date,
                DateTime = data.DateTime,
                CreateDate = data.CreateDate,
                Comment = data.Comment,
                Online = data.Online,
                Confirmed = data.Confirmed,
                Notified = data.Notified,
                FromUrl = data.FromUrl,
                VisitId = data.VisitId,
                CreatedUserId = data.CreatedUserId,
                Deleted = data.Deleted,
                PaidFull = data.PaidFull,
                Prepaid = data.Prepaid,
                PrepaidConfirmed = data.PrepaidConfirmed,
                LastChangeDate = data.LastChangeDate,
                EventId = data.ActivityId,
                ClientId = data.Client?.Id
            };
        }

        public static DAL.Entities.Client ToEntity(this Models.ClientData modelClient, string comment)
        {
            if (modelClient == null)
                return null;

            var name = modelClient.Name.Trim();
            var initial = !string.IsNullOrWhiteSpace(modelClient?.Surname)
                ? char.ToUpper(modelClient.Surname.Trim()[0]) + "."
                : string.Empty;

            var dispName = string.IsNullOrEmpty(initial) ? name : $"{name} {initial}";

            var categoryLevel = modelClient.ClientTags?.FirstOrDefault()?.Title;

            return new DAL.Entities.Client
            {
                Id = modelClient.Id,
                Name = dispName,
                Phone = modelClient.Phone,
                Email = modelClient.Email,
                SuccessVisitsCount = modelClient.SuccessVisitsCount,
                FailVisitsCount = modelClient.FailVisitsCount,
                Level = categoryLevel ?? comment
            };
        }

        public static EventDto ToDto(this Event evt)
        {
            var group = evt.Service?.CategoryId == _gamesId ? Group.Game :
                            evt.Service?.CategoryId == _trainsId ? Group.Train :
                                Group.Default;
            return new EventDto
            {
                Id = evt.Id,
                Date = evt.Date,
                Capacity = evt.Capacity,

                Title = evt.Service?.Title,
                PriceMin = evt.Service?.PriceMin ?? 0,
                PriceMax = evt.Service?.PriceMax ?? 0,
                Comment = evt.Service?.Comment,

                LocationName = evt.Staff?.Name,
                LocationUrl = evt.Staff?.LocationUrl,
                Picture = evt.Service?.ImageUrl ?? evt.Staff?.AvatarBig,
                RecordsCount = evt.RecordsCount,
                MessageId = evt.MessageId,
                TextHash = evt.TextHash,
                Group = group,
                Records = evt.Records?.Select(r => r.ToDto()).ToList()
            };
        }

        public static RecordData ToDto(this Record record)
        {
            return new RecordData
            {
                Id = record.Id,
                CompanyId = record.CompanyId,
                StaffId = record.StaffId,
                Services = record.Services?.Select(s => s.ToModel()).ToList(),

                Staff = record.Staff.ToModel(),

                Client = record.Client.ToDto(),

                ClientsCount = record.ClientsCount,
                Date = record.Date,
                DateTime = record.DateTime,
                CreateDate = record.CreateDate,
                Comment = record.Comment,
                Online = record.Online,
                Confirmed = record.Confirmed,
                Notified = record.Notified,
                FromUrl = record.FromUrl,
                VisitId = record.VisitId,
                CreatedUserId = record.CreatedUserId,
                Deleted = record.Deleted,
                PaidFull = record.PaidFull,
                Prepaid = record.Prepaid,
                PrepaidConfirmed = record.PrepaidConfirmed,
                LastChangeDate = record.LastChangeDate,
                ActivityId = record.EventId // Assuming EventId = ActivityId in DTO
            };
        }

        public static PadelInDubai.Models.ClientData ToDto(this DAL.Entities.Client client)
        {
            if (client == null) return null;

            return new Models.ClientData
            {
                DisplayName = client.Name,
                Email = client.Email,
                Id = client.Id,
                Name = client.Name,
                FailVisitsCount = client.FailVisitsCount,
                Phone = client.Phone,
                SuccessVisitsCount = client.SuccessVisitsCount,
                ClientTags = client.ClientTags?.Select(ct => ct.ToDto()).ToList(),
                Level = client.Level
            };
        }


        public static PadelInDubai.Models.ClientTagData ToDto(this DAL.Entities.ClientTag tag)
        {
            return new Models.ClientTagData
            {
                CompanyId = tag.CompanyId,
                EntitySlug = tag.EntitySlug,
                Id = tag.Id,
                IsDeleted = tag.IsDeleted,
                Title = tag.Title
            };
        }

            //public static Event ToEntity(this EventDto evt)
            //{
            //    return new EventDto
            //    {
            //        Id = evt.Id,
            //        Date = evt.Date,
            //        Capacity = evt.Capacity,

            //        Title = evt.Service?.Title,
            //        PriceMin = evt.Service?.PriceMin ?? 0,
            //        PriceMax = evt.Service?.PriceMax ?? 0,
            //        Comment = evt.Service?.Comment,

            //        LocationName = evt.Staff?.Name,
            //        Picture = evt.Staff?.AvatarBig,
            //        RecordsCount = evt.RecordsCount,
            //        MessageId = evt.MessageId,
            //        TextHash = evt.TextHash,
            //        Group = group
            //    };
            //}
        }
}

