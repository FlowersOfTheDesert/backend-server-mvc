namespace backend_server_mvc.Model.Shade
{
    public class WeatherShadeConfig : BaseShadeConfig
    {
        public float Longitude { get; set; }
        public float Latitude { get; set; }
        public float OpenShadeThresholdTemp { get; set; }
        public float CloseShadeThresholdTemp { get; set; }

    }
}
