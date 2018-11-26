using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VideoSpider.Cache;
using VideoSpider.Infrastructure;
using VideoSpider.Infrastructure.Extension;
using VideoSpider.Model;
using VideoSpider.Repository;

namespace VideoSpider.Services
{
    public class SpiderService
    {
        private static readonly object _obj = new object();
        private static SpiderService _instance;
        private static readonly CancellationTokenSource _cancellation = new CancellationTokenSource();

        private bool isRunning = false;
        private CancellationToken _cancellationToken;
        private List<Task> _tasks;
        private ConcurrentQueue<string> _queue;
        private VideoRepository _videoRepository;
        private VideoSourceRepository _videoSourceRepository;

        private ICacheManager _cache;
        private const int CacheTime = 60 * 24;//1天

        private SpiderService()
        {
            _cancellationToken = _cancellation.Token;
            _tasks = new List<Task>();
            _queue = new ConcurrentQueue<string>();
            _videoRepository = new VideoRepository();
            _videoSourceRepository = new VideoSourceRepository();

            _cache = new MemoryCacheManager();
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
            isRunning = true;
            Logger.ColorConsole("Start");
            StartThread(5);
            Monitor();

        }

        public void Stop()
        {
            Logger.ColorConsole("Thread Cancel...");
            _cancellation.Cancel();
            Task.WaitAll(_tasks.ToArray());
            Logger.ColorConsole("Thread Stop");

            isRunning = false;
        }

        private void StartThread(int threadCount)
        {
            for (int i = 0; i < threadCount; i++)
            {
                Logger.ColorConsole(string.Format("开启线程[{0}]", i + 1));

                var task = Task.Factory.StartNew(index =>
                {
                    Logger.ColorConsole(string.Format("线程[{0}]已启动,线程ID:{1}", index, Thread.CurrentThread.ManagedThreadId));

                    while (true)
                    {
                        try
                        {
                            _cancellationToken.ThrowIfCancellationRequested();
                            var url = string.Empty;
                            if (_queue.TryDequeue(out url))
                            {
                                //Logger.ColorConsole(string.Format("线程[{0}]处理数据:{1}", index, url));
                                DoWork(url);
                            }
                            else
                            {
                                Thread.Sleep(1000 * 10);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex is OperationCanceledException)
                                break;
                            else
                                Logger.ColorConsole(string.Format("线程[{0}]出现异常:{1}", index, ex.Message), ConsoleColor.Red);
                        }
                        finally
                        {
                            Thread.Sleep(100);
                        }
                    }


                }, i + 1);
                _tasks.Add(task);
            }
        }

        private void Monitor()
        {
            while (true)
            {
                CheckUpdate();
                Thread.Sleep(1000 * 60 * 5);
            }
        }

