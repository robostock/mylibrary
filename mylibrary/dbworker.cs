using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlServerCe;

namespace mylibrary
{
    class dbworker
    {
        private string dbfilename = "MyLibraryDB.sdf";
        private string dbpathname;
        private string fullfilename;
        private string programpath;

        public string ConnectionString
        {
            get {
                return "Data Source = "+dbfilename;
            }
        }

        private void createdb()
        {
            SqlCeEngine engine = new SqlCeEngine(ConnectionString);
            engine.CreateDatabase();
            engine.Dispose();

            try
            {
                SqlCeConnection connection = new SqlCeConnection(ConnectionString);
                connection.Open();

                SqlCeCommand cmd = connection.CreateCommand();
                cmd.CommandText = "CREATE TABLE mylib (id int PRIMARY KEY, serviecename varbinary(1100), username varbinary(100), pswrd varbinary(100), ipadr varbinary(100), uid varbinary(100), comment varbinary(1100), pubid int, secid int)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "CREATE TABLE pubkeys (pubid int PRIMARY KEY, filename nvarchar, date datetime, keyvalue varbinary(8000))";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "CREATE TABLE seckeys (secid int PRIMARY KEY, filename nvarchar, date datetime, keyvalue varbinary(8000))";
                cmd.ExecuteNonQuery();

                connection.Close();
            }
            catch (Exception exp)
            {
                System.Windows.Forms.MessageBox.Show(exp.Message);
            }

        }

        private void createbackupcopy()
        {
            DateTime dtnow = DateTime.Now;
            string sdt = dtnow.ToShortDateString();
            sdt = sdt.Replace(".","");

            if (System.IO.File.Exists(programpath + "\\" + dbfilename))
            {
                string newname = dbfilename.Remove(dbfilename.Length - 4, 4);
                newname = newname + "-" + sdt + ".sdf";

                System.IO.File.Copy(programpath + "\\" + dbfilename, programpath + "\\" + newname,true);
            }
        }

        public dbworker(string programpath_value)
        {
            programpath = programpath_value;
            fullfilename = programpath+"\\"+dbfilename;

            if (!System.IO.File.Exists(fullfilename))
            {
                createdb();
            }
            else
            {
                createbackupcopy();
            }
        }

    }
}
