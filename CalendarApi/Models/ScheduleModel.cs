namespace CalendarApi.Models
{
    public class ScheduleModel
    {
        public class TourData
        {
            public List<string> ByTour { get; set; }
            public List<DateEntry> Dates { get; set; }
        }

        public class DateEntry
        {
            public string Tour { get; set; }
            public string Location { get; set; }
            public List<string> Date { get; set; }
            public string ToDate { get; set; }
            public WeekSchedule Week { get; set; }
            public List<NextDateEntry> NextDates { get; set; }
        }

        public class WeekSchedule
        {
            public bool Sunday { get; set; }
            public bool Monday { get; set; }
            public bool Tuesday { get; set; }
            public bool Wednesday { get; set; }
            public bool Thursday { get; set; }
            public bool Friday { get; set; }
            public bool Saturday { get; set; }
        }

        public class NextDateEntry
        {
            public string startDate { get; set; }
            public string endDate { get; set; }
            public WeekSchedule week { get; set; }
        }

    }
}
