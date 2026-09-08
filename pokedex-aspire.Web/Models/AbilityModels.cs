namespace pokedex_aspire.Models;

public class AbilityDetailDto
{
    public int id { get; set; }
    public string? name { get; set; }
    public bool is_main_series { get; set; }
    public Generation? generation { get; set; }
    public List<EffectEntry>? effect_entries { get; set; }
    public List<AbilityPokemonEntry>? pokemon { get; set; }
}

public class AbilityPokemonEntry
{
    public bool is_hidden { get; set; }
    public int slot { get; set; }
    public AbilityPokemonReference? pokemon { get; set; }
}

public class AbilityPokemonReference
{
    public string? name { get; set; }
    public string? url { get; set; }
}