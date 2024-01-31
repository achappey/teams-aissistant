namespace TeamsAIssistant.Handlers.Plugins.BAG.Models;

internal class Adres
{
    public string? OpenbareRuimteNaam { get; set; }
    
    public string? KorteNaam { get; set; }
    
    public string? Postcode { get; set; }
   
    public int Huisnummer { get; set; }
    
    public string? WoonplaatsNaam { get; set; }
   
    public double? Latitude
    {
        get
        {
            return AdresseerbaarObjectGeometrie?.Punt?.Latitude;
        }
        set { }
    }

   
    public double? Longitude
    {
        get
        {
            return AdresseerbaarObjectGeometrie?.Punt?.Longitude;
        }
        set { }
    }

   
    public string? NummeraanduidingIdentificatie { get; set; }

   
    public string? OpenbareRuimteIdentificatie { get; set; }

   
    public string? WoonplaatsIdentificatie { get; set; }

   
    public string? AdresseerbaarObjectIdentificatie { get; set; }

   
    public List<string>? PandIdentificaties { get; set; }

   
    public string? Adresregel5 { get; set; }

   
    public string? Adresregel6 { get; set; }

   
    public string? TypeAdresseerbaarObject { get; set; }

   
    public AdresseerbaarObjectGeometry? AdresseerbaarObjectGeometrie { get; set; }

   
    public string? AdresseerbaarObjectStatus { get; set; }

   
    public List<string>? Gebruiksdoelen { get; set; }

   
    public int? Oppervlakte { get; set; }

   
    public List<string>? OorspronkelijkBouwjaar { get; set; }

   
    public List<string>? PandStatussen { get; set; }

   
    public Links? Links { get; set; }
}

internal class AdresseerbaarObjectGeometry
{
    public Point? Punt { get; set; }
}

internal class Point
{
    public string? Type { get; set; }
    public List<double>? Coordinates { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

internal class Links
{
    public Self? Self { get; set; }
    public Self? OpenbareRuimte { get; set; }
    public Self? Nummeraanduiding { get; set; }
    public Self? Woonplaats { get; set; }
    public Self? AdresseerbaarObject { get; set; }
    public List<Building>? Panden { get; set; }
}
