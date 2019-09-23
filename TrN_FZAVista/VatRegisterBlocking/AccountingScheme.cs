namespace SchamatyKsiegowe
{
    using Hydra;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;

    public class AccountingScheme : Callback
    {

        private ClaWindow ComboRejestr;
        private ClaWindow ComboRejestrOrg;
        private ClaWindow ComboSchemat;
        private ClaWindow ComboSchematOrg;
        private bool DataLock = true;
        private List<Registy> ListaRejestrow = new List<Registy>();
        private List<string> ListaSchematow = new List<string>();
        private int SchemaIndex;
        private int SchemaIndexXL;
        private SqlCommand SQLCommand;
        private SqlConnection SQLConn;

        public override void Cleanup()
        {
        }

        private bool ComboRejestr_OnAfterAccepted(Procedures ProcedureId, int ControlId, Events Event)
        {
            string SelectValue = this.ComboRejestr.FromRaw.Split(new char[] { '|' })[(Convert.ToInt32(this.ComboRejestr.SelectedRaw) * 2) - 2];
            this.ComboRejestrOrg.SelectedRaw = (this.ListaRejestrow.IndexOf(this.ListaRejestrow.Find(p => p.Code == SelectValue)) + 1).ToString();
            this.ComboRejestrOrg.PostEvent(Events.Accepted);
            return true;
        }

        private bool ComboSchemat_OnAfterAccepted(Procedures ProcedureId, int ControlId, Events Event)
        {
            if (!this.DataLock)
            {
                int num = Convert.ToInt32(this.ComboSchemat.SelectedRaw);
                if (num == 1)
                {
                    this.SchemaIndexXL = 1;
                    this.ComboSchematOrg.SelectedRaw = "1";
                    this.ComboSchematOrg.PostEvent(Events.Accepted);
                }
                else
                {
                    string item = this.ComboSchemat.FromRaw.Split(new char[] { '|' })[num - 1];
                    this.SchemaIndexXL = this.ListaSchematow.IndexOf(item) + 2;
                    this.ComboSchematOrg.SelectedRaw = this.SchemaIndexXL.ToString();
                    this.ComboSchematOrg.PostEvent(Events.Accepted);
                }
            }
            return true;
        }

        private bool ComboSchematOrg_OnAfterAccepted(Procedures ProcedureId, int ControlId, Events Event)
        {
            if (this.SchemaIndexXL != Convert.ToInt32(this.ComboSchematOrg.SelectedRaw))
            {
                this.ComboSchemat.SelectedRaw = this.SchemaIndex.ToString();
            }
            this.SchemaIndex = Convert.ToInt32(this.ComboSchemat.SelectedRaw);
            return true;
        }

        private void GetAccountingSchema(int GIDTyp)
        {
            this.ListaSchematow.Clear();
            string str = string.Format("SELECT S.SCH_Nazwa\r\nFROM CDN.Schematy S\r\nINNER JOIN CDN.TypSchemat T ON S.SCH_GIDTyp = T.SCT_SCHTyp and S.SCH_GIDNumer = T.SCT_SCHNumer\r\nWHERE T.SCT_TRNTyp = {0} and ISNULL(SCH_ARCHIWALNE,0)<>1\r\nORDER BY SCT_Pozycja", GIDTyp);
            this.SQLConn.Open();
            try
            {
                this.SQLCommand.CommandText = str;
                SqlDataReader reader = this.SQLCommand.ExecuteReader();
                while (reader.Read())
                {
                    this.ListaSchematow.Add(reader["SCH_Nazwa"].ToString());
                }
                reader.Close();
            }
            finally
            {
                this.SQLConn.Close();
            }
        }

        private bool GetFilterSchema(int CentrumID)
        {
            
            string str = string.Format(@"SELECT 
	                    CASE WHEN CHARINDEX('DH MM',CDN.GetCostCenter({0}))>0 THEN 1 ELSE 0 END AS Result", CentrumID);
            this.SQLConn.Open();
            bool result = false;
            try
            {
                this.SQLCommand.CommandText = str;
                result = Convert.ToBoolean(SQLCommand.ExecuteScalar());

            }
            finally
            {

                this.SQLConn.Close();
            }
            return result;
        }

        private RegisterType GetRegisterType(int GIDTyp)
        {
            switch (GIDTyp)
            {
                case 0x5f1:
                case 0x5f9:
                    return RegisterType.Zakup;

                case 0x7f1:
                case 0x7f9:
                    return RegisterType.Sprzedaz;
            }
            return RegisterType.Brak;
        }

        private void GetRegisterVAT(RegisterType Type)
        {
            this.ListaRejestrow.Clear();
            if (Type != RegisterType.Brak)
            {
                string str = string.Format("SELECT Naz_Nazwa, Naz_Opis \r\nFROM CDN.Nazwy\r\nWHERE Naz_GIDTyp = 576 and left(Naz_Nazwa1,1) = {0}\r\nORDER BY Naz_Nazwa", (short)Type);
                this.SQLConn.Open();
                try
                {
                    this.SQLCommand.CommandText = str;
                    SqlDataReader reader = this.SQLCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        this.ListaRejestrow.Add(new Registy(reader["Naz_Nazwa"].ToString(), reader["Naz_Opis"].ToString()));
                    }
                    reader.Close();
                }
                finally
                {
                    this.SQLConn.Close();
                }
            }
        }

        public override void Init()
        {
            this.SQLConn = new SqlConnection();
            this.SQLCommand = new SqlCommand();
            this.SQLCommand.Connection = this.SQLConn;
            if (Runtime.ConfigurationDictionary.LoginZintegrowany)
            {
                this.SQLConn.ConnectionString = "Integrated Security=true; Data Source=" + Runtime.ConfigurationDictionary.Serwer.ToString() + ";Initial Catalog=" + Runtime.ConfigurationDictionary.Baza.ToString();
            }
            else
            {
                this.SQLConn.ConnectionString = "Password=cdn;Persist Security Info=True;User ID=cdnxlado;Initial Catalog=" + Runtime.ConfigurationDictionary.Baza.ToString() + ";Data Source=" + Runtime.ConfigurationDictionary.Serwer.ToString();
            }
            base.AddSubscription(false, 0, Events.OpenWindow, new TakeEventDelegate(this.OnOpenWindow));
            base.AddSubscription(false, 0, Events.ResizeWindow, new TakeEventDelegate(this.OnResizeWindow));
        }

        private void Message(string Info)
        {
            //Runtime.WindowController.UnlockThread();
            MessageBox.Show(Info, "Test", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            //Runtime.WindowController.LockThread();
        }

        internal bool OnOpenWindow(Procedures ProcID, int ControlID, Events Event)
        {
           
           // Func<Registy, bool> predicate = null;
            bool isModimmole = this.GetFilterSchema(Runtime.ConfigurationDictionary.IdCentrumWlasciwego);
            this.ComboSchematOrg = base.GetWindow().AllChildren["?sEkr_Schemat"];
            this.ComboSchematOrg.OnAfterAccepted += new TakeEventDelegate(this.ComboSchematOrg_OnAfterAccepted);
            this.GetAccountingSchema(TraNag.TrN_GIDTyp);
            this.ComboSchemat = this.ComboSchematOrg.Parent.Children.Add(ControlTypes.droplist);

            if (isModimmole)
                this.ComboSchemat.FromRaw = "*<indefinite>|" + (from s in this.ListaSchematow
                                                                where s.StartsWith("02")
                                                                select s).ToString<string>("|");
            else
                this.ComboSchemat.FromRaw = "*<indefinite>|" + (from s in this.ListaSchematow
                                                                where !s.StartsWith("02")
                                                                select s).ToString<string>("|");
            this.ComboSchemat.OnAfterAccepted += new TakeEventDelegate(this.ComboSchemat_OnAfterAccepted);
            this.ComboSchemat.VScrollRaw = "1";
            this.ComboSchemat.DropRaw = "6";
            this.ComboSchematOrg.Visible = TraNag.TrN_Zaksiegowano != 0;
            this.ComboSchemat.Enabled = TraNag.TrN_Zaksiegowano == 0;
            this.ComboSchemat.Visible = TraNag.TrN_Zaksiegowano == 0;
            if (this.ComboSchematOrg.SelectedRaw == "1")
            {
                this.ComboSchemat.SelectedRaw = "1";
            }
            else if (Convert.ToInt32(this.ComboSchematOrg.SelectedRaw) > 0)
            {
                string item = this.ListaSchematow[Convert.ToInt32(this.ComboSchematOrg.SelectedRaw) - 2].Trim();
                this.ComboSchemat.SelectedRaw = (this.ComboSchemat.FromRaw.Split(new char[] { '|' }).ToList<string>().IndexOf(item) + 1).ToString();
            }
            else
            {
                this.ComboSchemat.SelectedRaw = "1";
            }

            if (this.GetRegisterType(TraNag.TrN_GIDTyp) != RegisterType.Brak)
            {
                this.ComboRejestrOrg = base.GetWindow().AllChildren["?TrN:VatRejestr"];
                this.GetRegisterVAT(this.GetRegisterType(TraNag.TrN_GIDTyp));
                this.ComboRejestr = this.ComboRejestrOrg.Parent.Children.Add(ControlTypes.droplist);
                this.ComboRejestr.FormatRaw = this.ComboRejestrOrg.FormatRaw;
                this.ComboRejestr.VScrollRaw = "1";
                this.ComboRejestr.DropWidthRaw = this.ComboRejestrOrg.DropWidthRaw;
                this.ComboRejestr.DropRaw = "6";
                this.ComboRejestr.Visible = true;
                this.ComboRejestr.OnAfterAccepted += new TakeEventDelegate(this.ComboRejestr_OnAfterAccepted);
                this.ComboRejestrOrg.Visible = TraNag.TrN_Zaksiegowano != 0;
                this.ComboRejestr.Enabled = TraNag.TrN_Zaksiegowano == 0;
                this.ComboRejestr.Visible = TraNag.TrN_Zaksiegowano == 0;
                IEnumerable<Registy> enumerable;
                if(isModimmole)
                    enumerable= this.ListaRejestrow.Where<Registy>(x=>x.Code.StartsWith("M"));
                else
                    enumerable = this.ListaRejestrow.Where<Registy>(x => !x.Code.StartsWith("M"));

                this.ComboRejestr.FromRaw = (from p in enumerable select string.Format("{0}|{1}*", p.Code, p.Name)).ToString<string>("|");
                //this.ComboRejestr.SelectedRaw = ((from p in enumerable select p.Code).ToList<string>().IndexOf(this.ComboRejestrOrg.ContentsRaw) ).ToString();
            //     Runtime.WindowController.UnlockThread();
            //MessageBox.Show("Test2", "Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //Runtime.WindowController.LockThread();
                
                this.ComboRejestr.SelectedRaw = ((from p in enumerable select p.Code).ToList<string>().IndexOf(this.ComboRejestrOrg.ContentsRaw) + 1).ToString();
                if (string.IsNullOrEmpty(ComboRejestrOrg.ContentsRaw) || enumerable.Where(x => x.Code.Contains(ComboRejestrOrg.ContentsRaw)).Count() == 0)
                {
                        this.ComboRejestr.SelectedRaw = 1.ToString();
                        string SelectValue = this.ComboRejestr.FromRaw.Split(new char[] { '|' })[(Convert.ToInt32(this.ComboRejestr.SelectedRaw) * 2) - 2];
                        this.ComboRejestrOrg.SelectedRaw = (this.ListaRejestrow.IndexOf(this.ListaRejestrow.Find(p => p.Code == SelectValue)) + 1).ToString(); 
                        this.ComboRejestrOrg.PostEvent(Events.Accepted);
                }
            }
            this.DataLock = false;
            return true;
        }

        private bool OnResizeWindow(Procedures ProcID, int ControlID, Events Event)
        {
            this.ComboSchemat.Bounds = new Rectangle(this.ComboSchematOrg.Bounds.Left, this.ComboSchematOrg.Bounds.Top, this.ComboSchematOrg.Bounds.Width, this.ComboSchematOrg.Bounds.Height);
            if (this.GetRegisterType(TraNag.TrN_GIDTyp) != RegisterType.Brak)
            {
                this.ComboRejestr.Bounds = new Rectangle(this.ComboRejestrOrg.Bounds.Left, this.ComboRejestrOrg.Bounds.Top, this.ComboRejestrOrg.Bounds.Width, this.ComboRejestrOrg.Bounds.Height);
            }
            return true;
        }
    }
}

