using System;
using System.Collections.Generic;
using System.Text;
using VideoSpider.Model;

namespace VideoSpider.Repository
{
    public class VideoSourceRepository : RepositoryBase<VideoSource>
    {
        public VideoSourceRepository() : base("VideoSource")
        {
        }
    }
}
