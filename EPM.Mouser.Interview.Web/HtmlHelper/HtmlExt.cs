namespace EPM.Mouser.Interview.Web.HtmlHelper
{
    public class HtmlExt
    {
        public static string GetClassForAvailableStock(int availableStocks)
        {
            if(availableStocks < 10 && availableStocks > 0)
            {
                return "orange-cell";
            }
            else if (availableStocks < 0)
            {
                return "red-cell";
            }
            else
            {
                return "normal-cell";
            }
        }
    }
}