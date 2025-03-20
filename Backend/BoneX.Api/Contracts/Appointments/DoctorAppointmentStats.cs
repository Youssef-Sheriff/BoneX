namespace BoneX.Api.Contracts.Appointments;

public class DoctorAppointmentStats
{
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public int TodayAppointments { get; set; }
    public int ThisWeekAppointments { get; set; }
}
