﻿using KSLiveDataContext;
using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Core.Initialize();
            //var context=new KSLiveDataContext.LiveContext("sf1413191");
            //var b = context.Json.LiveRoomUrl;
            Application.Current.DispatcherUnhandledException += (ss, ee) =>
            {
                ee.Handled = true;
                MessageBox.Show("发生错误\n" + ee.Exception.ToString());
            };
        }
        public void ReloadListUsers()
        {
        }
        //protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        //{
        //    Title = "保存...";
        //    KSLive.VideoSaveingChange();
        //    base.OnPreviewMouseDoubleClick(e);
        //}

        private void BT_ChangeLiveRoom_Click(object sender, RoutedEventArgs e)
        {
            var uid = TEXT_UID.Text;
            if (uid == "") return;
            //LiveContext context = new LiveContext(uid);
            //KSLive.Context?.Update();
            if (KSLive.IsNowSaveingLive)
            {
                MessageBox.Show("目前正在录制中无法切换直播间");
            }
            else
            {

                KSLive.UID = uid;
                BD_LiveInfo.DataContext = KSLive.Context;
            }
            //else if (context.Json == null)
            //{
            //    MessageBox.Show("切换失败 直播间可能不存在");
            //}
            //else if (!context.Json.IsLiving)
            //{
            //    MessageBox.Show("直播间未开启");
            //}
            //else
            //{
            //    KSLive.UID = uid;
            //    BD_LiveInfo.DataContext = KSLive.Context;
            //}
        }

        private void BT_LiveSaveStart_Click(object sender, RoutedEventArgs e)
        {
            if (KSLive.Context == null)
            {
                MessageBox.Show("直播间不存在");
                return;
            }
            KSLive.Context.Update();
            if (KSLive.Context.Json == null)
            {
                MessageBox.Show("直播未开启");
                return;
            }
            if (KSLive.SaveTimeLength != null && KSLive.SaveTimeLength.Value.TotalSeconds < 1)
            {
                MessageBox.Show("录制时间过短 < 1分");
                return;
            }
            KSLive.VideoSaveingChange();
            SaveingChange(KSLive.IsNowSaveingLive);
        }
        public void SaveingChange(bool state)
        {
            this.Title = state ? "录制中..." : "KSLive";
            BT_LiveSaveStart.Content = state ? "结束录制 0:0:0" : "录制";
            BT_LiveSaveStart.Foreground = state ? Brushes.OrangeRed : Brushes.Black;
        }

        private void BT_OpenLiveSaveFolder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", KSLive.SaveDirectory.FullName);
        }

        private void LIST_SeelectLiveUserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BT_SeelectLiveUserList.IsChecked = false;
            if (LIST_SeelectLiveUserList.SelectedItem == null) return;
            KSUser user = LIST_SeelectLiveUserList.SelectedItem as KSUser;
            TEXT_UID.Text = user.UID;
            LIST_SeelectLiveUserList.SelectedIndex = -1;
        }

        private void BT_Input_Click(object sender, RoutedEventArgs e)
        {
            var uid = TEXT_InputUserID.Text;
            var uname = TEXT_InputUserName.Text;
            if (uid == "" || uname == "")
            {
                MessageBox.Show("不能为空");
                return;
            }
            TEXT_InputUserID.Text = "";
            TEXT_InputUserName.Text = "";
            KSUser u = new KSUser() { Name = uname, UID = uid };
            KSUserManager.UserList.Add(u);
            KSUserManager.Save();
            KSUserManager.Reload();
        }

        private void BT_UserDel_Click(object sender, RoutedEventArgs e)
        {
            Button bt = (sender as Button);
            KSUserManager.UserList.Remove(bt.DataContext as KSUser);
            KSUserManager.Save();
            KSUserManager.Reload();
        }

        private void Popup_MouseLeave(object sender, MouseEventArgs e)
        {
            BT_SeelectLiveUserList.IsChecked = false;
        }

        private void BT_SeelectLiveUserList_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!Popup_UserSelect.IsMouseOver && !TEXT_UID.IsMouseOver)
                BT_SeelectLiveUserList.IsChecked = false;
        }

        private void BT_Topmost_Click(object sender, RoutedEventArgs e)
        {
            Topmost = !Topmost;
            BT_Topmost.Content = Topmost ? "取消顶层" : "窗口顶层";
            BT_Topmost.Foreground = Topmost ? Brushes.DodgerBlue : Brushes.Black;
        }

        private void KSLive_LiveVideoSaveing(KSLiveControl c,byte[] arg1, TimeSpan arg2)
        {
            Dispatcher.Invoke(() =>
            {
                BT_LiveSaveStart.Content = "结束录制 "+arg2.Hours+":"+arg2.Minutes+":"+arg2.Seconds;
            });
        }
        private void KSLive_LiveVideoSaved_1(KSLiveControl arg1,TimeSpan arg3)
        {
            Dispatcher.Invoke(() =>
            {
                if (KSLive.IsNowSaveingLive)
                {
                    KSLive.VideoSaveingChange();
                }
                SaveingChange(KSLive.IsNowSaveingLive);
                //BT_LiveSaveStart.Content = "录制";
            });
        }

        private void KSLive_LiveVideoSaveStart(KSLiveControl obj)
        {
            SaveingChange(true);
        }

        private void SLIDER_Volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(this.KSLive.VIEW.MediaPlayer!=null)
            this.KSLive.VIEW.MediaPlayer.Volume = (int)e.NewValue;
        }
    }
}
