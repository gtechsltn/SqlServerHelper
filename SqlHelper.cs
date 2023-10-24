using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Data.Sql;
using Microsoft.Win32;
using Microsoft.SqlServer.Management.Smo;
using System.Text.RegularExpressions;

namespace SqlServerHelper
{
    public static class SqlHelper
    {
        public static void CreateDatabase(string dataBase, string server)
        {
            try
            {
                conStr = "server=" + server + ";uid=sa;";
                
                SqlConnection con = new SqlConnection(conStr);
                string str = "CREATE DATABASE " + dataBase;
                SqlCommand cmd = new SqlCommand(str, con);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception er)
            {
                MessageBox.Show(er.Message.ToString());
            }
        }
        public static void GenerateTables(string db, string server)
        {
            //generate tables from script (SQL Server Management Studio generated )
            try
            {
                string script = File.ReadAllText(@"tables.sql");
                string conStr;
               
                    conStr = "Data Source=" + server + ";Initial Catalog=" + db + ";User ID=sa";
               

                SqlConnection con = new SqlConnection(conStr);

                con.Open();

                IEnumerable<string> commandStrings = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                foreach (string commandString in commandStrings)
                {
                    if (commandString.Trim() != "")
                    {
                        new SqlCommand(commandString, con).ExecuteNonQuery();
                    }
                }

                con.Close();

            }
            catch (Exception er)
            {

                MessageBox.Show(er.Message.ToString());
            }
        }
        public static void GenerateFields(string db, string server)
        {
           //Generate missing field from sql script
           // the script contain following .....
             //IF NOT EXISTS (SELECT 1  FROM SYS.COLUMNS WHERE  
             //OBJECT_ID = OBJECT_ID(N'[dbo].table') AND name = 'fieldname')
             //BEGIN
             //ALTER TABLE [dbo].[table] ADD fieldname type
             //END
        
            try
            {
                string script = File.ReadAllText(@"CheckFields.sql");
                string conStr;
       
                conStr = "Data Source=" + server + ";Initial Catalog=" + db + ";User ID=sa"
       
                SqlConnection con = new SqlConnection(conStr);
                con.Open();

                IEnumerable<string> commandStrings = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                foreach (string commandString in commandStrings)
                {
                    if (commandString.Trim() != "")
                    {
                        new SqlCommand(commandString, con).ExecuteNonQuery();
                    }
                }
                con.Close();
            }
            catch (Exception er)
            {
                MessageBox.Show(er.Message.ToString());
            }
        }
        public static bool IsDBExist(string server, string db)
        {
            List<string> list = new List<string>();

            // Open connection to the database
            string conString = null; ;
            
                conString = "Data Source=" + server + ";Initial Catalog=master;User ID=sa";
           
            bool has = false;
            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                {
                    if (con.State == ConnectionState.Open) con.Close();
                    con.Open();


                    // Set up a command with the given query and associate
                    // this with the current connection.
                    using (SqlCommand cmd = new SqlCommand("SELECT name from sys.databases where name='" + db + "'", con))
                    {
                        using (IDataReader dr = cmd.ExecuteReader())
                        {

                            while (dr.Read())
                            {
                                list.Add(dr[0].ToString());
                            }
                            if (list.Count > 0) has = true;
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception er)
            {
                MessageBox.Show(er.Message.ToString());
            }
            return has;

        }

        public static List<string> GetDatabaseList(string server)
        {
            //List database on the server /local machine
            List<string> list = new List<string>();

            // Open connection to the database
            string conString = "server=" + server + ";uid=sa;";

            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();

               
                using (SqlCommand cmd = new SqlCommand("SELECT name from sys.databases", con))
                {
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(dr[0].ToString());
                        }
                    }
                }
                con.Close();
            }
            return list;

        }
        
        public static IEnumerable<string> ListLocalSqlInstances()
        {
            //List all local SQL instances using registry information
            if (Environment.Is64BitOperatingSystem)
            {
                using (var hive = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    foreach (string item in ListLocalSqlInstances(hive))
                    {
                        yield return item;
                    }
                }

                using (var hive = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                {
                    foreach (string item in ListLocalSqlInstances(hive))
                    {
                        yield return item;
                    }
                }
            }
            else
            {
                foreach (string item in ListLocalSqlInstances(Registry.LocalMachine))
                {
                    yield return item;
                }
            }
        }
    }
}