        private void CheckUpdate()
        {
            var goTo = true;
            Logger.ColorConsole("开始获取更新...");
            var url = "http://caijizy.com/?m=vod-index-pg-{0}.html";
            for (int i = 1; i < 100; i++)
            {
                if (!goTo)
                    break;

                Logger.ColorConsole2(string.Format("当前页码{0}", i));
                Logger.Info(string.Format("当前页码{0}", i));
                try
                {
                    var targetUrl = string.Format(url, i);
                    //Logger.ColorConsole2(targetUrl);

                    var html = HtmlHelper.Get(targetUrl).Result;
                    var list = Regex.Matches(html, "<td class=\"l\"><a href=\"(.+?)\" target=\"_blank\">(.+?)<font color=\"red\"> \\[(.*?)\\]</font>[\\s\\S]*?<font color=\"#2932E1\">(.+?)</font>");
                    foreach (Match item in list)
                    {
                        var dUrl = item.Groups[1].Value;
                        var dName = item.Groups[2].Value;
                        var dRemark = item.Groups[3].Value;
                        var time = item.Groups[4].Value.TrimX().ToDateTime();
                        if (!string.IsNullOrWhiteSpace(dUrl) && time != DateTime.MinValue)
                        {
                            if (time.AddMinutes(10) > DateTime.Now.Date)
                            {
                                dUrl = "http://caijizy.com" + dUrl;
                                if (!_cache.IsExist(dUrl.TrimX()))
                                {
                                    Logger.ColorConsole2(string.Format("{0}[{1}]", dName, dRemark));
                                    _queue.Enqueue(dUrl);
                                    continue;
                                }
                                else
                                    Logger.ColorConsole2("今日已存在");
                            }
                            else
                                Logger.ColorConsole2("今日已更新");
                        }
                        goTo = false;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.ColorConsole2(string.Format("获取更新异常:{0}", ex.Message), ConsoleColor.Red);
                }
                finally
                {
                    if (goTo)
                        Thread.Sleep(1000);
                }
            }

            Logger.ColorConsole("等待下次更新...");
        }


        private void DoWork(string url)
        {
            var html = HtmlHelper.Get(url).Result;
            if (!string.IsNullOrEmpty(html))
            {
                //图片
                var image = Regex.Match(html, "<!--图片开始--><img src=\"(.*?)\"/><!--图片结束-->").Groups[1].Value.TrimX();
                //名称
                var name = Regex.Match(html, "<!--片名开始-->(.*?)<!--片名结束-->").Groups[1].Value.TrimX();
                //别名
                var alias = Regex.Match(html, "<!--别名开始-->(.*?)<!--别名结束-->").Groups[1].Value.TrimX();
                //备注
                var remark = Regex.Match(html, "<!--备注开始-->(.*?)<!--备注结束-->").Groups[1].Value.TrimX();
                //主演
                var starring = Regex.Match(html, "<!--主演开始-->(.*?)<!--主演结束-->").Groups[1].Value.TrimX();
                //导演
                var director = Regex.Match(html, "<!--导演开始-->(.*?)<!--导演结束-->").Groups[1].Value.TrimX();
                //栏目分类
                var classify = Regex.Match(html, "<!--栏目开始-->(.*?)<!--栏目结束-->").Groups[1].Value.TrimX();
                //影片类型
                var type = Regex.Match(html, "<!--类型开始-->(.*?)<!--类型结束-->").Groups[1].Value.TrimX();
                //语言分类
                var language = Regex.Match(html, "<!--语言开始-->(.*?)<!--语言结束-->").Groups[1].Value.TrimX();
                //影片地区
                var region = Regex.Match(html, "<!--地区开始-->(.*?)<!--地区结束-->").Groups[1].Value.TrimX();
                //连载状态
                var state = Regex.Match(html, "<!--连载开始-->(.*?)<!--连载结束-->").Groups[1].Value.TrimX();
                //上映年份
                var releaseDate = Regex.Match(html, "<!--年代开始-->(.*?)<!--年代结束-->").Groups[1].Value.TrimX();
                //更新时间
                var updateTime = Regex.Match(html, "<!--时间开始-->(.*?)<!--时间结束-->").Groups[1].Value.TrimX();
                //简介
                var description = Regex.Match(html, "<!--简介开始-->(.*?)<!--简介结束-->").Groups[1].Value.TrimX();

                var sourceList = new List<Source>();
                //jsm3u8
                var jsm3u8 = Regex.Match(html, "<!--前jsm3u8-->(.*?)<!--后jsm3u8-->").Groups[1].Value;
                var list = Regex.Matches(jsm3u8, "<input.+?value=\"(.+?)\\$(.+?)\".+?>");
                foreach (Match item in list)
                {
                    var title = item.Groups[1].Value;
                    var address = item.Groups[2].Value;
                    //Logger.ColorConsole2(string.Format("{0}[{1}]{2}", name, title, address), ConsoleColor.Green);
                    sourceList.Add(new Source()
                    {
                        Title = title,
                        Address = address
                    });
                }

                //非空校验
                if (string.IsNullOrEmpty(name) || sourceList.Count == 0)
                    return;

                var source = _videoSourceRepository.FindOrDefault(x => x.Url == url.TrimX());
                if (source == null)
                {
                    //新增
                    var video = new Video()
                    {
                        Id = ObjectId.NextId(),
                        CreateTime = DateTime.Now,
                        UpdateTime = DateTime.Now,
                        Name = name,
                        Alias = alias,
                        Image = DownLoadImageToBase64(image),
                        Remark = remark,
                        Description = description,
                        Starring = starring,
                        Director = director,
                        Classify = classify,
                        Type = type,
                        Language = language,
                        Region = region,
                        State = state,
                        ReleaseDate = releaseDate
                    };
                    _videoRepository.Insert(video);

                    var nSource = new VideoSource();
                    nSource.Id = ObjectId.NextId();
                    nSource.VideoId = video.Id;
                    nSource.SourceName = "jsm3u8";
                    nSource.LastUpdateTime = updateTime.ToDateTime();
                    nSource.Url = url.TrimX();
                    nSource.Sources = sourceList;
                    nSource.CreateTime = DateTime.Now;
                    nSource.UpdateTime = DateTime.Now;
                    _videoSourceRepository.Insert(nSource);

                    Logger.ColorConsole(string.Format("新增成功:{0}[{1}]", name, remark), ConsoleColor.DarkGreen);
                    Logger.Info(string.Format("新增成功:{0}[{1}]{2}", name, remark, url));
                }
                else
                {
                    //更新
                    var vid = source.VideoId;
                    var video = _videoRepository.FindOrDefault(x => x.Id == vid);
                    if (video != null)
                    {
                        _videoRepository.Update(x => x.Id == video.Id, new Video()
                        {
                            Id = video.Id,
                            CreateTime = video.CreateTime,
                            UpdateTime = DateTime.Now,
                            Name = name,
                            Alias = alias,
                            Image = DownLoadImageToBase64(image),
                            Remark = remark,
                            Description = description,
                            Starring = starring,
                            Director = director,
                            Classify = classify,
                            Type = type,
                            Language = language,
                            Region = region,
                            State = state,
                            ReleaseDate = releaseDate
                        });
                        _videoSourceRepository.Update(x => x.Id == source.Id, new VideoSource()
                        {
                            Id = source.Id,
                            VideoId = source.VideoId,
                            SourceName = "jsm3u8",
                            LastUpdateTime = updateTime.ToDateTime(),
                            Url = url.TrimX(),
                            Sources = sourceList,
                            CreateTime = source.CreateTime,
                            UpdateTime = DateTime.Now
                        });
                        Logger.ColorConsole(string.Format("更新成功:{0}[{1}]", name, remark), ConsoleColor.DarkGreen);
                        Logger.Info(string.Format("更新成功:{0}[{1}]{2}", name, remark, url));
                    }
                    else
                    {
                        _videoSourceRepository.Delete(x => x.Id == source.Id);
                        Logger.ColorConsole(string.Format("更新失败:{0}[{1}]", name, remark), ConsoleColor.Red);
                        Logger.Info(string.Format("更新失败:{0}[{1}]{2}", name, remark, url));
                    }
                }
                _cache.Set(url, name, CacheTime);
            }
        }


        private string DownLoadImageToBase64(string url)
        {
            var bytes = HtmlHelper.DownLoad(url);
            if (bytes != null)
                return Convert.ToBase64String(bytes);
            else
                return "";
        }

    }
}
