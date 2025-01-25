using HueTwitterOrm;
using Life;
using Life.DB;
using Life.Network;
using Life.UI;
using ModKit.Helper;
using ModKit.Interfaces;
using ModKit.Internal;
using System;
using System.Data;
using System.IO;
using System.Linq;

namespace HueTwitter
{
    public class HueTwitter : ModKit.ModKit
    {
        public HueTwitter(IGameAPI aPI) : base(aPI)
        {
            PluginInformations = new PluginInformations(AssemblyHelper.GetName(), "1.0.0", "Zerox_Hue");
        }
        public Config config;
        public class Config
        {
            public int LevelAdminMinRequiredForDeleteTweet;
        }
        public void CreateConfig()
        {
            string directoryPath = pluginsPath + "/HueTwitter";

            string configFilePath = directoryPath + "/config.json";

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (!File.Exists(configFilePath))
            {
                var defaultConfig = new Config
                {
                    LevelAdminMinRequiredForDeleteTweet = 2,
                };
                string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(defaultConfig, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(configFilePath, jsonContent);
            }

            config = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(File.ReadAllText(configFilePath));
        }
        public override void OnPluginInit()
        {
            base.OnPluginInit();
            CreateConfig();
            Orm.RegisterTable<HueTwitterOrm.HueTwitterOrm>();
            Logger.LogSuccess($"{PluginInformations.SourceName} v{PluginInformations.Version}", "initialisé");
            new SChatCommand("/twitter", "twitter", "/twitter", (player, args) => { OnSlashTwitter(player); }).Register();
        }
        public void OnSlashTwitter(Player player)
        {
            UIPanel panel = new UIPanel("<color=#31a6e0>Twitter</color>", UIPanel.PanelType.Tab);
            panel.AddTabLine("<color=#1eb04c>Voir les tweets</color>", ui => { ViewTweet(player); });
            panel.AddTabLine("<color=#4232a8>Post un Tweet</color>", ui => { PostATweet(player); });
            panel.AddButton("Fermer", ui => player.ClosePanel(ui));
            panel.AddButton("Valider", ui => ui.SelectTab());
            player.ShowPanelUI(panel);
        }
        public async void ViewTweet(Player player)
        {
            var element = await HueTwitterOrm.HueTwitterOrm.Query(x => !x.IsDelete);
            UIPanel panel = new UIPanel("HueTwitter", UIPanel.PanelType.Tab);
            if (!element.Any())
            {
                panel.AddTabLine("<color=red>Aucun Tweet à l'horizon</color>", ui => player.ClosePanel(ui));
            }
            else
            {
                foreach (var elements in element)
                {
                    panel.AddTabLine($"<color=#fc031c>{elements.Title}</color><br><size=14>De <color=#032cfc>{elements.Author}</color></size>", ui =>
                    {
                        UIPanel panel1 = new UIPanel($"Tweet De : <color=#b6fc03>{elements.Author}</color>", UIPanel.PanelType.Text);
                        panel1.SetText($"Titre : <color=#b3a639>{elements.Title}</color> <br>De : <color=#b3a639>{elements.Author}</color> <br>Tweet : <color=#b3a639> {elements.Description} </color><br>Date : {elements.Jour}/{elements.Mois}/{elements.Année}");
                        panel1.AddButton("Fermer", ui1 => player.ClosePanel(ui1));
                        if (player.account.AdminLevel >= config.LevelAdminMinRequiredForDeleteTweet)
                        {
                            panel1.AddButton("<color=red>Supprimer</color>", ui1 => { DeleteTweet(player, elements); player.ClosePanel(ui1); });
                        }
                        player.ShowPanelUI(panel1);
                    });
                }
            }
            panel.AddButton("Fermer", ui => player.ClosePanel(ui));
            panel.AddButton("Valider", ui => ui.SelectTab());
            player.ShowPanelUI(panel);
        }
        public void PostATweet(Player player)
        {
            UIPanel panel = new UIPanel("Tweet Titre", UIPanel.PanelType.Input);
            panel.AddButton("Fermer", ui => player.ClosePanel(ui));
            panel.SetInputPlaceholder("Entrez le titre de votre tweet...");
            panel.AddButton("Valider", ui =>
            {
                UIPanel panel1 = new UIPanel("Tweet Descripton", UIPanel.PanelType.Input);
                panel1.AddButton("Fermer", ui1 => player.ClosePanel(ui1));
                panel1.SetInputPlaceholder("Entrez la description de votre tweet...");
                panel1.AddButton("Valider", async ui1 =>
                {
                    var instance = new HueTwitterOrm.HueTwitterOrm();
                    instance.Author = player.GetFullName();
                    instance.Année = DateTime.Now.Year;
                    string month = DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString();
                    instance.Mois = month;
                    string day = DateTime.Now.Day < 10 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
                    instance.Jour = day;
                    instance.Title = panel.inputText;
                    instance.Description = panel1.inputText;
                    instance.IsDelete = false;
                    if (!await instance.Save())
                    {
                        player.SendText("<color=red>[HueTwitter]</color> Une erreur est survenue lors de l'enregistrement du tweet merci de réessayer ultérierement ou en parler à un staff si le probléme persiste !");
                    }
                    else
                    {
                        player.SendText("<color=red>[HueTwitter]</color> Votre tweet a bien été posté !");
                    }
                    player.ClosePanel(ui1);
                });
                player.ShowPanelUI(panel1);
            });
            player.ShowPanelUI(panel);
        }
        public async void DeleteTweet(Player player, HueTwitterOrm.HueTwitterOrm elements)
        {
            elements.IsDelete = true;
            elements.Title = "null";
            elements.Author = "null";
            elements.Description = "null";
            await elements.Save();
            player.SendText("<color=red>[HueTwitter]</color> Tweet supprimé avec succés !");
        }
    }
}