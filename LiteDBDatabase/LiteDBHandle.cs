using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteDBDatabase
{
    //For General Backup Offline Info which reading from Website
    public class BackupOfflineInfo
    {
        public int Id { get; set; }
        public string infoName { get; set; } //This Info is unique => we will query by this name
        public object value { get; set; } //All object will be saving under Json format!
        public DateTime LastUpdate { get; set;}
    }

    //For CFP basic information
    public class CFPSystemInfo
    {
        public int Id { get; set; }
        public bool isFirstRun { get; set; } //Indicate CFP is first run or not
        public string CFPVersion { get; set; } //Version of CFP
        public string ProtectionCode { get; set; } //CFP protection code
        public DateTime LastUpdate { get; set; } //Record Last time update information
    }

    public class LiteDBHandle
    {
        //Ini for CFP Offline database
        //Create Default LiteDB of CFP: "Database\MyDatabase.db" if not yet exist
        public void IniDatabase()
        {
            //Check if Database file already exist or not
            string databaseFilePath = AppDomain.CurrentDomain.BaseDirectory + @"Database\MyDatabase.db";
            if (System.IO.File.Exists(databaseFilePath) == true) return;

            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(@"Database\MyDatabase.db"))
            {
                //Ini for CFP System Information
                var collection = db.GetCollection<CFPSystemInfo>("CFPSystemInfos");
                var cfpSystemInfo = new CFPSystemInfo
                {
                    isFirstRun = true,
                    CFPVersion = "0.0.0.0",
                    ProtectionCode = "00000000-00000000",
                    LastUpdate = DateTime.Now
                };
                collection.Insert(cfpSystemInfo);
                collection.Update(cfpSystemInfo);

                //Ini for General Offline Backup Info
                var col = db.GetCollection<BackupOfflineInfo>("BackupOfflineInfos");
                var backupOfflineInfo = new BackupOfflineInfo
                {
                    infoName = "FirstRun",
                    value = "For Ini Database only!"
                };
                // Create unique index in Name field
                col.EnsureIndex(x => x.infoName, true);
                col.Insert(backupOfflineInfo);
                col.Update(backupOfflineInfo);
            }
        }

        //Update CFP System Info
        public bool UpdateCFPInfo(string version, string protectionCode)
        {
            try
            {
                using (var db = new LiteDatabase(@"Database\MyDatabase.db"))
                {
                    //Ini for CFP System Information
                    var collection = db.GetCollection<CFPSystemInfo>("CFPSystemInfos");
                    var cfpSystemInfo = collection.FindById(1);

                    cfpSystemInfo.CFPVersion = version;
                    cfpSystemInfo.ProtectionCode = protectionCode;
                    cfpSystemInfo.isFirstRun = false;
                    cfpSystemInfo.LastUpdate = DateTime.Now;

                    return collection.Update(cfpSystemInfo);
                }
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public CFPSystemInfo GetCFPInfo()
        {
            try
            {
                using (var db = new LiteDatabase(@"Database\MyDatabase.db"))
                {
                    //Ini for CFP System Information
                    var collection = db.GetCollection<CFPSystemInfo>("CFPSystemInfos");
                    var cfpSystemInfo = collection.FindById(1);

                    return cfpSystemInfo;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // General Backup Info
        public bool isBackupInfoExist(string infoName)
        {
            try
            {
                using (var db = new LiteDatabase(@"Database\MyDatabase.db"))
                {
                    //Looking for Objective
                    var col = db.GetCollection<BackupOfflineInfo>("BackupOfflineInfos");
                    var test = col.Find(x => x.infoName == infoName).FirstOrDefault();
                    if (test != null) return true; //Already exist
                    return false; //Not yet exist
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool CreateBackupInfo(string infoName, object value)
        {
            try
            {
                using (var db = new LiteDatabase(@"Database\MyDatabase.db"))
                {
                    //Looking for Objective
                    var col = db.GetCollection<BackupOfflineInfo>("BackupOfflineInfos");
                    var test = col.Find(x => x.infoName == infoName).FirstOrDefault();
                    if (test != null) return false; //Already exist

                    //Update Info for Objective
                    var backupOfflineInfo = new BackupOfflineInfo
                    {
                        infoName = infoName,
                        value = value,
                        LastUpdate = DateTime.Now
                    };
                    col.Insert(backupOfflineInfo);

                    //Write to database
                    var result = col.Update(backupOfflineInfo);
                    return result;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool UpdateBackupInfo(string infoName, object value)
        {
            try
            {
                using (var db = new LiteDatabase(@"Database\MyDatabase.db"))
                {
                    //Looking for Objective
                    var col = db.GetCollection<BackupOfflineInfo>("BackupOfflineInfos");
                    var backupOfflineInfo = col.Find(x => x.infoName == infoName).FirstOrDefault();
                    if (backupOfflineInfo == null) return false;

                    //Update Info for Objective
                    backupOfflineInfo.value = value;
                    backupOfflineInfo.LastUpdate = DateTime.Now;

                    //Write to database
                    return col.Update(backupOfflineInfo);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool RemoveBackupInfo(string infoName)
        {
            try
            {
                using (var db = new LiteDatabase(@"Database\MyDatabase.db"))
                {
                    //Looking for Objective
                    var col = db.GetCollection<BackupOfflineInfo>("BackupOfflineInfos");
                    var backupOfflineInfo = col.Find(x => x.infoName == infoName).FirstOrDefault();
                    if (backupOfflineInfo == null) return false;

                    //Write to database
                    var test = col.Delete(x => x.infoName == infoName);
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        //Get all item object
        public BackupOfflineInfo GetBackupObjectItem(string infoName)
        {
            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(@"Database\MyDatabase.db"))
            {
                //Ini for General Offline Backup Info
                var col = db.GetCollection<BackupOfflineInfo>("BackupOfflineInfos");
                var backupOfflineInfo = col.Find(x => x.infoName == infoName).FirstOrDefault();

                return backupOfflineInfo;
            }
        }

        //Get value of backup object only
        public object GetBackupInfo(string infoName)
        {
            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(@"Database\MyDatabase.db"))
            {
                //Ini for General Offline Backup Info
                var col = db.GetCollection<BackupOfflineInfo>("BackupOfflineInfos");
                var backupOfflineInfo = col.Find(x => x.infoName == infoName).FirstOrDefault();

                if (backupOfflineInfo == null) return null;

                return backupOfflineInfo.value;
            }
        }
    }
}
