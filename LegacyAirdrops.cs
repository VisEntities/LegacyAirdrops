using Newtonsoft.Json;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Legacy Airdrops", "VisEntities", "1.0.0")]
    [Description(" ")]
    public class LegacyAirdrops : RustPlugin
    {
        #region Fields

        private static LegacyAirdrops _plugin;
        private static Configuration _config;

        #endregion Fields

        #region Configuration

        private class Configuration
        {
            [JsonProperty("Version")]
            public string Version { get; set; }

            [JsonProperty("Minimum Number Of Additional Airdrops")]
            public int MinimumNumberOfAdditionalAirdrops { get; set; }

            [JsonProperty("Maximum Number Of Additional Airdrops")]
            public int MaximumNumberOfAdditionalAirdrops { get; set; }

            [JsonProperty("Airdrop Spread Radius")]
            public float AirdropSpreadRadius { get; set; }
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            _config = Config.ReadObject<Configuration>();

            if (string.Compare(_config.Version, Version.ToString()) < 0)
                UpdateConfig();

            SaveConfig();
        }

        protected override void LoadDefaultConfig()
        {
            _config = GetDefaultConfig();
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(_config, true);
        }

        private void UpdateConfig()
        {
            PrintWarning("Config changes detected! Updating...");

            Configuration defaultConfig = GetDefaultConfig();

            if (string.Compare(_config.Version, "1.0.0") < 0)
                _config = defaultConfig;

            PrintWarning("Config update complete! Updated from version " + _config.Version + " to " + Version.ToString());
            _config.Version = Version.ToString();
        }

        private Configuration GetDefaultConfig()
        {
            return new Configuration
            {
                Version = Version.ToString(),
                MinimumNumberOfAdditionalAirdrops = 2,
                MaximumNumberOfAdditionalAirdrops = 3,
                AirdropSpreadRadius = 15
            };
        }

        #endregion Configuration

        #region Oxide Hooks

        private void Init()
        {
            _plugin = this;
        }

        private void Unload()
        {
            _config = null;
            _plugin = null;
        }

        private void OnSupplyDropDropped(SupplyDrop supplyDrop, CargoPlane cargoPlane)
        {
            if (supplyDrop == null || cargoPlane == null)
                return;

            int numberOfAdditionalDrops = Random.Range(_config.MinimumNumberOfAdditionalAirdrops, _config.MaximumNumberOfAdditionalAirdrops + 1);

            for (int i = 0; i < numberOfAdditionalDrops; i++)
            {
                Vector3 randomPosition = new Vector3(
                    Random.Range(-_config.AirdropSpreadRadius, _config.AirdropSpreadRadius),
                    0,
                    Random.Range(-_config.AirdropSpreadRadius, _config.AirdropSpreadRadius)
                );

                // Add a slight height variation to make the airdrops spawn at slightly different altitudes for realism.
                float heightVariance = Random.Range(-10f, 10f);

                Vector3 dropPosition = cargoPlane.dropPosition + randomPosition;
                dropPosition.y = cargoPlane.transform.position.y - heightVariance;

                Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                BaseEntity additionalSupplyDrop = GameManager.server.CreateEntity(cargoPlane.prefabDrop.resourcePath, dropPosition, randomRotation);
                if (additionalSupplyDrop != null)
                {
                    additionalSupplyDrop.Spawn();
                }
            }
        }

        #endregion Oxide Hooks
    }
}