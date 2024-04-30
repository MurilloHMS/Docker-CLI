using System.Configuration;


namespace ContainersArtsystem
{
    public class Settings
    {
        private string _Diretorio;

        public string Diretorio
        {
            get { return GetConfigValue("diretorio"); }
            set
            {
                _Diretorio = value;
                UpdateConfig("diretorio", _Diretorio);
            }
        }

        // Método privado para obter valores do appSettings
        private string GetConfigValue(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        // Método privado para atualizar valores no appSettings
        private void UpdateConfig(string key, string value)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[key].Value = value;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
