namespace online_school_admin.ViewModels;

public sealed class NotificationsViewModel : BaseViewModel
{
    public NotificationsTabViewModel Notifications { get; }
    public MailingCampaignsTabViewModel Campaigns { get; }

    public NotificationsViewModel(NotificationsTabViewModel notifications, MailingCampaignsTabViewModel campaigns)
    {
        Notifications = notifications;
        Campaigns = campaigns;
    }
}

