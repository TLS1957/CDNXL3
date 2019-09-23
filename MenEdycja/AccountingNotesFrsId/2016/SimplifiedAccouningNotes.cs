using Hydra;
namespace AccountingNotesFrsId
{
    [SubscribeProcedure((Procedures)Procedures.MunEdycja, "Accounting Notes FrsId")]
    public class SimplifiedAccountingNotes : AccountingNotesFrs
    {

        public SimplifiedAccountingNotes()
        {
        }
    }
}