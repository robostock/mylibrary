using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlServerCe;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;

namespace mylibrary
{
    enum keytype
    {
        pubring, secring
    }

    public partial class Form1 : Form
    {
        private SqlCeConnection connection;
        private SqlCeDataAdapter adapter;
        private DataSet ds = new DataSet();
        private DataSet decryptedds = new DataSet();

        private DataTable dt = new DataTable("data");

        private dbworker dbw;

        private byte[] key;
        private byte[] iv = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        //функция получает ключь
        private byte[] getkey()
        {
            string retstring = "";
            try
            {
                retstring = this.textBox2.Text;
                if (retstring.Length!=0)
                {
                    if (retstring.Length < 16)
                    {
                        int i = 0;
                        while (retstring.Length < 16)
                        {
                            retstring = retstring + retstring[i];
                            i++;
                        }
                    }
                }
            }
            catch { };
            
            return Encoding.UTF8.GetBytes(retstring);
        }

        //клонирует и дешифрует датасет
        private void clonedatasets(ref DataSet dest, DataSet source)
        {
            if (dest == null)
            {
                dest = new DataSet();
            }
            else
            {
                dest.Clear();
                dest.Dispose();
                dest = new DataSet();
            }

            //клонирование датасета
            foreach (DataTable dt in source.Tables)
            {
                if (!dest.Tables.Contains(dt.TableName))
                {
                    dest.Tables.Add(dt.TableName);
                }

                foreach (DataColumn dc in dt.Columns)
                {
                    if(!dest.Tables[dt.TableName].Columns.Contains(dc.ColumnName))
                    {
                        if ((dc.ColumnName == "serviecename") || (dc.ColumnName == "username") || (dc.ColumnName == "pswrd") || (dc.ColumnName == "ipadr") || (dc.ColumnName == "uid") || (dc.ColumnName == "comment"))
                        {
                            dest.Tables[dt.TableName].Columns.Add(dc.ColumnName, System.Type.GetType("System.String"));
                        }
                        else
                        {
                            dest.Tables[dt.TableName].Columns.Add(dc.ColumnName, dc.DataType);
                        }
                    }
                }
                dest.Tables[dt.TableName].PrimaryKey = new DataColumn[] {dest.Tables[dt.TableName].Columns[0]};
            }

            //расшифровка ds
            if (key != null)
            {
                foreach (DataTable dt in source.Tables)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        DataRow newdr = dest.Tables[dt.TableName].NewRow();
                        foreach (DataColumn dc in dt.Columns)
                        {
                            if ((dc.ColumnName == "serviecename") || (dc.ColumnName == "username") || (dc.ColumnName == "pswrd") || (dc.ColumnName == "ipadr") || (dc.ColumnName == "uid") || (dc.ColumnName == "comment"))
                            {
                                byte[] encryptedvalue = null;

                                if (!(dr[dc.ColumnName].GetType().ToString()).Contains("DBNull"))
                                {
                                    encryptedvalue = (byte[])dr[dc.ColumnName];


                                    string value;
                                    try
                                    {
                                        value = cryptoclass.DecryptStringFromBytes(encryptedvalue, key, iv);
                                        value = cryptoclass.deletepaddingstring(value);
                                    }
                                    catch (CryptographicException exp)
                                    {
                                        //System.Windows.Forms.MessageBox.Show(exp.Message);
                                        value = Encoding.UTF8.GetString(encryptedvalue);
                                    }
                                    newdr[dc.ColumnName] = value;
                                }                                
                            }
                            else
                            {
                                newdr[dc.ColumnName] = dr[dc.ColumnName];
                            }
                        }
                        dest.Tables[dt.TableName].Rows.Add(newdr);
                    }
                }
            }
        }

        private void showdata()
        {
            key = getkey();

            string connectionstring = "DataSource = " + Application.StartupPath + "\\" + "MyLibraryDB.sdf";
            connection = new SqlCeConnection(connectionstring);//Properties.Settings.Default.librarydbconnectionstring);
            connection.Open();

            SqlCeCommand selectcmd = new SqlCeCommand("SELECT * FROM mylib", connection);
            adapter = new SqlCeDataAdapter(selectcmd);

            SqlCeCommandBuilder cb = new SqlCeCommandBuilder(adapter);

            adapter.Fill(ds);
            ds.Tables[0].PrimaryKey = new DataColumn[] { ds.Tables[0].Columns["id"] };

            clonedatasets(ref decryptedds, ds);

            dataGridView1.DataSource = decryptedds;
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.DataMember = decryptedds.Tables[0].TableName;
        }

        public Form1()
        {
            InitializeComponent();

            dbw = new dbworker(Application.StartupPath);

            key = getkey();

            showdata();

            /*string connectionstring = "DataSource = " + Application.StartupPath + "\\" + "MyLibraryDB.sdf";
            connection = new SqlCeConnection(connectionstring);//Properties.Settings.Default.librarydbconnectionstring);
            connection.Open();

            SqlCeCommand selectcmd = new SqlCeCommand("SELECT * FROM mylib", connection);
            adapter = new SqlCeDataAdapter(selectcmd);

            SqlCeCommandBuilder cb = new SqlCeCommandBuilder(adapter);
            
            adapter.Fill(ds);
            ds.Tables[0].PrimaryKey = new DataColumn[] { ds.Tables[0].Columns["id"] };
            
            clonedatasets(ref decryptedds, ds);

            dataGridView1.DataSource = decryptedds;
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.DataMember = decryptedds.Tables[0].TableName;*/
           
            dataGridView1.Columns["id"].Width = 30;
            dataGridView1.Columns["id"].HeaderText = "ID";
            dataGridView1.Columns["serviecename"].Width = 120;
            dataGridView1.Columns["serviecename"].HeaderText = "Service Name";
            dataGridView1.Columns["username"].Width = 100;
            dataGridView1.Columns["username"].HeaderText = "Login";
            dataGridView1.Columns["pswrd"].Width = 100;
            dataGridView1.Columns["pswrd"].HeaderText = "Password";
            
            dataGridView1.Columns["ipadr"].Width = 100;
            dataGridView1.Columns["ipadr"].HeaderText = "IP";

            dataGridView1.Columns["uid"].Width = 40;
            dataGridView1.Columns["uid"].HeaderText = "UID";
            dataGridView1.Columns["comment"].Width = 300;
            dataGridView1.Columns["comment"].HeaderText = "Комментарий";

            dataGridView1.Columns["pubid"].Width = 50;
            dataGridView1.Columns["secid"].Width = 50;
        }

        private void Открыть_Click(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                key = getkey();

                cryptoclass.EncryptDataSet(ref ds, decryptedds, key, iv);

                adapter.Update(ds);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
            finally
            {
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.FileInfo file = new System.IO.FileInfo(openFileDialog1.FileName);
                if (file.Length <= 8000)
                {
                    textBox3.Text = openFileDialog1.FileName;

                    SaveKeystoDB(textBox3.Text, textBox4.Text);
                }
                else
                {
                    MessageBox.Show("Размер файла больше 8 000 байт");
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.FileInfo file = new System.IO.FileInfo(openFileDialog1.FileName);
                if (file.Length <= 8000)
                {
                    textBox4.Text = openFileDialog1.FileName;

                    SaveKeystoDB(textBox3.Text, textBox4.Text);
                }
                else
                {
                    MessageBox.Show("Размер файла больше 8 000 байт");
                }
            }
        }

        private int getmaxindex(string tablename)
        {
            string idclmnname = "secid";
            if (tablename.Contains("pub"))
            {
                idclmnname = "pubid";
            }
            string cmdstr = "SELECT TOP(1) "+idclmnname+" FROM " + tablename+" ORDER BY date DESC";
            SqlCeCommand cmd = new SqlCeCommand(cmdstr, connection);
            cmd.Parameters.Add("@md", SqlDbType.DateTime);
            object v = cmd.ExecuteScalar();
            return (int)v;
        }

        private bool SaveKeystoDB(string pubringname, string secringname)
        {
            if (dataGridView1.SelectedRows.Count != 1)
            {
                MessageBox.Show("Необходимо выделить сервис, для которого необходимо сохранить ключи!");
                return false;
            }
            int selectedindex = dataGridView1.SelectedRows[dataGridView1.SelectedRows.Count - 1].Index;

            if( (pubringname!="")&&(secringname!="") )
            {
                if(System.IO.File.Exists(pubringname)&&System.IO.File.Exists(secringname))
                {
                    key = getkey();

                    string pubkeystring = System.IO.File.ReadAllText(pubringname,Encoding.Default);
                    pubkeystring = cryptoclass.paddingstring(pubkeystring);
                    byte[] pub = cryptoclass.EncryptStringToBytes(pubkeystring, key, iv);
                    //byte[] pub = System.IO.File.ReadAllBytes(pubringname);

                    string seckeystring = System.IO.File.ReadAllText(secringname, Encoding.Default);
                    seckeystring = cryptoclass.paddingstring(seckeystring);
                    byte[] sec = cryptoclass.EncryptStringToBytes(seckeystring, key, iv);
                    //byte[] sec = System.IO.File.ReadAllBytes(secringname);

                    string pubcmdstring = @"INSERT INTO pubkeys(filename, date, keyvalue) VALUES(@filename, @date, @keyvalue)";
                    SqlCeCommand pubcmd = new SqlCeCommand(pubcmdstring, connection);
                    pubcmd.Parameters.Add("@filename", pubringname);
                    pubcmd.Parameters.Add("@date", DateTime.Now);
                    pubcmd.Parameters.Add("@keyvalue", pub);
                    
                    int pubres = pubcmd.ExecuteNonQuery();
                    int pubringindex = getmaxindex("pubkeys");

                    string seccmdstring = @"INSERT INTO seckeys(filename, date, keyvalue) VALUES(@filename, @date, @keyvalue)";
                    SqlCeCommand seccmd = new SqlCeCommand(seccmdstring, connection);
                    seccmd.Parameters.Add("@filename", secringname);
                    seccmd.Parameters.Add("@date", DateTime.Now);
                    seccmd.Parameters.Add("@keyvalue", sec);

                    int seccres = seccmd.ExecuteNonQuery();
                    int secringindex = getmaxindex("seckeys");

                    dataGridView1.Rows[selectedindex].Cells["pubid"].Value = pubringindex;
                    dataGridView1.Rows[selectedindex].Cells["secid"].Value = secringindex;
                }
                else
                {
                    MessageBox.Show("один из указанных ключей не существует");
                }
            }
            else
            {
                MessageBox.Show("один из ключей не выбран для загрузки. Для загрузки одно ключа, во втором поле укажите тот же ключ!");
            }
            return false;
        }

        private byte[] getkey(int grdvwselectedindex,keytype kt)
        {
            DataGridViewRow dgrwrow = dataGridView1.SelectedRows[dataGridView1.SelectedRows.Count-1];

            int serviceid = (int)dgrwrow.Cells["id"].Value;
            string dbname = (kt == keytype.pubring) ? "pubkeys" : "seckeys";

            string cmdstr = (kt == keytype.pubring) ? ("SELECT pubid FROM mylib WHERE id=") : ("SELECT secid FROM mylib WHERE id=");
            cmdstr = cmdstr + serviceid.ToString();

            SqlCeCommand cmd = new SqlCeCommand(cmdstr, connection);

            int keyid = (int)cmd.ExecuteScalar();

            cmdstr = (kt == keytype.pubring) ? ("SELECT keyvalue FROM pubkeys WHERE pubid=") : ("SELECT keyvalue FROM seckeys WHERE secid=");
            cmdstr = cmdstr + keyid.ToString();

            cmd = new SqlCeCommand(cmdstr, connection);

            object o_key = cmd.ExecuteScalar();

            if (o_key.GetType().ToString() == "System.Byte[]")
            {
                return (byte[])o_key;
            }
            else
            {
                MessageBox.Show("В поле ключа хранится неверный тип данных:" + o_key.GetType().ToString());
            }

            return null;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = textBox3.Text;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (dataGridView1.SelectedRows.Count == 1)
                {
                    int idx = dataGridView1.SelectedRows[dataGridView1.SelectedRows.Count - 1].Index;
                    byte[] filedata = getkey(idx, keytype.pubring);

                    key = getkey();
                    string filedatastring = cryptoclass.DecryptStringFromBytes(filedata,key,iv);
                    filedatastring = cryptoclass.deletepaddingstring(filedatastring);
                    System.IO.File.WriteAllText(saveFileDialog1.FileName, filedatastring, Encoding.Default);

                    //System.IO.File.WriteAllBytes(saveFileDialog1.FileName, filedata);
                }
                else
                {
                    if (dataGridView1.SelectedRows.Count == 0)
                        MessageBox.Show("Вы не выделили сервис для которого необходимо сохранить ключи");
                    else
                        MessageBox.Show("Для сохранения ключей необходимо выбрать один сервис");

                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = textBox4.Text;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (dataGridView1.SelectedRows.Count == 1)
                {
                    int idx = dataGridView1.SelectedRows[dataGridView1.SelectedRows.Count - 1].Index;
                    byte[] filedata = getkey(idx, keytype.secring);

                    key = getkey();
                    string filedatastring = cryptoclass.DecryptStringFromBytes(filedata, key, iv);
                    filedatastring = cryptoclass.deletepaddingstring(filedatastring);
                    System.IO.File.WriteAllText(saveFileDialog1.FileName, filedatastring, Encoding.Default);

                    //System.IO.File.WriteAllBytes(saveFileDialog1.FileName, filedata);
                }
                else
                {
                    if (dataGridView1.SelectedRows.Count == 0)
                        MessageBox.Show("Вы не выделили сервис для которого необходимо сохранить ключи");
                    else
                        MessageBox.Show("Для сохранения ключей необходимо выбрать один сервис");

                }
            }
        }

        private string getkeyname(int keyid, keytype kt)
        {

            string cmdstr = (kt == keytype.pubring) ? ("SELECT filename FROM pubkeys WHERE pubid=") : ("SELECT filename FROM seckeys WHERE secid=");
            cmdstr = cmdstr + keyid.ToString();

            SqlCeCommand cmd = new SqlCeCommand(cmdstr, connection);

            object o_key = cmd.ExecuteScalar();

            this.Text = o_key.GetType().ToString();

            if (o_key.GetType().ToString() == "System.String")
            {
                return (string)o_key;
            }
            else
            {
                MessageBox.Show("В поле ключа хранится неверный тип данных:" + o_key.GetType().ToString());
            }

            return null;
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                int idx = dataGridView1.SelectedRows[dataGridView1.SelectedRows.Count - 1].Index;
                //this.Text = dataGridView1.Rows[idx].Cells["pubid"].Value.GetType().ToString();
                if (!dataGridView1.Rows[idx].Cells["pubid"].Value.GetType().ToString().Contains("DBNull"))
                {
                    int keyid = (int)dataGridView1.Rows[idx].Cells["pubid"].Value;
                    textBox3.Text = System.IO.Path.GetFileName(getkeyname(keyid, keytype.pubring));
                }
                else
                    textBox3.Text = "";
                if (!dataGridView1.Rows[idx].Cells["secid"].Value.GetType().ToString().Contains("DBNull"))
                {
                    int keyid = (int)dataGridView1.Rows[idx].Cells["secid"].Value;
                    textBox4.Text = System.IO.Path.GetFileName(getkeyname(keyid, keytype.secring));
                }
                else
                    textBox4.Text = "";
                //надо получить название ключа
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            key = getkey();
            
            cryptoclass.EncryptDataSet(ref ds, decryptedds, key, iv);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
        }

        private void button6_Click(object sender, EventArgs e)
        {
            showdata();
        }

        

    }
}
    


