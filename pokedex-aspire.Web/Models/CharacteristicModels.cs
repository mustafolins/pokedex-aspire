namespace pokedex_aspire.Models;

public class CharacteristicIndexDto
{
    public int count { get; set; }
    public string? next { get; set; }
    public string? previous { get; set; }
    public List<CharacteristicReferenceDto>? results { get; set; }
}

public class CharacteristicReferenceDto
{
    public string? url { get; set; }
}

public class CharacteristicDto
{
    public int id { get; set; }
    public int gene_modulo { get; set; }
    public List<int>? possible_values { get; set; }
    public CharacteristicStatDto? highest_stat { get; set; }
    public List<CharacteristicDescriptionDto>? descriptions { get; set; }
}

public class CharacteristicStatDto
{
    public string? name { get; set; }
    public string? url { get; set; }
}

public class CharacteristicDescriptionDto
{
    public string? description { get; set; }
    public CharacteristicLanguageDto? language { get; set; }
}

public class CharacteristicLanguageDto
{
    public string? name { get; set; }
    public string? url { get; set; }
}