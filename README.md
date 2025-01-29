# Weatherapplication

[> Startdocument van **Monique Sabong Land**, **Rick Vinke**, **Thijs Janse** en **Chris Klunder**.](./start-document/WEATHERAPPLICATION-STARTDOCUMENT.md)

## Installatie handleiding
Hieronder staan de stappen die genomen worden om gebruik te kunnen maken van de applicatie.

1. Zorg ervoor dat windows developer mode aan staat;
2. Installeer Visual Studio 2022 inclusief de ".net desktop development" en de ".NET Multi-platform App UI development" module;
3. Pull de repository;
4. Installeer .NET 8.0 van https://dotnet.microsoft.com/en-us/download/dotnet/8.0;
5. Navigeer in een terminal naar het pad van het project op hetzelfde niveau als het ".sln" bestand;
6. Run het commando "dotnet --version" om te testen of de goede .NET versie is geïnstalleerd. Dit zou .NET 8.0 moeten zijn.
    -Als dit niet het geval is, verwijder de andere .NET versies van "C:\Program Files\dotnet\sdk" totdat dit commando .NET 8.0 weergeeft;
7. Run het commando "dotnet workload install Maui";
8. Open het project in Visual Studio en start de applicatie.