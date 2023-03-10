namespace WorkitemCreator
{
    using System;
    using System.IO;
    using System.Windows;
    using Newtonsoft.Json;

    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (File.Exists(Config.CONFIG_FILE_NAME) == false)
            {
                throw new FileNotFoundException("Unable to locate configuration", Config.CONFIG_FILE_NAME);
            }

            var configContents = File.ReadAllText(Config.CONFIG_FILE_NAME);
            var config = JsonConvert.DeserializeObject<Config>(configContents);
            config.CurrentLogFilePath = $".\\{nameof(WorkitemCreator)}.{DateTimeOffset.Now:yyyy.MM.dd.HH.mm.ss}.log";
            if (e.Args.Length > 0)
            {
                var pat = e.Args[0].Trim();
                config.AzDoPat = pat;
            }

            var main = new MainWindow(config);

            main.Show();
        }
    }
}