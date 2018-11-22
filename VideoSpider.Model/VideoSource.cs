using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VideoSpider.Model
{
    public class VideoSource
    {
        public long Id { get; set; }
        public long VideoId { get; set; }
        /// <summary>
        /// 来源名称
        /// </summary>
        public string SourceName { get; set; }
        /// <summary>
        /// 来源最后更新时间
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime LastUpdateTime { get; set; }
        /// <summary>
        /// 来源url
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 视频源
        /// </summary>
        public List<Source> Sources { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateTime { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime UpdateTime { get; set; }
    }

    public class Source
    {
        public string Title { get; set; }
        public string Address { get; set; }
    }
}
