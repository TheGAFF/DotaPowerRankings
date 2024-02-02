# Dota2 Power Rankings Backend

Generates a massive JSON object of player and team power rankings for use in leagues such as RD2L. This would then be
used to fuel your React/Vue/whatever front-end.

## Tech Stack

1. Windows 10 (Not tested on Mac / Linux)
2. C# / .NET Core
3. [Entity Framework Core 7](https://docs.microsoft.com/en-us/ef/core/)
4. [Postgres SQL](https://www.postgresql.org/download/)
5. [Stratz API](https://docs.stratz.com/index.html) ( You will need an [API key](https://stratz.com/api)) (Most
   calculations come from the data here)
5. [OpenDota API](https://docs.opendota.com/) ( You will need an [API key](https://www.opendota.com/api-keys)) (For
   toxicity score calculation)
6. [text-generation-webui](https://github.com/oobabooga/text-generation-webui) with one of Mistral's Mixtral model. You
   can use
   OpenAI instead of this, though.
6. [OpenAI API](https://openai.com/api/) ( You will need an [API key](https://beta.openai.com/account/api-keys)) as an
   alternative to `text-generation-webui`
6. [Google Sheets API](https://developers.google.com/sheets/api/guides/concepts) (for consuming RD2L draft sheet data)
7. [JetBrains Rider IDE](https://www.jetbrains.com/rider/download/) ( You can probably use VS or VS Code, but I haven't
   tested)
8. [JetBrains DataGrip](https://www.jetbrains.com/datagrip/download/) (mostly for viewing data and writing SQL queries)

## Getting Started

1. Download the above tech / get your API keys.
2. Create an `appsettings.json` file using `appsettings.json.example.json` as a guide.
3. Create a `google-sheets-key.json` file using `google-sheets-key.json.example.json` as a guide.
4. Build Solution
5. Run Solution

## Endpoints

1. `[GET] /Dota` - Retrieves data from Stratz / OpenDota and stores it in your Postgres DB.
2. `[POST] /Dota/Pre-Season-Rankings` - Generates Player and Team Ranking Info
3. `[POST] /Dota/Post-Season-Rankings` - After the season is over, this generates Player, Team, AND Post Season Awards.

## Road Map

1. Add League / Team parsing via Stratz
2. More OpenAI text generation
3. rd2l.gg info gathering (via their APIs or web-scraping)