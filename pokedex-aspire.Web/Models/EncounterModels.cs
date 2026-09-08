public class LanguageDto
{
    public string? name { get; set; }
    public string? url { get; set; }
}

public class NameDto
{
    public string? name { get; set; }
    public LanguageDto? language { get; set; }
}

public class EncounterMethodDto
{
    public int id { get; set; }
    public string? name { get; set; }
    public int order { get; set; }
    public List<NameDto>? names { get; set; }
}

public class EncounterDto
{
    public EncounterLocationAreaDto? location_area { get; set; }
    public List<EncounterVersionDetailDto>? version_details { get; set; }
}

public class EncounterLocationAreaDto
{
    public string? name { get; set; }
    public string? url { get; set; }
}

public class EncounterVersionDetailDto
{
    public int max_chance { get; set; }
    public EncounterVersionDto? version { get; set; }
    public List<EncounterDetailDto>? encounter_details { get; set; }
}

public class EncounterVersionDto
{
    public string? name { get; set; }
    public string? url { get; set; }
}

public class EncounterDetailDto
{
    public int chance { get; set; }
    public int min_level { get; set; }
    public int max_level { get; set; }
    public EncounterMethodReferenceDto? method { get; set; }
    public List<EncounterConditionValueDto>? condition_values { get; set; }
}

public class EncounterMethodReferenceDto
{
    public string? name { get; set; }
    public string? url { get; set; }
}

public class EncounterConditionValueDto
{
    public string? name { get; set; }
    public string? url { get; set; }
}

