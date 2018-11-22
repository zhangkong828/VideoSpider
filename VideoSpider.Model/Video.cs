using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VideoSpider.Model
{
    public class Video
    {
        public long Id { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateTime { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime UpdateTime { get; set; }


        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 别名
        /// </summary>
        public string Alias { get; set; }
        /// <summary>
        /// 封面
        /// </summary>
        public string Image { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 简介
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 主演
        /// </summary>
        public string Starring { get; set; }
        /// <summary>
        /// 导演
        /// </summary>
        public string Director { get; set; }
        /// <summary>
        /// 栏目分类
        /// </summary>
        public string Classify { get; set; }
        /// <summary>
        /// 影片类型
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 语言分类
        /// </summary>
        public string Language { get; set; }
        /// <summary>
        /// 影片地区
        /// </summary>
        public string Region { get; set; }
        /// <summary>
        /// 连载状态
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// 上映年份
        /// </summary>
        public string ReleaseDate { get; set; }
        /// <summary>
        /// 原始更新时间
        /// </summary>
        public string OriginalUpdateTime { get; set; }
       
       
    }
}
