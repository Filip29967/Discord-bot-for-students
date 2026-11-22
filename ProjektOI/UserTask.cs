namespace ProjektOI;

public class UserTask
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime Deadline { get; set; }
    public DateTime ReminderTime { get; set; }

    public bool ReminderSent { get; set; } = false;

    public ulong UserId { get; set; }
    public ulong ChannelId { get; set; }
}