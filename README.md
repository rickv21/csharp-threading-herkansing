# Weatherapplication

[> Startdocument van **Monique Sabong Land**, **Rick Vinke**, **Thijs Janse** en **Chris Klunder**.](./start-document/WEATHERAPPLICATION-STARTDOCUMENT.md)

Voor het vak C# Threading hebben we als groep een weerapplicatie gemaakt.
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
Hieronder staan de stappen die genomen worden om gebruik te kunnen maken van de applicatie.
                                
1. Pull de Git repository, dit kan op meerdere manieren:  
    A: Als je [Git](https://git-scm.com/) geinstalleerd hebt, dan kan je in je terminal het commando `git clone https://github.com/rickv21/csharp-threading-herkansing.git` uitvoeren.

    B: Je kan de code downloaden door [hier](https://github.com/rickv21/csharp-threading-herkansing/archive/refs/heads/master.zip) te klikken. In dit geval moet je het .zip archief uitpakken.

2. Ga naar de map van het project en open PowerShell in deze map.

3. Voer ./Add-AppDevPackage.ps1 uit in PowerShell, je zal om administrator rechten worden gevraagd. Na het toevoegen van het cerificaat zal het een foutmelding geven, dit kan je negeren.

4. Dubbel klik op WeatherApp_1.0.0.0_x64.msix en klik op installeren.

5. De applicatie is nu geinstalleerd en je kan het gebruiken.

## Problemen en oplossingen

- "Bij het uitvoeren van het PowerShell script krijg ik de error "Add-AppDevPackage.ps1 cannot be loaded because the execution of scripts is disabled on this system.""   - Open PowerShell als Administrator en voer dit commando uit: `set-executionpolicy remotesigned`. Dit laat je toe om scripts van het internet uit te voeren. Vergeet niet als je klaar bent om dit weer terug te veranderen met `set-executionpolicy restricted` wanneer je klaar bent.
- "De applicatie meld dat het .NET 8 of iets anders .NET gerelarteerds nodig heeft."
   - De applicatie hoort alles wat het nodig is te bevatten, maar als dit niet werkt dan moetne de vereisten van de applicatie worden geinstalleerd.
     Dit zijn Visual Studio 2022 inclusief ".NET 8", ".NET Desktop Development" en de ".NET Multi-platform App UI development" modules.
- "Zelfs na het installeren van het certificaat wil de applicatie niet installeren."
    - .NET MAUI is erg streng met certificaten. Als de meegeleverde certificaat niet werkt dan kan je de applicatie helaas alleen uitvoeren door Visual Studio 2022 met de vereisten uit de vorige vraag te installeren en het via Visual Studio uit te voeren.

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