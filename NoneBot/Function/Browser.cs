using NoneBot.Function.Browsers;

namespace NoneBot.Function
{
    public class CBrowser
    {
        private CChromium m_hChromium = new CChromium();

        public CChromium GetChromium { get { return m_hChromium; } }
    }
}
