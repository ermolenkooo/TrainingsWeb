using BLL;

namespace React.Server
{
    public class MyOptionsFactory
    {
        public static MyOptions Create(IServiceProvider provider)
        {
            var myOptions = new MyOptions();
            myOptions.InitializeAsync().Wait();
            return myOptions;
        }
    }
}
