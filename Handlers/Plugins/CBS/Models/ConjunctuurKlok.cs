
using Newtonsoft.Json;

namespace TeamsAIssistant.Handlers.Plugins.CBS.Models;

internal class ConjunctuurKlok
{
    public IEnumerable<string>? Maand { get; set; }
    public IEnumerable<IEnumerable<IEnumerable<dynamic>>>? Bbp { get; set; }
    public IEnumerable<IEnumerable<IEnumerable<dynamic>>>? Consumentenvertrouwen { get; set; }
    public IEnumerable<IEnumerable<IEnumerable<dynamic>>>? Consumptie { get; set; }
    public IEnumerable<IEnumerable<IEnumerable<dynamic>>>? Faillissementen { get; set; }
    
    [JsonProperty("Gewerkte uren")]
    public IEnumerable<IEnumerable<IEnumerable<dynamic>>>? Gewerkteuren { get; set; }
    
    public IEnumerable<IEnumerable<IEnumerable<dynamic>>>? Investeringen { get; set; }

    [JsonProperty("Omzet uitzendbranche")]
    public IEnumerable<IEnumerable<IEnumerable<dynamic>>>? OmzetUitzendbranche { get; set; }

    [JsonProperty("Prijzen koopwoningen")]
    public IEnumerable<IEnumerable<IEnumerable<dynamic>>>? PrijzenKoopwoningen { get; set; }

    public IEnumerable<IEnumerable<IEnumerable<dynamic>>>? Producentenvertrouwen { get; set; }

    public IEnumerable<IEnumerable<IEnumerable<dynamic>>>? Productie { get; set; }
    public IEnumerable<IEnumerable<IEnumerable<dynamic>>>? Uitvoer { get; set; }
    public IEnumerable<IEnumerable<IEnumerable<dynamic>>>? Vacatures { get; set; }
    public IEnumerable<IEnumerable<IEnumerable<dynamic>>>? Werkloosheid { get; set; }

    
}