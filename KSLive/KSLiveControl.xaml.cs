using KSLiveDataContext;
using LibVLCSharp.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KSLive
{
    /// <summary>
    /// KSLiveControl.xaml 的交互逻辑
    /// </summary>
    public partial class KSLiveControl : UserControl
    {


        public String UID
        {
            get { return (String)GetValue(UIDProperty); }
            set { SetValue(UIDProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UID.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UIDProperty =
            DependencyProperty.Register("UID", typeof(String), typeof(KSLiveControl), new PropertyMetadata(null));



        public bool LastConvertToMp4
        {
            get { return (bool)GetValue(LastConvertToMp4Property); }
            set { SetValue(LastConvertToMp4Property, value); }
        }

        // Using a DependencyProperty as the backing store for LastConvertToMp4.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LastConvertToMp4Property =
            DependencyProperty.Register("LastConvertToMp4", typeof(bool), typeof(KSLiveControl), new PropertyMetadata(true));



        public bool IsNowSaveingLive { get; private set; }
        public LiveContext Context { get; set; }
        public FileInfo SaveFile { get => new FileInfo("./SaveVideos/[" +Context.Json.LiveUserName+"]"+ UID + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".flv"); }
        private FileInfo LastSaveFile { get; set; }
        private FileStream LastSaveFileWrite { get; set; }
        private LibVLC VLC = new LibVLC();
        public KSLiveControl()
        {
            InitializeComponent();
            SizeChanged += KSLiveControl_SizeChanged;
        }
        
        private void KSLiveControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            {
                Width = Height * 0.56;
            }
        }

        public void VideoSaveingChange()
        {
            IsNowSaveingLive = !IsNowSaveingLive;
            if (IsNowSaveingLive)
            {
                LastSaveFile = SaveFile;
                LastSaveFile.Directory.Create();
                //LastSaveFileWrite = LastSaveFile.CreateText();
                LastSaveFileWrite = LastSaveFile.Create();
                Context.Video.ToDownloadAction(DownloadVideoStream);
            }
            IsNowSaveingLive = IsNowSaveingLive;
        }
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == UIDProperty)
            {
                Context = new LiveContext(UID);
                if (Context.Json == null) return;
                if (VIEW.MediaPlayer == null)
                {
                    VIEW.MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(new Media(VLC, Context.Json.LiveRoomUrl, FromType.FromLocation));
                }
                else
                {
                    VIEW.MediaPlayer.Media = new Media(VLC, Context.Json.LiveRoomUrl, FromType.FromLocation);
                }
                VIEW.MediaPlayer.Play();
            }
            base.OnPropertyChanged(e);
        }
        private bool DownloadVideoStream(byte[] bs)
        {
            if (!IsNowSaveingLive)
            {
                LastSaveFileWrite.Close();
                LastSaveFileWrite.Dispose();
                bool Convert = false;
                Dispatcher.Invoke(() => Convert = LastConvertToMp4);
                if (Convert)
                {
                    string path = LastSaveFile.Directory.FullName+"\\"+LastSaveFile.Name;
                    string cmd = "-i "+path+" -c:v copy -c:a copy "+path+".mp4";
                   var p = Process.Start(new ProcessStartInfo("./ffmpeg.exe", cmd)
                    {
                        CreateNoWindow=true,
                    });
                    p.WaitForExit();
                    LastSaveFile.Delete();
                }
                return false;
            }

            LastSaveFileWrite.Write(bs);
            return IsNowSaveingLive;
        }
    }
}
