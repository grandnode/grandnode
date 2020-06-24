namespace Grand.Services.Logging.ActivityLogComment
{
    public interface IActivityEntityKeywordsProvider
    {
        ActivityLogEntity GetLogEntity(string activityKeyword);
    }
}
