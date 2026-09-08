import asyncio
import io
import logging
import os
import time
from typing import Any

from PIL import Image, ImageOps, UnidentifiedImageError

from predictions import RawPrediction, to_api_prediction


MODEL_ID = "imzynoxprince/pokemons-image-classifier-gen1-gen9"
MODEL_REVISION = "78fccd021ff34aff277f85e8066ed6a01aee2488"
MAX_IMAGE_PIXELS = 16_000_000

logger = logging.getLogger(__name__)
Image.MAX_IMAGE_PIXELS = MAX_IMAGE_PIXELS


class InvalidImageError(ValueError):
    pass


class PokemonClassifier:
    def __init__(self) -> None:
        self._processor: Any | None = None
        self._model: Any | None = None
        self._torch: Any | None = None
        self._device = "cpu"
        self._load_lock = asyncio.Lock()
        self._inference_lock = asyncio.Lock()
        self._load_task: asyncio.Task[None] | None = None
        self._load_error: str | None = None

    @property
    def is_ready(self) -> bool:
        return self._processor is not None and self._model is not None

    @property
    def load_error(self) -> str | None:
        return self._load_error

    @property
    def device(self) -> str:
        return self._device

    def start_loading(self) -> None:
        if self._load_task is None:
            self._load_task = asyncio.create_task(self.load_async())

    async def load_async(self) -> None:
        if self.is_ready:
            return

        async with self._load_lock:
            if self.is_ready:
                return

            try:
                await asyncio.to_thread(self._load_sync)
                self._load_error = None
            except Exception as error:
                self._load_error = str(error)
                logger.exception("Failed to load Pokemon classifier")
                raise

    def _load_sync(self) -> None:
        import torch
        from transformers import AutoImageProcessor, AutoModelForImageClassification

        requested_device = os.getenv("VISION_DEVICE", "cpu").lower()
        self._device = (
            "cuda"
            if requested_device == "cuda" and torch.cuda.is_available()
            else "cpu"
        )
        logger.info("Loading %s at revision %s on %s", MODEL_ID, MODEL_REVISION, self._device)

        processor = AutoImageProcessor.from_pretrained(
            MODEL_ID,
            revision=MODEL_REVISION,
        )
        model = AutoModelForImageClassification.from_pretrained(
            MODEL_ID,
            revision=MODEL_REVISION,
        )
        model.to(self._device)
        model.eval()

        self._torch = torch
        self._processor = processor
        self._model = model
        logger.info("Pokemon classifier ready")

    async def classify_async(self, image_bytes: bytes, result_count: int = 3) -> dict[str, object]:
        started_at = time.perf_counter()

        try:
            image = await asyncio.to_thread(self._decode_image, image_bytes)
        except (UnidentifiedImageError, OSError, Image.DecompressionBombError) as error:
            raise InvalidImageError("The uploaded file is not a valid supported image.") from error

        await self.load_async()
        async with self._inference_lock:
            raw_predictions = await asyncio.to_thread(
                self._classify_sync,
                image,
                result_count,
            )

        predictions = []
        for prediction in raw_predictions:
            api_prediction = to_api_prediction(prediction)
            if api_prediction is not None:
                predictions.append(api_prediction)
            if len(predictions) == result_count:
                break

        return {
            "modelRevision": MODEL_REVISION,
            "device": self._device,
            "latencyMs": round((time.perf_counter() - started_at) * 1000, 1),
            "predictions": predictions,
        }

    @staticmethod
    def _decode_image(image_bytes: bytes) -> Image.Image:
        with Image.open(io.BytesIO(image_bytes)) as source:
            if source.width * source.height > MAX_IMAGE_PIXELS:
                raise InvalidImageError("The image dimensions are too large.")
            return ImageOps.exif_transpose(source).convert("RGB")

    def _classify_sync(self, image: Image.Image, result_count: int) -> list[RawPrediction]:
        inputs = self._processor(images=image, return_tensors="pt")
        inputs = {name: value.to(self._device) for name, value in inputs.items()}

        with self._torch.inference_mode():
            logits = self._model(**inputs).logits[0]
            probabilities = self._torch.softmax(logits, dim=-1)
            top = self._torch.topk(probabilities, k=min(result_count + 2, len(probabilities)))

        predictions = []
        for class_id, confidence in zip(top.indices.tolist(), top.values.tolist()):
            label = self._model.config.id2label[class_id]
            predictions.append(RawPrediction(class_id, label, confidence))
        return predictions