
using RomanVillaDataScraper;

var url = "https://en.wikipedia.org/wiki/List_of_Roman_villas_in_England";

var ds = new VillaDataScraper(new HttpClient());
var list = await ds.FetchVillaInfo(url);

var json = System.Text.Json.JsonSerializer.Serialize(list);
await File.WriteAllTextAsync("villas.json", json);