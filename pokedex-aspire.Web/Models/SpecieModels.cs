
public class Color
{
    public string? name { get; set; }
    public string? url { get; set; }
}

public class EggGroup
{
    public string? name { get; set; }
    public string? url { get; set; }
}

public class EvolutionChain
{
    public string? url { get; set; }
}

public class EvolvesFromSpecies
{
    public string? name { get; set; }
    public string? url { get; set; }
}

public class FlavorTextEntry
{
    public string? flavor_text { get; set; }
    public Language? language { get; set; }
    public Version? version { get; set; }
}

public class FormDescription
{
    public string? description { get; set; }
    public Language? language { get; set; }
}

public class Genera
{
    public string? genus { get; set; }
    public Language? language { get; set; }
}

public class Generation
{
    public string? name { get; set; }
    public string? url { get; set; }
}

public class GrowthRate
{
    public string? name { get; set; }
    public string? url { get; set; }
}

public class Habitat
{
    public string? name { get; set; }
    public string? url { get; set; }
}

public class Language
{
    public string? name { get; set; }
    public string? url { get; set; }
}

public class Name
{
    public string? name { get; set; }
    public Language? language { get; set; }
}

public class Pokedex
{
    public string? name { get; set; }
    public string? url { get; set; }
}

public class PokedexNumber
{
    public int entry_number { get; set; }
    public Pokedex? pokedex { get; set; }
}

public class Pokemon
{
    public string? name { get; set; }
    public string? url { get; set; }
}

public class SpecieDto
{
    public int id { get; set; }
    public string? name { get; set; }
    public int order { get; set; }
    public int gender_rate { get; set; }
    public int capture_rate { get; set; }
    public int base_happiness { get; set; }
    public bool is_baby { get; set; }
    public bool is_legendary { get; set; }
    public bool is_mythical { get; set; }
    public int hatch_counter { get; set; }
    public bool has_gender_differences { get; set; }
    public bool forms_switchable { get; set; }
    public GrowthRate? growth_rate { get; set; }
    public List<PokedexNumber>? pokedex_numbers { get; set; }
    public List<EggGroup>? egg_groups { get; set; }
    public Color? color { get; set; }
    public Shape? shape { get; set; }
    public EvolvesFromSpecies? evolves_from_species { get; set; }
    public EvolutionChain? evolution_chain { get; set; }
    public Habitat? habitat { get; set; }
    public Generation? generation { get; set; }
    public List<Name>? names { get; set; }
    public List<FlavorTextEntry>? flavor_text_entries { get; set; }
    public List<FormDescription>? form_descriptions { get; set; }
    public List<Genera>? genera { get; set; }
    public List<Variety>? varieties { get; set; }
}

public class Shape
{
    public string? name { get; set; }
    public string? url { get; set; }
}

public class Variety
{
    public bool is_default { get; set; }
    public Pokemon? pokemon { get; set; }
}

public class Version
{
    public string? name { get; set; }
    public string? url { get; set; }
}

