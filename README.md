# Weatherapplication

[> Startdocument van **Monique Sabong Land**, **Rick Vinke**, **Thijs Janse** en **Chris Klunder**.](./start-document/WEATHERAPPLICATION-STARTDOCUMENT.md)

Voor het vak C# Threading hebben wij als groep een weerapplicatie gemaakt.
Deze weerapplicatie haalt gegevens op van veel verschillende publieke weer API's.
Het combineert deze gegevens en laat het gemiddelde als resultaat zien.

Volgens de vereisten van de opdracht is deze applicatie gemaakt in .NET MAUI en .NET 8.

### Threading manieren
Een van de vereisten van de opdracht is het gebruik van 4 verschillende threading manieren.  
Deze sectie beschrijft de threading manieren die in dit project gebruikt zijn en waarvoor.

- Multithreading: Voor het exporteren van weerdata.
- Threadpool: Voor het ophalen van het huidige weer bij het beheren van plaatsen.
- Task Parellel Library (TPL): Voor het ophalen van weerdata op meerdere plaatsen tegelijk.
- Async & await: Voor het ophalen van de data van de API('s) en het laden van de verschillende pagina's in de applicatie.

## Installatie handleiding
Voor het gebruik van de applicatie is een computer nodig met de volgende vereisten:  
   - x86 gebaseerde processor
   - Windows 10/11
   - Internet verbinding

Hieronder staan de stappen die genomen moeten worden om gebruik te kunnen maken van de applicatie.
                                
1. Download de gecompileerde versie van de applicatie [hier](https://github.com/rickv21/csharp-threading-herkansing/releases/download/v1/WeatherApp.zip) (v1 GitHub release).

2. Pak het .zip bestand uit en open WeatherApp.exe om het bestand uit te voeren.

## Problemen en oplossingen

- "De applicatie meldt dat het .NET 8 of iets anders .NET gerelateerds nodig heeft."
   - De gecompileerde versie van de applicatie hoort alles wat nodig is te bevatten, maar als dit niet werkt, moeten de vereisten van de applicatie worden geïnstalleerd.
     Dit zijn Visual Studio 2022 inclusief ".NET 8", ".NET Desktop Development" en de ".NET Multi-platform App UI development" modules.
- "In de applicatie krijg ik bij het ophalen van weerdata de fout melding: "API key not found in environment variables."
    - In de gecompileerde versie van de applicatie zitten onze API keys voor alle weer API's inbegrepen. Deze API keys zijn niet inbegrepen bij de code en dit geeft dus een foutmelding wanneer de code zelf gecompileerd is. Om dit op te lossen moet `.env-example` worden hernoemd naar `.env` en moeten eigen API keys worden toegevoegd. Iedere API (behalve Geocoding en OpenWeatherMap) kunnen volledig worden uitgeschakeld om de applicatie te laten werken zonder de bijbehorende API keys. 
- "Sommige API's geven geen data terug."  
    - Dit kan komen door problemen aan de API kant of omdat het aantal aanvragen van de API is opgebruikt. Iedere API die de applicatie gebruikt, heeft een limiet. Sommige zijn erg laag, omdat er gebruik wordt gemaakt van gratis accounts. Zelfs als er een aantal API's stoppen met werken, kan de applicatie blijven doorwerken met de resterende API's.
    - Dit zijn de limieten van de gebruikte API's:
        - Open Weather Map: 1000 requests per dag.
        - Weerlive: 300 requests per dag.
        - WeatherAPI: 1000000 requests per maand.
        - AccuWeather: 50 requests per dag.
        - WeatherBit: 50 requests per dag.
        - WeatherStack: 100 requests per maand.
        - VisualCrossing: 1000 requests per dag.
        - Geocoding: 3000 requests per dag.
   - Er is een systeem in de applicatie die een melding moet geven als je aan het limiet zit met één van de API's, zodat je deze kan uitzetten. Het kan echter zo zijn dat deze niet accuraat is als het configuratie bestand eerder is verwijderd of als de applicatie door meerdere mensen wordt gebruikt.

## Functionaliteiten

- Het ophalen van weerdata van verschillende publieke API's.
- Weergave van een kaart met een wolklaag waarbij je kan inzoomen.
- Het opslaan van plaatsen om hiervan weerdata in te zien.
- Het inzien van weerdata van verschillende dagen.
- Het inzien van gemiddelde weerdata van de komende week.
- Het weergeven van pop-ups met waarschuwingen in het geval van bepaalde weersituaties.
- Het exporteren van binnengehaalde weerdata als JSON, TXT en CSV.
- Een responsive ontwerp.
- Multithreaded.