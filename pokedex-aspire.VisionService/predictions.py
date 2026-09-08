from dataclasses import dataclass


POKEAPI_SPECIES_COUNT = 1025
INVALID_CLASS_LABEL = ".ipynb_checkpoints"


@dataclass(frozen=True)
class RawPrediction:
    class_id: int
    label: str
    confidence: float


def to_api_prediction(prediction: RawPrediction) -> dict[str, object] | None:
    if prediction.label == INVALID_CLASS_LABEL:
        return None

    pokemon_id = (
        prediction.class_id + 1
        if 0 <= prediction.class_id < POKEAPI_SPECIES_COUNT
        else None
    )
    return {
        "label": prediction.label,
        "pokemonId": pokemon_id,
        "confidence": prediction.confidence,
        "supported": pokemon_id is not None,
    }