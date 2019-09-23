using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hydra;
using System.Data.SqlClient;
using System.Windows.Forms;
//test
[assembly: CallbackAssemblyDescription("Accounting Notes FrsId", "Plugin adding center on accounting notes and simplified accounting notes", "MO", "1.1", "2013.6", "20-12-2013")]
namespace AccountingNotesFrsId
{
    
    public class AccountingNotesFrs : Callback
    {
        public override void Cleanup()
        {

        }
        public override void Init()
        {
            base.AddSubscription(false, 0, Events.OpenWindow, new TakeEventDelegate(this.OnOpenWindow));

        }
        bool OnOpenWindow(Procedures ProcID, int ControlID, Events Event)
        {
            UpdateFrsId();
            return true;

        }
        private void UpdateFrsId()
        {
            //Runtime.WindowController.UnlockThread();
            //MessageBox.Show(Hydra.MemNag.MEN_GIDNumer.ToString() + " frs: " +Runtime.Config.IdCentrumWlasciwego.ToString(), "Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //Runtime.WindowController.LockThread();
            #region Query
            string query = @"IF EXISTS( SELECT * FROM CDN.MemNag WHERE MEN_GIDNumer=@MeN_Gidnumer AND FrsId IS NULL)
	                            UPDATE CDN.MemNag SET FrsId=@FrsId WHERE MEN_GIDNumer=@MeN_Gidnumer";
            #endregion

            #region Connection
            SqlConnection SQLConn = new SqlConnection();
            if (Runtime.ConfigurationDictionary.LoginZintegorwany)
            {
                SQLConn.ConnectionString = "Integrated Security=true; " +
                "Data Source=" + Runtime.ConfigurationDictionary.Serwer.ToString() + ";" +
                "Initial Catalog=" + Runtime.ConfigurationDictionary.Baza.ToString();
            } // end of if
            else
            {
                SQLConn.ConnectionString = "Password=cdn;Persist Security Info=True;User ID=cdnxlado;" +
                "Initial Catalog=" + Runtime.ConfigurationDictionary.Baza.ToString() +
                ";Data Source=" + Runtime.ConfigurationDictionary.Serwer.ToString();
            }
            #endregion

            SQLConn.Open();
            using (SqlCommand com = new SqlCommand(query, SQLConn))
            {
                com.Parameters.AddWithValue("@MeN_Gidnumer", Hydra.MemNag.MEN_GIDNumer);
                com.Parameters.AddWithValue("@FrsId", Runtime.Config.IdCentrumWlasciwego);
                com.ExecuteNonQuery();
            }
            SQLConn.Close();


        }
    }
}

