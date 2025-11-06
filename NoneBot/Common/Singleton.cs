namespace NoneBot.Common
{
    public class TSingleton<T> where T : class, new()
    {
        public static T ms_hInstance
        {
            get;
            private set;
        }

        static TSingleton()
        {
            if (ms_hInstance == null)
            {
                ms_hInstance = new T();
            }
        }

        public virtual void Clear()
        {
            ms_hInstance = null;
            ms_hInstance = new T();
        }


        public static T CreateInstance()
        {
            try
            {
                if (ms_hInstance == null)
                {
                    ms_hInstance = new T();
                }

                return ms_hInstance;
            }
            catch
            {
                return ms_hInstance;
            }
        }
    }
}
