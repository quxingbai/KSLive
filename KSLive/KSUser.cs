using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSLive
{
    public class KSUser
    {
        public String Name { get; set; }
        public String UID { get; set; }
    }
    public class KSUserManager
    {

        private static FileInfo DataFile = new FileInfo("./UserListData.txt");
        //public static List<KSUser> UserList = new List<KSUser>();
        public static ObservableCollection<KSUser> UserList = new ObservableCollection<KSUser>();
        static KSUserManager()
        {
            Reload();
        }
        private static KSUser[] Users()
        {
            if (!DataFile.Exists)
            {
                Save();
            }
            var fs = DataFile.OpenText();
            var data = JsonConvert.DeserializeObject<KSUser[]>(fs.ReadToEnd());
            fs.Close();
            fs.Dispose();
            return data;
        }
        public static void Reload()
        {
            if (!DataFile.Exists)
            {
                //var c = DataFile.CreateText();
                //c.WriteLine("[]");
                //c.Close();
                //c.Dispose();
            }
            UserList.Clear();
            var us = Users();
            foreach (var i in us)
            {
                UserList.Add(i);
            }
            //UserList.AddRange(us);
        }
        public static void Save()
        {
            DataFile.Directory.Create();
            var open = DataFile.CreateText();
            open.WriteLine(JsonConvert.SerializeObject(UserList));
            open.Close();
            open.Dispose();
        }
    }
}
