using System;
using VideoSpider.Services;

namespace VideoSpider
{
    class Program
    {
        static void Main(string[] args)
        {
            SpiderService.Create().Start();
            Console.ReadKey();
        }
    }
}
