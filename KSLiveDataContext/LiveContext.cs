using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KSLiveDataContext
{
    public class LiveContext
    {
        public class JsonData
        {
            private LiveContext context;
            private dynamic json;
            public JsonData(LiveContext context)
            {
                this.context = context;
                var s = context.GetContextJson();
                s.Wait();
                json = s.Result;

            }
            public bool IsLiving { get => json.liveroom.isLiving; }
            public String LiveUserName { get => json.liveroom.author.name; }
            public String LiveUserText { get => json.liveroom.author.description; }
            public String LiveUserImage { get => json.liveroom.author.avatar; }
            public String ID { get => json.liveroom.author.id; }


            public String LiveRoomCover { get => json.liveroom.liveStream.coverUrl; }
            public String LiveRoomText { get => json.liveroom.liveStream.caption; }
            public String LiveRoomUrl { get => json.liveroom.liveStream.playUrls[0].adaptationSet.representation[0].url; }

        }
        public class VideoContext
        {
            private LiveContext context { get; set; }
            public VideoContext(LiveContext context)
            {
                this.context = context;
            }
            public Task ToDownloadAction(Func<byte[],bool> Downloaded)
            {
                return Task.Run(() =>
                {
                    var s = context.GetVideoStream();
                    s.Wait();
                    var stream = s.Result;
                    long read = 0;
                    byte[] bs = new byte[1024*500];
                    while ((read = stream.Read(bs, 0, bs.Length)) > 0)
                    {
                        byte[] bss = new byte[read];
                        Array.Copy(bs, bss, read);
                        var b = Downloaded.Invoke(bss);
                        Array.Clear(bss, 0, bss.Length);
                        if (!b) break;
                    }
                });
            }
        }
        private String UID { get; set; }
        private String LiveUrl { get => "https://live.kuaishou.com/u/" + UID; }
        private HttpClient http = new HttpClient();
        private JsonData _Json = null;
        public JsonData Json { get => _Json ?? ((_Json = new JsonData(this)).IsLiving ? _Json : (_Json = null)); }
        private VideoContext _Video { get; set; }
        public VideoContext Video { get => _Video ?? (_Json == null ? null : (_Video = new VideoContext(this))); }
        public LiveContext(String UID)
        {
            this.UID = UID;
        }
        public void Update()
        {
            this._Json = null;
            this._Video = null;
        }
        public Task<dynamic?> GetContextJson()
        {
            return Task.Run(() =>
            {
                var htmlSucc = http.GetStringAsync(LiveUrl);
                htmlSucc.Wait();
                var html = htmlSucc.Result;
                var math = Regex.Match(html, "__INITIAL_STATE__=.*interestMaskStore.*};");
                var val = math.Value;
                if (val == "") return null;
                string sub = val.Substring("__INITIAL_STATE__=".Length, val.Length - "__INITIAL_STATE__".Length - 2);
                return JsonConvert.DeserializeObject(sub);
            });
        }
        public Task<Stream> GetVideoStream()
        {
            var url = Json.LiveRoomUrl;
            return http.GetStreamAsync(url);
        }
    }
}
