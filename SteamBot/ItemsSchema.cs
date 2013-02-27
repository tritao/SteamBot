using Steam;
using Steam.TF2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SteamBot
{
    class ItemsSchema
    {
        public IDictionary<int, TF2Schema> Cache;
        public IDictionary<int, TF2ItemSchema> TF2ItemIds;
        public IDictionary<int, TF2ItemSchema> TF2ItemClassIds;
        public SteamWebClient WebClient;
        public TF2WebClient TF2WebClient;

        public ItemsSchema(SteamWebClient webClient, string webAPIKey)
        {
            WebClient = webClient;
            Cache = new Dictionary<int, TF2Schema>();
            TF2ItemIds = new SortedDictionary<int, TF2ItemSchema>();
            TF2ItemClassIds = new SortedDictionary<int, TF2ItemSchema>();
            TF2WebClient = new TF2WebClient(webAPIKey);
        }

        public void Load(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                int id;
                if (!int.TryParse(Path.GetFileNameWithoutExtension(file), out id))
                    return;

                // Workaround schema deserialization problem.
                var text = File.ReadAllText(file)
                    .Replace(@"<craft_class></craft_class>", String.Empty)
                    .Replace(@"<craft_material_type></craft_material_type>", String.Empty);

                Console.WriteLine("Loading items from file {0}", Path.GetFileName(file));

                var schema = TF2Schema.Deserialize(text);
                Cache[id] = schema;

                // Build id cache.
                BuildCache(schema);
            }
        }

        public IEnumerable<string> LookupSchemas()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Schemas");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var files = Directory.EnumerateFiles(path, "*.xml");

            if (files.Count() == 0)
            {
                Console.WriteLine("Getting the TF2 items schema...");

                var schemaXML = TF2WebClient.GetSchema();
                File.WriteAllText(Path.Combine(path, "440.xml"), schemaXML);

                var schema = TF2Schema.Deserialize(schemaXML);

                files = Directory.EnumerateFiles(path, "*.xml");
            }

            return files;
        }

        void BuildCache(TF2Schema schema)
        {
            Console.WriteLine("Building items cache...");

            foreach (var item in schema.Items)
                TF2ItemIds[item.DefIndex] = item;

            var tf2Schema = Cache[(int)SteamWebAppId.TF2];
            tf2Schema.Prices = TF2WebClient.GetTF2AssetPrices();

            foreach (var asset in tf2Schema.Prices.Assets)
            {
                var defindex = int.Parse(asset.Class[0].value);
                var classid = int.Parse(asset.ClassId);

                if (!TF2ItemIds.ContainsKey(defindex))
                    continue;

                var item = TF2ItemIds[defindex];
                if (item == null) continue;

                item.ClassId = classid;
                TF2ItemClassIds[classid] = item;
            }

            foreach (var asset in tf2Schema.Prices.Assets)
            {
                var defindex = int.Parse(asset.Class[0].value);
                var classid = int.Parse(asset.ClassId);

                if (!TF2ItemIds.ContainsKey(defindex))
                    continue;

                var item = TF2ItemIds[defindex];
                if (item == null) continue;

                item.ClassId = classid;
                TF2ItemClassIds[classid] = item;
            }
        }
    }
}
