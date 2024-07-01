// RomanVillaDataScraper -- VillaDataScraper.cs
// 
// Copyright (C) 2024 Matthew W. McKenzie and Kenz LLC
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Text.RegularExpressions;
using HtmlAgilityPack;
using RomanVillaLibrary.Models;

namespace RomanVillaDataScraper;

public partial class VillaDataScraper(HttpClient httpClient)
{
    public async Task<List<VillaInfo>> FetchVillaInfo(string url)
    {
        Console.Write($"FetchVillaInfo URL: {url}");
        var data = new List<VillaInfo>();
        
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var pageContents = await response.Content.ReadAsStringAsync();

        var document = new HtmlDocument();
        document.LoadHtml(pageContents);

        var tables = document.DocumentNode
            .SelectNodes("//div[@id='mw-content-text']//table[contains(@class, 'wikitable')]");

        if (tables == null) return data;
        
        foreach (var table in tables)
        {
            var rows = table.SelectNodes(".//tr[position()>1]"); // Skip the header row

            if (rows != null)
            {
                ExtractDataFromRows(rows, data);
            }
        }

        return data;
    }

    private void ExtractDataFromRows(HtmlNodeCollection rows, List<VillaInfo> data)
    {
        foreach (var row in rows)
        {
            var cells = row.SelectNodes("td");

            if (cells is { Count: >= 4 })
            {
                CreateVillaInfo(cells, data);
            }
        }
    }

    private void CreateVillaInfo(HtmlNodeCollection cells, List<VillaInfo> data)
    {
        var villaName = MyRegex().Replace(cells[0].InnerText.Trim(), "");

        var wikiLinkText = cells[1].InnerText.Trim();
        var wikiLink = cells[1].SelectSingleNode(".//a")?.GetAttributeValue("href", "N/A") ?? "N/A";

        var gridReference = cells[2].InnerText.Trim();

        var link = cells[3].SelectSingleNode(".//a")?.GetAttributeValue("href", "N/A") ?? "N/A";

        // Clean up text
        wikiLink = wikiLink.Replace(",", "").Replace("\"", "");
        wikiLinkText = wikiLinkText.Replace(",", "").Replace("\"", "");

        data.Add(new VillaInfo
        {
            VillaName = villaName,
            WikiLinkText = wikiLinkText,
            WikiLink = wikiLink,
            GridReference = gridReference,
            ExternalLink = link
        });
    }

    [GeneratedRegex(@"[^a-zA-Z]")]
    private static partial Regex MyRegex();
}