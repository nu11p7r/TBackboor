namespace NoneBot.Function.Handler
{
    public class CHandler
    {
        private CSystem m_hSystem = new CSystem();
        private CDDoS m_hDDoS = new CDDoS();
        //private CBrowser m_hBrowser = new CBrowser();

        public CSystem GetSystem { get { return m_hSystem; } }
        public CDDoS GetDDoS { get { return m_hDDoS; } }
        //public CBrowser GetBrowser { get { return m_hBrowser; } }
    }
}
