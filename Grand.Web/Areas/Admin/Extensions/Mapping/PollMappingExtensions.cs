using Grand.Domain.Polls;
using Grand.Services.Helpers;
using Grand.Web.Areas.Admin.Models.Polls;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class PollMappingExtensions
    {
        //poll items
        public static PollModel ToModel(this Poll entity, IDateTimeHelper dateTimeHelper)
        {
            var poll = entity.MapTo<Poll, PollModel>();
            poll.StartDate = entity.StartDateUtc.ConvertToUserTime(dateTimeHelper);
            poll.EndDate = entity.EndDateUtc.ConvertToUserTime(dateTimeHelper);
            return poll;
        }

        public static Poll ToEntity(this PollModel model, IDateTimeHelper dateTimeHelper)
        {
            var poll = model.MapTo<PollModel, Poll>();
            poll.StartDateUtc = model.StartDate.ConvertToUtcTime(dateTimeHelper);
            poll.EndDateUtc = model.EndDate.ConvertToUtcTime(dateTimeHelper);
            return poll;

        }

        public static Poll ToEntity(this PollModel model, Poll destination, IDateTimeHelper dateTimeHelper)
        {
            var poll = model.MapTo(destination);
            poll.StartDateUtc = model.StartDate.ConvertToUtcTime(dateTimeHelper);
            poll.EndDateUtc = model.EndDate.ConvertToUtcTime(dateTimeHelper);
            return poll;

        }

        //poll answer
        public static PollAnswerModel ToModel(this PollAnswer entity)
        {
            return entity.MapTo<PollAnswer, PollAnswerModel>();
        }

        public static PollAnswer ToEntity(this PollAnswerModel model)
        {
            return model.MapTo<PollAnswerModel, PollAnswer>();
        }

        public static PollAnswer ToEntity(this PollAnswerModel model, PollAnswer destination)
        {
            return model.MapTo(destination);
        }
    }
}