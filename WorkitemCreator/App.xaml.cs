namespace WorkitemCreator
{
    using System.IO;
    using System.Windows;
    using Newtonsoft.Json;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
         
            if (File.Exists("config.json") == false)
            {
                throw new FileNotFoundException("Unable to locate configuration", "config.json");
            }

            var configContents = File.ReadAllText("config.json");
            var config = JsonConvert.DeserializeObject<Config>(configContents);
            var main = new MainWindow(config);

            main.Show();
        }
    }
}