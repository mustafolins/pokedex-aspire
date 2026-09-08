import unittest

from predictions import RawPrediction, to_api_prediction


class PredictionMappingTests(unittest.TestCase):
    def test_base_class_maps_to_national_pokedex_id(self):
        result = to_api_prediction(RawPrediction(24, "Pikachu", 0.91))

        self.assertEqual(result["pokemonId"], 25)
        self.assertTrue(result["supported"])

    def test_checkpoint_class_is_rejected(self):
        result = to_api_prediction(RawPrediction(1025, ".ipynb_checkpoints", 0.8))

        self.assertIsNone(result)

    def test_event_class_has_no_pokeapi_id(self):
        result = to_api_prediction(RawPrediction(1026, "Foombrella", 0.75))

        self.assertIsNone(result["pokemonId"])
        self.assertFalse(result["supported"])


if __name__ == "__main__":
    unittest.main()