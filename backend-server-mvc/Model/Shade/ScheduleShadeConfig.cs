namespace backend_server_mvc.Model.Shade
{
    public class ScheduleShadeConfig : BaseShadeConfig
    {
        public TimeOnly OpenTime { get; set; }
        public TimeOnly CloseTime { get; set; }
        public string Timezone { get; set; }

    }
}
