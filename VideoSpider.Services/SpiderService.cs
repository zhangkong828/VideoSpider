namespace VideoSpider.Services
{
    public class SpiderService
    {
        private static readonly object _obj = new object();
        private static SpiderService _instance;

        private SpiderService()
        {

        }

        public static SpiderService Create()
        {
            if (_instance == null)
            {
                lock (_obj)
                {
                    if (_instance == null)
                    {
                        _instance = new SpiderService();
                    }
                }
            }
            return _instance;
        }


        public void Start()
        {

        }

        public void Stop()
        {

        }
    }
}
