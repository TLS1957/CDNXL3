using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hydra;
using System.Data.SqlClient;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing;


// This give the add-in a name
[assembly: CallbackAssemblyDescription("CheckInvoiceReference", "Check invoice reference on PIs", "Tadeusz Liszka", "2.0", "2019.0.1", "23-08-2019")]

namespace CheckInvoiceReference
{
    // Which window should this be shown on? In this case, a manual PI, i.e. A-Vista PI
    [SubscribeProcedure((Procedures)Procedures.TrN_FZAVista, "Check Invoice Reference")]

    public class InvoiceReference : Callback
    {
        // Define the button
        ClaWindow InvRefButton;
        //ClaWindow Sheet;
        ClaWindow SaveButton;   
        SqlConnection SQLConn;
        SqlCommand SQLCommand;

        private bool SaveButtonClick(Procedures ProcId, int ControlId, Events Event)
        {
            return CheckIfExists("SaveButton");
        }


        public override void Init()
        {


            //test inicjacji Hydry ***************************************************************************
            //            MessageBox.Show("Inicjacja SQL", "CheckInvoiceReference", MessageBoxButtons.OK);
            // ***********************************************************************************************

            SQLConn = new SqlConnection();
            SQLCommand = new SqlCommand();
            SQLCommand.Connection = SQLConn;

            string connectionString = (string)typeof(SqlConnection).GetField("_connectionString", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Runtime.ActiveRuntime.Repository.Connection);
            SQLConn.ConnectionString = connectionString;

            //************************************************************************************************
            //MessageBox.Show(connectionString, "connectionString" , MessageBoxButtons.OK);
            // ***********************************************************************************************

            //************************************************************************************************************************
            // Stary kod dla wersji CDNXL < 2016
            //************************************************************************************************************************

            //if (Runtime.ConfigurationDictionary.LoginZintegrowany)
            //{
            //    SQLConn.ConnectionString = "Integrated Security=true; " +
            //        "Data Source=" + Runtime.ConfigurationDictionary.Serwer.ToString() + ";" +
            //        "Initial Catalog=" + Runtime.ConfigurationDictionary.Baza.ToString();
            //} // end of if
            //else
            //{
            //  SQLConn.ConnectionString = "Password=cdn;Persist Security Info=True;User ID=cdnxlado;" +
            //  "Initial Catalog=" + Runtime.ConfigurationDictionary.Baza.ToString() +
            //  ";Data Source=" + Runtime.ConfigurationDictionary.Serwer.ToString();
            //} // end of else
            //*************************************************************************************************************************************

            // Only show button when creating a new invoice
            if (TraNag.TrN_KntNumer == 0)
            {
                AddSubscription(false, 0, Events.OpenWindow, new TakeEventDelegate(OnOpenWindow));
            }   // end if

        } // end of Init

        public override void Cleanup() { }

        bool OnOpenWindow(Procedures ProcID, int ControlID, Events Event)
        {
            ClaWindow Parent = GetWindow();
            SaveButton = Parent.AllChildren["?Cli_Zapisz"];
            SaveButton.OnBeforeAccepted += new TakeEventDelegate(SaveButtonClick);
            InvRefButton = Parent.Children.Add(ControlTypes.button);
            InvRefButton.Bounds = new Rectangle(330, 58, 60, 13);
            InvRefButton.Visible = true;
            InvRefButton.TextRaw = "Check Invoice";
            InvRefButton.ToolTipRaw = "Check Invoice Reference";
            InvRefButton.OnBeforeAccepted += new TakeEventDelegate(MyButtonInvoiceClick);
            InvRefButton.Visible = true;
            InvRefButton.CursorRaw = Cursors.Hand.ToString();
 
            //Sheet.Visible = true; - Instrukcja powoduje błą. Mimo zakomentowania Hydra działa prawidłowo. TLS 2019-08-23

            return true;

        } // end of OnOpenWindow

        bool MyButtonInvoiceClick(Procedures ProcId, int ControlId, Events Event)
        {
            return CheckIfExists("InvoiceButton");
        } 

        bool CheckIfExists(String ButtonName)
        {            
            if (string.IsNullOrEmpty(TraNag.TrN_DokumentObcy) || (TraNag.TrN_KntNumer == 0))
            {
                //Runtime.WindowController.UnlockThread();
                MessageBox.Show("Please enter Invoice Reference and select a Customer", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                //Runtime.WindowController.LockThread();
                return true;
            }
            else
            {                  

                String SQL;
                String Document;
                                
                SQL = "SELECT cdn.numerdokumentutrn(trn_gidtyp, trn_spityp, trn_trntyp, trn_trnnumer, trn_trnrok, trn_trnseria) As DocNum, trn_gidnumer, trn_gidtyp, TrN_DokumentObcy, Knt_Akronim, Knt_Nazwa1 FROM CDN.TraNag INNER JOIN CDN.KntKarty ON Knt_GIDNumer = TrN_KntNumer" +
                    " WHERE replace(TrN_DokumentObcy,' ','') = replace('" + TraNag.TrN_DokumentObcy + "',' ','') AND Knt_GIDNumer = " + TraNag.TrN_KntNumer.ToString() +
                    " AND TrN_Stan IN (3,4,5) AND TrN_GIDNumer <> " + TraNag.TrN_GIDNumer.ToString();
                                
                SQLCommand.CommandText = SQL;
                SQLConn.Open();
                SqlDataReader SQLQuery = SQLCommand.ExecuteReader();
                SQLQuery.Read();     
 
                if (SQLQuery.HasRows == true)
                {
                    Document = SQLQuery.GetString(0);
                    //Runtime.WindowController.UnlockThread();
                    MessageBox.Show("The Invoice Reference has already been used for " + Environment.NewLine + Document, "Found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    //Runtime.WindowController.LockThread();
                    SQLConn.Close();
                    ClaWindow Parent = GetWindow();
                    Parent.PostEvent(Events.ReCalculate);
                    return false;
                }
                else 
                {
                    if (ButtonName == "InvoiceButton")
                    {
                        //Runtime.WindowController.UnlockThread();
                        MessageBox.Show("The Invoice Reference has not been used for this customer.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //Runtime.WindowController.LockThread();
                    }
                    SQLConn.Close();
                    ClaWindow Parent = GetWindow();
                    Parent.PostEvent(Events.ReCalculate);
                    return true;
                }
                

            } // end if 
            

            //return true;
        } // end of CheckIfExists      
    }
}
