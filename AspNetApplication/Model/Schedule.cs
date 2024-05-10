namespace AspNetApplication.Model
{
    public class Schedule
    {
        public int schedule_id { get; set; }
        public string subject_name { get; set; }
        public string teacher_name { get; set; }
        public string day_week { get; set; }
        public string started_time { get; set; }
        public string ending_time { get; set; }
    }
}
