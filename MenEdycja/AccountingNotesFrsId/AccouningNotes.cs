using Hydra;
namespace AccountingNotesFrsId
{
    [SubscribeProcedure((Procedures)Procedures.MenEdycja, "Accounting Notes FrsId")]
    public class AccountingNotes : AccountingNotesFrs
    {
        
        public AccountingNotes()
        {
        }
    }
}