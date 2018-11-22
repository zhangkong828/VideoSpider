using System;
using System.Collections.Generic;
using System.Text;
using VideoSpider.Model;

namespace VideoSpider.Repository
{
    public class VideoRepository : RepositoryBase<Video>
    {
        public VideoRepository() : base("Video")
        {
        }
    }
}
